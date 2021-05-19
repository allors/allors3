// <copyright file="RemoteWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Meta;
    using Numbers;

    public class Workspace : IWorkspace
    {
        private readonly Dictionary<long, WorkspaceRecord> recordById;

        internal Workspace(string name, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, HttpClient httpClient)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.Lifecycle = state;

            this.ObjectFactory = new ReflectionObjectFactory(this.MetaPopulation, instance);
            this.Database = new Database(this.MetaPopulation, httpClient, new WorkspaceIdGenerator());

            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();
            this.WorkspaceIdsByWorkspaceClass = new Dictionary<IClass, long[]>();

            this.recordById = new Dictionary<long, WorkspaceRecord>();

            this.Numbers = new ArrayNumbers();

            this.Lifecycle.OnInit(this);
        }

        public string Name { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ReflectionObjectFactory ObjectFactory { get; }

        internal INumbers Numbers { get; }

        public ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());

        public Database Database { get; }

        internal Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        internal Dictionary<IClass, long[]> WorkspaceIdsByWorkspaceClass { get; }

        internal WorkspaceRecord Get(long identity)
        {
            _ = this.recordById.TryGetValue(identity, out var workspaceObject);
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
            if (!this.recordById.TryGetValue(identity, out var originalWorkspaceObject))
            {
                this.recordById[identity] = new WorkspaceRecord(this.Database, identity, @class, ++version, changedRoleByRoleType);
            }
            else
            {
                this.recordById[identity] = new WorkspaceRecord(originalWorkspaceObject, changedRoleByRoleType);
            }
        }
    }
}
