// <copyright file="LocalSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Threading.Tasks;
    using Meta;

    public class Session : Adapters.Session
    {
        internal Session(Adapters.Workspace workspace, ISessionServices sessionServices) : base(workspace, sessionServices) => this.Services.OnInit(this);

        public new Workspace Workspace => (Workspace)base.Workspace;

        public override Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null) =>
            this.Invoke(new[] { method }, options);

        public override Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var result = new Invoke(this, this.Workspace);
            result.Execute(methods, options);
            return Task.FromResult<IInvokeResult>(result);
        }

        public override Task<IPullResult> Pull(params Data.Pull[] pulls)
        {
            var result = new Pull(this, this.Workspace);
            result.Execute(pulls);

            this.OnPulled(result);

            return Task.FromResult<IPullResult>(result);
        }

        public override Task<IPullResult> Pull(Data.Procedure procedure, params Data.Pull[] pulls)
        {
            var result = new Pull(this, this.Workspace);

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
            var workspaceId = this.Workspace.WorkspaceIdGenerator.Next();
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
            var databaseRecord = this.Workspace.DatabaseConnection.GetRecord(id);
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
            var syncObjects = this.Workspace.DatabaseConnection.ObjectsToSync(pull);
            this.Workspace.DatabaseConnection.Sync(syncObjects, pull.AccessControlLists);

            foreach (var databaseObject in pull.DatabaseObjects)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(databaseObject.Id, out var strategy))
                {
                    ((DatabaseOriginState)strategy.DatabaseOriginState).OnPulled();
                }
                else
                {
                    this.InstantiateDatabaseStrategy(databaseObject.Id);
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

                    this.OnDatabasePushResponseNew(workspaceId, databaseId);
                }
            }

            this.PushToDatabaseTracker.Created = null;

            foreach (var @object in push.Objects)
            {
                var strategy = this.GetStrategy(@object.Id);
                this.OnDatabasePushResponse(strategy);
            }

            return push;
        }
    }
}
