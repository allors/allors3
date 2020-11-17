// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Meta;

    public class Session : ISession
    {
        private readonly Dictionary<long, Strategy> strategyByWorkspaceId;

        private readonly IList<DatabaseStrategy> existingDatabaseStrategies;
        private ISet<DatabaseStrategy> newDatabaseStrategies;
        private SessionChangeSet sessionChangeSet;

        public Session(Workspace workspace, ISessionStateLifecycle stateLifecycle)
        {
            this.Workspace = workspace;
            this.WorkspaceDatabase = this.Workspace.WorkspaceDatabase;
            this.StateLifecycle = stateLifecycle;
            this.Workspace.RegisterSession(this);

            this.strategyByWorkspaceId = new Dictionary<long, Strategy>();
            this.existingDatabaseStrategies = new List<DatabaseStrategy>();

            this.State = new State();
            this.sessionChangeSet = new SessionChangeSet(this);
            this.StateLifecycle.OnInit(this);
        }

        ~Session() => this.Workspace.UnregisterSession(this);

        public ISessionStateLifecycle StateLifecycle { get; }

        IWorkspace ISession.Workspace => this.Workspace;

        internal Workspace Workspace { get; }

        internal WorkspaceDatabase WorkspaceDatabase { get; }

        internal bool HasDatabaseChanges => this.newDatabaseStrategies?.Count > 0 || this.existingDatabaseStrategies.Any(v => v.HasDatabaseChanges);

        internal State State { get; }

        public async Task<ICallResult> Call(Method method, CallOptions options = null) => await this.Call(new[] { method }, options);

        public async Task<ICallResult> Call(Method[] methods, CallOptions options = null) => throw new NotImplementedException();

        public async Task<ICallResult> Call(string service, object args) => throw new NotImplementedException();

        public T Create<T>() where T : class, IObject => (T)this.Create((IClass)this.Workspace.ObjectFactory.GetObjectType<T>());

        public IObject Create(IClass @class) => @class.Origin switch
        {
            Origin.Database => this.CreateDatabaseObject(@class),
            Origin.Workspace => this.CreateWorkspaceObject(@class),
            Origin.Session => this.CreateSessionObject(@class),
            _ => throw new Exception($"Unsupported origin: {@class.Origin}"),
        };

        public T Instantiate<T>(T @object) where T : IObject => (T)this.Instantiate(@object.WorkspaceId);

        public IObject Instantiate(long id)
        {
            var workspaceId = this.WorkspaceDatabase.ToWorkspaceId(id);

            if (workspaceId == 0)
            {
                return null;
            }

            if (!this.strategyByWorkspaceId.TryGetValue(workspaceId, out var strategy))
            {
                if (this.Workspace.WorkspaceOrSessionClassByWorkspaceId.TryGetValue(workspaceId, out var @class))
                {
                    strategy = new WorkspaceStrategy(this, @class, workspaceId);
                    this.strategyByWorkspaceId[workspaceId] = strategy;
                }
                else
                {
                    if (!this.WorkspaceDatabase.DatabaseIdByWorkspaceId.TryGetValue(workspaceId, out var databaseId))
                    {
                        return null;
                    }

                    var databaseObject = this.WorkspaceDatabase.Get(databaseId);
                    strategy = new DatabaseStrategy(this, databaseObject, workspaceId);
                    this.existingDatabaseStrategies.Add((DatabaseStrategy)strategy);
                    this.strategyByWorkspaceId[workspaceId] = strategy;
                }
            }

            return strategy.Object;
        }

        public IEnumerable<IObject> Instantiate(IEnumerable<long> ids) => ids.Select(this.Instantiate);

        public async Task<ILoadResult> Load(params Pull[] pulls)
        {
            var loadResult = new LoadResult(this.Workspace);
            this.WorkspaceDatabase.Load(pulls, loadResult);
            return loadResult;
        }

        public async Task<ILoadResult> Load(string service, object args) => throw new NotImplementedException();

        public void Reset()
        {
            if (this.newDatabaseStrategies != null)
            {
                foreach (var databaseStrategy in this.newDatabaseStrategies)
                {
                    databaseStrategy.Reset();
                }
            }

            foreach (var databaseStrategy in this.existingDatabaseStrategies)
            {
                databaseStrategy.Reset();
            }
        }

        public void Refresh(bool merge = false)
        {
            if (this.newDatabaseStrategies != null)
            {
                foreach (var databaseStrategy in this.newDatabaseStrategies)
                {
                    databaseStrategy.Refresh(merge);
                }
            }

            foreach (var databaseStrategy in this.existingDatabaseStrategies)
            {
                databaseStrategy.Refresh(merge);
            }
        }

        public async Task<ISaveResult> Save() => throw new NotImplementedException();

        internal IChangeSet Checkpoint(WorkspaceChangeSet workspaceChangeSet)
        {
            try
            {
                this.sessionChangeSet.Merge(workspaceChangeSet, this.State.Checkpoint());
                return this.sessionChangeSet;
            }
            finally
            {
                this.sessionChangeSet = null;
            }
        }

        internal IEnumerable<IObject> GetAssociation(IDatabaseObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            foreach (var association in this.WorkspaceDatabase.Get(associationType.ObjectType).Select(v => this.Instantiate(v.DatabaseId)))
            {
                if (((IDatabaseObject)association).Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = (IObject)((DatabaseStrategy)association.Strategy).GetDatabase(roleType);
                        if (role != null && role.WorkspaceId == @object.WorkspaceId)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = (IObject[])((DatabaseStrategy)association.Strategy).GetDatabase(roleType);
                        if (roles != null && roles.Contains(@object))
                        {
                            yield return association;
                        }
                    }
                }
            }
        }

        internal IObject GetForAssociation(long id)
        {
            var workspaceId = this.WorkspaceDatabase.ToWorkspaceId(id);
            if (workspaceId == 0)
            {
                return null;
            }

            this.strategyByWorkspaceId.TryGetValue(workspaceId, out var strategy);
            return strategy?.Object;
        }
        
        private IObject CreateDatabaseObject(IClass @class)
        {
            var workspaceId = this.WorkspaceDatabase.NextWorkspaceId();
            var strategy = new DatabaseStrategy(this, @class, workspaceId);
            this.newDatabaseStrategies ??= new HashSet<DatabaseStrategy>();
            this.newDatabaseStrategies.Add(strategy);
            this.strategyByWorkspaceId.Add(strategy.WorkspaceId, strategy);
            return strategy.Object;
        }

        private IObject CreateWorkspaceObject(IClass @class)
        {
            var workspaceId = this.WorkspaceDatabase.NextWorkspaceId();
            this.Workspace.RegisterWorkspaceIdForWorkspaceObject(@class, workspaceId);
            var strategy = new WorkspaceStrategy(this, @class, workspaceId);
            this.strategyByWorkspaceId[strategy.WorkspaceId] = strategy;
            return strategy.Object;
        }

        private IObject CreateSessionObject(IClass @class)
        {
            var workspaceId = this.WorkspaceDatabase.NextWorkspaceId();
            var strategy = new SessionStrategy(this, @class, workspaceId);
            this.strategyByWorkspaceId[strategy.WorkspaceId] = strategy;
            return strategy.Object;
        }
    }
}
