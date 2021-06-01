// <copyright file="RemoteSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
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

    public class Session : Adapters.Session
    {
        internal Session(Adapters.Workspace workspace, ISessionServices sessionServices) : base(workspace, sessionServices) => this.Services.OnInit(this);

        public new Workspace Workspace => (Workspace)base.Workspace;

        public override async Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) => await this.Invoke(new[] { method }, options);

        public override async Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                List = methods.Select(v => new Invocation
                {
                    Id = v.Object.Id,
                    Version = ((Strategy)v.Object.Strategy).DatabaseOriginState.Version,
                    Method = v.MethodType.Tag
                }).ToArray(),
                Options = options != null
                    ? new Allors.Protocol.Json.Api.Invoke.InvokeOptions
                    {
                        ContinueOnError = options.ContinueOnError,
                        Isolated = options.Isolated
                    }
                    : null
            };

            var invokeResponse = await this.Workspace.DatabaseConnection.Invoke(invokeRequest);
            return new InvokeResult(this, invokeResponse);
        }

        public override async Task<IPullResult> Pull(params Pull[] pulls)
        {
            var pullRequest = new PullRequest { List = pulls.Select(v => v.ToJson()).ToArray() };

            var pullResponse = await this.Workspace.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPullResult> Pull(Procedure procedure, params Pull[] pulls)
        {
            var pullRequest = new PullRequest
            {
                Procedure = procedure.ToJson(),
                List = pulls.Select(v => v.ToJson()).ToArray()
            };

            var pullResponse = await this.Workspace.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPushResult> Push()
        {
            var result = await this.PushToDatabase();

            if (!result.HasErrors)
            {
                _ = this.PushToWorkspace(result);
            }

            return result;
        }

        private async Task<IPullResult> OnPull(PullResponse pullResponse)
        {
            var syncRequest = this.Workspace.DatabaseConnection.Diff(pullResponse);
            if (syncRequest.Objects.Length > 0)
            {
                await this.Sync(syncRequest);
            }

            foreach (var v in pullResponse.Pool)
            {
                if (!this.StrategyByWorkspaceId.ContainsKey(v.Id))
                {
                    _ = this.InstantiateDatabaseStrategy(v.Id);
                }
            }

            return new PullResult(this, pullResponse);
        }

        private async Task Sync(SyncRequest syncRequest)
        {
            var database = (DatabaseConnection)base.Workspace.DatabaseConnection;
            var syncResponse = await database.Sync(syncRequest);
            var securityRequest = database.SyncResponse(syncResponse);

            foreach (var databaseObject in syncResponse.Objects)
            {
                if (!this.StrategyByWorkspaceId.TryGetValue(databaseObject.Id, out var strategy))
                {
                    _ = this.InstantiateDatabaseStrategy(databaseObject.Id);
                }
                else
                {
                    ((DatabaseOriginState)strategy.DatabaseOriginState).OnPulled();
                }
            }

            if (securityRequest != null)
            {
                var securityResponse = await database.Security(securityRequest);
                securityRequest = database.SecurityResponse(securityResponse);

                if (securityRequest != null)
                {
                    securityResponse = await database.Security(securityRequest);
                    _ = database.SecurityResponse(securityResponse);
                }
            }
        }

        public override T Create<T>(IClass @class)
        {
            var workspaceId = base.Workspace.WorkspaceIdGenerator.Next();
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

        public override Adapters.Strategy InstantiateDatabaseStrategy(long id)
        {
            var databaseRecord = (DatabaseRecord)base.Workspace.DatabaseConnection.GetRecord(id);
            var strategy = new Strategy(this, databaseRecord);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

            return strategy;
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

        private PushRequest PushRequest() => new PushRequest
        {
            NewObjects = this.PushToDatabaseTracker.Created?.Select(v => ((Strategy)v).DatabasePushNew()).ToArray(),
            Objects = this.PushToDatabaseTracker.Changed?.Select(v => ((Strategy)v.Strategy).DatabasePushExisting()).ToArray()
        };

        private async Task<PushResult> PushToDatabase()
        {
            var pushRequest = this.PushRequest();
            var pushResponse = await this.Workspace.DatabaseConnection.Push(pushRequest);

            if (!pushResponse.HasErrors)
            {
                if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
                {
                    foreach (var pushResponseNewObject in pushResponse.NewObjects)
                    {
                        var workspaceId = pushResponseNewObject.WorkspaceId;
                        var databaseId = pushResponseNewObject.DatabaseId;

                        var strategy = this.StrategyByWorkspaceId[workspaceId];

                        _ = this.PushToDatabaseTracker.Created.Remove(strategy);

                        this.RemoveStrategy(strategy);
                        var databaseRecord = this.Workspace.DatabaseConnection.OnPushed(databaseId, strategy.Class);
                        strategy.DatabasePushResponse(databaseRecord);
                        this.AddStrategy(strategy);
                    }
                }
                
                var objects = pushRequest.Objects?.Select(v => v.DatabaseId).ToArray() ?? Array.Empty<long>();
                if (pushResponse.NewObjects != null)
                {
                    objects = objects.Union(pushResponse.NewObjects.Select(v => v.DatabaseId)).ToArray();
                }

                var syncRequests = new SyncRequest { Objects = objects };
                await this.Sync(syncRequests);
                
                foreach (var id in objects)
                {
                    if (!this.StrategyByWorkspaceId.ContainsKey(id))
                    {
                        _ = this.InstantiateDatabaseStrategy(id);
                    }

                    var strategy = this.GetStrategy(id);
                    ((DatabaseOriginState)strategy.DatabaseOriginState).Reset();
                }
            }

            var result = new PushResult(this, pushResponse);
            return result;
        }
    }
}
