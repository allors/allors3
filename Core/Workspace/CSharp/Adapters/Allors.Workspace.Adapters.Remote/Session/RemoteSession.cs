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
        private readonly Dictionary<long, RemoteStrategy> databaseStrategyByWorkspaceId;
        private readonly Dictionary<long, RemoteStrategy> workspaceStrategyByWorkspaceId;
        private readonly Dictionary<long, RemoteStrategy> sessionStrategyByWorkspaceId;

        private readonly IList<RemoteStrategy> existingDatabaseStrategies;
        private ISet<RemoteStrategy> newDatabaseStrategies;

        private ISet<RemoteDatabaseState> changedDatabaseStates;
        private ISet<RemoteWorkspaceState> changedWorkspaceStates;

        private ISet<IStrategy> created;
        private ISet<IStrategy> instantiated;

        internal RemoteSession(RemoteWorkspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.Database = this.Workspace.Database;
            this.SessionLifecycle = sessionLifecycle;

            this.databaseStrategyByWorkspaceId = new Dictionary<long, RemoteStrategy>();
            this.workspaceStrategyByWorkspaceId = new Dictionary<long, RemoteStrategy>();
            this.sessionStrategyByWorkspaceId = new Dictionary<long, RemoteStrategy>();
            this.existingDatabaseStrategies = new List<RemoteStrategy>();

            this.SessionState = new SessionState();
            this.SessionLifecycle.OnInit(this);
        }

        public ISessionLifecycle SessionLifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        internal RemoteWorkspace Workspace { get; }

        internal RemoteDatabase Database { get; }

        internal bool HasDatabaseChanges => this.newDatabaseStrategies?.Count > 0 || this.existingDatabaseStrategies.Any(v => v.HasDatabaseChanges);

        internal SessionState SessionState { get; }

        public async Task<ICallResult> Call(Method method, CallOptions options = null) => await this.Call(new[] { method }, options);

        public async Task<ICallResult> Call(Method[] methods, CallOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                Invocations = methods.Select(v => new Invocation
                {
                    Id = v.Object.Identity.ToString(),
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

        public T Instantiate<T>(long? identity) where T : IObject => identity.HasValue ? this.Instantiate<T>((long)identity) : default;

        public T Instantiate<T>(long identity) where T : IObject => (T)this.Get(identity)?.Object;

        public IEnumerable<T> Instantiate<T>(IEnumerable<IObject> objects) where T : IObject => objects.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<T> objects) where T : IObject => objects.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<long> identities) where T : IObject => identities.Select(this.Instantiate<T>);

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

        public void Reset()
        {
            if (this.newDatabaseStrategies != null)
            {
                foreach (var databaseStrategy in this.newDatabaseStrategies)
                {
                    databaseStrategy.Reset();
                }
            }

            foreach (var databaseStrategy in this.existingDatabaseStrategies)
            {
                databaseStrategy.Reset();
            }

            foreach (var kvp in this.workspaceStrategyByWorkspaceId)
            {
                kvp.Value.Reset();
            }
        }

        public void Merge()
        {
            if (this.newDatabaseStrategies != null)
            {
                foreach (var databaseStrategy in this.newDatabaseStrategies)
                {
                    databaseStrategy.Merge();
                }
            }

            foreach (var databaseStrategy in this.existingDatabaseStrategies)
            {
                databaseStrategy.Merge();
            }

            foreach (var kvp in this.workspaceStrategyByWorkspaceId)
            {
                kvp.Value.Merge();
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

                this.Reset();
            }

            return new RemoteSaveResult(pushResponse);
        }

        public IChangeSet Checkpoint()
        {
            var changeSet = new RemoteChangeSet(this, this.created, this.instantiated, this.SessionState.Checkpoint());

            foreach (var changed in this.changedWorkspaceStates)
            {
                changed.Checkpoint(changeSet);
            }

            foreach (var changed in this.changedDatabaseStates)
            {
                changed.Checkpoint(changeSet);
            }

            return changeSet;
        }

        internal RemoteStrategy Get(long? identity) => identity.HasValue ? this.Get(identity.Value) : null;

        internal RemoteStrategy Get(long identity)
        {
            if (identity == 0)
            {
                return default;
            }

            if (this.databaseStrategyByWorkspaceId.TryGetValue(identity, out var databaseStrategy))
            {
                return databaseStrategy;
            }

            if (this.workspaceStrategyByWorkspaceId.TryGetValue(identity, out var workspaceStrategy))
            {
                return workspaceStrategy;
            }

            if (this.sessionStrategyByWorkspaceId.TryGetValue(identity, out var sessionStrategy))
            {
                return sessionStrategy;
            }

            return identity < 0 ?
                this.InstantiateWorkspaceObject(identity) :
                this.InstantiateDatabaseObject(identity);
        }

        internal object GetRole(RemoteStrategy association, IRoleType roleType)
        {
            this.SessionState.GetRole(association, roleType, out var role);
            if (roleType.ObjectType.IsUnit)
            {
                return role;
            }

            if (roleType.IsOne)
            {
                return this.Instantiate<IObject>((long?)role);
            }

            var ids = (IEnumerable<long>)role;
            return ids?.Select(this.Instantiate<IObject>).ToArray() ?? this.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
        }

        internal void SetRole(RemoteStrategy association, IRoleType roleType, object role) => this.SessionState.SetRole(association, roleType, role);

        internal IEnumerable<IObject> GetAssociation(IObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.Database.Get(associationType.ObjectType)
                .Select(v => this.Instantiate<IObject>(v.Identity)))
            {
                if (association.Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = ((RemoteStrategy)association.Strategy).GetCompositeRole<IObject>(roleType);
                        if (role != null && role.Identity == @object.Identity)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = ((RemoteStrategy)association.Strategy).GetCompositesRole<IObject>(roleType);
                        if (roles?.Contains(@object) == true)
                        {
                            yield return association;
                        }
                    }
                }
            }
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

                    var strategy = this.databaseStrategyByWorkspaceId[workspaceId];
                    this.newDatabaseStrategies.Remove(strategy);
                    this.existingDatabaseStrategies.Add(strategy);

                    this.databaseStrategyByWorkspaceId.Remove(workspaceId);
                    this.databaseStrategyByWorkspaceId.Add(databaseId, strategy);

                    var databaseObject = this.Database.PushResponse(databaseId, strategy.Class);
                    strategy.DatabasePushResponse(databaseObject);
                }

                // TODO: Replace identities

            }

            if (this.newDatabaseStrategies?.Count > 0)
            {
                throw new Exception("Not all new objects received ids");
            }

            this.newDatabaseStrategies = null;
        }

        internal void OnChange(RemoteDatabaseState state)
        {
            this.changedDatabaseStates ??= new HashSet<RemoteDatabaseState>();
            _ = this.changedDatabaseStates.Add(state);
        }

        internal void OnChange(RemoteWorkspaceState state)
        {
            this.changedWorkspaceStates ??= new HashSet<RemoteWorkspaceState>();
            _ = this.changedWorkspaceStates.Add(state);
        }

        private IObject CreateDatabaseObject(IClass @class)
        {
            var workspaceId = this.Database.Identities.NextId();
            var strategy = new RemoteStrategy(this, @class, workspaceId);
            this.newDatabaseStrategies ??= new HashSet<RemoteStrategy>();
            this.newDatabaseStrategies.Add(strategy);
            this.databaseStrategyByWorkspaceId.Add(strategy.Identity, strategy);

            this.OnCreate(strategy);

            return strategy.Object;
        }

        private IObject CreateWorkspaceObject(IClass @class)
        {
            var workspaceId = this.Database.Identities.NextId();
            this.Workspace.RegisterWorkspaceObject(@class, workspaceId);
            var strategy = new RemoteStrategy(this, @class, workspaceId);
            this.workspaceStrategyByWorkspaceId[strategy.Identity] = strategy;

            this.OnCreate(strategy);

            return strategy.Object;
        }

        private IObject CreateSessionObject(IClass @class)
        {
            var workspaceId = this.Database.Identities.NextId();
            var strategy = new RemoteStrategy(this, @class, workspaceId);
            this.sessionStrategyByWorkspaceId[strategy.Identity] = strategy;

            this.OnCreate(strategy);

            return strategy.Object;
        }

        private RemoteStrategy InstantiateDatabaseObject(long identity)
        {
            var databaseRoles = this.Database.Get(identity);
            var strategy = new RemoteStrategy(this, databaseRoles);
            this.existingDatabaseStrategies.Add(strategy);
            this.databaseStrategyByWorkspaceId[identity] = strategy;

            this.OnInstantiate(strategy);

            return strategy;
        }

        private RemoteStrategy InstantiateWorkspaceObject(long identity)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(identity, out var @class))
            {
                return null;
            }

            var strategy = new RemoteStrategy(this, @class, identity);
            this.workspaceStrategyByWorkspaceId[identity] = strategy;

            this.OnInstantiate(strategy);

            return strategy;
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
                var identity = long.Parse(v[0]);
                if (!this.databaseStrategyByWorkspaceId.ContainsKey(identity))
                {
                    this.InstantiateDatabaseObject(identity);
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

        private void OnCreate(RemoteStrategy strategy)
        {
            this.created ??= new HashSet<IStrategy>();
            _ = this.created.Add(strategy);
        }

        private void OnInstantiate(RemoteStrategy strategy)
        {
            this.instantiated ??= new HashSet<IStrategy>();
            _ = this.instantiated.Add(strategy);
        }
    }
}
