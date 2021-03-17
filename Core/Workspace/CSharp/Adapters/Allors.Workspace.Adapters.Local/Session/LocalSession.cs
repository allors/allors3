// <copyright file="LocalSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Meta;

    public class LocalSession : ISession
    {
        private readonly Dictionary<long, LocalStrategy> strategyByWorkspaceId;
        private readonly Dictionary<IClass, LocalStrategy[]> strategiesByClass;

        private readonly IList<LocalStrategy> existingDatabaseStrategies;
        private ISet<LocalStrategy> newDatabaseStrategies;
        private ISet<LocalStrategy> workspaceStrategies;

        private ISet<LocalDatabaseState> changedDatabaseStates;
        private ISet<LocalWorkspaceState> changedWorkspaceStates;

        private ISet<IStrategy> created;
        private ISet<IStrategy> instantiated;

        internal LocalSession(LocalWorkspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.Database = this.Workspace.LocalDatabase;
            this.SessionLifecycle = sessionLifecycle;

            this.strategyByWorkspaceId = new Dictionary<long, LocalStrategy>();
            this.strategiesByClass = new Dictionary<IClass, LocalStrategy[]>();
            this.existingDatabaseStrategies = new List<LocalStrategy>();

            this.SessionState = new LocalSessionState();
            this.SessionLifecycle.OnInit(this);
        }

        public ISessionLifecycle SessionLifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        internal LocalWorkspace Workspace { get; }

        internal LocalDatabase Database { get; }

        internal LocalSessionState SessionState { get; }

        public Task<ICallResult> Call(Method method, CallOptions options = null) => this.Call(new[] { method }, options);

        public Task<ICallResult> Call(Method[] methods, CallOptions options = null)
        {
            var localInvokeResult = new LocalInvokeResult(this.Workspace);
            localInvokeResult.Execute(methods, options);
            return Task.FromResult<ICallResult>(new LocalCallResult(localInvokeResult));
        }

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public T Create<T>(IClass @class) where T : IObject
        {
            var workspaceId = this.Database.Identities.NextId();
            var strategy = new LocalStrategy(this, @class, workspaceId);
            this.AddStrategy(strategy);

            if (@class.Origin == Origin.Database)
            {
                this.newDatabaseStrategies ??= new HashSet<LocalStrategy>();
                this.newDatabaseStrategies.Add(strategy);
            }

            if (@class.Origin == Origin.Workspace)
            {
                this.workspaceStrategies ??= new HashSet<LocalStrategy>();
                this.workspaceStrategies.Add(strategy);
                // TODO: move to Push
                this.Workspace.RegisterWorkspaceObject(@class, workspaceId);
            }

            this.created ??= new HashSet<IStrategy>();
            _ = this.created.Add(strategy);

            return (T)strategy.Object;
        }

        public T Get<T>(IObject @object) where T : IObject => this.Get<T>(@object.Identity);

        public T Get<T>(T @object) where T : IObject => this.Get<T>(@object.Identity);

        public T Get<T>(long? identity) where T : IObject => identity.HasValue ? this.Get<T>((long)identity) : default;

        public T Get<T>(long identity) where T : IObject => (T)this.GetStrategy(identity)?.Object;

        public IEnumerable<T> Get<T>(IEnumerable<IObject> objects) where T : IObject => objects.Select(this.Get<T>);

        public IEnumerable<T> Get<T>(IEnumerable<T> objects) where T : IObject => objects.Select(this.Get);

        public IEnumerable<T> Get<T>(IEnumerable<long> identities) where T : IObject => identities.Select(this.Get<T>);

        public IEnumerable<T> GetAll<T>() where T : class, IObject
        {
            var objectType = (IComposite)this.Workspace.ObjectFactory.GetObjectType<T>();
            return this.GetAll<T>(objectType);
        }

        public IEnumerable<T> GetAll<T>(IComposite objectType) where T : IObject
        {
            foreach (var @class in objectType.DatabaseClasses)
            {
                switch (@class.Origin)
                {
                    case Origin.Workspace:
                        if (this.Workspace.WorkspaceIdsByWorkspaceClass.TryGetValue(@class, out var ids))
                        {
                            foreach (var id in ids)
                            {
                                if (this.strategyByWorkspaceId.TryGetValue(id, out var strategy))
                                {
                                    yield return (T)strategy.Object;
                                }
                                else
                                {
                                    strategy = this.InstantiateWorkspaceObject(id);
                                    yield return (T)strategy.Object;
                                }
                            }
                        }

                        break;
                    default:
                        if (this.strategiesByClass.TryGetValue(@class, out var strategies))
                        {
                            foreach (var strategy in strategies)
                            {
                                yield return (T)strategy.Object;
                            }
                        }

                        break;
                }
            }
        }

        public Task<ILoadResult> Load(params Data.Pull[] pulls)
        {
            var pullResult = new LocalPullResult(this.Workspace);
            pullResult.Execute(pulls);

            return this.OnPull(pullResult);
        }

        public Task<ILoadResult> Load(string service, object args) => throw new System.NotImplementedException();

        public void Reset()
        {
            foreach (var kvp in this.strategyByWorkspaceId)
            {
                kvp.Value.Reset();
            }
        }

        public void Merge()
        {
            foreach (var kvp in this.strategyByWorkspaceId)
            {
                kvp.Value.Merge();
            }
        }

        public Task<ISaveResult> Save()
        {
            var newStrategies = this.newDatabaseStrategies?.ToArray() ?? Array.Empty<LocalStrategy>();
            var changedStrategies = this.existingDatabaseStrategies?.Where(v => v.HasDatabaseChanges).ToArray() ?? Array.Empty<LocalStrategy>();

            var localPushResult = new LocalPushResult(this.Workspace);
            localPushResult.Execute(newStrategies, changedStrategies);

            if (!localPushResult.HasErrors)
            {
                this.PushResponse(localPushResult);

                var objects = localPushResult.Objects;

                this.Workspace.LocalDatabase.Sync(objects, localPushResult.AccessControlLists);

                foreach (var databaseObject in objects)
                {
                    var identity = databaseObject.Id;
                    if (!this.strategyByWorkspaceId.ContainsKey(identity))
                    {
                        this.InstantiateDatabaseObject(identity);
                    }
                }

                if (this.workspaceStrategies != null)
                {
                    foreach (var workspaceStrategy in this.workspaceStrategies)
                    {
                        workspaceStrategy.WorkspaceSave();
                    }
                }

                this.Reset();
            }

            return Task.FromResult<ISaveResult>(new LocalSaveResult(localPushResult));
        }

        public IChangeSet Checkpoint()
        {
            var changeSet = new LocalChangeSet(this, this.created, this.instantiated, this.SessionState.Checkpoint());

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

        internal LocalStrategy GetStrategy(long? identity) => identity.HasValue ? this.GetStrategy(identity.Value) : null;

        internal LocalStrategy GetStrategy(long identity)
        {
            if (identity == 0)
            {
                return default;
            }


            if (this.strategyByWorkspaceId.TryGetValue(identity, out var sessionStrategy))
            {
                return sessionStrategy;
            }

            return identity < 0 ? this.InstantiateWorkspaceObject(identity) : null;
        }

        internal object GetRole(LocalStrategy association, IRoleType roleType)
        {
            this.SessionState.GetRole(association, roleType, out var role);
            if (roleType.ObjectType.IsUnit)
            {
                return role;
            }

            if (roleType.IsOne)
            {
                return this.Get<IObject>((long?)role);
            }

            var ids = (IEnumerable<long>)role;
            return ids?.Select(this.Get<IObject>).ToArray() ?? this.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
        }

        internal IEnumerable<IObject> GetAssociation(LocalStrategy role, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.Get(associationType.ObjectType))
            {
                if (!association.CanRead(roleType))
                {
                    continue;
                }

                if (association.IsAssociationForRole(roleType, role))
                {
                    yield return association.Object;
                }
            }
        }

        internal void OnChange(LocalDatabaseState state)
        {
            this.changedDatabaseStates ??= new HashSet<LocalDatabaseState>();
            _ = this.changedDatabaseStates.Add(state);
        }

        internal void OnChange(LocalWorkspaceState state)
        {
            this.changedWorkspaceStates ??= new HashSet<LocalWorkspaceState>();
            _ = this.changedWorkspaceStates.Add(state);
        }

        internal LocalStrategy InstantiateDatabaseObject(long identity)
        {
            var databaseObject = this.Database.Get(identity);
            var strategy = new LocalStrategy(this, databaseObject);
            this.existingDatabaseStrategies.Add(strategy);
            this.AddStrategy(strategy);
            this.OnInstantiate(strategy);

            return strategy;
        }

        private LocalStrategy InstantiateWorkspaceObject(long identity)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(identity, out var @class))
            {
                return null;
            }

            var strategy = new LocalStrategy(this, @class, identity);
            this.workspaceStrategies ??= new HashSet<LocalStrategy>();
            this.workspaceStrategies.Add(strategy);
            this.AddStrategy(strategy);
            this.OnInstantiate(strategy);

            return strategy;
        }

        private void OnInstantiate(LocalStrategy strategy)
        {
            this.instantiated ??= new HashSet<IStrategy>();
            _ = this.instantiated.Add(strategy);
        }

        internal IEnumerable<LocalStrategy> Get(IComposite objectType)
        {
            var classes = new HashSet<IClass>(objectType.DatabaseClasses);
            return this.strategyByWorkspaceId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }

        private void AddStrategy(LocalStrategy strategy)
        {
            this.strategyByWorkspaceId.Add(strategy.Identity, strategy);

            var @class = strategy.Class;
            if (!this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                strategies = new[] { strategy };
            }
            else
            {
                strategies = NullableSortableArraySet.Add(strategies, strategy);
            }

            this.strategiesByClass[@class] = strategies;
        }

        private void RemoveStrategy(LocalStrategy strategy)
        {
            this.strategyByWorkspaceId.Remove(strategy.Identity);

            var @class = strategy.Class;
            if (this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                strategies = NullableSortableArraySet.Remove(strategies, strategy);
                this.strategiesByClass[@class] = strategies;
            }
        }

        private Task<ILoadResult> OnPull(LocalPullResult pullResult)
        {
            var syncObjects = this.Database.ObjectsToSync(pullResult);

            this.Workspace.LocalDatabase.Sync(syncObjects, pullResult.AccessControlLists);

            foreach (var databaseObject in pullResult.Objects)
            {
                var identity = databaseObject.Id;
                if (!this.strategyByWorkspaceId.ContainsKey(identity))
                {
                    this.InstantiateDatabaseObject(identity);
                }
            }

            var loadResult = new LocalLoadResult(this, pullResult);
            return Task.FromResult<ILoadResult>(loadResult);
        }

        internal void PushResponse(LocalPushResult pushResponse)
        {
            if (pushResponse.ObjectByNewId?.Count > 0)
            {
                foreach (var kvp in pushResponse.ObjectByNewId)
                {
                    var workspaceId = kvp.Key;
                    var databaseId = kvp.Value.Id;

                    var strategy = this.strategyByWorkspaceId[workspaceId];

                    this.newDatabaseStrategies.Remove(strategy);
                    this.RemoveStrategy(strategy);

                    // TODO: Is this necessary?
                    var databaseObject = this.Database.PushResponse(databaseId, strategy.Class);
                    strategy.DatabasePushResponse(databaseObject);

                    this.existingDatabaseStrategies.Add(strategy);
                    this.AddStrategy(strategy);
                }
            }

            if (this.newDatabaseStrategies?.Count > 0)
            {
                throw new Exception("Not all new objects received ids");
            }

            this.newDatabaseStrategies = null;
        }
    }
}
