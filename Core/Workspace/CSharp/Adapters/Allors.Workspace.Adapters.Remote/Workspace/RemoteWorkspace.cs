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
        private readonly Dictionary<Identity, RemoteWorkspaceObject> workspaceRolesByIdentity;

        internal RemoteWorkspace(IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, HttpClient httpClient)
        {
            this.MetaPopulation = metaPopulation;
            this.StateLifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.Database = new RemoteDatabase(this.MetaPopulation, httpClient, new Identities());

            this.WorkspaceClassByWorkspaceId = new Dictionary<Identity, IClass>();

            this.DomainDerivationById = new ConcurrentDictionary<Guid, IDomainDerivation>();

            this.workspaceRolesByIdentity = new Dictionary<Identity, RemoteWorkspaceObject>();

            this.StateLifecycle.OnInit(this);
        }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle StateLifecycle { get; }

        public IDictionary<Guid, IDomainDerivation> DomainDerivationById { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ObjectFactory ObjectFactory { get; }

        public RemoteDatabase Database { get; }

        internal Dictionary<Identity, IClass> WorkspaceClassByWorkspaceId { get; }

        public ISession CreateSession() => new RemoteSession(this, this.StateLifecycle.CreateSessionContext());

        internal RemoteWorkspaceObject Get(Identity identity)
        {
            this.workspaceRolesByIdentity.TryGetValue(identity, out var workspaceRoles);
            return workspaceRoles;
        }

        internal void RegisterWorkspaceObject(IClass @class, Identity workspaceId) => this.WorkspaceClassByWorkspaceId.Add(workspaceId, @class);

        internal void Push(Identity identity, IClass @class, long version, Dictionary<IRelationType, object> changedRoleByRoleType)
        {
            if (!this.workspaceRolesByIdentity.TryGetValue(identity, out var originalWorkspaceRoles))
            {
                this.workspaceRolesByIdentity[identity] = new RemoteWorkspaceObject(this.Database, identity, @class, ++version, changedRoleByRoleType);
            }
            else
            {
                this.workspaceRolesByIdentity[identity] = originalWorkspaceRoles.Update(changedRoleByRoleType);
            }
        }
    }
}
