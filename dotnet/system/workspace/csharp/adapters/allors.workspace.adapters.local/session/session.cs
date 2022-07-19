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

        public override T Create<T>(IClass @class)
        {
            var workspaceId = this.Workspace.DatabaseConnection.NextId();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);
            this.PushToDatabaseTracker.OnCreated(strategy);
            this.ChangeSetTracker.OnCreated(strategy);
            return (T)strategy.Object;
        }

        private void InstantiateDatabaseStrategy(long id)
        {
            var databaseRecord = this.Workspace.DatabaseConnection.GetRecord(id);
            var strategy = new Strategy(this, (DatabaseRecord)databaseRecord);
            this.AddStrategy(strategy);

            this.ChangeSetTracker.OnInstantiated(strategy);
        }

        public override Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options = null) =>
               this.InvokeAsync(new[] { method }, options);

        public override Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options = null)
        {
            var result = new Invoke(this);
            result.Execute(methods, options);
            return Task.FromResult<IInvokeResult>(result);
        }

        public override Task<IPullResult> CallAsync(Data.Procedure procedure, params Data.Pull[] pull)
        {
            var result = new Pull(this);

            result.Execute(procedure);
            result.Execute(pull);
            result.AddDependencies();

            this.OnPulled(result);

            return Task.FromResult<IPullResult>(result);
        }

        public override Task<IPullResult> CallAsync(object args, string name)
        {
            var result = new Pull(this);

            result.Execute(args, name);

            return Task.FromResult<IPullResult>(result);
        }

        public override Task<IPullResult> PullAsync(params Data.Pull[] pulls)
        {
            foreach (var pull in pulls)
            {
                if (pull.ObjectId < 0 || pull.Object?.Id < 0)
                {
                    throw new ArgumentException($"Id is not in the database");
                }
            }

            var result = new Pull(this);
            result.Execute(pulls);

            this.OnPulled(result);

            return Task.FromResult<IPullResult>(result);
        }

        public override Task<IPushResult> PushAsync()
        {
            var databaseTracker = this.PushToDatabaseTracker;

            var result = new Push(this);

            result.Execute(databaseTracker);

            if (result.HasErrors)
            {
                return Task.FromResult<IPushResult>(result);
            }

            databaseTracker.Changed = null;

            if (result.ObjectByNewId?.Count > 0)
            {
                foreach (var kvp in result.ObjectByNewId)
                {
                    var workspaceId = kvp.Key;
                    var databaseId = kvp.Value.Id;

                    this.OnDatabasePushResponseNew(workspaceId, databaseId);
                }
            }

            databaseTracker.Created = null;

            foreach (var @object in result.Objects)
            {
                var strategy = this.GetStrategy(@object.Id);
                strategy.OnDatabasePushed();
            }

            return Task.FromResult<IPushResult>(result);
        }

        internal void OnPulled(Pull pull)
        {
            var syncObjects = this.Workspace.DatabaseConnection.ObjectsToSync(pull);
            this.Workspace.DatabaseConnection.Sync(syncObjects, pull.AccessControl);

            foreach (var databaseObject in pull.DatabaseObjects)
            {
                if (this.StrategyByWorkspaceId.TryGetValue(databaseObject.Id, out var strategy))
                {
                    strategy.DatabaseOriginState.OnPulled(pull);
                }
                else
                {
                    this.InstantiateDatabaseStrategy(databaseObject.Id);
                }
            }
        }
    }
}
