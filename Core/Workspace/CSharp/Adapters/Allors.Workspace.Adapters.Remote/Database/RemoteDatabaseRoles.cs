// <copyright file="RemoteDatabaseRoles.cs" company="Allors bvba">
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

    public class RemoteDatabaseRoles
    {
        private RemotePermission[] deniedPermissions;
        private RemoteAccessControl[] accessControls;

        private Dictionary<Guid, object> roleByRelationTypeId;
        private SyncResponseRole[] syncResponseRoles;

        internal RemoteDatabaseRoles(RemoteDatabase database, Identity identity, IClass @class)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = 0;
        }

        internal RemoteDatabaseRoles(RemoteDatabase database, RemoteResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.Database = database;
            this.Identity = database.Identities.GetOrCreate(long.Parse(syncResponseObject.Id));
            this.Class = (IClass)this.Database.MetaPopulation.Find(Guid.Parse(syncResponseObject.ObjectTypeOrKey));
            this.Version = !string.IsNullOrEmpty(syncResponseObject.Version) ? long.Parse(syncResponseObject.Version) : 0;
            this.syncResponseRoles = syncResponseObject.Roles;
            this.SortedAccessControlIds = ctx.ReadSortedAccessControlIds(syncResponseObject.AccessControls);
            this.SortedDeniedPermissionIds = ctx.ReadSortedDeniedPermissionIds(syncResponseObject.DeniedPermissions);
        }

        public RemoteDatabase Database { get; }

        public IClass Class { get; }

        public Identity Identity { get; }

        public long Version { get; private set; }

        public string SortedAccessControlIds { get; }

        public string SortedDeniedPermissionIds { get; }

        private Dictionary<Guid, object> RoleByRelationTypeId
        {
            get
            {
                if (this.syncResponseRoles != null)
                {
                    var identities = this.Database.Identities;

                    var metaPopulation = this.Database.MetaPopulation;
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
                                    return value != null ? identities.Get(long.Parse(value)) : (Identity)null;
                                }
                                else
                                {
                                    return value != null
                                        ? value.Split(Encoding.SeparatorChar).Select(w => identities.Get(long.Parse(w))).ToArray()
                                        : Array.Empty<Identity>();
                                }
                            }
                        });

                    this.syncResponseRoles = null;
                }

                return this.roleByRelationTypeId;
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
            this.RoleByRelationTypeId?.TryGetValue(roleType.RelationType.Id, out @object);
            return @object;
        }

        public bool IsPermitted(RemotePermission permission) =>
            permission != null &&
            !this.DeniedPermissions.Contains(permission) &&
            this.AccessControls.Any(v => v.PermissionIds.Any(w => w == permission.Id));
    }
}
