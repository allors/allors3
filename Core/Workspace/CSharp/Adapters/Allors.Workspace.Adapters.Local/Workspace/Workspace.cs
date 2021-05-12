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
    using Numbers;
    using IObjectFactory = Allors.Workspace.IObjectFactory;

    public class Workspace : IWorkspace
    {
        private readonly Dictionary<long, WorkspaceRecord> recordById;

        public Workspace(string name, long userId, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, IDatabase database)
        {
            this.Name = name;
            this.UserId = userId;
            this.MetaPopulation = metaPopulation;
            this.Lifecycle = state;

            this.ObjectFactory = new ReflectionObjectFactory(this.MetaPopulation, instance);
            this.DatabaseAdapter = new DatabaseAdapter(this.MetaPopulation, database);

            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();
            this.WorkspaceIdsByWorkspaceClass = new Dictionary<IClass, long[]>();
            this.recordById = new Dictionary<long, WorkspaceRecord>();

            this.Numbers = new ArrayNumbers();

            this.Lifecycle.OnInit(this);
        }

        public string Name { get; }

        public long UserId { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;
        internal ReflectionObjectFactory ObjectFactory { get; }

        internal INumbers Numbers { get; }

        internal DatabaseAdapter DatabaseAdapter { get; }

        internal Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        internal Dictionary<IClass, long[]> WorkspaceIdsByWorkspaceClass { get; }

        public ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());

        internal WorkspaceRecord GetRecord(long id)
        {
            _ = this.recordById.TryGetValue(id, out var workspaceObject);
            return workspaceObject;
        }

        internal void Push(long id, IClass @class, long version, Dictionary<IRelationType, object> changedRoleByRoleType)
        {
            if (!this.WorkspaceClassByWorkspaceId.ContainsKey(id))
            {
                this.WorkspaceClassByWorkspaceId.Add(id, @class);

                _ = this.WorkspaceIdsByWorkspaceClass.TryGetValue(@class, out var ids);
                _ = this.Numbers.Add(ids, id);
                this.WorkspaceIdsByWorkspaceClass[@class] = ids;

            }

            if (!this.recordById.TryGetValue(id, out var originalWorkspaceRecord))
            {
                this.recordById[id] = new WorkspaceRecord(this.DatabaseAdapter, id, @class, ++version, changedRoleByRoleType);
            }
            else
            {
                this.recordById[id] = new WorkspaceRecord(originalWorkspaceRecord, changedRoleByRoleType);
            }
        }
    }
}
