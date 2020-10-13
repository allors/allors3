// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Meta;

    public class Workspace : IWorkspace
    {
        private readonly Population population;

        private readonly Dictionary<long, long> workspaceIdByDatabaseId;

        private long worskpaceIdCounter;

        public Workspace(IMetaPopulation metaPopulation, Type instance, IWorkspaceStateLifecycle state, HttpClient httpClient)
        {
            this.MetaPopulation = metaPopulation;
            this.StateLifecycle = state;

            this.worskpaceIdCounter = 0;
            this.workspaceIdByDatabaseId = new Dictionary<long, long>();

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.Database = new Database(this.MetaPopulation, httpClient);
            this.Sessions = new HashSet<Session>();

            this.population = new Population();
            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();

            this.StateLifecycle.OnInit(this);
        }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceStateLifecycle StateLifecycle { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;

        internal ObjectFactory ObjectFactory { get; }

        internal Database Database { get; }

        internal ISet<Session> Sessions { get; }

        public Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        public ISession CreateSession() => new Session(this, this.StateLifecycle.CreateSessionState());

        internal void RegisterSession(Session session) => this.Sessions.Add(session);

        internal void UnregisterSession(Session session) => this.Sessions.Remove(session);

        internal void RegisterWorkspaceIdForDatabaseObject(long databaseId, long workspaceId) => this.workspaceIdByDatabaseId.Add(databaseId, workspaceId);

        internal void RegisterWorkspaceIdForWorkspaceObject(IClass @class, long workspaceId) => this.WorkspaceClassByWorkspaceId.Add(workspaceId, @class);

        internal long NextWorkspaceId() => --this.worskpaceIdCounter;
        
        internal long ToWorkspaceId(long id)
        {
            if (id > 0)
            {
                if (!this.workspaceIdByDatabaseId.TryGetValue(id, out var workspaceId))
                {
                    workspaceId = this.NextWorkspaceId();
                    this.workspaceIdByDatabaseId.Add(id, workspaceId);

                }

                return workspaceId;
            }

            return id;
        }

        internal void Set(Strategy strategy, IRoleType roleType, object value) => this.population.SetRole(strategy.WorkspaceId, roleType, value);

        internal object Get(Strategy strategy, IRoleType roleType)
        {
            this.population.GetRole(strategy.WorkspaceId, roleType, out var role);
            return role;
        }
    }
}
