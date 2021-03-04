// <copyright file="RemoteSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Data;
    using Meta;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Sync;
    using Protocol.Json;

    public class RemoteSession : ISession
    {
        private readonly Dictionary<Identity, RemoteStrategy> databaseStrategyByWorkspaceId;
        private readonly Dictionary<Identity, RemoteStrategy> workspaceStrategyByWorkspaceId;
        private readonly Dictionary<Identity, RemoteStrategy> sessionStrategyByWorkspaceId;

        private readonly IList<RemoteStrategy> existingDatabaseStrategies;
        private ISet<RemoteStrategy> newDatabaseStrategies;

        private RemoteChangeSet changeSet;

        internal RemoteSession(RemoteWorkspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.Database = this.Workspace.Database;
            this.SessionLifecycle = sessionLifecycle;

            this.databaseStrategyByWorkspaceId = new Dictionary<Identity, RemoteStrategy>();
            this.workspaceStrategyByWorkspaceId = new Dictionary<Identity, RemoteStrategy>();
            this.sessionStrategyByWorkspaceId = new Dictionary<Identity, RemoteStrategy>();
            this.existingDatabaseStrategies = new List<RemoteStrategy>();

            this.State = new State();
            this.changeSet = new RemoteChangeSet(this);
            this.SessionLifecycle.OnInit(this);
        }

        public ISessionLifecycle SessionLifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        internal RemoteWorkspace Workspace { get; }

        internal RemoteDatabase Database { get; }

        internal bool HasDatabaseChanges => this.newDatabaseStrategies?.Count > 0 || this.existingDatabaseStrategies.Any(v => v.HasDatabaseChanges);

        internal State State { get; }

        public async Task<ICallResult> Call(Method method, CallOptions options = null) => await this.Call(new[] { method }, options);

        public async Task<ICallResult> Call(Method[] methods, CallOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                Invocations = methods.Select(v => new Invocation
                {
                    Id = v.Object.Identity?.ToString(),
                    Version = ((RemoteStrategy)v.Object.Strategy).DatabaseVersion.ToString(),
                    Method = v.MethodType.IdAsString,
                }).ToArray(),
                InvokeOptions = options != null ? new InvokeOptions
                {
                    ContinueOnError = options.ContinueOnError,
                    Isolated = options.Isolated
                } : null,
            };

            var invokeResponse = await this.Database.Invoke(invokeRequest);
            return new RemoteCallResult(invokeResponse);
        }

        public async Task<ICallResult> Call(string service, object args)
        {
            var invokeResponse = await this.Database.Invoke(service, args);
            return new RemoteCallResult(invokeResponse);
        }

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public T Create<T>(IClass @class) where T : IObject => @class.Origin switch
        {
            Origin.Database => (T)this.CreateDatabaseObject(@class),
            Origin.Workspace => (T)this.CreateWorkspaceObject(@class),
            Origin.Session => (T)this.CreateSessionObject(@class),
            _ => throw new Exception($"Unsupported origin: {@class.Origin}"),
        };

        public T Instantiate<T>(IObject @object) where T : IObject => this.Instantiate<T>(@object.Identity);

        public T Instantiate<T>(T @object) where T : IObject => this.Instantiate<T>(@object.Identity);

        public T Instantiate<T>(long id) where T : IObject => this.Instantiate<T>(this.Database.Identities.Get(id));

        public T Instantiate<T>(Identity identity) where T : IObject
        {
            if (identity == null)
            {
                return default;
            }

            if (this.databaseStrategyByWorkspaceId.TryGetValue(identity, out var databaseStrategy))
            {
                return (T)databaseStrategy.Object;
            }

            if (this.workspaceStrategyByWorkspaceId.TryGetValue(identity, out var workspaceStrategy))
            {
                return (T)workspaceStrategy.Object;
            }

            if (this.sessionStrategyByWorkspaceId.TryGetValue(identity, out var sessionStrategy))
            {
                return (T)sessionStrategy.Object;
            }

            if (identity is DatabaseIdentity)
            {
                var databaseRoles = this.Database.Get(identity);
                databaseStrategy = new RemoteStrategy(this, databaseRoles);
                this.existingDatabaseStrategies.Add(databaseStrategy);
                this.databaseStrategyByWorkspaceId[identity] = databaseStrategy;
                return (T)databaseStrategy.Object;
            }

            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(identity, out var @class))
            {
                return default;
            }

            if (identity is WorkspaceIdentity)
            {
                workspaceStrategy = new RemoteStrategy(this, @class, identity);
                this.workspaceStrategyByWorkspaceId[identity] = workspaceStrategy;
                return (T)workspaceStrategy.Object;
            }

            sessionStrategy = new RemoteStrategy(this, @class, identity);
            this.sessionStrategyByWorkspaceId[identity] = sessionStrategy;
            return (T)sessionStrategy.Object;
        }

        public IEnumerable<T> Instantiate<T>(IEnumerable<IObject> objects) where T : IObject => objects.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<T> objects) where T : IObject => objects.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<long> ids) where T : IObject => ids.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<Identity> identities) where T : IObject => identities.Select(this.Instantiate<T>);

        public async Task<ILoadResult> Load(params Pull[] pulls)
        {
            var pullRequest = new PullRequest { Pulls = pulls.Select(v => v.ToJson()).ToArray() };
            var pullResponse = await this.Database.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public async Task<ILoadResult> Load(string service, object args)
        {
            if (args is Pull pull)
            {
                args = new PullRequest { Pulls = new[] { pull.ToJson() } };
            }

            if (args is IEnumerable<Pull> pulls)
            {
                args = new PullRequest { Pulls = pulls.Select(v => v.ToJson()).ToArray() };
            }

            var pullResponse = await this.Database.Pull(service, args);
            return await this.OnPull(pullResponse);
        }

        public void Refresh(bool merge = false)
        {
            if (this.newDatabaseStrategies != null)
            {
                foreach (var databaseStrategy in this.newDatabaseStrategies)
                {
                    databaseStrategy.Refresh(merge);
                }
            }

            foreach (var databaseStrategy in this.existingDatabaseStrategies)
            {
                databaseStrategy.Refresh(merge);
            }
        }

        public async Task<ISaveResult> Save()
        {
            var saveRequest = this.PushRequest();
            var pushResponse = await this.Database.Push(saveRequest);
            if (!pushResponse.HasErrors)
            {
                this.PushResponse(pushResponse);

                var objects = saveRequest.Objects.Select(v => v.DatabaseId).ToArray();
                if (pushResponse.NewObjects != null)
                {
                    objects = objects.Union(pushResponse.NewObjects.Select(v => v.DatabaseId)).ToArray();
                }

                var syncRequests = new SyncRequest
                {
                    Objects = objects,
                };

                await this.Sync(syncRequests);

                foreach (var workspaceStrategy in this.workspaceStrategyByWorkspaceId.Values)
                {
                    workspaceStrategy.WorkspaceSave();
                }

                this.Refresh();
            }

            return new RemoteSaveResult(pushResponse);
        }

        public IChangeSet Checkpoint()
        {
            try
            {
                return this.changeSet;
            }
            finally
            {
                this.changeSet = new RemoteChangeSet(this);
            }
        }

        internal object GetRole(Identity association, IRoleType roleType)
        {
            this.State.GetRole(association, roleType, out var role);
            if (roleType.ObjectType.IsUnit)
            {
                return role;
            }

            if (roleType.IsOne)
            {
                return this.Instantiate<IObject>((Identity)role);
            }

            var ids = (IEnumerable<Identity>)role;
            return ids?.Select(this.Instantiate<IObject>).ToArray() ?? this.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
        }

        internal void SetRole(Identity association, IRoleType roleType, object role) => this.State.SetRole(association, roleType, role);

        internal IEnumerable<IObject> GetAssociation(IObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.Database.Get(associationType.ObjectType)
                .Select(v => this.Instantiate<IObject>(v.Identity)))
            {
                if (((IObject)association).Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = (IObject)((RemoteStrategy)association.Strategy).Get(roleType);
                        if (role != null && role.Identity == @object.Identity)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = (IObject[])((RemoteStrategy)association.Strategy).Get(roleType);
                        if (roles != null && roles.Contains(@object))
                        {
                            yield return association;
                        }
                    }
                }
            }
        }

        internal IObject GetForAssociation(Identity id)
        {
            if (id == null)
            {
                return null;
            }

            // TODO:
            //this.strategyByWorkspaceId.TryGetValue(id, out var strategy);
            //return strategy?.Object;

            return default;
        }

        internal PushRequest PushRequest() => new PushRequest
        {
            NewObjects = this.newDatabaseStrategies?.Select(v => v.DatabaseSaveNew()).ToArray(),
            Objects = this.existingDatabaseStrategies.Where(v => v.HasDatabaseChanges).Select(v => v.DatabaseSaveExisting()).ToArray(),
        };

        internal void PushResponse(PushResponse pushResponse)
        {
            if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
            {
                foreach (var pushResponseNewObject in pushResponse.NewObjects)
                {
                    var workspaceId = long.Parse(pushResponseNewObject.WorkspaceId);
                    var databaseId = long.Parse(pushResponseNewObject.DatabaseId);

                    var identity = this.Database.Identities.GetAndUpdate(workspaceId, databaseId);

                    var strategy = this.databaseStrategyByWorkspaceId[identity];
                    this.newDatabaseStrategies.Remove(strategy);
                    this.existingDatabaseStrategies.Add(strategy);

                    var databaseObject = this.Database.PushResponse(identity, strategy.Class);
                    strategy.DatabasePushResponse(databaseObject);
                }
            }

            if (this.newDatabaseStrategies?.Count > 0)
            {
                throw new Exception("Not all new objects received ids");
            }

            this.newDatabaseStrategies = null;
        }

        private IObject CreateDatabaseObject(IClass @class)
        {
            var workspaceId = this.Database.Identities.NextDatabaseIdentity();
            var strategy = new RemoteStrategy(this, @class, workspaceId);
            this.newDatabaseStrategies ??= new HashSet<RemoteStrategy>();
            this.newDatabaseStrategies.Add(strategy);
            this.databaseStrategyByWorkspaceId.Add(strategy.Identity, strategy);
            return strategy.Object;
        }

        private IObject CreateWorkspaceObject(IClass @class)
        {
            var workspaceId = this.Database.Identities.NextWorkspaceIdentity();
            this.Workspace.RegisterWorkspaceObject(@class, workspaceId);
            var strategy = new RemoteStrategy(this, @class, workspaceId);
            this.workspaceStrategyByWorkspaceId[strategy.Identity] = strategy;
            return strategy.Object;
        }

        private IObject CreateSessionObject(IClass @class)
        {
            var workspaceId = this.Database.Identities.NextWorkspaceIdentity();
            var strategy = new RemoteStrategy(this, @class, workspaceId);
            this.sessionStrategyByWorkspaceId[strategy.Identity] = strategy;
            return strategy.Object;
        }

        private async Task<ILoadResult> OnPull(PullResponse pullResponse)
        {
            var syncRequest = this.Database.Diff(pullResponse);
            if (syncRequest.Objects.Length > 0)
            {
                await this.Sync(syncRequest);
            }

            foreach (var v in pullResponse.Objects)
            {
                var id = long.Parse(v[0]);
                var identity = this.Database.Identities.GetOrCreate(id);
                if (!this.databaseStrategyByWorkspaceId.ContainsKey(identity))
                {
                    //var strategy = this.Instantiate(identity);
                    //this.changeSet.OnInstantiated(strategy);
                }
            }

            return new RemoteLoadResult(this, pullResponse);
        }

        private async Task Sync(SyncRequest syncRequest)
        {
            var syncResponse = await this.Database.Sync(syncRequest);
            var securityRequest = this.Database.SyncResponse(syncResponse);

            if (securityRequest != null)
            {
                var securityResponse = await this.Database.Security(securityRequest);
                securityRequest = this.Database.SecurityResponse(securityResponse);

                if (securityRequest != null)
                {
                    securityResponse = await this.Database.Security(securityRequest);
                    this.Database.SecurityResponse(securityResponse);
                }
            }
        }
    }
}
