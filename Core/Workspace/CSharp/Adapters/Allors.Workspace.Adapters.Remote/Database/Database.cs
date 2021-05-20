// <copyright file="RemoteDatabase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Auth;
    using Collections;
    using Meta;
    using Polly;

    public class Database : Adapters.Database
    {
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> executePermissionByOperandTypeByClass;

        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<long, DatabaseRecord> recordsById;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> writePermissionByOperandTypeByClass;

        internal Database(IMetaPopulation metaPopulation, HttpClient httpClient, WorkspaceIdGenerator workspaceIdGenerator) : base(metaPopulation)
        {
            this.HttpClient = httpClient;

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.AccessControlById = new Dictionary<long, AccessControl>();
            this.PermissionById = new Dictionary<long, Permission>();

            this.recordsById = new Dictionary<long, DatabaseRecord>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
        }

        public HttpClient HttpClient { get; }

        internal IAsyncPolicy Policy { get; set; } = Polly.Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public string UserId { get; private set; }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        internal Dictionary<long, Permission> PermissionById { get; }

        ~Database() => this.HttpClient.Dispose();

        public async Task<bool> Login(Uri url, string username, string password)
        {
            var request = new AuthenticationTokenRequest { Login = username, Password = password };
            using var response = await this.PostAsJsonAsync(url, request);
            _ = response.EnsureSuccessStatusCode();
            var authResult = await this.ReadAsAsync<AuthenticationTokenResponse>(response);
            if (!authResult.Authenticated)
            {
                return false;
            }

            this.HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authResult.Token);
            this.UserId = authResult.UserId;

            return true;
        }

        internal Permission GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            switch (operation)
            {
                case Operations.Read:
                    if (this.readPermissionByOperandTypeByClass.TryGetValue(@class,
                        out var readPermissionByOperandType))
                    {
                        if (readPermissionByOperandType.TryGetValue(operandType, out var readPermission))
                        {
                            return readPermission;
                        }
                    }

                    return null;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var writePermissionByOperandType))
                    {
                        if (writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
                        {
                            return writePermission;
                        }
                    }

                    return null;

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var executePermissionByOperandType))
                    {
                        if (executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                        {
                            return executePermission;
                        }
                    }

                    return null;
            }
        }

        internal DatabaseRecord PushResponse(long identity, IClass @class)
        {
            var databaseObject = new DatabaseRecord(this, identity, @class);
            this.recordsById[identity] = databaseObject;
            return databaseObject;
        }

        internal SecurityRequest SyncResponse(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this.AccessControlById, this.PermissionById);
            foreach (var syncResponseObject in syncResponse.Objects)
            {
                var databaseObjects = new DatabaseRecord(this, ctx, syncResponseObject);
                this.recordsById[databaseObjects.Id] = databaseObjects;
            }

            if (ctx.MissingAccessControlIds.Count > 0 || ctx.MissingPermissionIds.Count > 0)
            {
                return new SecurityRequest
                {
                    AccessControls = ctx.MissingAccessControlIds.Select(v => v).ToArray(),
                    Permissions = ctx.MissingPermissionIds.Select(v => v).ToArray()
                };
            }

            return null;
        }

        internal SyncRequest Diff(PullResponse response) =>
            new SyncRequest
            {
                Objects = response.Pool
                    .Where(v =>
                    {
                        if (!this.recordsById.TryGetValue(v.Id, out var databaseObject))
                        {
                            return true;
                        }

                        if (!databaseObject.Version.Equals(v.Version))
                        {
                            return true;
                        }

                        // TODO: Update AccessControlIds if proper subset
                        if (v.AccessControls == null)
                        {
                            if (databaseObject.AccessControlIds.Count > 0)
                            {
                                return true;
                            }
                        }
                        else if (!databaseObject.AccessControlIds.SetEquals(v.AccessControls))
                        {
                            return true;
                        }

                        if (v.DeniedPermissions == null)
                        {
                            if (databaseObject.DeniedPermissionIds.Count > 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (databaseObject.DeniedPermissionIds.SetEquals(v.DeniedPermissions))
                            {
                                return false;
                            }

                            if (!databaseObject.DeniedPermissionIds.IsProperSubsetOf(v.DeniedPermissions))
                            {
                                return true;
                            }

                            databaseObject.UpdateDeniedPermissions(v.DeniedPermissions);
                        }

                        return false;
                    })
                    .Select(v => v.Id).ToArray()
            };

        internal DatabaseRecord GetRecord(long identity)
        {
            _ = this.recordsById.TryGetValue(identity, out var databaseObjects);
            return databaseObjects;
        }

        internal SecurityRequest SecurityResponse(SecurityResponse securityResponse)
        {
            if (securityResponse.Permissions != null)
            {
                foreach (var syncResponsePermission in securityResponse.Permissions)
                {
                    var id = syncResponsePermission[0];
                    var @class = (IClass)this.MetaPopulation.FindByTag((int)syncResponsePermission[1]);
                    var metaObject = this.MetaPopulation.FindByTag((int)syncResponsePermission[2]);
                    var operandType = (IOperandType)(metaObject as IRelationType)?.RoleType ?? (IMethodType)metaObject;
                    var operation = (Operations)syncResponsePermission[3];
                    var permission = new Permission(id, @class, operandType, operation);
                    this.PermissionById[id] = permission;

                    switch (operation)
                    {
                        case Operations.Read:
                            if (!this.readPermissionByOperandTypeByClass.TryGetValue(@class,
                                out var readPermissionByOperandType))
                            {
                                readPermissionByOperandType = new Dictionary<IOperandType, Permission>();
                                this.readPermissionByOperandTypeByClass[@class] = readPermissionByOperandType;
                            }

                            readPermissionByOperandType[operandType] = permission;

                            break;

                        case Operations.Write:
                            if (!this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var writePermissionByOperandType))
                            {
                                writePermissionByOperandType = new Dictionary<IOperandType, Permission>();
                                this.writePermissionByOperandTypeByClass[@class] = writePermissionByOperandType;
                            }

                            writePermissionByOperandType[operandType] = permission;

                            break;

                        case Operations.Execute:
                            if (!this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var executePermissionByOperandType))
                            {
                                executePermissionByOperandType = new Dictionary<IOperandType, Permission>();
                                this.executePermissionByOperandTypeByClass[@class] = executePermissionByOperandType;
                            }

                            executePermissionByOperandType[operandType] = permission;

                            break;
                    }
                }
            }

            HashSet<long> missingPermissionIds = null;
            if (securityResponse.AccessControls != null)
            {
                foreach (var syncResponseAccessControl in securityResponse.AccessControls)
                {
                    var id = syncResponseAccessControl.Id;
                    var version = syncResponseAccessControl.Version;
                    var permissionsIds = syncResponseAccessControl.PermissionIds
                        ?.Select(v =>
                        {
                            if (this.PermissionById.ContainsKey(v))
                            {
                                return v;
                            }

                            _ = (missingPermissionIds ??= new HashSet<long>()).Add(v);

                            return v;
                        });

                    var permissionIdSet = permissionsIds != null
                        ? (ISet<long>)new HashSet<long>(permissionsIds)
                        : EmptySet<long>.Instance;

                    this.AccessControlById[id] = new AccessControl(id) { Version = version, PermissionIds = permissionIdSet };
                }
            }

            if (missingPermissionIds != null)
            {
                return new SecurityRequest { Permissions = missingPermissionIds.ToArray() };
            }

            return null;
        }

        internal async Task<PullResponse> Pull(PullRequest pullRequest)
        {
            var uri = new Uri("pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullRequest);
            _ = response.EnsureSuccessStatusCode();
            return await this.ReadAsAsync<PullResponse>(response);
        }

        internal async Task<PullResponse> Pull(
            string name,
            IEnumerable<KeyValuePair<string, object>> values = null,
            IEnumerable<KeyValuePair<string, IObject>> objects = null,
            IEnumerable<KeyValuePair<string, IEnumerable<IObject>>> collections = null)
        {
            var pullArgs = new PullArgs
            {
                Values = values?.ToDictionary(v => v.Key, v => v.Value),
                Objects = objects?.ToDictionary(v => v.Key, v => v.Value.Id),
                Collections = collections?.ToDictionary(v => v.Key, v => v.Value.Select(v => v.Id).ToArray())
            };

            var uri = new Uri(name + "/pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullArgs);
            _ = response.EnsureSuccessStatusCode();
            return await this.ReadAsAsync<PullResponse>(response);
        }

        internal async Task<SyncResponse> Sync(SyncRequest syncRequest)
        {
            var uri = new Uri("sync", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, syncRequest);
            _ = response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<SyncResponse>(response);
        }

        internal async Task<PushResponse> Push(PushRequest pushRequest)
        {
            var uri = new Uri("push", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pushRequest);
            _ = response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<PushResponse>(response);
        }

        internal async Task<InvokeResponse> Invoke(InvokeRequest invokeRequest)
        {
            var uri = new Uri("invoke", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, invokeRequest);
            _ = response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<InvokeResponse>(response);
        }

        internal async Task<SecurityResponse> Security(SecurityRequest securityRequest)
        {
            var uri = new Uri("security", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, securityRequest);
            _ = response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<SecurityResponse>(response);
        }

        internal async Task<HttpResponseMessage> PostAsJsonAsync(Uri uri, object args) =>
            await this.Policy.ExecuteAsync(
                async () =>
                {
                    // TODO: use SerializeToUtf8Bytes()
                    var json = JsonSerializer.Serialize(args);
                    return await this.HttpClient.PostAsync(
                        uri,
                        new StringContent(json, Encoding.UTF8, "application/json"));
                });

        internal async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        internal DatabaseRecord OnPushed(long id, IClass @class)
        {
            var record = new DatabaseRecord(this, id, @class);
            this.recordsById[id] = record;
            return record;
        }
    }
}
