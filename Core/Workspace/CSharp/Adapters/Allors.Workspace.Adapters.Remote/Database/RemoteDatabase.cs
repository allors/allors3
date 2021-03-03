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
    using Meta;
    using Polly;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Auth;

    public class RemoteDatabase
    {
        private readonly Dictionary<Identity, RemoteDatabaseRoles> databaseRolesByDatabaseId;

        private readonly Dictionary<IClass, Dictionary<IOperandType, RemotePermission>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, RemotePermission>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, RemotePermission>> executePermissionByOperandTypeByClass;

        public RemoteDatabase(IMetaPopulation metaPopulation, HttpClient httpClient, Identities identities)
        {
            this.MetaPopulation = metaPopulation;
            this.HttpClient = httpClient;
            this.Identities = identities;

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.AccessControlById = new Dictionary<long, RemoteAccessControl>();
            this.PermissionById = new Dictionary<long, RemotePermission>();

            this.databaseRolesByDatabaseId = new Dictionary<Identity, RemoteDatabaseRoles>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, RemotePermission>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, RemotePermission>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, RemotePermission>>();
        }

        ~RemoteDatabase() => this.HttpClient.Dispose();

        public IMetaPopulation MetaPopulation { get; }

        public HttpClient HttpClient { get; }

        public Identities Identities { get; }

        public IAsyncPolicy Policy { get; set; } = Polly.Policy
           .Handle<HttpRequestException>()
           .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public string UserId { get; private set; }

        internal Dictionary<long, RemoteAccessControl> AccessControlById { get; }

        internal Dictionary<long, RemotePermission> PermissionById { get; }

        public async Task<bool> Login(Uri url, string username, string password)
        {
            var request = new { UserName = username, Password = password };
            using (var response = await this.PostAsJsonAsync(url, request))
            {
                response.EnsureSuccessStatusCode();
                var authResult = await this.ReadAsAsync<AuthenticationTokenResponse>(response);
                if (!authResult.Authenticated)
                {
                    return false;
                }

                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
                this.UserId = authResult.UserId;

                return true;
            }
        }

        internal RemoteDatabaseRoles PushResponse(Identity identity, IClass @class)
        {
            var databaseObject = new RemoteDatabaseRoles(this, identity, @class);
            this.databaseRolesByDatabaseId[identity] = databaseObject;
            return databaseObject;
        }

        internal SecurityRequest SyncResponse(SyncResponse syncResponse)
        {
            var ctx = new RemoteResponseContext(this.AccessControlById, this.PermissionById);
            foreach (var syncResponseObject in syncResponse.Objects)
            {
                var databaseRoles = new RemoteDatabaseRoles(this, ctx, syncResponseObject);
                this.databaseRolesByDatabaseId[databaseRoles.Identity] = databaseRoles;
            }

            if (ctx.MissingAccessControlIds.Count > 0 || ctx.MissingPermissionIds.Count > 0)
            {
                return new SecurityRequest
                {
                    AccessControls = ctx.MissingAccessControlIds.Select(v => v.ToString()).ToArray(),
                    Permissions = ctx.MissingPermissionIds.Select(v => v.ToString()).ToArray(),
                };
            }

            return null;
        }

        internal SyncRequest Diff(PullResponse response)
        {
            var ctx = new RemoteResponseContext(this.AccessControlById, this.PermissionById);

            return new SyncRequest
            {
                Objects = response.Objects
                    .Where(v =>
                    {
                        var id = long.Parse(v[0]);
                        var identity = this.Identities.GetOrCreate(id);
                        this.databaseRolesByDatabaseId.TryGetValue(identity, out var databaseObject);
                        if (databaseObject == null)
                        {
                            return true;
                        }

                        var sortedAccessControlIds = v.Length > 2 ? ctx.ReadSortedAccessControlIds(v[2]) : null;
                        var sortedDeniedPermissionIds = v.Length > 3 ? ctx.ReadSortedDeniedPermissionIds(v[3]) : null;

                        var version = long.Parse(v[1]);
                        if (!databaseObject.Version.Equals(version))
                        {
                            return true;
                        }

                        if (v.Length == 2)
                        {
                            return false;
                        }

                        if (v.Length == 3)
                        {
                            if (databaseObject.SortedDeniedPermissionIds != null)
                            {
                                return true;
                            }

                            return !Equals(databaseObject.SortedAccessControlIds, sortedAccessControlIds);
                        }

                        return !Equals(databaseObject.SortedAccessControlIds, sortedAccessControlIds) ||
                               !Equals(databaseObject.SortedDeniedPermissionIds, sortedDeniedPermissionIds);
                    })
                    .Select(v => v[0]).ToArray(),
            };
        }

        internal RemoteDatabaseRoles Get(Identity databaseId)
        {
            var databaseObject = this.databaseRolesByDatabaseId[databaseId];
            if (databaseObject == null)
            {
                throw new Exception($"Object with id {databaseId} is not present.");
            }

            return databaseObject;
        }

        internal SecurityRequest SecurityResponse(SecurityResponse securityResponse)
        {
            if (securityResponse.Permissions != null)
            {
                foreach (var syncResponsePermission in securityResponse.Permissions)
                {
                    var id = long.Parse(syncResponsePermission[0]);
                    var @class = (IClass)this.MetaPopulation.Find(Guid.Parse(syncResponsePermission[1]));
                    var metaObject = this.MetaPopulation.Find(Guid.Parse(syncResponsePermission[2]));
                    IOperandType operandType = (metaObject as IRelationType)?.RoleType;
                    operandType ??= metaObject as IMethodType;

                    Enum.TryParse(syncResponsePermission[3], out Operations operation);

                    var permission = new RemotePermission(id, @class, operandType, operation);
                    this.PermissionById[id] = permission;

                    switch (operation)
                    {
                        case Operations.Read:
                            if (!this.readPermissionByOperandTypeByClass.TryGetValue(@class,
                                out var readPermissionByOperandType))
                            {
                                readPermissionByOperandType = new Dictionary<IOperandType, RemotePermission>();
                                this.readPermissionByOperandTypeByClass[@class] = readPermissionByOperandType;
                            }

                            readPermissionByOperandType[operandType] = permission;

                            break;

                        case Operations.Write:
                            if (!this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var writePermissionByOperandType))
                            {
                                writePermissionByOperandType = new Dictionary<IOperandType, RemotePermission>();
                                this.writePermissionByOperandTypeByClass[@class] = writePermissionByOperandType;
                            }

                            writePermissionByOperandType[operandType] = permission;

                            break;

                        case Operations.Execute:
                            if (!this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var executePermissionByOperandType))
                            {
                                executePermissionByOperandType = new Dictionary<IOperandType, RemotePermission>();
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
                    var id = long.Parse(syncResponseAccessControl.Id);
                    var version = long.Parse(syncResponseAccessControl.Version);
                    var permissionsIds = syncResponseAccessControl.PermissionIds
                        ?.Split(',')
                        .Select(v =>
                        {
                            var permissionId = long.Parse(v);
                            if (!this.PermissionById.ContainsKey(permissionId))
                            {
                                missingPermissionIds ??= new HashSet<long>();
                                missingPermissionIds.Add(permissionId);
                            }

                            return permissionId;
                        });

                    this.AccessControlById[id] = new RemoteAccessControl(id, version, new HashSet<long>(permissionsIds));
                }
            }

            if (missingPermissionIds != null)
            {
                return new SecurityRequest
                {
                    Permissions = missingPermissionIds.Select(v => v.ToString()).ToArray(),
                };
            }

            return null;
        }

        internal IEnumerable<RemoteDatabaseRoles> Get(IComposite objectType)
        {
            var classes = new HashSet<IClass>(objectType.DatabaseClasses);
            return this.databaseRolesByDatabaseId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }

        internal RemotePermission GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            switch (operation)
            {
                case Operations.Read:
                    if (this.readPermissionByOperandTypeByClass.TryGetValue(@class, out var readPermissionByOperandType))
                    {
                        if (readPermissionByOperandType.TryGetValue(operandType, out var readPermission))
                        {
                            return readPermission;
                        }
                    }

                    return null;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class, out var writePermissionByOperandType))
                    {
                        if (writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
                        {
                            return writePermission;
                        }
                    }

                    return null;

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class, out var executePermissionByOperandType))
                    {
                        if (executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                        {
                            return executePermission;
                        }
                    }

                    return null;
            }
        }

        internal async Task<PullResponse> Pull(PullRequest pullRequest)
        {
            var uri = new Uri("pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullRequest);
            response.EnsureSuccessStatusCode();
            return await this.ReadAsAsync<PullResponse>(response);
        }

        internal async Task<PullResponse> Pull(string name, object pullRequest)
        {
            var uri = new Uri(name + "/pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<PullResponse>(response);
        }

        internal async Task<SyncResponse> Sync(SyncRequest syncRequest)
        {
            var uri = new Uri("sync", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, syncRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<SyncResponse>(response);
        }

        internal async Task<PushResponse> Push(PushRequest pushRequest)
        {
            var uri = new Uri("push", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pushRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<PushResponse>(response);
        }

        internal async Task<InvokeResponse> Invoke(InvokeRequest invokeRequest)
        {
            var uri = new Uri("invoke", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, invokeRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<InvokeResponse>(response);
        }

        internal async Task<InvokeResponse> Invoke(string service, object args)
        {
            var uri = new Uri(service + "/Pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, args);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<InvokeResponse>(response);
        }

        internal async Task<SecurityResponse> Security(SecurityRequest securityRequest)
        {
            var uri = new Uri("security", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, securityRequest);
            response.EnsureSuccessStatusCode();

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
    }
}
