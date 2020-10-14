// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace.Data;
    using Meta;
    using Protocol.Database.Invoke;
    using Protocol.Database.Pull;
    using Protocol.Database.Push;
    using Protocol.Database.Sync;

    public class Session : ISession
    {
        private readonly Dictionary<long, IStrategy> strategyByWorkspaceId;

        private readonly IList<DatabaseStrategy> existingDatabaseStrategies;
        private ISet<DatabaseStrategy> newDatabaseStrategies;

        public Session(Workspace workspace, ISessionStateLifecycle stateLifecycle)
        {
            this.Workspace = workspace;
            this.StateLifecycle = stateLifecycle;
            this.Workspace.RegisterSession(this);

            this.strategyByWorkspaceId = new Dictionary<long, IStrategy>();
            this.existingDatabaseStrategies = new List<DatabaseStrategy>();

            this.Population = new Population();
            this.StateLifecycle.OnInit(this);
        }

        ~Session() => this.Workspace.UnregisterSession(this);

        public ISessionStateLifecycle StateLifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;

        internal Workspace Workspace { get; }

        internal bool HasDatabaseChanges => this.newDatabaseStrategies?.Count > 0 || this.existingDatabaseStrategies.Any(v => v.HasDatabaseChanges);

        internal Population Population { get; }

        public async Task<ICallResult> Call(Method method, CallOptions options = null) => await this.Call(new[] { method }, options);

        public async Task<ICallResult> Call(Method[] methods, CallOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                I = methods.Select(v => new Invocation
                {
                    I = v.Object.DatabaseId?.ToString(),
                    V = v.Object.Strategy.Version.ToString(),
                    M = v.MethodType.IdAsString,
                }).ToArray(),
                O = options != null ? new InvokeOptions
                {
                    C = options.ContinueOnError,
                    I = options.Isolated
                } : null,
            };

            var invokeResponse = await this.Workspace.Database.Invoke(invokeRequest);
            return new CallResult(invokeResponse);
        }

        public async Task<ICallResult> Call(string service, object args)
        {
            var invokeResponse = await this.Workspace.Database.Invoke(service, args);
            return new CallResult(invokeResponse);
        }

        public T Create<T>() where T : class, IObject => (T)this.Create((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public IObject Create(IClass @class) => @class.Origin switch
        {
            Origin.Database => this.CreateDatabaseObject(@class),
            Origin.Workspace => this.CreateWorkspaceObject(@class),
            Origin.Session => this.CreateSessionObject(@class),
            _ => throw new Exception($"Unsupported origin: {@class.Origin}"),
        };

        public T Instantiate<T>(T @object) where T : IObject => (T)this.Instantiate(@object.WorkspaceId);

        public IObject Instantiate(long id)
        {
            var workspaceId = this.Workspace.ToWorkspaceId(id);

            if (!this.strategyByWorkspaceId.TryGetValue(workspaceId, out var strategy))
            {
                if (this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(workspaceId, out var @class))
                {
                    strategy = new WorkspaceStrategy(this, @class, workspaceId);
                    this.strategyByWorkspaceId[workspaceId] = strategy;
                }
                else
                {
                    var databaseObject = this.Workspace.Database.Get(id);
                    strategy = new DatabaseStrategy(this, databaseObject, workspaceId);
                    this.existingDatabaseStrategies.Add((DatabaseStrategy)strategy);
                    this.strategyByWorkspaceId[workspaceId] = strategy;
                }
            }

            return strategy.Object;
        }
        
        public async Task<ILoadResult> Load(params Pull[] pulls)
        {
            var pullRequest = new PullRequest { P = pulls.Select(v => v.ToJson()).ToArray() };
            var pullResponse = await this.Workspace.Database.Pull(pullRequest);
            var syncRequest = this.Workspace.Database.Diff(pullResponse);
            if (syncRequest.Objects.Length > 0)
            {
                await this.Load(syncRequest);
            }

            return new LoadResult(this, pullResponse);
        }

        public async Task<ILoadResult> Load(object args, string pullService = null)
        {
            if (args is Pull pull)
            {
                args = new PullRequest { P = new[] { pull.ToJson() } };
            }

            if (args is IEnumerable<Pull> pulls)
            {
                args = new PullRequest { P = pulls.Select(v => v.ToJson()).ToArray() };
            }

            var pullResponse = await this.Workspace.Database.Pull(pullService, args);
            var syncRequest = this.Workspace.Database.Diff(pullResponse);

            if (syncRequest.Objects.Length > 0)
            {
                await this.Load(syncRequest);
            }

            return new LoadResult(this, pullResponse);
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
            var pushResponse = await this.Workspace.Database.Push(saveRequest);
            if (!pushResponse.HasErrors)
            {
                this.PushResponse(pushResponse);

                var objects = saveRequest.Objects.Select(v => v.I).ToArray();
                if (pushResponse.NewObjects != null)
                {
                    objects = objects.Union(pushResponse.NewObjects.Select(v => v.I)).ToArray();
                }

                var syncRequests = new SyncRequest
                {
                    Objects = objects,
                };

                await this.Load(syncRequests);

                this.Reset();
            }

            return new SaveResult(pushResponse);
        }

        internal IEnumerable<IObject> GetAssociation(IObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.Workspace.Database.Get((IComposite)associationType.ObjectType).Select(v => this.Instantiate(v.Id)))
            {
                if (association.Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = (IObject)((DatabaseStrategy)association.Strategy).GetAssociationForDatabase(roleType);
                        if (role != null && role.WorkspaceId == @object.WorkspaceId)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = (IObject[])((DatabaseStrategy)association.Strategy).GetAssociationForDatabase(roleType);
                        if (roles != null && roles.Contains(@object))
                        {
                            yield return association;
                        }
                    }
                }
            }
        }

        internal IObject GetForAssociation(long id)
        {
            var workspaceId = this.Workspace.ToWorkspaceId(id);
            this.strategyByWorkspaceId.TryGetValue(workspaceId, out var strategy);
            return strategy?.Object;
        }

        internal PushRequest PushRequest() => new PushRequest
        {
            NewObjects = this.newDatabaseStrategies?.Select(v => v.SaveNew()).ToArray(),
            Objects = this.existingDatabaseStrategies.Where(v => v.HasDatabaseChanges).Select(v => v.SaveExisting()).ToArray(),
        };

        internal void PushResponse(PushResponse pushResponse)
        {
            if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
            {
                foreach (var pushResponseNewObject in pushResponse.NewObjects)
                {
                    var workspaceId = long.Parse(pushResponseNewObject.WI);
                    var databaseId = long.Parse(pushResponseNewObject.I);

                    this.Workspace.RegisterWorkspaceIdForDatabaseObject(databaseId, workspaceId);
                    
                    var strategy = (DatabaseStrategy)this.strategyByWorkspaceId[workspaceId];
                    this.newDatabaseStrategies.Remove(strategy);
                    this.existingDatabaseStrategies.Add(strategy);

                    var databaseObject = this.Workspace.Database.PushResponse(databaseId, strategy.Class);
                    strategy.PushResponse(databaseObject);
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
            var workspaceId = this.Workspace.NextWorkspaceId();
            var strategy = new DatabaseStrategy(this, @class, workspaceId);
            this.newDatabaseStrategies ??= new HashSet<DatabaseStrategy>();
            this.newDatabaseStrategies.Add(strategy);
            this.strategyByWorkspaceId.Add(strategy.WorkspaceId, strategy);
            return strategy.Object;
        }

        private IObject CreateWorkspaceObject(IClass @class)
        {
            var workspaceId = this.Workspace.NextWorkspaceId();
            this.Workspace.RegisterWorkspaceIdForWorkspaceObject(@class, workspaceId);
            var strategy = new WorkspaceStrategy(this, @class, workspaceId);
            this.strategyByWorkspaceId[strategy.WorkspaceId] = strategy;
            return strategy.Object;
        }

        private IObject CreateSessionObject(IClass @class)
        {
            var workspaceId = this.Workspace.NextWorkspaceId();
            var strategy = new SessionStrategy(this, @class, workspaceId);
            this.strategyByWorkspaceId[strategy.WorkspaceId] = strategy;
            return strategy.Object;
        }

        private async Task Load(SyncRequest syncRequest)
        {
            var syncResponse = await this.Workspace.Database.Sync(syncRequest);
            var securityRequest = this.Workspace.Database.SyncResponse(syncResponse);

            if (securityRequest != null)
            {
                var securityResponse = await this.Workspace.Database.Security(securityRequest);
                securityRequest = this.Workspace.Database.SecurityResponse(securityResponse);

                if (securityRequest != null)
                {
                    securityResponse = await this.Workspace.Database.Security(securityRequest);
                    this.Workspace.Database.SecurityResponse(securityResponse);
                }
            }
        }

        internal IObject Object(long workspaceId)
        {
            var strategyProxy = this.strategyByWorkspaceId[workspaceId];
            var @object = this.Workspace.ObjectFactory.Create(strategyProxy);
            return @object;
        }
    }
}
