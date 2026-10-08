// <copyright file="Session.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Collections;
    using Connection;
    using Data;
    using Derivations;
    using Meta;
    using ConnectionPullResult = Allors.Workspace.Connection.PullResult;

    public class Session : ISession
    {
        private readonly Dictionary<IClass, ISet<Strategy>> strategiesByClass;

        private ISet<IDependency> dependencies;

        private IDictionary<IRoleType, ISet<IRule>> activeRulesByRoleType;

        internal Session(Workspace workspace, ISessionServices sessionServices)
        {
            this.Workspace = workspace;
            this.Services = sessionServices;

            this.StrategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();
            this.SessionOriginState = new SessionOriginState(workspace.StrategyRanges);

            this.ChangeSetTracker = new ChangeSetTracker(this);
            this.PushToDatabaseTracker = new PushToDatabaseTracker();

            this.Services.OnInit(this);
        }

        // TODO: push to concrete classes and implement
        public ISet<IDependency> Dependencies => EmptySet<IDependency>.Instance;

        public bool HasChanges => this.StrategyByWorkspaceId.Any(kvp => kvp.Value.HasChanges);

        public ISessionServices Services { get; }

        IWorkspace ISession.Workspace => this.Workspace;

        public event EventHandler OnChange;

        public virtual void OnChanged(EventArgs e) => this.OnChange?.Invoke(this, e);

        public Workspace Workspace { get; }

        public ChangeSetTracker ChangeSetTracker { get; }

        public PushToDatabaseTracker PushToDatabaseTracker { get; }

        public SessionOriginState SessionOriginState { get; }

        private IDatabaseConnection Connection => this.Workspace.Connection;

        private Dictionary<long, Strategy> StrategyByWorkspaceId { get; }

        public override string ToString() => $"session: {base.ToString()}";

        internal static bool IsNewId(long id) => id < 0;

        public void Activate(IEnumerable<IRule> rules)
        {
            if (rules == null)
            {
                return;
            }

            if (this.activeRulesByRoleType == null)
            {
                this.activeRulesByRoleType = new Dictionary<IRoleType, ISet<IRule>>();
                this.dependencies = new HashSet<IDependency>();
            }

            foreach (var rule in rules)
            {
                if (!this.activeRulesByRoleType.TryGetValue(rule.RoleType, out var activeRules))
                {
                    activeRules = new HashSet<IRule>();
                    this.activeRulesByRoleType.Add(rule.RoleType, activeRules);
                }

                activeRules.Add(rule);
                if (rule.Dependencies == null)
                {
                    continue;
                }

                foreach (var dependency in rule.Dependencies)
                {
                    this.dependencies.Add(dependency);
                }
            }
        }

        public IRule Resolve(Strategy strategy, IRoleType roleType)
        {
            if (this.activeRulesByRoleType != null && this.activeRulesByRoleType.TryGetValue(roleType, out var activeRules) && activeRules.Count > 0)
            {
                var rule = this.Workspace.GetRule(roleType, strategy);

                if (rule != null && activeRules.Contains(rule))
                {
                    return rule;
                }
            }

            return null;
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
            foreach (var strategy in strategies.Where(v => v != null))
            {
                strategy.Reset();
            }
        }

        public T Create<T>(IClass @class) where T : class, IObject
        {
            var workspaceId = this.Workspace.IdGenerator.Next();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);
            this.PushToDatabaseTracker.OnCreated(strategy);
            this.ChangeSetTracker.OnCreated(strategy);
            return (T)strategy.Object;
        }

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.Configuration.ObjectFactory.GetObjectType<T>());

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
            var objectType = (IComposite)this.Workspace.Configuration.ObjectFactory.GetObjectType<T>();
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

        public async Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options = null) => await this.InvokeAsync(new[] { method }, options);

        public async Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options = null)
        {
            var invocations = methods
                .Select(v => new Invocation(v.Object.Id, ((Strategy)v.Object.Strategy).DatabaseOriginState.Version, v.MethodType))
                .ToArray();

            var invoked = await this.Connection.InvokeAsync(invocations, options);
            return new InvokeResult(this, invoked);
        }

        public async Task<IPullResult> CallAsync(object args, string name)
        {
            var pulled = await this.Connection.PullAsync(name, args);
            return this.OnPull(pulled);
        }

        public async Task<IPullResult> PullAsync(params Pull[] pulls)
        {
            var pulled = await this.Connection.PullAsync(pulls);
            return this.OnPull(pulled);
        }

        public async Task<IPullResult> CallAsync(Procedure procedure, params Pull[] pull)
        {
            var pulled = await this.Connection.PullAsync(pull, procedure);
            return this.OnPull(pulled);
        }

        public async Task<IPushResult> PushAsync()
        {
            var databaseTracker = this.PushToDatabaseTracker;

            var newObjects = databaseTracker.Created?.Select(v => v.DatabaseOriginState.PushNew()).ToArray();
            var changedObjects = databaseTracker.Changed?.Select(v => v.PushExisting()).ToArray();

            var pushed = await this.Connection.PushAsync(newObjects, changedObjects);

            if (pushed.HasErrors)
            {
                return new PushResult(this, pushed);
            }

            foreach (var kvp in pushed.DatabaseIdByWorkspaceId)
            {
                var workspaceId = kvp.Key;
                var databaseId = kvp.Value;
                this.OnDatabasePushResponseNew(workspaceId, databaseId);
            }

            databaseTracker.Created = null;
            databaseTracker.Changed = null;

            if (changedObjects != null)
            {
                foreach (var changedObject in changedObjects)
                {
                    var strategy = this.GetStrategy(changedObject.Id);
                    strategy.OnDatabasePushed();
                }
            }

            return new PushResult(this, pushed);
        }

        private void AddStrategy(Strategy strategy)
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

        private void OnDatabasePushResponseNew(long workspaceId, long databaseId)
        {
            var strategy = this.StrategyByWorkspaceId[workspaceId];
            this.PushToDatabaseTracker.Created.Remove(strategy);
            strategy.OnDatabasePushNewId(databaseId);
            this.AddStrategy(strategy);
            strategy.OnDatabasePushed();
        }

        private void InstantiateDatabaseStrategy(long id)
        {
            var record = this.Connection.GetRecord(id);
            var strategy = new Strategy(this, record);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);
        }

        private PullResult OnPull(ConnectionPullResult pulled)
        {
            var pullResult = new PullResult(this, pulled);

            if (pullResult.HasErrors)
            {
                return pullResult;
            }

            foreach (var id in pulled.Pool)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(id, out var strategy))
                {
                    strategy.DatabaseOriginState.OnPulled(pullResult);
                }
                else
                {
                    this.InstantiateDatabaseStrategy(id);
                }
            }

            return pullResult;
        }

        private IEnumerable<Strategy> StrategiesForClass(IComposite objectType)
        {
            // TODO: Optimize
            var classes = new HashSet<IClass>(objectType.Classes);
            return this.StrategyByWorkspaceId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value).Distinct();
        }
    }
}
