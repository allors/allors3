// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Database;
    using Derivations;
    using Meta;
    using IChangeSet = Allors.Workspace.IChangeSet;
    using IObjectFactory = Allors.Workspace.IObjectFactory;
    using ISession = Allors.Workspace.ISession;

    public class Workspace : IWorkspace
    {
        public Workspace(string name, IMetaPopulation metaPopulation, Type domainType, IWorkspaceLifecycle stateLifecycle, Allors.Database.IDatabase database)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.ObjectFactory = new Adapters.ObjectFactory(metaPopulation, domainType);
            this.StateLifecycle = stateLifecycle;
            this.Database = database;
            this.DomainDerivationById = new ConcurrentDictionary<Guid, IDomainDerivation>();
            this.Sessions = new ConcurrentDictionary<Session, object>();

            this.DatabaseStore = new DatabaseStore(this.MetaPopulation);

            this.State = new State();
            this.WorkspaceOrSessionClassByWorkspaceId = new Dictionary<long, IClass>();

            this.StateLifecycle.OnInit(this);
        }

        public string Name { get; }

        public IMetaPopulation MetaPopulation { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal Adapters.ObjectFactory ObjectFactory { get; }

        public IWorkspaceLifecycle StateLifecycle { get; }

        public IDictionary<Guid, IDomainDerivation> DomainDerivationById { get; }

        internal ConcurrentDictionary<Session, object> Sessions { get; }
        IEnumerable<ISession> IWorkspace.Sessions => this.Sessions.Keys;

        public IDatabase Database { get; }

        internal DatabaseStore DatabaseStore { get; }

        internal State State { get; }

        internal Dictionary<long, IClass> WorkspaceOrSessionClassByWorkspaceId { get; }

        public ISession CreateSession() => new Session(this, this.StateLifecycle.CreateSessionContext());

        public IChangeSet[] Checkpoint() => throw new NotImplementedException();

        internal void RegisterSession(Session session) => this.Sessions[session] = null;

        internal void UnregisterSession(Session session) => this.Sessions.TryRemove(session, out var dummy);
    }
}
