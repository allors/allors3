// <copyright file="LocalSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Meta;
    using Numbers;

    public class Session : Adapters.Session
    {
        internal Session(Workspace workspace, ISessionLifecycle sessionLifecycle) : base(workspace, sessionLifecycle)
        {
            this.Lifecycle.OnInit(this);
        }

        public override Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) =>
            this.Invoke(new[] { method }, options);

        public override Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var result = new Invoke(this, (Workspace)this.Workspace);
            result.Execute(methods, options);
            return Task.FromResult<IInvokeResult>(result);
        }

        public override Task<IPullResult> Pull(params Data.Pull[] pulls)
        {
            var result = new Pull(this, (Workspace)this.Workspace);
            result.Execute(pulls);

            this.OnPulled(result);

            return Task.FromResult<IPullResult>(result);
        }

        public override Task<IPullResult> Pull(Data.Procedure procedure, params Data.Pull[] pulls)
        {
            var result = new Pull(this, (Workspace)this.Workspace);

            result.Execute(procedure);
            result.Execute(pulls);

            this.OnPulled(result);

            return Task.FromResult<IPullResult>(result);
        }

        public override Task<IPushResult> Push()
        {
            var result = new Push(this);

            this.PushToDatabase(result);
            if (!result.HasErrors)
            {
                this.PushToWorkspace(result);
            }

            return Task.FromResult<IPushResult>(result);
        }

        public override T Create<T>(IClass @class)
        {
            var workspaceId = this.Workspace.Database.Configuration.WorkspaceIdGenerator.Next();
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
            var databaseRecord = this.Workspace.Database.GetRecord(id);
            var strategy = new Strategy(this, (DatabaseRecord)databaseRecord);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

            return strategy;
        }

        protected override Adapters.Strategy InstantiateWorkspaceStrategy(long id)
        {
            if (!this.Workspace.WorkspaceClassByWorkspaceId.TryGetValue(id, out var @class))
            {
                return null;
            }

            var strategy = new Strategy(this, @class, id);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);

            return strategy;
        }

        private void OnPulled(Pull pull)
        {
            var syncObjects = ((DatabaseConnection)this.Workspace.Database).ObjectsToSync(pull);

            ((DatabaseConnection)this.Workspace.Database).Sync(syncObjects, pull.AccessControlLists);

            foreach (var databaseObject in pull.DatabaseObjects)
            {
                if (!this.StrategyByWorkspaceId.TryGetValue(databaseObject.Id, out var strategy))
                {
                    this.InstantiateDatabaseStrategy(databaseObject.Id);
                }
                else
                {
                    strategy.DatabaseOriginState.OnPulled();
                }
            }
        }

        private IPushResult PushToDatabase(Push push)
        {
            push.Execute(this.PushToDatabaseTracker);

            if (push.HasErrors)
            {
                return push;
            }

            this.PushToDatabaseTracker.Changed = null;

            if (push.ObjectByNewId?.Count > 0)
            {
                foreach (var kvp in push.ObjectByNewId)
                {
                    var workspaceId = kvp.Key;
                    var databaseId = kvp.Value.Id;

                    var strategy = this.StrategyByWorkspaceId[workspaceId];

                    _ = this.PushToDatabaseTracker.Created.Remove(strategy);

                    this.RemoveStrategy(strategy);
                    var databaseRecord = ((DatabaseConnection)this.Workspace.Database).OnPushed(databaseId, strategy.Class);
                    strategy.DatabasePushResponse(databaseRecord);
                    this.AddStrategy(strategy);
                }
            }

            if (this.PushToDatabaseTracker.Created?.Count > 0)
            {
                throw new Exception("Not all new objects received ids");
            }

            this.PushToDatabaseTracker.Created = null;

            ((DatabaseConnection)this.Workspace.Database).Sync(push.Objects, push.AccessControlLists);

            foreach (var @object in push.Objects)
            {
                if (!this.StrategyByWorkspaceId.ContainsKey(@object.Id))
                {
                    _ = this.InstantiateDatabaseStrategy(@object.Id);
                }

                var strategy = this.GetStrategy(@object.Id);
                strategy.DatabaseOriginState.Reset();
            }

            return push;
        }
    }
}
