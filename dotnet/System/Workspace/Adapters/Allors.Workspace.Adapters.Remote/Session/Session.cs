// <copyright file="RemoteSession.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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

        private readonly SemaphoreSlim operationGate = new SemaphoreSlim(1, 1);

        private List<EventArgs> pendingChangeEvents;

        private DatabaseConnection DatabaseConnection { get; }

        public new Workspace Workspace => (Workspace)base.Workspace;

        public override void OnChanged(EventArgs e)
        {
            if (this.pendingChangeEvents != null) this.pendingChangeEvents.Add(e);
            else base.OnChanged(e);
        }

        public override T Create<T>(IClass @class)
        {
            var workspaceId = base.Workspace.DatabaseConnection.NextId();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);
            this.PushToDatabaseTracker.OnCreated(strategy);
            this.ChangeSetTracker.OnCreated(strategy);
            return (T)strategy.Object;
        }

        internal async Task<IPullResult> OnPull(PullResponse pullResponse)
        {
            var pullResult = new PullResult(this, pullResponse, this.DatabaseConnection.UnitConvert);
            if (pullResult.HasErrors) return pullResult;

            var context = this.DatabaseConnection.OnPullResponse(pullResult.Selection);
            var request = context.SyncRequest;
            if (request.o.Length > 0) context.OnSync(await this.DatabaseConnection.Sync(request));

            var strategies = context.Records.ToDictionary(entry => entry.Key, entry =>
            {
                if (this.StrategyByWorkspaceId.TryGetValue(entry.Key, out var existing))
                {
                    if (existing.Class != entry.Value.Class) throw ResponseContext.Invalid($"Sync changed the class of object {entry.Key} from '{existing.Class.Tag}' to '{entry.Value.Class.Tag}'.");
                    return existing;
                }

                return new Strategy(this, entry.Value);
            });
            pullResult.Prepare(id => id == 0 ? null : strategies[id].Object);
            this.DatabaseConnection.Publish(context);
            var changeEvents = new List<EventArgs>();
            this.pendingChangeEvents = changeEvents;
            try
            {
                foreach (var entry in strategies)
                {
                    if (this.StrategyByWorkspaceId.ContainsKey(entry.Key)) entry.Value.DatabaseOriginState.OnPulled(context.Records[entry.Key]);
                    else
                    {
                        this.AddStrategy(entry.Value);
                        this.ChangeSetTracker.OnInstantiated(entry.Value);
                    }
                }
            }
            finally { this.pendingChangeEvents = null; }

            foreach (var e in changeEvents) this.OnChanged(e);

            return pullResult;
        }

        private async Task<T> InOrder<T>(Func<Task<T>> operation)
        {
            await this.operationGate.WaitAsync();
            try { return await operation(); }
            finally { this.operationGate.Release(); }
        }

        public override async Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options = null) => await this.InvokeAsync(new[] { method }, options);

        public override Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options = null) => this.InOrder(async () =>
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
            return (IInvokeResult)new InvokeResult(this, invokeResponse);
        });

        public override Task<IPullResult> PullAsync(params Pull[] pulls) => this.InOrder(async () =>
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
        });

        public override Task<IPushResult> PushAsync() => this.InOrder(async () =>
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
                return (IPushResult)new PushResult(this, pushResponse);
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

            return (IPushResult)new PushResult(this, pushResponse);
        });
    }
}
