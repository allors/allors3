// <copyright file="LocalWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using Database;
    using Meta;
    using IObjectFactory = Allors.Workspace.IObjectFactory;
    using ObjectFactory = Adapters.ObjectFactory;

    public class Workspace : IWorkspace
    {
        private readonly Dictionary<long, WorkspaceObject> objectById;

        public Workspace(string name, long userId, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, IDatabase database)
        {
            this.Name = name;
            this.UserId = userId;
            this.MetaPopulation = metaPopulation;
            this.Lifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.DatabaseAdapter = new DatabaseAdapter(this.MetaPopulation, database);

            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();
            this.WorkspaceIdsByWorkspaceClass = new Dictionary<IClass, long[]>();
            this.objectById = new Dictionary<long, WorkspaceObject>();

            this.Lifecycle.OnInit(this);
        }

        public string Name { get; }

        public long UserId { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ObjectFactory ObjectFactory { get; }

        public ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());

        internal DatabaseAdapter DatabaseAdapter { get; }

        internal Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        internal Dictionary<IClass, long[]> WorkspaceIdsByWorkspaceClass { get; }

        internal WorkspaceObject Get(long identity)
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
                this.objectById[identity] = new WorkspaceObject(this.DatabaseAdapter, identity, @class, ++version, changedRoleByRoleType);
            }
            else
            {
                this.objectById[identity] = new WorkspaceObject(originalWorkspaceObject, changedRoleByRoleType);
            }
        }
    }
}
