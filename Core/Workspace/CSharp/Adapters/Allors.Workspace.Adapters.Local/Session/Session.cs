// <copyright file="LocalSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading.Tasks;
    using Data;
    using Meta;
    using Numbers;

    public class Session : ISession
    {
        private readonly Dictionary<long, Strategy> strategyByWorkspaceId;
        private readonly Dictionary<IClass, ISet<Strategy>> strategiesByClass;

        private ISet<Strategy> databaseStrategies;
        private ISet<Strategy> workspaceStrategies;

        private ISet<Strategy> newDatabaseStrategies;
        private ISet<DatabaseOriginState> changedDatabaseStates;
        private ISet<WorkspaceOriginState> changedWorkspaceStates;

        private ChangeSet changeSet;

        internal Session(Workspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.DatabaseAdapter = this.Workspace.DatabaseAdapter;
            this.Lifecycle = sessionLifecycle;
            this.Numbers = this.Workspace.Numbers;

            this.strategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();

            this.SessionOriginState = new SessionOriginState(workspace.Numbers);

            this.changeSet = new ChangeSet(this);

            this.Lifecycle.OnInit(this);
        }

        public ISessionLifecycle Lifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        internal Workspace Workspace { get; }

        internal INumbers Numbers { get; }

        internal DatabaseAdapter DatabaseAdapter { get; }

        internal SessionOriginState SessionOriginState { get; }

        public Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) => this.Invoke(new[] { method }, options);

        public Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var localInvokeResult = new InvokeResult(this, this.Workspace);
            localInvokeResult.Execute(methods, options);
            return Task.FromResult<IInvokeResult>(localInvokeResult);
        }

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public T Create<T>(IClass @class) where T : IObject
        {
            var workspaceId = this.DatabaseAdapter.Identities.NextId();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);

            switch (@class.Origin)
            {
                case Origin.Database:
                    _ = (this.newDatabaseStrategies ??= new HashSet<Strategy>()).Add(strategy);
                    break;
                case Origin.Workspace:
                    _ = (this.workspaceStrategies ??= new HashSet<Strategy>()).Add(strategy);
                    // TODO: move to Push
                    this.Workspace.RegisterRecord(@class, workspaceId);
                    break;
            }

            _ = this.changeSet.Created.Add(strategy);

            return (T)strategy.Object;
        }

        public T Get<T>(IObject @object) where T : IObject => this.Get<T>(@object.Id);

        public T Get<T>(T @object) where T : IObject => this.Get<T>(@object.Id);

        public T Get<T>(long? id) where T : IObject => id.HasValue ? this.Get<T>((long)id) : default;

        public T Get<T>(long id) where T : IObject => (T)this.GetStrategy(id)?.Object;

        public T Get<T>(string idAsString) where T : IObject
        {
            if (long.TryParse(idAsString, out var id))
            {
                return (T)this.GetStrategy(id)?.Object;
            }

            return default;
        }

        public IEnumerable<T> Get<T>(IEnumerable<IObject> objects) where T : IObject => objects.Select(this.Get<T>);

        public IEnumerable<T> Get<T>(IEnumerable<T> objects) where T : IObject => objects.Select(this.Get);

        public IEnumerable<T> Get<T>(IEnumerable<long> identities) where T : IObject => identities.Select(this.Get<T>);

        public IEnumerable<T> Get<T>(IEnumerable<string> identities) where T : IObject => this.Get<T>(identities.Select(v =>
        {
            _ = long.TryParse(v, out var id);
            return id;
        }));

        public IEnumerable<T> GetAll<T>() where T : IObject
        {
            var objectType = (IComposite)this.Workspace.ObjectFactory.GetObjectType<T>();
            return this.GetAll<T>(objectType);
        }

        public IEnumerable<T> GetAll<T>(IComposite objectType) where T : IObject
        {
            foreach (var @class in objectType.Classes)
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
                                    strategy = this.InstantiateWorkspaceStrategy(id);
                                    yield return (T)strategy.Object;
                                }
                            }
                        }

                        break;
                    default:
                        if (this.strategiesByClass.TryGetValue(@class, out var workspaceIds))
                        {
                            foreach (var workspaceId in this.Numbers.Enumerate(workspaceIds))
                            {
                                if (this.strategyByWorkspaceId.TryGetValue(workspaceId, out var strategy))
                                {
                                    yield return (T)strategy.Object;
                                }
                            }
                        }

                        break;
                }
            }
        }

        public Task<IPullResult> Pull(params Pull[] pulls)
        {
            var pullResult = new PullResult(this, this.Workspace);

            pullResult.Execute(pulls);

            return this.OnPull(pullResult);
        }

        public Task<IPullResult> Pull(Data.Procedure procedure, params Pull[] pulls)
        {
            var pullResult = new PullResult(this, this.Workspace);

            pullResult.Execute(procedure);
            pullResult.Execute(pulls);

            return this.OnPull(pullResult);
        }

        public void Reset()
        {
            foreach (var kvp in this.strategyByWorkspaceId)
            {
                kvp.Value.Reset();
            }
        }

        public Task<IPushResult> Push()
        {
            var newStrategies = this.newDatabaseStrategies?.ToArray() ?? Array.Empty<Strategy>();
            var changedStrategies = this.databaseStrategies?.Where(v => v.HasDatabaseChanges).ToArray() ?? Array.Empty<Strategy>();

            var localPushResult = new PushResult(this, this.Workspace);
            localPushResult.Execute(newStrategies, changedStrategies);

            if (!localPushResult.HasErrors)
            {
                this.PushResponse(localPushResult);

                var objects = localPushResult.Objects;

                this.Workspace.DatabaseAdapter.Sync(objects, localPushResult.AccessControlLists);

                foreach (var databaseObject in objects)
                {
                    var id = databaseObject.Id;
                    if (!this.strategyByWorkspaceId.ContainsKey(id))
                    {
                        _ = this.InstantiateDatabaseStrategy(id);
                    }
                }

                if (this.workspaceStrategies != null)
                {
                    foreach (var workspaceStrategy in this.workspaceStrategies)
                    {
                        workspaceStrategy.WorkspacePush();
                    }
                }

                this.Reset();
            }

            return Task.FromResult<IPushResult>(localPushResult);
        }

        public IChangeSet Checkpoint()
        {
            try
            {
                this.SessionOriginState.Checkpoint(this.changeSet);

                if (this.changedWorkspaceStates != null)
                {
                    foreach (var changed in this.changedWorkspaceStates)
                    {
                        changed.Checkpoint(this.changeSet);
                    }
                }

                if (this.changedDatabaseStates != null)
                {
                    foreach (var changed in this.changedDatabaseStates)
                    {
                        changed.Checkpoint(this.changeSet);
                    }
                }

                return this.changeSet;
            }
            finally
            {
                this.changeSet = new ChangeSet(this);
            }
        }

        internal Strategy GetStrategy(long id)
        {
            if (id == 0)
            {
                return default;
            }

            if (this.strategyByWorkspaceId.TryGetValue(id, out var sessionStrategy))
            {
                return sessionStrategy;
            }

            return id < 0 ? this.InstantiateWorkspaceStrategy(id) : null;
        }

        internal object GetRole(Strategy association, IRoleType roleType)
        {
            var role = this.SessionOriginState.Get(association.Id, roleType);
            if (roleType.ObjectType.IsUnit)
            {
                return role;
            }

            if (roleType.IsOne)
            {
                return this.Get<IObject>((long?)role);
            }

            return role != null ? this.Numbers.Enumerate(role).Select(this.Get<IObject>).ToArray() : this.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
        }

        internal IEnumerable<T> GetAssociation<T>(long role, IAssociationType associationType) where T : IObject
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
                    yield return (T)association.Object;
                }
            }
        }

        internal void OnChange(DatabaseOriginState state) => _ = (this.changedDatabaseStates ??= new HashSet<DatabaseOriginState>()).Add(state);

        internal void OnChange(WorkspaceOriginState state) => _ = (this.changedWorkspaceStates ??= new HashSet<WorkspaceOriginState>()).Add(state);

        private Strategy InstantiateDatabaseStrategy(long id)
        {
            var databaseRecord = this.DatabaseAdapter.GetRecord(id);
            var strategy = new Strategy(this, databaseRecord);
            _ = (this.databaseStrategies ??= new HashSet<Strategy>()).Add(strategy);
            this.AddStrategy(strategy);
            this.OnInstantiated(strategy);
            return strategy;
        }

        private Strategy InstantiateWorkspaceStrategy(long id)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(id, out var @class))
            {
                return null;
            }

            var strategy = new Strategy(this, @class, id);
            _ = (this.workspaceStrategies ??= new HashSet<Strategy>()).Add(strategy);
            this.AddStrategy(strategy);
            this.OnInstantiated(strategy);

            return strategy;
        }

        private IEnumerable<Strategy> Get(IComposite objectType)
        {
            var classes = new HashSet<IClass>(objectType.Classes);
            return this.strategyByWorkspaceId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }

        private void AddStrategy(Strategy strategy)
        {
            this.strategyByWorkspaceId.Add(strategy.Id, strategy);

            var @class = strategy.Class;
            if (!this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                this.strategiesByClass[@class] = new HashSet<Strategy> { strategy };
            }
            else
            {
                _ = strategies.Add(strategy);
            }
        }

        private void RemoveStrategy(Strategy strategy)
        {
            _ = this.strategyByWorkspaceId.Remove(strategy.Id);

            var @class = strategy.Class;
            if (!this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                return;
            }

            _ = strategies.Remove(strategy);
            if (strategies.Count == 0)
            {
                _ = this.strategiesByClass.Remove(@class);
            }
        }

        private Task<IPullResult> OnPull(PullResult pullResult)
        {
            var syncObjects = this.DatabaseAdapter.ObjectsToSync(pullResult);

            this.Workspace.DatabaseAdapter.Sync(syncObjects, pullResult.AccessControlLists);

            foreach (var databaseObject in pullResult.DatabaseObjects)
            {
                var id = databaseObject.Id;
                if (!this.strategyByWorkspaceId.ContainsKey(id))
                {
                    _ = this.InstantiateDatabaseStrategy(id);
                }
            }

            return Task.FromResult<IPullResult>(pullResult);
        }

        private void PushResponse(PushResult pushResponse)
        {
            if (pushResponse.ObjectByNewId?.Count > 0)
            {
                foreach (var kvp in pushResponse.ObjectByNewId)
                {
                    var workspaceId = kvp.Key;
                    var databaseId = kvp.Value.Id;

                    var strategy = this.strategyByWorkspaceId[workspaceId];

                    _ = this.newDatabaseStrategies.Remove(strategy);
                    this.RemoveStrategy(strategy);

                    // TODO: Is this necessary?
                    var databaseObject = this.DatabaseAdapter.PushResponse(databaseId, strategy.Class);
                    strategy.DatabasePushResponse(databaseObject);

                    _ = (this.databaseStrategies ??= new HashSet<Strategy>()).Add(strategy);
                    this.AddStrategy(strategy);
                }
            }

            if (this.newDatabaseStrategies?.Count > 0)
            {
                throw new Exception("Not all new objects received ids");
            }

            this.newDatabaseStrategies = null;
        }

        private void OnInstantiated(Strategy strategy) => this.changeSet.Instantiated.Add(strategy);
    }
}
