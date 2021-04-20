// <copyright file="SyncResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Sync;
    using Security;

    public class SyncResponseBuilder
    {
        private readonly AccessControlsWriter accessControlsWriter;
        private readonly PermissionsWriter permissionsWriter;

        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly Action<IEnumerable<IObject>> prefetch;

        public SyncResponseBuilder(ITransaction transaction, IAccessControlLists accessControlLists, ISet<IClass> allowedClasses, Action<IEnumerable<IObject>> prefetch)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.prefetch = prefetch;

            this.AccessControlLists = accessControlLists;

            this.accessControlsWriter = new AccessControlsWriter(this.AccessControlLists);
            this.permissionsWriter = new PermissionsWriter(this.AccessControlLists);
        }

        public IAccessControlLists AccessControlLists { get; }

        public SyncResponse Build(SyncRequest syncRequest)
        {
            var objects = this.transaction.Instantiate(syncRequest.Objects)
                .Where(v => this.allowedClasses?.Contains(v.Strategy.Class) == true)
                .ToArray();

            this.prefetch(objects);

            static SyncResponseRole CreateSyncResponseRole(IObject @object, IRoleType roleType)
            {
                var syncResponseRole = new SyncResponseRole { RoleType = roleType.RelationType.Tag };

                if (roleType.ObjectType.IsUnit)
                {
                    syncResponseRole.Value = UnitConvert.ToString(@object.Strategy.GetUnitRole(roleType));
                }
                else if (roleType.IsOne)
                {
                    syncResponseRole.Object = @object.Strategy.GetCompositeRole(roleType)?.Id;
                }
                else
                {
                    var roles = @object.Strategy.GetCompositeRoles(roleType);
                    if (roles.Count > 0)
                    {
                        syncResponseRole.Collection = roles.Select(roleObject => roleObject.Id).ToArray();
                    }
                }

                return syncResponseRole;
            }

            return new SyncResponse
            {
                Objects = objects.Select(v =>
                {
                    var @class = (IClass)v.Strategy.Class;
                    var acl = this.AccessControlLists[v];

                    return new SyncResponseObject
                    {
                        Id = v.Id,
                        Version = v.Strategy.ObjectVersion,
                        ObjectType = v.Strategy.Class.Tag,
                        // TODO: Cache
                        Roles = @class.DatabaseRoleTypes?.Where(v => v.RelationType.WorkspaceNames.Length > 0)
                            .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                            .Select(w => CreateSyncResponseRole(v, w))
                            .ToArray(),
                        AccessControls = this.accessControlsWriter.Write(v)?.ToArray(),
                        DeniedPermissions = this.permissionsWriter.Write(v)?.ToArray(),
                    };
                }).ToArray(),
                AccessControls = this.AccessControlLists.EffectivePermissionIdsByAccessControl.Keys
                    .Select(v => new[] { v.Strategy.ObjectId, v.Strategy.ObjectVersion })
                    .ToArray(),
            };
        }
    }
}
