// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;

    public class Workspace : IWorkspace
    {
        public Workspace(IMetaPopulation metaPopulation, Type instance, IWorkspaceStateLifecycle state, IDatabase database)
        {
            this.MetaPopulation = metaPopulation;
            this.StateLifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.WorkspaceDatabase = new WorkspaceDatabase(this.MetaPopulation, database);
            this.Sessions = new HashSet<Session>();

            this.State = new State();
            this.WorkspaceOrSessionClassByWorkspaceId = new Dictionary<long, IClass>();

            this.DomainDerivationById = new ConcurrentDictionary<Guid, IDomainDerivation>();

            this.StateLifecycle.OnInit(this);
        }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceStateLifecycle StateLifecycle { get; }

        internal ISet<Session> Sessions { get; }
        IEnumerable<ISession> IWorkspace.Sessions => this.Sessions;

        public IDictionary<Guid, IDomainDerivation> DomainDerivationById { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;

        internal ObjectFactory ObjectFactory { get; }

        internal WorkspaceDatabase WorkspaceDatabase { get; }

        internal State State { get; }

        internal Dictionary<long, IClass> WorkspaceOrSessionClassByWorkspaceId { get; }

        public ISession CreateSession() => new Session(this, this.StateLifecycle.CreateSessionState());

        public IChangeSet[] Checkpoint()
        {
            var workspaceChangeSet = new WorkspaceChangeSet(this.WorkspaceDatabase.Checkpoint(), this.State.Checkpoint());
            return this.Sessions.Select(v => v.Checkpoint(workspaceChangeSet)).ToArray();
        }

        internal void RegisterSession(Session session) => this.Sessions.Add(session);

        internal void UnregisterSession(Session session) => this.Sessions.Remove(session);

        internal void RegisterWorkspaceIdForWorkspaceObject(IClass @class, long workspaceId) => this.WorkspaceOrSessionClassByWorkspaceId.Add(workspaceId, @class);

        internal void RegisterWorkspaceIdForSessionObject(IClass @class, long workspaceId) => this.WorkspaceOrSessionClassByWorkspaceId.Add(workspaceId, @class);
    }
}
