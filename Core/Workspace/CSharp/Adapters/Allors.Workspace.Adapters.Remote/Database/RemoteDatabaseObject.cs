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
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Sync;

    internal class RemoteDatabaseObject
    {
        private RemotePermission[] deniedPermissions;
        private RemoteAccessControl[] accessControls;

        private Dictionary<IRelationType, object> roleByRelationType;
        private SyncResponseRole[] syncResponseRoles;

        internal RemoteDatabaseObject(RemoteDatabase database, long identity, IClass @class)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = 0;
        }

        internal RemoteDatabaseObject(RemoteDatabase database, RemoteResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.Database = database;
            this.Identity = long.Parse(syncResponseObject.Id);
            this.Class = (IClass)this.Database.MetaPopulation.Find(Guid.Parse(syncResponseObject.ObjectTypeOrKey));
            this.Version = !string.IsNullOrEmpty(syncResponseObject.Version) ? long.Parse(syncResponseObject.Version) : 0;
            this.syncResponseRoles = syncResponseObject.Roles;
            this.SortedAccessControlIds = ctx.ReadSortedAccessControlIds(syncResponseObject.AccessControls);
            this.SortedDeniedPermissionIds = ctx.ReadSortedDeniedPermissionIds(syncResponseObject.DeniedPermissions);
        }

        internal RemoteDatabase Database { get; }

        internal IClass Class { get; }

        internal long Identity { get; }

        internal long Version { get; private set; }

        internal string SortedAccessControlIds { get; }

        internal string SortedDeniedPermissionIds { get; }

        private Dictionary<IRelationType, object> RoleByRelationType
        {
            get
            {
                if (this.syncResponseRoles != null)
                {
                    var meta = this.Database.MetaPopulation;
                    var identities = this.Database.Identities;

                    var metaPopulation = this.Database.MetaPopulation;
                    this.roleByRelationType = this.syncResponseRoles.ToDictionary(
                        v => (IRelationType)meta.Find(Guid.Parse(v.RoleType)),
                        v =>
                        {
                            var value = v.Value;
                            var roleType = ((IRelationType)metaPopulation.Find(Guid.Parse(v.RoleType))).RoleType;

                            var objectType = roleType.ObjectType;
                            if (objectType.IsUnit)
                            {
                                return UnitConvert.Parse(roleType.ObjectType.Id, value);
                            }
                            else
                            {
                                if (roleType.IsOne)
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

                return this.roleByRelationType;
            }
        }

        private RemoteAccessControl[] AccessControls =>
            this.accessControls = this.accessControls switch
            {
                null when this.SortedAccessControlIds == null => Array.Empty<RemoteAccessControl>(),
                null => this.SortedAccessControlIds.Split(Encoding.SeparatorChar)
                    .Select(v => this.Database.AccessControlById[long.Parse(v)])
                    .ToArray(),
                _ => this.accessControls
            };

        private RemotePermission[] DeniedPermissions =>
            this.deniedPermissions = this.deniedPermissions switch
            {
                null when this.SortedDeniedPermissionIds == null => Array.Empty<RemotePermission>(),
                null => this.SortedDeniedPermissionIds.Split(Encoding.SeparatorChar)
                    .Select(v => this.Database.PermissionById[long.Parse(v)])
                    .ToArray(),
                _ => this.deniedPermissions
            };

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.RoleByRelationType?.TryGetValue(roleType.RelationType, out @object);
            return @object;
        }

        public bool IsPermitted(RemotePermission permission) =>
            permission != null &&
            !this.DeniedPermissions.Contains(permission) &&
            this.AccessControls.Any(v => v.PermissionIds.Any(w => w == permission.Id));
    }
}
