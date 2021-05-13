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

        internal Session(Workspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.DatabaseAdapter = this.Workspace.DatabaseAdapter;
            this.Lifecycle = sessionLifecycle;
            this.Numbers = this.Workspace.Numbers;

            this.strategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();
            this.SessionOriginState = new SessionOriginState(workspace.Numbers);

            this.ChangeSetTracker = new ChangeSetTracker(this);
            this.PushToWorkspaceTracker = new PushToWorkspaceTracker();
            this.PushToDatabaseTracker = new PushToDatabaseTracker(this);

            this.Lifecycle.OnInit(this);
        }

        public ISessionLifecycle Lifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        internal Workspace Workspace { get; }

        internal INumbers Numbers { get; }

        internal ChangeSetTracker ChangeSetTracker { get; }

        internal PushToWorkspaceTracker PushToWorkspaceTracker { get; }

        internal PushToDatabaseTracker PushToDatabaseTracker { get; }

        internal DatabaseAdapter DatabaseAdapter { get; }

        internal SessionOriginState SessionOriginState { get; }

        public Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) => this.Invoke(new[] { method }, options);

        public Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var result = new InvokeResult(this, this.Workspace);
            result.Execute(methods, options);
            return Task.FromResult<IInvokeResult>(result);
        }

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public T Create<T>(IClass @class) where T : IObject
        {
            var workspaceId = this.DatabaseAdapter.WorkspaceIdGenerator.Next();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);

            if (@class.Origin != Origin.Session)
            {
                this.PushToWorkspaceTracker.OnCreated(strategy);
                if (@class.Origin == Origin.Database)
                {
                    this.PushToDatabaseTracker.OnCreated(strategy);
                }
            }

            this.ChangeSetTracker.OnCreated(strategy);

            return (T)strategy.Object;
        }

        public T Get<T>(IObject @object) where T : IObject => this.Get<T>(@object.Id);

        public T Get<T>(T @object) where T : IObject => this.Get<T>(@object.Id);

        public T Get<T>(long? id) where T : IObject => id.HasValue ? this.Get<T>((long)id) : default;

        public T Get<T>(long id) where T : IObject => (T)this.GetStrategy(id)?.Object;

        public T Get<T>(string idAsString) where T : IObject => long.TryParse(idAsString, out var id) ? (T)this.GetStrategy(id)?.Object : default;

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

        public Task<IPullResult> Pull(params Pull[] pulls)
        {
            var result = new PullResult(this, this.Workspace);
            result.Execute(pulls);
            return this.OnPull(result);
        }

        public Task<IPullResult> Pull(Data.Procedure procedure, params Pull[] pulls)
        {
            var result = new PullResult(this, this.Workspace);

            result.Execute(procedure);
            result.Execute(pulls);

            return this.OnPull(result);
        }
        
        public Task<IPushResult> Push()
        {
            var result = new PushResult(this);

            this.PushToWorkspaceTracker.Push(result);
            if (result.HasErrors)
            {
                this.PushToDatabaseTracker.Push(result);
            }

            return Task.FromResult<IPushResult>(result);
        }

        public Task<IPushResult> PushToDatabase() => Task.FromResult<IPushResult>(this.PushToDatabaseTracker.Push(new PushResult(this)));

        public IPushResult PushToWorkspace() => this.PushToWorkspaceTracker.Push(new PushResult(this));

        public IChangeSet Checkpoint()
        {
            var changeSet = this.ChangeSetTracker.Checkpoint();
            this.SessionOriginState.Checkpoint(changeSet);
            return changeSet;
        }

        internal void OnPush(PushResult result)
        {
            this.Workspace.DatabaseAdapter.Sync(result.Objects, result.AccessControlLists);

            foreach (var @object in result.Objects)
            {
                if (!this.strategyByWorkspaceId.ContainsKey(@object.Id))
                {
                    _ = this.InstantiateDatabaseStrategy(@object.Id);
                }

                var strategy = this.GetStrategy(@object.Id);
                strategy.DatabaseOriginState.Reset();
            }
        }

        internal void OnNewObjectPushed(long workspaceId, long databaseId)
        {
            var strategy = this.strategyByWorkspaceId[workspaceId];
            this.RemoveStrategy(strategy);
            var databaseRecord = this.DatabaseAdapter.OnPushed(databaseId, strategy.Class);
            strategy.DatabasePushResponse(databaseRecord);
            this.AddStrategy(strategy);
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

        private Strategy InstantiateDatabaseStrategy(long id)
        {
            var databaseRecord = this.DatabaseAdapter.GetRecord(id);
            var strategy = new Strategy(this, databaseRecord);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

            return strategy;
        }

        private Strategy InstantiateWorkspaceStrategy(long id)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(id, out var @class))
            {
                return null;
            }

            var strategy = new Strategy(this, @class, id);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

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
    }
}
