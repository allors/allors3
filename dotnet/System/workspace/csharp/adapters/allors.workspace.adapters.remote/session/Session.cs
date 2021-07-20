// <copyright file="RemoteSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Data;
    using Meta;
    using Protocol.Json;
    using InvokeOptions = InvokeOptions;

    public class Session : Adapters.Session
    {
        internal Session(Workspace workspace, ISessionServices sessionServices) : base(workspace, sessionServices)
        {
            this.Services.OnInit(this);
            this.DatabaseConnection = workspace.DatabaseConnection;
        }

        private DatabaseConnection DatabaseConnection { get; }

        public new Workspace Workspace => (Workspace)base.Workspace;

        public override async Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) => await this.Invoke(new[] { method }, options);

        public override async Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                l = methods.Select(v => new Invocation
                {
                    i = v.Object.Id,
                    v = ((Strategy)v.Object.Strategy).DatabaseOriginState.Version,
                    m = v.MethodType.Tag
                }).ToArray(),
                o = options != null
                    ? new Allors.Protocol.Json.Api.Invoke.InvokeOptions
                    {
                        c = options.ContinueOnError,
                        i = options.Isolated
                    }
                    : null
            };

            var invokeResponse = await this.Workspace.DatabaseConnection.Invoke(invokeRequest);
            return new InvokeResult(this, invokeResponse);
        }

        public override async Task<IPullResult> Pull(params Pull[] pull)
        {
            var pullRequest = new PullRequest { l = pull.Select(v => v.ToJson(this.DatabaseConnection.UnitConvert)).ToArray() };

            var pullResponse = await this.Workspace.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPullResult> Call(Procedure procedure, params Pull[] pull)
        {
            var pullRequest = new PullRequest
            {
                p = procedure.ToJson(this.DatabaseConnection.UnitConvert),
                l = pull.Select(v => v.ToJson(this.DatabaseConnection.UnitConvert)).ToArray()
            };

            var pullResponse = await this.Workspace.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPushResult> Push()
        {
            var pushRequest = new PushRequest
            {
                n = this.PushToDatabaseTracker.Created?.Select(v => ((Strategy)v).DatabasePushNew()).ToArray(),
                o = this.PushToDatabaseTracker.Changed?.Select(v => ((Strategy)v.Strategy).DatabasePushExisting()).ToArray()
            };
            var pushResponse = await this.Workspace.DatabaseConnection.Push(pushRequest);

            if (pushResponse.HasErrors)
            {
                return new PushResult(this, pushResponse);
            }

            if (pushResponse.n != null)
            {
                foreach (var pushResponseNewObject in pushResponse.n)
                {
                    var workspaceId = pushResponseNewObject.w;
                    var databaseId = pushResponseNewObject.d;

                    this.OnDatabasePushResponseNew(workspaceId, databaseId);
                }
            }

            this.PushToDatabaseTracker.Created = null;
            this.PushToDatabaseTracker.Changed = null;

            if (pushRequest.o != null)
            {
                foreach (var id in pushRequest.o.Select(v => v.d))
                {
                    var strategy = this.GetStrategy(id);
                    strategy.OnDatabasePushed();
                }
            }

            return new PushResult(this, pushResponse);
        }

        public override T Create<T>(IClass @class)
        {
            var workspaceId = base.Workspace.DatabaseConnection.NextId();
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

        private void InstantiateDatabaseStrategy(long id)
        {
            var databaseRecord = (DatabaseRecord)base.Workspace.DatabaseConnection.GetRecord(id);
            var strategy = new Strategy(this, databaseRecord);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);
        }

        protected override Adapters.Strategy InstantiateWorkspaceStrategy(long id)
        {
            if (!base.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(id, out var @class))
            {
                return null;
            }

            var strategy = new Strategy(this, @class, id);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

            return strategy;
        }

        private async Task<IPullResult> OnPull(PullResponse pullResponse)
        {
            var syncRequest = this.Workspace.DatabaseConnection.OnPullResponse(pullResponse);
            if (syncRequest.o.Length > 0)
            {
                var database = (DatabaseConnection)base.Workspace.DatabaseConnection;
                var syncResponse = await database.Sync(syncRequest);
                var securityRequest = database.OnSyncResponse(syncResponse);

                foreach (var databaseObject in syncResponse.o)
                {
                    if (!this.StrategyByWorkspaceId.TryGetValue(databaseObject.i, out var strategy))
                    {
                        this.InstantiateDatabaseStrategy(databaseObject.i);
                    }
                    else
                    {
                        strategy.DatabaseOriginState.OnPulled();
                    }
                }

                if (securityRequest != null)
                {
                    var securityResponse = await database.Security(securityRequest);
                    securityRequest = database.SecurityResponse(securityResponse);

                    if (securityRequest != null)
                    {
                        securityResponse = await database.Security(securityRequest);
                        database.SecurityResponse(securityResponse);
                    }
                }
            }

            foreach (var v in pullResponse.p)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(v.i, out var strategy))
                {
                    strategy.DatabaseOriginState.OnPulled();
                }
                else
                {
                    this.InstantiateDatabaseStrategy(v.i);
                }
            }

            return new PullResult(this, pullResponse);
        }
    }
}
