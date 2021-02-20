// <copyright file="LocalSession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Meta;

    public class LocalSession : ISession
    {
        private readonly Dictionary<long, Strategy> strategyByWorkspaceId;

        private readonly IList<LocalDatabaseStrategy> existingDatabaseStrategies;
        private ISet<LocalDatabaseStrategy> newDatabaseStrategies;

        public LocalSession(LocalWorkspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.SessionLifecycle = sessionLifecycle;
            this.Workspace.RegisterSession(this);

            this.strategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.existingDatabaseStrategies = new List<LocalDatabaseStrategy>();

            this.State = new State();

            this.SessionLifecycle.OnInit(this);
        }

        ~LocalSession() => this.Workspace.UnregisterSession(this);

        IWorkspace ISession.Workspace => this.Workspace;
        internal LocalWorkspace Workspace { get; }

        internal State State { get; }

        public ISessionLifecycle SessionLifecycle { get; }

        public T Create<T>() where T : class, IObject => throw new System.NotImplementedException();

        public IObject Create(IClass @class) => throw new System.NotImplementedException();

        public T Instantiate<T>(T @object) where T : IObject => throw new System.NotImplementedException();

        public IObject Instantiate(long id)
        {
            var databaseStore = this.Workspace.LocalDatabase;

            if (id == 0)
            {
                return null;
            }

            if (!this.strategyByWorkspaceId.TryGetValue(id, out var strategy))
            {
                if (this.Workspace.WorkspaceOrSessionClassByWorkspaceId.TryGetValue(id, out var @class))
                {
                    //strategy = new WorkspaceStrategy(this, @class, workspaceId);
                    //this.strategyByWorkspaceId[workspaceId] = strategy;
                }
                else
                {
                    //if (!databaseStore.DatabaseIdByWorkspaceId.TryGetValue(workspaceId, out var databaseId))
                    //{
                    //    return null;
                    //}

                    if (databaseStore.DatabaseRolesById.TryGetValue(id, out var databaseRoles))
                    {
                        strategy = new LocalDatabaseStrategy(this, databaseRoles, id);
                        this.existingDatabaseStrategies.Add((LocalDatabaseStrategy)strategy);
                        this.strategyByWorkspaceId[id] = strategy;
                    }
                    else
                    {
                        System.Console.WriteLine(0);
                    }
                }
            }

            return strategy?.Object;
        }

        public IEnumerable<IObject> Instantiate(IEnumerable<long> ids) => throw new System.NotImplementedException();

        public void Reset() => throw new System.NotImplementedException();

        public void Refresh(bool merge = false) => throw new System.NotImplementedException();

        public Task<ICallResult> Call(Method method, CallOptions options = null) => throw new System.NotImplementedException();

        public Task<ICallResult> Call(Method[] methods, CallOptions options = null) => throw new System.NotImplementedException();

        public Task<ICallResult> Call(string service, object args) => throw new System.NotImplementedException();

        public Task<ILoadResult> Load(params Data.Pull[] pulls)
        {
            var pullResult = new LocalPullResult(this.Workspace);
            pullResult.Execute(pulls);

            this.Workspace.LocalDatabase.Sync(pullResult);

            var loadResult = new LocalLoadResult(this, pullResult);

            return Task.FromResult<ILoadResult>(loadResult);
        }

        public Task<ILoadResult> Load(string service, object args) => throw new System.NotImplementedException();

        public Task<ISaveResult> Save() => throw new System.NotImplementedException();
    }
}
