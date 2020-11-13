// <copyright file="Database.cs" company="Allors bvba">
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
    using Allors.Workspace.Meta;
    using Polly;
    using Protocol.Database.Invoke;
    using Protocol.Database.Pull;
    using Protocol.Database.Push;
    using Protocol.Database.Security;
    using Protocol.Database.Sync;
    using Protocol.Json;

    public class Database
    {
        private readonly Dictionary<long, DatabaseObject> databaseObjectByDatabaseId;

        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> executePermissionByOperandTypeByClass;

        private long worskpaceIdCounter;
        private DatabaseChangeSet databaseChangeSet;

        public Database(IMetaPopulation metaPopulation, HttpClient httpClient)
        {
            this.MetaPopulation = metaPopulation;
            this.HttpClient = httpClient;

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.AccessControlById = new Dictionary<long, AccessControl>();
            this.PermissionById = new Dictionary<long, Permission>();

            this.databaseObjectByDatabaseId = new Dictionary<long, DatabaseObject>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();

            this.worskpaceIdCounter = 0;
            this.WorkspaceIdByDatabaseId = new Dictionary<long, long>();
            this.DatabaseIdByWorkspaceId = new Dictionary<long, long>();

            this.databaseChangeSet = new DatabaseChangeSet();
        }

        ~Database() => this.HttpClient.Dispose();

        public IMetaPopulation MetaPopulation { get; }

        public HttpClient HttpClient { get; }

        public IAsyncPolicy Policy { get; set; } = Polly.Policy
           .Handle<HttpRequestException>()
           .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public string UserId { get; private set; }

        internal Dictionary<long, long> WorkspaceIdByDatabaseId { get; }

        internal Dictionary<long, long> DatabaseIdByWorkspaceId { get; }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        internal Dictionary<long, Permission> PermissionById { get; }

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

        internal DatabaseObject PushResponse(long databaseId, IClass @class)
        {
            var databaseObject = new DatabaseObject(this, databaseId, @class);
            this.databaseObjectByDatabaseId[databaseId] = databaseObject;
            return databaseObject;
        }

        internal SecurityRequest SyncResponse(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this.AccessControlById, this.PermissionById);
            foreach (var syncResponseObject in syncResponse.Objects)
            {
                var databaseObject = new DatabaseObject(this, ctx, syncResponseObject);
                this.databaseObjectByDatabaseId[databaseObject.DatabaseId] = databaseObject;
                if (!this.DatabaseIdByWorkspaceId.TryGetValue(databaseObject.DatabaseId, out var workspaceId))
                {
                    workspaceId = this.NextWorkspaceId();
                    this.WorkspaceIdByDatabaseId[databaseObject.DatabaseId] = workspaceId;
                    this.DatabaseIdByWorkspaceId[workspaceId] = databaseObject.DatabaseId;
                }
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
            var ctx = new ResponseContext(this.AccessControlById, this.PermissionById);

            return new SyncRequest
            {
                Objects = response.Objects
                    .Where(v =>
                    {
                        var id = long.Parse(v[0]);
                        this.databaseObjectByDatabaseId.TryGetValue(id, out var databaseObject);
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

        internal long NextWorkspaceId() => --this.worskpaceIdCounter;

        internal long ToWorkspaceId(long id)
        {
            if (id <= 0)
            {
                return id;
            }

            this.WorkspaceIdByDatabaseId.TryGetValue(id, out var workspaceId);
            return workspaceId;
        }

        internal DatabaseObject Get(long databaseId)
        {
            var databaseObject = this.databaseObjectByDatabaseId[databaseId];
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

                    this.AccessControlById[id] = new AccessControl(id, version, new HashSet<long>(permissionsIds));
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

        internal IEnumerable<DatabaseObject> Get(IComposite objectType)
        {
            var classes = new HashSet<IClass>(objectType.DatabaseClasses);
            return this.databaseObjectByDatabaseId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }

        internal Permission GetPermission(IClass @class, IOperandType operandType, Operations operation)
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
            var uri = new Uri("allors/pull", UriKind.Relative);
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
            var uri = new Uri("allors/sync", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, syncRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<SyncResponse>(response);
        }

        internal async Task<PushResponse> Push(PushRequest pushRequest)
        {
            var uri = new Uri("allors/push", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pushRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<PushResponse>(response);
        }

        internal async Task<InvokeResponse> Invoke(InvokeRequest invokeRequest)
        {
            var uri = new Uri("allors/invoke", UriKind.Relative);
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
            var uri = new Uri("allors/security", UriKind.Relative);
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

        internal DatabaseChangeSet Checkpoint()
        {
            try
            {
                return this.databaseChangeSet;
            }
            finally
            {
                this.databaseChangeSet = null;
            }
        }
    }
}
