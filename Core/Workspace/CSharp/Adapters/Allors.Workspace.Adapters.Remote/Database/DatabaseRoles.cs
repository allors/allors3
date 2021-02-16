// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using Meta;
    using System.Linq;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Sync;

    public class DatabaseRoles
    {
        private Permission[] deniedPermissions;
        private AccessControl[] accessControls;

        private Dictionary<Guid, object> roleByRelationTypeId;
        private SyncResponseRole[] syncResponseRoles;

        internal DatabaseRoles(DatabaseStore databaseStore, long databaseId, IClass @class)
        {
            this.DatabaseStore = databaseStore;
            this.DatabaseId = databaseId;
            this.Class = @class;
            this.Version = 0;
        }

        internal DatabaseRoles(DatabaseStore databaseStore, ResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.DatabaseStore = databaseStore;
            this.DatabaseId = long.Parse(syncResponseObject.Id);
            this.Class = (IClass)this.DatabaseStore.MetaPopulation.Find(Guid.Parse(syncResponseObject.ObjectTypeOrKey));
            this.Version = !string.IsNullOrEmpty(syncResponseObject.Version) ? long.Parse(syncResponseObject.Version) : 0;
            this.syncResponseRoles = syncResponseObject.Roles;
            this.SortedAccessControlIds = ctx.ReadSortedAccessControlIds(syncResponseObject.AccessControls);
            this.SortedDeniedPermissionIds = ctx.ReadSortedDeniedPermissionIds(syncResponseObject.DeniedPermissions);
        }

        public DatabaseStore DatabaseStore { get; }

        public IClass Class { get; }

        public long DatabaseId { get; }

        public long Version { get; private set; }

        public string SortedAccessControlIds { get; }

        public string SortedDeniedPermissionIds { get; }

        private Dictionary<Guid, object> RoleByRelationTypeId
        {
            get
            {
                if (this.syncResponseRoles != null)
                {
                    var metaPopulation = this.DatabaseStore.MetaPopulation;
                    this.roleByRelationTypeId = this.syncResponseRoles.ToDictionary(
                        v => Guid.Parse(v.RoleType),
                        v =>
                        {
                            var value = v.Value;
                            var RoleType = ((IRelationType)metaPopulation.Find(Guid.Parse(v.RoleType))).RoleType;

                            var objectType = RoleType.ObjectType;
                            if (objectType.IsUnit)
                            {
                                return UnitConvert.Parse(RoleType.ObjectType.Id, value);
                            }
                            else
                            {
                                if (RoleType.IsOne)
                                {
                                    return value != null ? long.Parse(value) : (long?)null;
                                }
                                else
                                {
                                    return value != null
                                        ? value.Split(Encoding.SeparatorChar).Select(long.Parse).ToArray()
                                        : Array.Empty<long>();
                                }
                            }
                        });

                    this.syncResponseRoles = null;
                }

                return this.roleByRelationTypeId;
            }
        }

        private AccessControl[] AccessControls =>
            this.accessControls = this.accessControls switch
            {
                null when this.SortedAccessControlIds == null => Array.Empty<AccessControl>(),
                null => this.SortedAccessControlIds.Split(Encoding.SeparatorChar)
                    .Select(v => this.DatabaseStore.AccessControlById[long.Parse(v)])
                    .ToArray(),
                _ => this.accessControls
            };

        private Permission[] DeniedPermissions =>
            this.deniedPermissions = this.deniedPermissions switch
            {
                null when this.SortedDeniedPermissionIds == null => Array.Empty<Permission>(),
                null => this.SortedDeniedPermissionIds.Split(Encoding.SeparatorChar)
                    .Select(v => this.DatabaseStore.PermissionById[long.Parse(v)])
                    .ToArray(),
                _ => this.deniedPermissions
            };

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
