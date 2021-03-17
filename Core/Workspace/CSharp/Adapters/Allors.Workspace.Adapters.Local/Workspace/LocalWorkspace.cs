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
    using Database.Domain;
    using Derivations;
    using Meta;
    using IObjectFactory = Workspace.IObjectFactory;
    using ObjectFactory = Adapters.ObjectFactory;

    public class LocalWorkspace : IWorkspace
    {
        private readonly Dictionary<long, LocalWorkspaceObject> objectById;

        public LocalWorkspace(string name, long userId, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, Allors.Database.IDatabase database)
        {
            this.Name = name;
            this.UserId = userId;
            this.MetaPopulation = metaPopulation;
            this.StateLifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.Database = database;

            var databaseContext = this.Database.Context();
            this.LocalDatabase = new LocalDatabase(this.MetaPopulation, new Identities(), databaseContext.PermissionsCache);

            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();
            this.WorkspaceIdsByWorkspaceClass = new Dictionary<IClass, long[]>();

            this.DomainDerivationById = new ConcurrentDictionary<Guid, IDomainDerivation>();

            this.objectById = new Dictionary<long, LocalWorkspaceObject>();

            this.StateLifecycle.OnInit(this);
        }

        public string Name { get; }

        public long UserId { get;  }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle StateLifecycle { get; }

        public IDictionary<Guid, IDomainDerivation> DomainDerivationById { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ObjectFactory ObjectFactory { get; }

        public IDatabase Database { get; }

        internal LocalDatabase LocalDatabase { get; }

        internal Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        internal Dictionary<IClass, long[]> WorkspaceIdsByWorkspaceClass { get; }

        public ISession CreateSession() => new LocalSession(this, this.StateLifecycle.CreateSessionContext());

        internal LocalWorkspaceObject Get(long identity)
        {
            this.objectById.TryGetValue(identity, out var workspaceObject);
            return workspaceObject;
        }

        internal void RegisterWorkspaceObject(IClass @class, long workspaceId)
        {
            this.WorkspaceClassByWorkspaceId.Add(workspaceId, @class);

            if (!this.WorkspaceIdsByWorkspaceClass.TryGetValue(@class, out var ids))
            {
                ids = new[] { workspaceId };
            }
            else
            {
                ids = NullableSortableArraySet.Add(ids, workspaceId);
            }

            this.WorkspaceIdsByWorkspaceClass[@class] = ids;
        }

        internal void Push(long identity, IClass @class, long version, Dictionary<IRelationType, object> changedRoleByRoleType)
        {
            if (!this.objectById.TryGetValue(identity, out var originalWorkspaceObject))
            {
                this.objectById[identity] = new LocalWorkspaceObject(this.LocalDatabase, identity, @class, ++version, changedRoleByRoleType);
            }
            else
            {
                this.objectById[identity] = new LocalWorkspaceObject(originalWorkspaceObject, changedRoleByRoleType);
            }
        }
    }
}
