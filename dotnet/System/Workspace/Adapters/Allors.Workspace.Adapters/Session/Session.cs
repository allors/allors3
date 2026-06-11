// <copyright file="RemoteSession.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Collections;
    using Data;
    using Meta;
    using Signals;

    public abstract class Session : ISession
    {
        private readonly Dictionary<IClass, ISet<Strategy>> strategiesByClass;

        private readonly IStateSignal<long> graphRevision;
        private readonly ISignal<bool> hasChangesSignal;
        private long revision;
        private int graphHolds;
        private bool graphTouchPending;

        protected Session(Workspace workspace, ISessionServices sessionServices)
        {
            this.Workspace = workspace;
            this.Services = sessionServices;

            this.StrategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();
            this.SessionOriginState = new SessionOriginState(workspace.StrategyRanges);

            this.ChangeSetTracker = new ChangeSetTracker(this);
            this.PushToDatabaseTracker = new PushToDatabaseTracker();

            // Each session gets its own factory: the default engine's reactive graph and
            // effect scheduler are single-threaded, and sessions may live on different
            // threads (e.g. one per Blazor Server circuit).
            this.SignalFactory = workspace.Configuration.SignalFactoryBuilder();

            this.graphRevision = this.SignalFactory.State(0L);
            this.hasChangesSignal = this.SignalFactory.Computed(() =>
            {
                _ = this.graphRevision.Value;
                return this.HasChanges;
            });

            this.Services.OnInit(this);
        }

        // The dependency-driven prefetch hook is no longer populated (the rule engine
        // that fed it has been replaced by signals); kept as an empty set so the pull
        // path remains source-compatible.
        public ISet<IDependency> Dependencies => EmptySet<IDependency>.Instance;

        public bool HasChanges => this.StrategyByWorkspaceId.Any(kvp => kvp.Value.HasChanges);

        ISignal<bool> ISession.HasChanges => this.hasChangesSignal;

        public ISessionServices Services { get; }

        IWorkspace ISession.Workspace => this.Workspace;

        public event EventHandler OnChange;

        public virtual void OnChanged(EventArgs e)
        {
            this.TouchGraph();
            this.OnChange?.Invoke(this, e);
        }

        public Workspace Workspace { get; }

        public ChangeSetTracker ChangeSetTracker { get; }

        public PushToDatabaseTracker PushToDatabaseTracker { get; }

        public SessionOriginState SessionOriginState { get; }

        public ISignalFactory SignalFactory { get; }

        internal IStateSignal<long> GraphRevision => this.graphRevision;

        protected Dictionary<long, Strategy> StrategyByWorkspaceId { get; }

        public override string ToString() => $"session: {base.ToString()}";

        internal static bool IsNewId(long id) => id < 0;

        // Bumps the session-wide graph revision; every reactive signal depends on it
        // and therefore recomputes on next read. The new value comes from an untracked
        // backing counter: reading graphRevision.Value here would register the revision
        // as a dependency of whichever effect or computed is performing the write,
        // re-running it on every subsequent change, forever.
        internal void TouchGraph()
        {
            if (this.graphHolds > 0)
            {
                this.graphTouchPending = true;
                return;
            }

            this.graphRevision.Value = ++this.revision;
        }

        // Hold/Release bracket a multi-record operation (pull, push response, reset):
        // TouchGraph calls in between coalesce into a single bump on the final Release,
        // so effects observe only the fully merged state instead of every intermediate
        // record merge.
        protected internal void HoldGraph() => this.graphHolds++;

        protected internal void ReleaseGraph()
        {
            this.graphHolds--;
            if (this.graphHolds == 0 && this.graphTouchPending)
            {
                this.graphTouchPending = false;
                this.graphRevision.Value = ++this.revision;
            }
        }

        public void Reset()
        {
            var changeSet = this.Checkpoint();

            var strategies = new HashSet<IStrategy>(changeSet.Created);

            foreach (var roles in changeSet.RolesByAssociationType.Values)
            {
                strategies.UnionWith(roles);
            }

            foreach (var associations in changeSet.AssociationsByRoleType.Values)
            {
                strategies.UnionWith(associations);
            }

            //TODO: Koen, fix strategy = null
            this.HoldGraph();
            try
            {
                foreach (var strategy in strategies.Where(v => v != null))
                {
                    strategy.Reset();
                }
            }
            finally
            {
                this.ReleaseGraph();
            }
        }

        public abstract T Create<T>(IClass @class) where T : class, IObject;

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>());
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

            this.SessionOriginState.Checkpoint(changeSet);

            this.ChangeSetTracker.Created = null;
            this.ChangeSetTracker.Instantiated = null;
            this.ChangeSetTracker.DatabaseOriginStates = null;

            return changeSet;
        }

        #region Instantiate
        public T Instantiate<T>(IObject @object) where T : class, IObject => this.Instantiate<T>(@object.Id);

        public T Instantiate<T>(T @object) where T : class, IObject => this.Instantiate<T>(@object.Id);

        public T Instantiate<T>(long? id) where T : class, IObject => id.HasValue ? this.Instantiate<T>((long)id) : default;

        public T Instantiate<T>(long id) where T : class, IObject => (T)this.GetStrategy(id)?.Object;

        public T Instantiate<T>(string idAsString) where T : class, IObject => long.TryParse(idAsString, out var id) ? (T)this.GetStrategy(id)?.Object : default;

        public IEnumerable<T> Instantiate<T>(IEnumerable<IObject> objects) where T : class, IObject => objects.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<T> objects) where T : class, IObject => objects.Select(this.Instantiate);

        public IEnumerable<T> Instantiate<T>(IEnumerable<long> ids) where T : class, IObject => ids.Select(this.Instantiate<T>);

        public IEnumerable<T> Instantiate<T>(IEnumerable<string> ids) where T : class, IObject => this.Instantiate<T>(ids.Select(
            v =>
            {
                long.TryParse(v, out var id);
                return id;
            }));

        public IEnumerable<T> Instantiate<T>() where T : class, IObject
        {
            var objectType = (IComposite)this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
            return this.Instantiate<T>(objectType);
        }

        public IEnumerable<T> Instantiate<T>(IComposite objectType) where T : class, IObject
        {
            foreach (var @class in objectType.Classes)
            {
                if (this.strategiesByClass.TryGetValue(@class, out var strategies))
                {
                    foreach (var strategy in strategies)
                    {
                        yield return (T)strategy.Object;
                    }
                }
            }
        }

        #endregion

        public Strategy GetStrategy(long id)
        {
            if (id == 0)
            {
                return null;
            }

            return this.StrategyByWorkspaceId.TryGetValue(id, out var sessionStrategy) ? sessionStrategy : null;
        }

        public Strategy GetCompositeAssociation(Strategy role, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.StrategiesForClass(associationType.ObjectType))
            {
                if (!association.CanRead(roleType))
                {
                    continue;
                }

                if (association.IsCompositeAssociationForRole(roleType, role))
                {
                    return association;
                }
            }

            return null;
        }

        public IEnumerable<Strategy> GetCompositesAssociation(Strategy role, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.StrategiesForClass(associationType.ObjectType))
            {
                if (!association.CanRead(roleType))
                {
                    continue;
                }

                if (association.IsCompositesAssociationForRole(roleType, role))
                {
                    yield return association;
                }
            }
        }

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

            this.TouchGraph();
        }

        public void OnDatabasePushResponseNew(long workspaceId, long databaseId)
        {
            var strategy = this.StrategyByWorkspaceId[workspaceId];
            this.PushToDatabaseTracker.Created.Remove(strategy);
            strategy.OnDatabasePushNewId(databaseId);
            this.AddStrategy(strategy);
            strategy.OnDatabasePushed();
        }

        private IEnumerable<Strategy> StrategiesForClass(IComposite objectType)
        {
            // TODO: Optimize
            var classes = new HashSet<IClass>(objectType.Classes);
            return this.StrategyByWorkspaceId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value).Distinct();
        }

        public abstract Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options = null);
        public abstract Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options = null);

        public Task<IInvokeResult> InvokeAsync(IMethodSignal method, InvokeOptions options = null) =>
            this.InvokeAsync(new Method(method.Object, method.MethodType), options);

        public Task<IInvokeResult> InvokeAsync(IMethodSignal[] methods, InvokeOptions options = null) =>
            this.InvokeAsync(methods.Select(v => new Method(v.Object, v.MethodType)).ToArray(), options);

        public abstract Task<IPullResult> CallAsync(Procedure procedure, params Pull[] pull);
        public abstract Task<IPullResult> CallAsync(object args, string name);
        public abstract Task<IPullResult> PullAsync(params Pull[] pull);
        public abstract Task<IPushResult> PushAsync();
    }
}
