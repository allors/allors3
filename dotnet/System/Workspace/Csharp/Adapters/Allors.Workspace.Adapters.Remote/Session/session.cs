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
    using Data;
    using Meta;
    using Protocol.Json;
    using InvokeOptions = Allors.Workspace.InvokeOptions;

    public class Session : Adapters.Session
    {
        internal Session(Workspace workspace, ISessionServices sessionServices) : base(workspace, sessionServices)
        {
            this.Services.OnInit(this);
            this.DatabaseConnection = workspace.DatabaseConnection;
        }

        private DatabaseConnection DatabaseConnection { get; }

        public new Workspace Workspace => (Workspace)base.Workspace;

        public override T Create<T>(IClass @class)
        {
            var workspaceId = base.Workspace.DatabaseConnection.NextId();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);
            this.PushToDatabaseTracker.OnCreated(strategy);
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

        internal async Task<IPullResult> OnPull(PullResponse pullResponse)
        {
            var pullResult = new PullResult(this, pullResponse);

            if (pullResult.HasErrors)
            {
                return pullResult;
            }

            var syncRequest = this.Workspace.DatabaseConnection.OnPullResponse(pullResponse);
            if (syncRequest.o.Length > 0)
            {
                var database = (DatabaseConnection)base.Workspace.DatabaseConnection;
                var syncResponse = await database.Sync(syncRequest);
                var accessRequest = database.OnSyncResponse(syncResponse);

                if (accessRequest != null)
                {
                    var accessResponse = await database.Access(accessRequest);
                    var permissionRequest = database.AccessResponse(accessResponse);
                    if (permissionRequest != null)
                    {
                        var permissionResponse = await database.Permission(permissionRequest);
                        database.PermissionResponse(permissionResponse);
                    }
                }
            }

            foreach (var v in pullResponse.p)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(v.i, out var strategy))
                {
                    strategy.DatabaseOriginState.OnPulled(pullResult);
                }
                else
                {
                    this.InstantiateDatabaseStrategy(v.i);
                }
            }

            return pullResult;
        }

        public override async Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options = null) => await this.InvokeAsync(new[] { method }, options);

        public override async Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options = null)
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

            var invokeResponse = await this.DatabaseConnection.Invoke(invokeRequest);
            return new InvokeResult(this, invokeResponse);
        }

        public override async Task<IPullResult> CallAsync(object args, string name)
        {
            var pullResponse = await this.DatabaseConnection.Pull(args, name);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPullResult> PullAsync(params Pull[] pulls)
        {
            foreach (var pull in pulls)
            {
                if (pull.ObjectId < 0 || pull.Object?.Id < 0)
                {
                    throw new ArgumentException($"Id is not in the database");
                }
            }

            var pullRequest = new PullRequest { l = pulls.Select(v => v.ToJson(this.DatabaseConnection.UnitConvert)).ToArray() };

            var pullResponse = await this.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPullResult> CallAsync(Procedure procedure, params Pull[] pull)
        {
            var pullRequest = new PullRequest
            {
                p = procedure.ToJson(this.DatabaseConnection.UnitConvert),
                l = pull.Select(v => v.ToJson(this.DatabaseConnection.UnitConvert)).ToArray()
            };

            var pullResponse = await this.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullResponse);
        }

        public override async Task<IPushResult> PushAsync()
        {
            var databaseTracker = this.PushToDatabaseTracker;

            var pushRequest = new PushRequest
            {
                n = databaseTracker.Created?.Select(v => ((DatabaseOriginState)v.DatabaseOriginState).PushNew()).ToArray(),
                o = databaseTracker.Changed?.Select(v => ((DatabaseOriginState)v.Strategy.DatabaseOriginState).PushExisting()).ToArray()
            };
            var pushResponse = await this.DatabaseConnection.Push(pushRequest);

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

            databaseTracker.Created = null;
            databaseTracker.Changed = null;

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
    }
}
