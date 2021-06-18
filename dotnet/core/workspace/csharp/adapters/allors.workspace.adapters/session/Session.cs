// <copyright file="RemoteSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Meta;

    public abstract class Session : ISession
    {
        private readonly Dictionary<IClass, ISet<Strategy>> strategiesByClass;

        protected Session(Workspace workspace, ISessionServices sessionServices)
        {
            this.Workspace = workspace;
            this.Services = sessionServices;

            this.StrategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();
            this.SessionOriginState = new SessionOriginState(this.Workspace.Numbers);

            this.ChangeSetTracker = new ChangeSetTracker();
            this.PushToDatabaseTracker = new PushToDatabaseTracker();
            this.PushToWorkspaceTracker = new PushToWorkspaceTracker();

            this.Services.OnInit(this);
        }

        public ISessionServices Services { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        public Workspace Workspace { get; }

        public ChangeSetTracker ChangeSetTracker { get; }

        public PushToDatabaseTracker PushToDatabaseTracker { get; }

        public PushToWorkspaceTracker PushToWorkspaceTracker { get; }

        public SessionOriginState SessionOriginState { get; }

        protected Dictionary<long, Strategy> StrategyByWorkspaceId { get; }

        public T Create<T>() where T : class, IObject =>
            this.Create<T>((IClass)this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>());

        public T Get<T>(IObject @object) where T : IObject => this.Get<T>(@object.Id);

        public T Get<T>(T @object) where T : IObject => this.Get<T>(@object.Id);

        public T Get<T>(long? id) where T : IObject => id.HasValue ? this.Get<T>((long)id) : default;

        public T Get<T>(long id) where T : IObject => (T)this.GetStrategy(id)?.Object;

        public T Get<T>(string idAsString) where T : IObject =>
            long.TryParse(idAsString, out var id) ? (T)this.GetStrategy(id)?.Object : default;

        public IEnumerable<T> Get<T>(IEnumerable<IObject> objects) where T : IObject => objects.Select(this.Get<T>);

        public IEnumerable<T> Get<T>(IEnumerable<T> objects) where T : IObject => objects.Select(this.Get);

        public IEnumerable<T> Get<T>(IEnumerable<long> identities) where T : IObject => identities.Select(this.Get<T>);

        public IEnumerable<T> Get<T>(IEnumerable<string> identities) where T : IObject => this.Get<T>(identities.Select(
            v =>
            {
                long.TryParse(v, out var id);
                return id;
            }));

        public IEnumerable<T> GetAll<T>() where T : IObject
        {
            var objectType = (IComposite)this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
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
                                if (this.StrategyByWorkspaceId.TryGetValue(id, out var strategy))
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

        public IChangeSet Checkpoint()
        {
            var changeSet = new ChangeSet(this, this.ChangeSetTracker.Created, this.ChangeSetTracker.Instantiated);

            if (this.ChangeSetTracker.DatabaseOriginStates != null)
            {
                foreach (var databaseOriginState in this.ChangeSetTracker.DatabaseOriginStates)
                {
                    databaseOriginState.Checkpoint(changeSet);
                }
            }

            if (this.ChangeSetTracker.WorkspaceOriginStates != null)
            {
                foreach (var workspaceOriginState in this.ChangeSetTracker.WorkspaceOriginStates)
                {
                    workspaceOriginState.Checkpoint(changeSet);
                }
            }

            this.SessionOriginState.Checkpoint(changeSet);

            this.ChangeSetTracker.Created = null;
            this.ChangeSetTracker.Instantiated = null;
            this.ChangeSetTracker.DatabaseOriginStates = null;
            this.ChangeSetTracker.WorkspaceOriginStates = null;

            return changeSet;
        }

        public abstract Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null);

        public abstract Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null);

        public abstract Task<IPullResult> Pull(params Pull[] pulls);

        public abstract Task<IPullResult> Pull(Procedure procedure, params Pull[] pulls);

        public abstract Task<IPushResult> Push();

        public Strategy GetStrategy(long id)
        {
            if (id == 0)
            {
                return default;
            }

            if (this.StrategyByWorkspaceId.TryGetValue(id, out var sessionStrategy))
            {
                return sessionStrategy;
            }

            return IsNewId(id) ? this.InstantiateWorkspaceStrategy(id) : null;
        }

        public object GetRole(Strategy association, IRoleType roleType)
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

            return role != null
                ? this.Workspace.Numbers.Enumerate(role).Select(this.Get<IObject>).ToArray()
                : this.Workspace.DatabaseConnection.EmptyArray(roleType.ObjectType);
        }

        public IEnumerable<T> GetAssociation<T>(long role, IAssociationType associationType) where T : IObject
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

        public abstract T Create<T>(IClass @class) where T : class, IObject;

        public abstract Strategy InstantiateDatabaseStrategy(long id);

        protected abstract Strategy InstantiateWorkspaceStrategy(long id);

        protected void AddStrategy(Strategy strategy)
        {
            this.StrategyByWorkspaceId.Add(strategy.Id, strategy);

            var @class = strategy.Class;
            if (!this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                this.strategiesByClass[@class] = new HashSet<Strategy> { strategy };
            }
            else
            {
                strategies.Add(strategy);
            }
        }

        protected void RemoveStrategy(Strategy strategy)
        {
            this.StrategyByWorkspaceId.Remove(strategy.Id);

            var @class = strategy.Class;
            if (!this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                return;
            }

            strategies.Remove(strategy);
        }

        protected IPushResult PushToWorkspace(IPushResult result)
        {
            if (this.PushToWorkspaceTracker.Created != null)
            {
                foreach (var strategy in this.PushToWorkspaceTracker.Created)
                {
                    strategy.WorkspaceOriginState.Push();
                }
            }

            if (this.PushToWorkspaceTracker.Changed != null)
            {
                foreach (var state in this.PushToWorkspaceTracker.Changed)
                {
                    if (this.PushToWorkspaceTracker.Created?.Contains(state.Strategy) == true)
                    {
                        continue;
                    }

                    state.Push();
                }
            }

            this.PushToWorkspaceTracker.Created = null;
            this.PushToWorkspaceTracker.Changed = null;

            return result;
        }

        protected void OnDatabasePushResponseNew(long workspaceId, long databaseId)
        {
            var strategy = this.StrategyByWorkspaceId[workspaceId];
            this.PushToDatabaseTracker.Created.Remove(strategy);

            this.RemoveStrategy(strategy);
            strategy.OnDatabasePushNewId(databaseId);
            this.AddStrategy(strategy);

            this.OnDatabasePushResponse(strategy);

        }

        protected void OnDatabasePushResponse(Adapters.Strategy strategy)
        {
            var databaseRecord = this.Workspace.DatabaseConnection.OnPushResponse(strategy.Class, strategy.Id);
            strategy.OnDatabasePushResponse(databaseRecord);
        }


        internal static bool IsNewId(long id) => id < 0;

        private IEnumerable<Strategy> Get(IComposite objectType)
        {
            var classes = new HashSet<IClass>(objectType.Classes);
            return this.StrategyByWorkspaceId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }
    }
}
