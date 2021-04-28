// <copyright file="RemoteDatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using Meta;
    using System.Linq;
    using Allors.Protocol.Json.Api.Sync;

    internal class DatabaseObject
    {
        private static readonly HashSet<long> EmptySet = new HashSet<long>();

        private Permission[] deniedPermissions;
        private AccessControl[] accessControls;

        private Dictionary<IRelationType, object> roleByRelationType;
        private SyncResponseRole[] syncResponseRoles;

        internal DatabaseObject(Database database, long identity, IClass @class)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = 0;
        }

        internal DatabaseObject(Database database, ResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.Database = database;
            this.Identity = syncResponseObject.Id;
            this.Class = (IClass)this.Database.MetaPopulation.FindByTag(syncResponseObject.ObjectType);
            this.Version = syncResponseObject.Version;
            this.syncResponseRoles = syncResponseObject.Roles;
            this.AccessControlIds = syncResponseObject.AccessControls != null ? new HashSet<long>(ctx.CheckForMissingAccessControls(syncResponseObject.AccessControls)) : EmptySet;
            this.DeniedPermissionIds = syncResponseObject.DeniedPermissions != null ? new HashSet<long>(ctx.CheckForMissingPermissions(syncResponseObject.DeniedPermissions)): EmptySet;
        }

        internal Database Database { get; }

        internal IClass Class { get; }

        internal long Identity { get; }

        internal long Version { get; private set; }

        internal HashSet<long> AccessControlIds { get; }

        internal HashSet<long> DeniedPermissionIds { get; private set; }

        private Dictionary<IRelationType, object> RoleByRelationType
        {
            get
            {
                if (this.syncResponseRoles != null)
                {
                    var meta = this.Database.MetaPopulation;

                    var metaPopulation = this.Database.MetaPopulation;
                    this.roleByRelationType = this.syncResponseRoles.ToDictionary(
                        v => (IRelationType)meta.FindByTag(v.RoleType),
                        v =>
                        {
                            var roleType = ((IRelationType)metaPopulation.FindByTag(v.RoleType)).RoleType;

                            var objectType = roleType.ObjectType;
                            if (objectType.IsUnit)
                            {
                                return UnitConvert.FromString(roleType.ObjectType.Tag, v.Value);
                            }

                            if (roleType.IsOne)
                            {
                                return v.Object;
                            }

                            return v.Collection;
                        });

                    this.syncResponseRoles = null;
                }

                return this.roleByRelationType;
            }
        }

        private AccessControl[] AccessControls =>
            this.accessControls = this.accessControls switch
            {
                null when this.AccessControlIds == null => Array.Empty<AccessControl>(),
                null => this.AccessControlIds
                    .Select(v => this.Database.AccessControlById[v])
                    .ToArray(),
                _ => this.accessControls
            };

        private Permission[] DeniedPermissions =>
            this.deniedPermissions = this.deniedPermissions switch
            {
                null when this.DeniedPermissionIds == null => Array.Empty<Permission>(),
                null => this.DeniedPermissionIds
                    .Select(v => this.Database.PermissionById[v])
                    .ToArray(),
                _ => this.deniedPermissions
            };

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.RoleByRelationType?.TryGetValue(roleType.RelationType, out @object);
            return @object;
        }

        public bool IsPermitted(Permission permission) =>
            permission != null &&
            !this.DeniedPermissions.Contains(permission) &&
            this.AccessControls.Any(v => v.PermissionIds.Any(w => w == permission.Id));

        internal void UpdateDeniedPermissions(long[] deniedPermissions)
        {
            if (this.deniedPermissions == null)
            {
                this.DeniedPermissionIds = EmptySet;
            }
            else
            {
                if (!this.DeniedPermissionIds.SetEquals(deniedPermissions))
                {
                    this.DeniedPermissionIds = new HashSet<long>(deniedPermissions);
                }
            }
        }
    }
}
