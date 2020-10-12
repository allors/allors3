// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using Protocol.Database.Sync;
    using Allors.Workspace.Meta;
    using System.Linq;
    using Protocol.Database;

    public class DatabaseObject
    {
        private AccessControl[] accessControls;

        private Permission[] deniedPermissions;
        private string sortedAccessControlIds;
        private string sortedDeniedPermissionIds;

        internal DatabaseObject(Database database, long objectId, IClass @class)
        {
            this.Database = database;
            this.Id = objectId;
            this.Class = @class;
            this.Version = 0;
        }

        internal DatabaseObject(Database database, ResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.Database = database;
            this.Id = long.Parse(syncResponseObject.I);
            this.Class = (IClass)this.Database.MetaPopulation.Find(Guid.Parse(syncResponseObject.T));
            this.Version = !string.IsNullOrEmpty(syncResponseObject.V) ? long.Parse(syncResponseObject.V) : 0;
            this.Roles = syncResponseObject.R?.Select(v => new DatabaseRole(this.Database.MetaPopulation, v)).ToArray();
            this.SortedAccessControlIds = ctx.ReadSortedAccessControlIds(syncResponseObject.A);
            this.SortedDeniedPermissionIds = ctx.ReadSortedDeniedPermissionIds(syncResponseObject.D);
        }

        public IClass Class { get; }

        public long Id { get; }

        public DatabaseRole[] Roles { get; }

        public string SortedAccessControlIds
        {
            get => this.sortedAccessControlIds;
            set
            {
                this.sortedAccessControlIds = value;
                this.accessControls = null;
            }
        }

        public string SortedDeniedPermissionIds
        {
            get => this.sortedDeniedPermissionIds;
            set
            {
                this.sortedDeniedPermissionIds = value;
                this.accessControls = null;
            }
        }

        public long Version { get; private set; }

        public Database Database { get; }

        public bool IsPermitted(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            if (this.accessControls == null && this.SortedAccessControlIds != null)
            {
                this.accessControls = this.SortedAccessControlIds.Split(Encoding.SeparatorChar).Select(v => this.Database.AccessControlById[long.Parse(v)]).ToArray();
                if (this.deniedPermissions != null)
                {
                    this.deniedPermissions = this.SortedDeniedPermissionIds.Split(Encoding.SeparatorChar).Select(v => this.Database.PermissionById[long.Parse(v)]).ToArray();
                }
            }

            if (this.deniedPermissions != null && this.deniedPermissions.Contains(permission))
            {
                return false;
            }

            if (this.accessControls != null && this.accessControls.Length > 0)
            {
                return this.accessControls.Any(v => v.PermissionIds.Any(w => w == permission.Id));
            }

            return false;
        }
    }
}
