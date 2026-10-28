// <copyright file="RemoteSession.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

        internal async Task<IPullResult> OnPull(PullRequest pullRequest, PullResponse pullResponse)
        {
            if (pullResponse == null)
            {
                throw ResponseContext.Invalid("Pull returned no response");
            }

            if (pullResponse.HasErrors)
            {
                return new PullResult(this, pullResponse, this.DatabaseConnection.UnitConvert);
            }

            var database = this.DatabaseConnection;
            var context = database.OnPullResponse(pullRequest, pullResponse);
            if (context.NeedsSync)
            {
                var syncResponse = await database.Sync(context.SyncRequest);
                if (syncResponse == null)
                {
                    throw ResponseContext.Invalid("Sync returned no response");
                }

                if (syncResponse.HasErrors)
                {
                    return new PullResult(this, new PullResponse
                    {
                        _e = syncResponse._e, _a = syncResponse._a, _v = syncResponse._v,
                        _m = syncResponse._m, _d = syncResponse._d,
                    }, database.UnitConvert);
                }

                context.OnSync(syncResponse);
            }

            var pullResult = new PullResult(this, context.Result, database.UnitConvert);
            var created = new List<Strategy>();
            foreach (var record in context.Records.Values)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(record.Id, out var existing))
                {
                    if (existing.Class != record.Class)
                    {
                        throw ResponseContext.Invalid($"Sync changed the class of session object {record.Id} from '{existing.Class.Tag}' to '{record.Class.Tag}'.");
                    }
                }
                else
                {
                    created.Add(new Strategy(this, record));
                }
            }

            database.Publish(context);
            foreach (var record in context.Records.Values)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(record.Id, out var strategy))
                {
                    strategy.DatabaseOriginState.OnPulled(pullResult, record);
                }
            }

            foreach (var strategy in created)
            {
                this.AddStrategy(strategy);
            }

            foreach (var strategy in created)
            {
                this.ChangeSetTracker.OnInstantiated(strategy);
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

        public override async Task<IPullResult> PullAsync(params Pull[] pulls)
        {
            foreach (var pull in pulls)
            {
                if (pull.ObjectId < 0 || pull.Object?.Id < 0)
                {
                    throw new ArgumentException($"Id is not in the database");
                }
            }

            var pullRequest = new PullRequest
            {
                l = pulls.Select(v => v.ToJson(this.DatabaseConnection.UnitConvert)).ToArray(),
                d = this.dependencies?.Where(v => v.PropertyType.ObjectType.IsComposite).Select(v => new PullDependency
                {
                    o = v.ObjectType.Tag,
                    r = (v.PropertyType as IRoleType)?.RelationType.Tag,
                    a = (v.PropertyType as IAssociationType)?.RelationType.Tag,
                }).ToArray(),
            };

            var pullResponse = await this.DatabaseConnection.Pull(pullRequest);
            return await this.OnPull(pullRequest, pullResponse);
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
