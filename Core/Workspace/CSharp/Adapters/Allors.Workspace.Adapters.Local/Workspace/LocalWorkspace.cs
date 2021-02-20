// <copyright file="LocalWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
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

    public class LocalWorkspace : IWorkspace
    {
        public LocalWorkspace(string name, IMetaPopulation metaPopulation, Type domainType, IWorkspaceLifecycle stateLifecycle, Allors.Database.IDatabase database)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.ObjectFactory = new Adapters.ObjectFactory(metaPopulation, domainType);
            this.StateLifecycle = stateLifecycle;
            this.Database = database;
            this.DomainDerivationById = new ConcurrentDictionary<Guid, IDomainDerivation>();
            this.Sessions = new ConcurrentDictionary<LocalSession, object>();

            this.LocalDatabase = new LocalDatabase(this.MetaPopulation);

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

        internal ConcurrentDictionary<LocalSession, object> Sessions { get; }
        IEnumerable<ISession> IWorkspace.Sessions => this.Sessions.Keys;

        public IDatabase Database { get; }

        internal LocalDatabase LocalDatabase { get; }

        internal State State { get; }

        internal Dictionary<long, IClass> WorkspaceOrSessionClassByWorkspaceId { get; }

        public ISession CreateSession() => new LocalSession(this, this.StateLifecycle.CreateSessionContext());

        public IChangeSet[] Checkpoint() => throw new NotImplementedException();

        internal void RegisterSession(LocalSession session) => this.Sessions[session] = null;

        internal void UnregisterSession(LocalSession session) => this.Sessions.TryRemove(session, out var dummy);
    }
}
