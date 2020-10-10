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
    using Protocol.Database.Invoke;
    using Protocol.Database.Pull;
    using Protocol.Database.Push;
    using Protocol.Database.Sync;
    using Allors.Workspace.Data;
    using Meta;
    using Result = Allors.Workspace.Result;

    public class Session : ISession
    {
        private static long idCounter = 0;

        private readonly Remote database;

        private readonly Dictionary<long, Strategy> strategyById = new Dictionary<long, Strategy>();
        private readonly Dictionary<long, Strategy> newStrategyById = new Dictionary<long, Strategy>();

        public Session(Workspace workspace, ISessionStateLifecycle stateLifecycle)
        {
            this.Workspace = workspace;
            this.StateLifecycle = stateLifecycle;
            this.DatabaseOrigin = this.Workspace.DatabaseOrigin;
            this.database = this.Workspace.Database;
            this.Workspace.RegisterContext(this);

            this.StateLifecycle.OnInit(this);
        }

        ~Session() => this.Workspace.UnregisterContext(this);

        IWorkspace ISession.Workspace => this.Workspace;

        public Workspace Workspace { get; }

        internal bool HasChanges => this.newStrategyById.Count > 0 || this.strategyById.Values.Any(v => v.HasChanges);

        public DatabaseOrigin DatabaseOrigin { get; }

        public ISessionStateLifecycle StateLifecycle { get; }

        public IObject Instantiate(long id)
        {
            if (!this.strategyById.TryGetValue(id, out var strategy))
            {
                if (!this.newStrategyById.TryGetValue(id, out strategy))
                {
                    var workspaceObject = this.DatabaseOrigin.Get(id);
                    strategy = new Strategy(this, workspaceObject);
                    this.strategyById[workspaceObject.Id] = strategy;
                }
            }

            return strategy.Object;
        }

        public IEnumerable<IObject> GetAssociation(IObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            var associations = this.DatabaseOrigin.Get((IComposite)associationType.ObjectType).Select(v => this.Instantiate(v.Id));
            foreach (var association in associations)
            {
                if (association.Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = (IObject)((Strategy)association.Strategy).GetForAssociation(roleType);
                        if (role != null && role.Id == @object.Id)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = (IObject[])((Strategy)association.Strategy).GetForAssociation(roleType);
                        if (roles != null && roles.Contains(@object))
                        {
                            yield return association;
                        }
                    }
                }
            }
        }

        public Task<InvokeResponse> Invoke(Method method, InvokeOptions options = null) => this.Invoke(new[] { method }, options);

        public Task<InvokeResponse> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                I = methods.Select(v => new Invocation
                {
                    I = v.Object.Id.ToString(),
                    V = v.Object.Strategy.Version.ToString(),
                    M = v.MethodType.IdAsString,
                }).ToArray(),
                O = options,
            };

            return this.database.Invoke(invokeRequest);
        }

        public Task<InvokeResponse> Invoke(string service, object args) => this.database.Invoke(service, args);

        public async Task<Result> Load(params Pull[] pulls)
        {
            var pullRequest = new PullRequest { P = pulls.Select(v => v.ToJson()).ToArray() };
            var pullResponse = await this.database.Pull(pullRequest);
            var syncRequest = this.DatabaseOrigin.Diff(pullResponse);
            if (syncRequest.Objects.Length > 0)
            {
                await this.Load(syncRequest);
            }

            var result = new Result(this, pullResponse);
            return result;
        }

        public async Task<Result> Load(object args, string pullService = null)
        {
            if (args is Pull pull)
            {
                args = new PullRequest { P = new[] { pull.ToJson() } };
            }

            if (args is IEnumerable<Pull> pulls)
            {
                args = new PullRequest { P = pulls.Select(v => v.ToJson()).ToArray() };
            }

            var pullResponse = await this.database.Pull(pullService, args);
            var syncRequest = this.DatabaseOrigin.Diff(pullResponse);

            if (syncRequest.Objects.Length > 0)
            {
                await this.Load(syncRequest);
            }

            var result = new Result(this, pullResponse);
            return result;
        }

        public async Task<PushResponse> Save()
        {
            var saveRequest = this.PushRequest();
            var pushResponse = await this.database.Push(saveRequest);
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

            return pushResponse;
        }

        public void Reset()
        {
            foreach (var newSessionObject in this.newStrategyById.Values)
            {
                newSessionObject.Reset();
            }

            foreach (var sessionObject in this.strategyById.Values)
            {
                sessionObject.Reset();
            }
        }

        public IObject Create(IClass @class)
        {
            var strategy = new Strategy(this, @class, --idCounter);
            this.newStrategyById[strategy.Id] = strategy;
            return strategy.Object;
        }

        internal void Refresh()
        {
            foreach (var sessionObject in this.strategyById.Values)
            {
                sessionObject.Refresh();
            }
        }

        internal PushRequest PushRequest() =>
            new PushRequest
            {
                NewObjects = this.newStrategyById.Select(v => v.Value.SaveNew()).ToArray(),
                Objects = this.strategyById.Select(v => v.Value.Save()).Where(v => v != null).ToArray(),
            };

        internal void PushResponse(PushResponse pushResponse)
        {
            if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
            {
                foreach (var pushResponseNewObject in pushResponse.NewObjects)
                {
                    var newId = long.Parse(pushResponseNewObject.NI);
                    var id = long.Parse(pushResponseNewObject.I);

                    var sessionObject = this.newStrategyById[newId];
                    sessionObject.PushResponse(id);

                    this.newStrategyById.Remove(newId);
                    this.strategyById[id] = sessionObject;
                }
            }

            if (this.newStrategyById != null && this.newStrategyById.Count != 0)
            {
                throw new Exception("Not all new objects received ids");
            }
        }

        internal IObject GetForAssociation(long id)
        {
            if (!this.strategyById.TryGetValue(id, out var sessionObject))
            {
                this.newStrategyById.TryGetValue(id, out sessionObject);
            }

            return sessionObject?.Object;
        }

        private async Task Load(SyncRequest syncRequest)
        {
            var syncResponse = await this.database.Sync(syncRequest);
            var securityRequest = this.DatabaseOrigin.Sync(syncResponse);

            if (securityRequest != null)
            {
                var securityResponse = await this.database.Security(securityRequest);
                securityRequest = this.DatabaseOrigin.Security(securityResponse);

                if (securityRequest != null)
                {
                    securityResponse = await this.database.Security(securityRequest);
                    this.DatabaseOrigin.Security(securityResponse);
                }
            }
        }
    }
}
