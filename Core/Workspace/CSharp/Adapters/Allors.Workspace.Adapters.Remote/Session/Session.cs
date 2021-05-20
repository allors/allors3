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
    using Numbers;
    using Protocol.Json;
    using InvokeOptions = Allors.Workspace.InvokeOptions;

    public class Session : ISession
    {
        private readonly Dictionary<IClass, ISet<Strategy>> strategiesByClass;
        private readonly Dictionary<long, Strategy> strategyByWorkspaceId;

        internal Session(Workspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.Database = this.Workspace.Database;
            this.Lifecycle = sessionLifecycle;
            this.Numbers = this.Workspace.Numbers;

            this.strategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();
            this.SessionOriginState = new SessionOriginState(this.Numbers);

            this.ChangeSetTracker = new ChangeSetTracker();
            this.PushToDatabaseTracker = new PushToDatabaseTracker();
            this.PushToWorkspaceTracker = new PushToWorkspaceTracker();

            this.Lifecycle.OnInit(this);
        }

        internal Workspace Workspace { get; }

        internal ChangeSetTracker ChangeSetTracker { get; }

        internal PushToDatabaseTracker PushToDatabaseTracker { get; }

        internal PushToWorkspaceTracker PushToWorkspaceTracker { get; }

        internal Database Database { get; }

        internal INumbers Numbers { get; }

        internal SessionOriginState SessionOriginState { get; }

        public ISessionLifecycle Lifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;

        public async Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) =>
            await this.Invoke(new[] {method}, options);

        public async Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                List = methods.Select(v => new Invocation
                {
                    Id = v.Object.Id,
                    Version = ((Strategy)v.Object.Strategy).DatabaseVersion,
                    Method = v.MethodType.Tag
                }).ToArray(),
                Options = options != null
                    ? new Allors.Protocol.Json.Api.Invoke.InvokeOptions
                    {
                        ContinueOnError = options.ContinueOnError, Isolated = options.Isolated
                    }
                    : null
            };

            var invokeResponse = await this.Database.Invoke(invokeRequest);
            return new InvokeResult(this, invokeResponse);
        }

        public T Create<T>() where T : class, IObject =>
            this.Create<T>((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public T Create<T>(IClass @class) where T : IObject
        {
            var workspaceId = this.Database.WorkspaceIdGenerator.Next();
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

        public IEnumerable<T> Get<T>(IEnumerable<string> identities) where T : IObject => this.Get<T>(identities.Select(
            v =>
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
            var pullRequest = new PullRequest {List = pulls.Select(v => v.ToJson()).ToArray()};

            var pullResponse = await this.Database.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public async Task<IPullResult> Pull(Procedure procedure, params Pull[] pulls)
        {
            var pullRequest = new PullRequest
            {
                Procedure = procedure.ToJson(), List = pulls.Select(v => v.ToJson()).ToArray()
            };

            var pullResponse = await this.Database.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public async Task<IPushResult> Push()
        {
            var result = await this.PushToDatabase();

            if (!result.HasErrors)
            {
                this.PushToWorkspace(result);
            }

            return result;
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
            var role = this.SessionOriginState.Get(association.Id, roleType);
            if (roleType.ObjectType.IsUnit)
            {
                return role;
            }

            if (roleType.IsOne)
            {
                return this.Get<IObject>((long?)role);
            }

            var ids = (IEnumerable<long>)role;
            return ids?.Select(this.Get<IObject>).ToArray() ??
                   this.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
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

        internal PushRequest PushRequest() => new PushRequest
        {
            NewObjects = this.PushToDatabaseTracker.Created?.Select(v => v.DatabasePushNew()).ToArray(),
            Objects = this.PushToDatabaseTracker.Changed?.Select(v => v.Strategy.DatabasePushExisting()).ToArray()
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

                    _ = this.PushToDatabaseTracker.Created.Remove(strategy);

                    this.RemoveStrategy(strategy);
                    var databaseRecord = this.Database.OnPushed(databaseId, strategy.Class);
                    strategy.DatabasePushResponse(databaseRecord);
                    this.AddStrategy(strategy);
                }
            }
        }

        internal Strategy InstantiateDatabaseObject(long id)
        {
            var databaseObject = this.Database.GetRecord(id);
            var strategy = new Strategy(this, databaseObject);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

            return strategy;
        }

        private Strategy InstantiateWorkspaceObject(long identity)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(identity, out var @class))
            {
                return null;
            }

            var strategy = new Strategy(this, @class, identity);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

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
                this.strategiesByClass[@class] = new HashSet<Strategy> {strategy};
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

        private async Task<PushResult> PushToDatabase()
        {
            var pushRequest = this.PushRequest();
            var pushResponse = await this.Database.Push(pushRequest);
            if (!pushResponse.HasErrors)
            {
                this.PushResponse(pushResponse);

                var objects = pushRequest.Objects?.Select(v => v.DatabaseId).ToArray() ?? Array.Empty<long>();
                if (pushResponse.NewObjects != null)
                {
                    objects = objects.Union(pushResponse.NewObjects.Select(v => v.DatabaseId)).ToArray();
                }

                var syncRequests = new SyncRequest {Objects = objects};

                await this.Sync(syncRequests);
            }

            var result = new PushResult(this, pushResponse);
            return result;
        }

        private IPushResult PushToWorkspace(IPushResult result)
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
    }
}
