// <copyright file="RemoteWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Derivations;
    using Meta;

    public class RemoteWorkspace : IWorkspace
    {
        public RemoteWorkspace(IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, HttpClient httpClient)
        {
            this.MetaPopulation = metaPopulation;
            this.StateLifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.Database = new RemoteDatabase(this.MetaPopulation, httpClient);
            this.Sessions = new HashSet<RemoteSession>();

            this.State = new State();
            this.WorkspaceOrSessionClassByWorkspaceId = new Dictionary<long, IClass>();

            this.DomainDerivationById = new ConcurrentDictionary<Guid, IDomainDerivation>();

            this.StateLifecycle.OnInit(this);
        }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle StateLifecycle { get; }

        internal ISet<RemoteSession> Sessions { get; }
        IEnumerable<ISession> IWorkspace.Sessions => this.Sessions;

        public IDictionary<Guid, IDomainDerivation> DomainDerivationById { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ObjectFactory ObjectFactory { get; }

        internal RemoteDatabase Database { get; }

        internal State State { get; }

        internal Dictionary<long, IClass> WorkspaceOrSessionClassByWorkspaceId { get; }

        public ISession CreateSession() => new RemoteSession(this, this.StateLifecycle.CreateSessionContext());

        public IChangeSet[] Checkpoint()
        {
            var workspaceChangeSet = this.State.Checkpoint();
            return this.Sessions.Select(v => v.Checkpoint(workspaceChangeSet)).ToArray();
        }

        internal void RegisterSession(RemoteSession session) => this.Sessions.Add(session);

        internal void UnregisterSession(RemoteSession session) => this.Sessions.Remove(session);

        internal void RegisterWorkspaceIdForWorkspaceObject(IClass @class, long workspaceId) => this.WorkspaceOrSessionClassByWorkspaceId.Add(workspaceId, @class);

        internal void RegisterWorkspaceIdForSessionObject(IClass @class, long workspaceId) => this.WorkspaceOrSessionClassByWorkspaceId.Add(workspaceId, @class);
    }
}
