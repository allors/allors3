// <copyright file="RemoteWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using Derivations;
    using Meta;

    public class RemoteWorkspace : IWorkspace
    {
        private readonly Dictionary<long, RemoteWorkspaceObject> objectById;

        internal RemoteWorkspace(string name, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, HttpClient httpClient)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.Lifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.Database = new RemoteDatabase(this.MetaPopulation, httpClient, new Identities());

            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();
            this.WorkspaceIdsByWorkspaceClass = new Dictionary<IClass, long[]>();

            this.objectById = new Dictionary<long, RemoteWorkspaceObject>();

            this.Lifecycle.OnInit(this);
        }

        public string Name { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ObjectFactory ObjectFactory { get; }

        public ISession CreateSession() => new RemoteSession(this, this.Lifecycle.CreateSessionContext());

        public RemoteDatabase Database { get; }

        internal Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        internal Dictionary<IClass, long[]> WorkspaceIdsByWorkspaceClass { get; }

        internal RemoteWorkspaceObject Get(long identity)
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
                this.objectById[identity] = new RemoteWorkspaceObject(this.Database, identity, @class, ++version, changedRoleByRoleType);
            }
            else
            {
                this.objectById[identity] = new RemoteWorkspaceObject(originalWorkspaceObject, changedRoleByRoleType);
            }
        }
    }
}
