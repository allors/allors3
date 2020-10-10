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
    using System.Threading.Tasks;
    using Allors.Workspace.Meta;
    using Newtonsoft.Json;
    using Polly;
    using Protocol.Database.Invoke;
    using Protocol.Database.Pull;
    using Protocol.Database.Push;
    using Protocol.Database.Security;
    using Protocol.Database.Sync;

    public class Database
    {
        private readonly Dictionary<long, DatabaseObject> databaseObjectById;

        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> executePermissionByOperandTypeByClass;

        public Database(ObjectFactory objectFactory, HttpClient httpClient)
        {
            this.ObjectFactory = objectFactory;

            this.HttpClient = httpClient;

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.AccessControlById = new Dictionary<long, AccessControl>();
            this.PermissionById = new Dictionary<long, Permission>();

            this.databaseObjectById = new Dictionary<long, DatabaseObject>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
        }

        ~Database() => this.HttpClient.Dispose();

        public ObjectFactory ObjectFactory { get; }

        public HttpClient HttpClient { get; }

        public string UserId { get; private set; }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        internal Dictionary<long, Permission> PermissionById { get; }

        public SyncRequest Diff(PullResponse response)
        {
            var ctx = new ResponseContext(this.AccessControlById, this.PermissionById);

            var syncRequest = new SyncRequest
            {
                Objects = response.Objects
                    .Where(v =>
                    {
                        var id = long.Parse(v[0]);
                        this.databaseObjectById.TryGetValue(id, out var workspaceObject);
                        var sortedAccessControlIds = v.Length > 2 ? ctx.ReadSortedAccessControlIds(v[2]) : null;
                        var sortedDeniedPermissionIds = v.Length > 3 ? ctx.ReadSortedDeniedPermissionIds(v[3]) : null;

                        if (workspaceObject == null)
                        {
                            return true;
                        }

                        var version = long.Parse(v[1]);
                        if (!workspaceObject.Version.Equals(version))
                        {
                            return true;
                        }

                        if (v.Length == 2)
                        {
                            return false;
                        }

                        if (v.Length == 3)
                        {
                            if (workspaceObject.SortedDeniedPermissionIds != null)
                            {
                                return true;
                            }

                            return !Equals(workspaceObject.SortedAccessControlIds, sortedAccessControlIds);
                        }

                        return !Equals(workspaceObject.SortedAccessControlIds, sortedAccessControlIds) ||
                               !Equals(workspaceObject.SortedDeniedPermissionIds, sortedDeniedPermissionIds);
                    })
                    .Select(v => v[0]).ToArray(),
            };

            return syncRequest;
        }

        public DatabaseObject Get(long id)
        {
            var workspaceObject = this.databaseObjectById[id];
            if (workspaceObject == null)
            {
                throw new Exception($"Object with id {id} is not present.");
            }

            return workspaceObject;
        }

        public SecurityRequest Sync(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this.AccessControlById, this.PermissionById);
            foreach (var syncResponseObject in syncResponse.Objects)
            {
                var workspaceObject = new DatabaseObject(this, ctx, syncResponseObject);
                this.databaseObjectById[workspaceObject.Id] = workspaceObject;
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

        public SecurityRequest Security(SecurityResponse securityResponse)
        {
            if (securityResponse.Permissions != null)
            {
                foreach (var syncResponsePermission in securityResponse.Permissions)
                {
                    var id = long.Parse(syncResponsePermission[0]);
                    var @class = (IClass)this.ObjectFactory.MetaPopulation.Find(Guid.Parse(syncResponsePermission[1]));
                    var metaObject = this.ObjectFactory.MetaPopulation.Find(Guid.Parse(syncResponsePermission[2]));
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
                    var id = long.Parse(syncResponseAccessControl.I);
                    var version = long.Parse(syncResponseAccessControl.V);
                    var permissionsIds = syncResponseAccessControl.P
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
                        }) ?? Array.Empty<long>();

                    var accessControl = new AccessControl(id, version, new HashSet<long>(permissionsIds));
                    this.AccessControlById[id] = accessControl;
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
            return this.databaseObjectById.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }

        public Permission GetPermission(IClass @class, IOperandType roleType, Operations operation)
        {
            switch (operation)
            {
                case Operations.Read:
                    if (this.readPermissionByOperandTypeByClass.TryGetValue(@class, out var readPermissionByOperandType))
                    {
                        if (readPermissionByOperandType.TryGetValue(roleType, out var readPermission))
                        {
                            return readPermission;
                        }
                    }

                    return null;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class, out var writePermissionByOperandType))
                    {
                        if (writePermissionByOperandType.TryGetValue(roleType, out var writePermission))
                        {
                            return writePermission;
                        }
                    }

                    return null;

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class, out var executePermissionByOperandType))
                    {
                        if (executePermissionByOperandType.TryGetValue(roleType, out var executePermission))
                        {
                            return executePermission;
                        }
                    }

                    return null;
            }
        }

        public IAsyncPolicy Policy { get; set; } = Polly.Policy
           .Handle<HttpRequestException>()
           .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public async Task<PullResponse> Pull(PullRequest pullRequest)
        {
            var uri = new Uri("allors/pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullRequest);
            response.EnsureSuccessStatusCode();
            var pullResponse = await this.ReadAsAsync<PullResponse>(response);
            return pullResponse;
        }

        public async Task<PullResponse> Pull(string name, object pullRequest)
        {
            var uri = new Uri(name + "/pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullRequest);
            response.EnsureSuccessStatusCode();
            var pullResponse = await this.ReadAsAsync<PullResponse>(response);
            return pullResponse;
        }

        public async Task<SyncResponse> Sync(SyncRequest syncRequest)
        {
            var uri = new Uri("allors/sync", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, syncRequest);
            response.EnsureSuccessStatusCode();

            var syncResponse = await this.ReadAsAsync<SyncResponse>(response);
            return syncResponse;
        }

        public async Task<PushResponse> Push(PushRequest pushRequest)
        {
            var uri = new Uri("allors/push", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pushRequest);
            response.EnsureSuccessStatusCode();

            var pushResponse = await this.ReadAsAsync<PushResponse>(response);
            return pushResponse;
        }

        public async Task<InvokeResponse> Invoke(InvokeRequest invokeRequest, InvokeOptions options = null)
        {
            var uri = new Uri("allors/invoke", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, invokeRequest);
            response.EnsureSuccessStatusCode();

            var invokeResponse = await this.ReadAsAsync<InvokeResponse>(response);
            return invokeResponse;
        }

        public async Task<InvokeResponse> Invoke(string service, object args)
        {
            var uri = new Uri(service + "/Pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, args);
            response.EnsureSuccessStatusCode();

            var invokeResponse = await this.ReadAsAsync<InvokeResponse>(response);
            return invokeResponse;
        }

        public async Task<SecurityResponse> Security(SecurityRequest securityRequest)
        {
            var uri = new Uri("allors/security", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, securityRequest);
            response.EnsureSuccessStatusCode();

            var syncResponse = await this.ReadAsAsync<SecurityResponse>(response);
            return syncResponse;
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync(Uri uri, object args) =>
            await this.Policy.ExecuteAsync(
                async () =>
                {
                    var json = JsonConvert.SerializeObject(args);
                    return await this.HttpClient.PostAsync(
                        uri,
                        new StringContent(json, Encoding.UTF8, "application/json"));
                });

        public async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var deserializedObject = JsonConvert.DeserializeObject<T>(json);
            return deserializedObject;
        }

        public async Task<bool> Login(Uri url, string username, string password)
        {
            var request = new { UserName = username, Password = password };
            using (var response = await this.PostAsJsonAsync(url, request))
            {
                response.EnsureSuccessStatusCode();
                var authResult = await this.ReadAsAsync<AuthenticationResult>(response);
                if (!authResult.Authenticated)
                {
                    return false;
                }

                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
                this.UserId = authResult.UserId;

                return true;
            }
        }

        public class AuthenticationResult
        {
            public bool Authenticated { get; set; }

            public string Token { get; set; }

            public string UserId { get; set; }
        }

        internal DatabaseObject New(long objectId, IClass @class)
        {
            var databaseObject = new DatabaseObject(this, objectId, @class);
            this.databaseObjectById[objectId] = databaseObject;
            return databaseObject;
        }
    }
}
