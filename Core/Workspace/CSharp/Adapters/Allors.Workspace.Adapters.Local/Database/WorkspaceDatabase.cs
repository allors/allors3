// <copyright file="Database.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Allors.Workspace.Meta;
    using Data;
    using Database;
    using Polly;
    using Security;

    public class WorkspaceDatabase
    {
        private readonly Dictionary<long, DatabaseObject> databaseObjectByDatabaseId;

        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, Permission>> executePermissionByOperandTypeByClass;

        private long worskpaceIdCounter;
        private DatabaseChangeSet databaseChangeSet;

        public WorkspaceDatabase(IMetaPopulation metaPopulation, IDatabase database)
        {
            this.MetaPopulation = metaPopulation;
            this.Database = database;

            this.AccessControlById = new Dictionary<long, AccessControl>();
            this.PermissionById = new Dictionary<long, Permission>();

            this.databaseObjectByDatabaseId = new Dictionary<long, DatabaseObject>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, Permission>>();

            this.worskpaceIdCounter = 0;
            this.WorkspaceIdByDatabaseId = new Dictionary<long, long>();
            this.DatabaseIdByWorkspaceId = new Dictionary<long, long>();

            this.databaseChangeSet = new DatabaseChangeSet();
        }

        public IMetaPopulation MetaPopulation { get; }

        public IDatabase Database { get; }

        public IAsyncPolicy Policy { get; set; } = Polly.Policy
           .Handle<HttpRequestException>()
           .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public string UserId { get; private set; }

        internal Dictionary<long, long> WorkspaceIdByDatabaseId { get; }

        internal Dictionary<long, long> DatabaseIdByWorkspaceId { get; }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        internal Dictionary<long, Permission> PermissionById { get; }

        internal DatabaseObject PushResponse(long databaseId, IClass @class)
        {
            var databaseObject = new DatabaseObject(this, databaseId, @class);
            this.databaseObjectByDatabaseId[databaseId] = databaseObject;
            return databaseObject;
        }

        internal long NextWorkspaceId() => --this.worskpaceIdCounter;

        internal long ToWorkspaceId(long id)
        {
            if (id <= 0)
            {
                return id;
            }

            this.WorkspaceIdByDatabaseId.TryGetValue(id, out var workspaceId);
            return workspaceId;
        }

        internal DatabaseObject Get(long databaseId)
        {
            var databaseObject = this.databaseObjectByDatabaseId[databaseId];
            if (databaseObject == null)
            {
                throw new Exception($"Object with id {databaseId} is not present.");
            }

            return databaseObject;
        }

        internal IEnumerable<DatabaseObject> Get(IComposite objectType)
        {
            var classes = new HashSet<IClass>(objectType.DatabaseClasses);
            return this.databaseObjectByDatabaseId.Where(v => classes.Contains(v.Value.Class)).Select(v => v.Value);
        }

        internal Permission GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            switch (operation)
            {
                case Operations.Read:
                    if (this.readPermissionByOperandTypeByClass.TryGetValue(@class, out var readPermissionByOperandType))
                    {
                        if (readPermissionByOperandType.TryGetValue(operandType, out var readPermission))
                        {
                            return readPermission;
                        }
                    }

                    return null;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class, out var writePermissionByOperandType))
                    {
                        if (writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
                        {
                            return writePermission;
                        }
                    }

                    return null;

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class, out var executePermissionByOperandType))
                    {
                        if (executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                        {
                            return executePermission;
                        }
                    }

                    return null;
            }
        }

        internal DatabaseChangeSet Checkpoint()
        {
            try
            {
                return this.databaseChangeSet;
            }
            finally
            {
                this.databaseChangeSet = null;
            }
        }

        internal void Load(Pull[] pulls, LoadResult loadResult)
        {
            using var databaseSession = this.Database.CreateSession();
        }
    }
}
