// <copyright file="RemoteSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Sync;
    using Data;
    using Meta;
    using Protocol.Json;
    using InvokeOptions = InvokeOptions;

    public class Session : ISession
    {
        private readonly Dictionary<long, Strategy> strategyByWorkspaceId;
        private readonly Dictionary<IClass, Strategy[]> strategiesByClass;

        private readonly IList<Strategy> existingDatabaseStrategies;
        private ISet<Strategy> newDatabaseStrategies;
        private ISet<Strategy> workspaceStrategies;

        private ISet<DatabaseOriginState> changedDatabaseStates;
        private ISet<WorkspaceOriginState> changedWorkspaceStates;

        private ISet<IStrategy> created;
        private ISet<IStrategy> instantiated;

        internal Session(Workspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.Database = this.Workspace.Database;
            this.Lifecycle = sessionLifecycle;

            this.strategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, Strategy[]>();
            this.existingDatabaseStrategies = new List<Strategy>();

            this.SessionState = new SessionOriginState();
            this.Lifecycle.OnInit(this);
        }

        public ISessionLifecycle Lifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;
        internal Workspace Workspace { get; }

        internal Database Database { get; }

        internal bool HasDatabaseChanges => this.newDatabaseStrategies?.Count > 0 || this.existingDatabaseStrategies.Any(v => v.HasDatabaseChanges);

        internal SessionOriginState SessionState { get; }

        public async Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) => await this.Invoke(new[] { method }, options);

        public async Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                List = methods.Select(v => new Invocation
                {
                    Id = v.Object.Id,
                    Version = ((Strategy)v.Object.Strategy).DatabaseVersion,
                    Method = v.MethodType.Tag,
                }).ToArray(),
                Options = options != null ? new Allors.Protocol.Json.Api.Invoke.InvokeOptions
                {
                    ContinueOnError = options.ContinueOnError,
                    Isolated = options.Isolated
                } : null,
            };

            var invokeResponse = await this.Database.Invoke(invokeRequest);
            return new InvokeResult(this, invokeResponse);
        }

        public T Create<T>() where T : class, IObject => this.Create<T>((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public T Create<T>(IClass @class) where T : IObject
        {
            var workspaceId = this.Database.WorkspaceIdGenerator.Next();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);

            if (@class.Origin == Origin.Database)
            {
                this.newDatabaseStrategies ??= new HashSet<Strategy>();
                _ = this.newDatabaseStrategies.Add(strategy);
            }

            if (@class.Origin == Origin.Workspace)
            {
                this.workspaceStrategies ??= new HashSet<Strategy>();
                _ = this.workspaceStrategies.Add(strategy);
                // TODO: move to Push
                this.Workspace.RegisterWorkspaceObject(@class, workspaceId);
            }

            this.created ??= new HashSet<IStrategy>();
            _ = this.created.Add(strategy);

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

        public async Task<IPullResult> Pull(params Pull[] pulls)
        {
            var pullRequest = new PullRequest
            {
                List = pulls.Select(v => v.ToJson()).ToArray()
            };

            var pullResponse = await this.Database.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public async Task<IPullResult> Pull(Procedure procedure, params Pull[] pulls)
        {
            var pullRequest = new PullRequest
            {
                Procedure = procedure.ToJson(),
                List = pulls.Select(v => v.ToJson()).ToArray()
            };

            var pullResponse = await this.Database.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public void Reset()
        {
            foreach (var kvp in this.strategyByWorkspaceId)
            {
                kvp.Value.Reset();
            }
        }

        public async Task<IPushResult> Push()
        {
            var pushRequest = this.PushRequest();
            var pushResponse = await this.Database.Push(pushRequest);
            if (!pushResponse.HasErrors)
            {
                this.PushResponse(pushResponse);

                var objects = pushRequest.Objects.Select(v => v.DatabaseId).ToArray();
                if (pushResponse.NewObjects != null)
                {
                    objects = objects.Union(pushResponse.NewObjects.Select(v => v.DatabaseId)).ToArray();
                }

                var syncRequests = new SyncRequest
                {
                    Objects = objects,
                };

                await this.Sync(syncRequests);

                if (this.workspaceStrategies != null)
                {
                    foreach (var workspaceStrategy in this.workspaceStrategies)
                    {
                        workspaceStrategy.WorkspacePush();
                    }
                }

                this.Reset();
            }

            return new PushResult(this, pushResponse);
        }

        public IChangeSet Checkpoint()
        {
            var changeSet = new ChangeSet(this, this.created, this.instantiated, this.SessionState.Checkpoint());

            if (this.changedWorkspaceStates != null)
            {
                foreach (var changed in this.changedWorkspaceStates)
                {
                    changed.Checkpoint(changeSet);
                }
            }

            if (this.changedDatabaseStates != null)
            {
                foreach (var changed in this.changedDatabaseStates)
                {
                    changed.Checkpoint(changeSet);
                }
            }

            this.created = null;
            this.instantiated = null;

            return changeSet;
        }

        internal Strategy GetStrategy(long? identity) => identity.HasValue ? this.GetStrategy(identity.Value) : null;

        internal Strategy GetStrategy(long identity)
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

        internal object GetRole(Strategy association, IRoleType roleType)
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

        internal IEnumerable<T> GetAssociation<T>(Strategy role, IAssociationType associationType) where T : IObject
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

        internal PushRequest PushRequest() => new PushRequest
        {
            NewObjects = this.newDatabaseStrategies?.Select(v => v.DatabasePushNew()).ToArray(),
            Objects = this.existingDatabaseStrategies.Where(v => v.HasDatabaseChanges).Select(v => v.DatabasePushExisting()).ToArray(),
        };

        internal void PushResponse(PushResponse pushResponse)
        {
            if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
            {
                foreach (var pushResponseNewObject in pushResponse.NewObjects)
                {
                    var workspaceId = pushResponseNewObject.WorkspaceId;
                    var databaseId = pushResponseNewObject.DatabaseId;

                    var strategy = this.strategyByWorkspaceId[workspaceId];

                    _ = this.newDatabaseStrategies.Remove(strategy);
                    this.RemoveStrategy(strategy);

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

        internal void OnChange(DatabaseOriginState state)
        {
            this.changedDatabaseStates ??= new HashSet<DatabaseOriginState>();
            _ = this.changedDatabaseStates.Add(state);
        }

        internal void OnChange(WorkspaceOriginState state)
        {
            this.changedWorkspaceStates ??= new HashSet<WorkspaceOriginState>();
            _ = this.changedWorkspaceStates.Add(state);
        }

        internal Strategy InstantiateDatabaseObject(long identity)
        {
            var databaseObject = this.Database.Get(identity);
            var strategy = new Strategy(this, databaseObject);
            this.existingDatabaseStrategies.Add(strategy);
            this.AddStrategy(strategy);
            this.OnInstantiate(strategy);

            return strategy;
        }

        private Strategy InstantiateWorkspaceObject(long identity)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(identity, out var @class))
            {
                return null;
            }

            var strategy = new Strategy(this, @class, identity);
            this.workspaceStrategies ??= new HashSet<Strategy>();
            _ = this.workspaceStrategies.Add(strategy);
            this.AddStrategy(strategy);
            this.OnInstantiate(strategy);

            return strategy;
        }

        private async Task<IPullResult> OnPull(PullResponse pullResponse)
        {
            var syncRequest = this.Database.Diff(pullResponse);
            if (syncRequest.Objects.Length > 0)
            {
                await this.Sync(syncRequest);
            }

            foreach (var v in pullResponse.Pool)
            {
                if (!this.strategyByWorkspaceId.ContainsKey(v.Id))
                {
                    _ = this.InstantiateDatabaseObject(v.Id);
                }
            }

            return new PullResult(this, pullResponse);
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
                    _ = this.Database.SecurityResponse(securityResponse);
                }
            }
        }

        private void OnInstantiate(Strategy strategy)
        {
            this.instantiated ??= new HashSet<IStrategy>();
            _ = this.instantiated.Add(strategy);
        }

        internal IEnumerable<Strategy> Get(IComposite objectType)
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
                strategies = new[] { strategy };
            }
            else
            {
                strategies = NullableSortableArraySet.Add(strategies, strategy);
            }

            this.strategiesByClass[@class] = strategies;
        }

        private void RemoveStrategy(Strategy strategy)
        {
            _ = this.strategyByWorkspaceId.Remove(strategy.Id);

            var @class = strategy.Class;
            if (this.strategiesByClass.TryGetValue(@class, out var strategies))
            {
                strategies = NullableSortableArraySet.Remove(strategies, strategy);
                this.strategiesByClass[@class] = strategies;
            }
        }
    }
}
