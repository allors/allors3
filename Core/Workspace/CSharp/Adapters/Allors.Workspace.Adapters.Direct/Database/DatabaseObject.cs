// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System;
    using System.Collections.Generic;
    using Meta;
    using System.Linq;
    using Security;

    public class DatabaseObject
    {
        private Permission[] deniedPermissions;
        private AccessControl[] accessControls;

        private Dictionary<Guid, object> roleByRelationTypeId;
        //private SyncResponseRole[] syncResponseRoles;

        internal DatabaseObject(WorkspaceDatabase workspaceDatabase, long databaseId, IClass @class)
        {
            this.WorkspaceDatabase = workspaceDatabase;
            this.DatabaseId = databaseId;
            this.Class = @class;
            this.Version = 0;
        }

        internal DatabaseObject(WorkspaceDatabase workspaceDatabase)
        {
            this.WorkspaceDatabase = workspaceDatabase;
            //this.DatabaseId = long.Parse(syncResponseObject.I);
            //this.Class = (IClass)this.WorkspaceDatabase.MetaPopulation.Find(Guid.Parse(syncResponseObject.T));
            //this.Version = !string.IsNullOrEmpty(syncResponseObject.V) ? long.Parse(syncResponseObject.V) : 0;
            //this.syncResponseRoles = syncResponseObject.R;
            //this.SortedAccessControlIds = ctx.ReadSortedAccessControlIds(syncResponseObject.A);
            //this.SortedDeniedPermissionIds = ctx.ReadSortedDeniedPermissionIds(syncResponseObject.D);
        }

        public WorkspaceDatabase WorkspaceDatabase { get; }

        public IClass Class { get; }

        public long DatabaseId { get; }

        public long Version { get; private set; }

        public string SortedAccessControlIds { get; }

        public string SortedDeniedPermissionIds { get; }

        private Dictionary<Guid, object> RoleByRelationTypeId
        {
            get
            {
                //if (this.syncResponseRoles != null)
                //{
                //    var metaPopulation = this.WorkspaceDatabase.MetaPopulation;
                //    this.roleByRelationTypeId = this.syncResponseRoles.ToDictionary(
                //        v => Guid.Parse(v.T),
                //        v =>
                //        {
                //            var value = v.V;
                //            var RoleType = ((IRelationType)metaPopulation.Find(Guid.Parse(v.T))).RoleType;

                //            var objectType = RoleType.ObjectType;
                //            if (objectType.IsUnit)
                //            {
                //                return UnitConvert.Parse(RoleType.ObjectType.Id, value);
                //            }
                //            else
                //            {
                //                if (RoleType.IsOne)
                //                {
                //                    return value != null ? long.Parse(value) : (long?)null;
                //                }
                //                else
                //                {
                //                    return value != null
                //                        ? value.Split(Encoding.SeparatorChar).Select(long.Parse).ToArray()
                //                        : Array.Empty<long>();
                //                }
                //            }
                //        });

                //    this.syncResponseRoles = null;
                //}

                return this.roleByRelationTypeId;
            }
        }

        private AccessControl[] AccessControls => throw new NotImplementedException();

        private Permission[] DeniedPermissions => throw new NotImplementedException();

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.RoleByRelationTypeId?.TryGetValue(roleType.RelationType.Id, out @object);
            return @object;
        }

        public bool IsPermitted(Permission permission) =>
            permission != null &&
            !this.DeniedPermissions.Contains(permission) &&
            this.AccessControls.Any(v => v.PermissionIds.Any(w => w == permission.Id));
    }
}
