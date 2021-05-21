// <copyright file="Workspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using Meta;
    using Numbers;

    public abstract class Workspace : IWorkspace
    {
        private readonly Dictionary<long, WorkspaceRecord> recordById;

        protected Workspace(DatabaseConnection database, IWorkspaceLifecycle lifecycle, INumbers numbers, WorkspaceIdGenerator workspaceIdGenerator)
        {
            this.Database = database;
            this.Lifecycle = lifecycle;
            this.WorkspaceIdGenerator = workspaceIdGenerator;
            this.Numbers = numbers;

            this.WorkspaceClassByWorkspaceId = new Dictionary<long, IClass>();
            this.WorkspaceIdsByWorkspaceClass = new Dictionary<IClass, long[]>();

            this.recordById = new Dictionary<long, WorkspaceRecord>();
        }

        IDatabaseConnection IWorkspace.DatabaseConnection => this.Database;
        public DatabaseConnection Database { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        public WorkspaceIdGenerator WorkspaceIdGenerator { get; }

        public INumbers Numbers { get; }

        public Dictionary<long, IClass> WorkspaceClassByWorkspaceId { get; }

        public Dictionary<IClass, long[]> WorkspaceIdsByWorkspaceClass { get; }

        public abstract ISession CreateSession();

        public WorkspaceRecord GetRecord(long id)
        {
            _ = this.recordById.TryGetValue(id, out var workspaceObject);
            return workspaceObject;
        }

        public void Push(long id, IClass @class, long version, Dictionary<IRelationType, object> changedRoleByRoleType)
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
                this.recordById[id] = new WorkspaceRecord(@class, id, ++version, changedRoleByRoleType);
            }
            else
            {
                this.recordById[id] = new WorkspaceRecord(originalWorkspaceRecord, changedRoleByRoleType);
            }
        }
    }
}
