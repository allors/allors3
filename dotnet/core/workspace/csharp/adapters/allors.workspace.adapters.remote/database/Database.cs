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
    using Numbers;
    using Polly;

    public class DatabaseConnection : Adapters.DatabaseConnection
    {
        private readonly Dictionary<long, DatabaseRecord> recordsById;

        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> executePermissionByOperandTypeByClass;

        private readonly WorkspaceIdGenerator workspaceIdGenerator;
        private readonly Func<IWorkspaceServices> servicesBuilder;

        public DatabaseConnection(Configuration configuration, Func<IWorkspaceServices> servicesBuilder, HttpClient httpClient, WorkspaceIdGenerator workspaceIdGenerator, INumbers numbers) : base(configuration)
        {
            this.HttpClient = httpClient;
            this.workspaceIdGenerator = workspaceIdGenerator;
            this.servicesBuilder = servicesBuilder;
            this.Numbers = numbers;

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.AccessControlById = new Dictionary<long, AccessControl>();

            this.recordsById = new Dictionary<long, DatabaseRecord>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();

            this.Permissions = new HashSet<long>();
        }

        public IAsyncPolicy Policy { get; set; } = Polly.Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public HttpClient HttpClient { get; }

        public INumbers Numbers { get; }

        public string UserId { get; private set; }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        internal ISet<long> Permissions { get; }

        ~DatabaseConnection() => this.HttpClient.Dispose();

        public async Task<bool> Login(Uri url, string username, string password)
        {
            var request = new AuthenticationTokenRequest { Login = username, Password = password };
            using var response = await this.PostAsJsonAsync(url, request);
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

        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation)
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

                    return 0;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var writePermissionByOperandType))
                    {
                        if (writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
                        {
                            return writePermission;
                        }
                    }

                    return 0;

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var executePermissionByOperandType))
                    {
                        if (executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                        {
                            return executePermission;
                        }
                    }

                    return 0;
            }
        }

        internal SecurityRequest SyncResponse(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this);
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
                        if (!this.recordsById.TryGetValue(v.Id, out var rec))
                        {
                            return true;
                        }

                        if (!rec.Version.Equals(v.Version))
                        {
                            return true;
                        }

                        if (!this.Numbers.AreEqual(rec.AccessControlIds, v.AccessControls))
                        {
                            return true;
                        }

                        if (!this.Numbers.AreEqual(rec.DeniedPermissions, v.DeniedPermissions))
                        {
                            return true;
                        }

                        // TODO: Use smarter updates for DeniedPermissions

                        return false;
                    })
                    .Select(v => v.Id).ToArray()
            };

        public override IWorkspace CreateWorkspace() => new Workspace(this, this.servicesBuilder(), this.Numbers, this.workspaceIdGenerator);

        public override Adapters.DatabaseRecord GetRecord(long id)
        {
            this.recordsById.TryGetValue(id, out var databaseObjects);
            return databaseObjects;
        }

        internal SecurityRequest SecurityResponse(SecurityResponse securityResponse)
        {
            if (securityResponse.Permissions != null)
            {
                foreach (var syncResponsePermission in securityResponse.Permissions)
                {
                    var id = syncResponsePermission[0];
                    var @class = (IClass)this.Configuration.MetaPopulation.FindByTag((int)syncResponsePermission[1]);
                    var metaObject = this.Configuration.MetaPopulation.FindByTag((int)syncResponsePermission[2]);
                    var operandType = (IOperandType)(metaObject as IRelationType)?.RoleType ?? (IMethodType)metaObject;
                    var operation = (Operations)syncResponsePermission[3];
                    this.Permissions.Add(id);

                    switch (operation)
                    {
                        case Operations.Read:
                            if (!this.readPermissionByOperandTypeByClass.TryGetValue(@class,
                                out var readPermissionByOperandType))
                            {
                                readPermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.readPermissionByOperandTypeByClass[@class] = readPermissionByOperandType;
                            }

                            readPermissionByOperandType[operandType] = id;

                            break;

                        case Operations.Write:
                            if (!this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var writePermissionByOperandType))
                            {
                                writePermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.writePermissionByOperandTypeByClass[@class] = writePermissionByOperandType;
                            }

                            writePermissionByOperandType[operandType] = id;

                            break;

                        case Operations.Execute:
                            if (!this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var executePermissionByOperandType))
                            {
                                executePermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.executePermissionByOperandTypeByClass[@class] = executePermissionByOperandType;
                            }

                            executePermissionByOperandType[operandType] = id;

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
                            if (this.Permissions.Contains(v))
                            {
                                return v;
                            }

                            (missingPermissionIds ??= new HashSet<long>()).Add(v);

                            return v;
                        });

                    var permissionIdSet = permissionsIds != null
                        ? (ISet<long>)new HashSet<long>(permissionsIds)
                        : EmptySet<long>.Instance;

                    this.AccessControlById[id] = new AccessControl { Version = version, PermissionIds = permissionIdSet };
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

        internal async Task<SecurityResponse> Security(SecurityRequest securityRequest)
        {
            var uri = new Uri("security", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, securityRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<SecurityResponse>(response);
        }

        internal DatabaseRecord OnPushed(long id, IClass @class)
        {
            var record = new DatabaseRecord(this, @class, id);
            this.recordsById[id] = record;
            return record;
        }

        private async Task<HttpResponseMessage> PostAsJsonAsync(Uri uri, object args) =>
            await this.Policy.ExecuteAsync(
                async () =>
                {
                    // TODO: use SerializeToUtf8Bytes()
                    var json = JsonSerializer.Serialize(args);
                    return await this.HttpClient.PostAsync(
                        uri,
                        new StringContent(json, Encoding.UTF8, "application/json"));
                });

        private async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
