// <copyright file="SyncResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Sync;

    public class SyncResponseBuilder
    {
        private readonly AccessControlsWriter accessControlsWriter;
        private readonly PermissionsWriter permissionsWriter;

        private readonly ISession session;
        private readonly ISet<IClass> allowedClasses;
        private readonly Action<IEnumerable<IObject>> prefetch;

        public SyncResponseBuilder(ISession session, IAccessControlLists accessControlLists, ISet<IClass> allowedClasses, Action<IEnumerable<IObject>> prefetch)
        {
            this.session = session;
            this.allowedClasses = allowedClasses;
            this.prefetch = prefetch;

            this.AccessControlLists = accessControlLists;

            this.accessControlsWriter = new AccessControlsWriter(this.AccessControlLists);
            this.permissionsWriter = new PermissionsWriter(this.AccessControlLists);
        }

        public IAccessControlLists AccessControlLists { get; }

        public SyncResponse Build(SyncRequest syncRequest)
        {
            var objects = this.session.Instantiate(syncRequest.Objects)
                .Where(v => this.allowedClasses?.Contains(v.Strategy.Class) == true)
                .ToArray();

            this.prefetch(objects);

            static SyncResponseRole CreateSyncResponseRole(IObject @object, IRoleType roleType)
            {
                var syncResponseRole = new SyncResponseRole { RoleType = roleType.RelationType.IdAsString };

                if (roleType.ObjectType.IsUnit)
                {
                    syncResponseRole.Value = UnitConvert.ToString(@object.Strategy.GetUnitRole(roleType));
                }
                else if (roleType.IsOne)
                {
                    syncResponseRole.Value = @object.Strategy.GetCompositeRole(roleType)?.Id.ToString();
                }
                else
                {
                    var roles = @object.Strategy.GetCompositeRoles(roleType);
                    if (roles.Count > 0)
                    {
                        syncResponseRole.Value = string.Join(
                            separator: Encoding.Separator,
                            values: roles.Select(roleObject => roleObject.Id.ToString()));
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
                        Id = v.Id.ToString(),
                        Version = v.Strategy.ObjectVersion.ToString(),
                        ObjectTypeOrKey = v.Strategy.Class.IdAsString,
                        // TODO: Cache
                        Roles = @class.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Length > 0)
                            .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                            .Select(w => CreateSyncResponseRole(v, w))
                            .ToArray(),
                        AccessControls = this.accessControlsWriter.Write(v),
                        DeniedPermissions = this.permissionsWriter.Write(v),
                    };
                }).ToArray(),
                AccessControls = this.AccessControlLists.EffectivePermissionIdsByAccessControl.Keys
                    .Select(v => new[] { v.Strategy.ObjectId.ToString(), v.Strategy.ObjectVersion.ToString(), })
                    .ToArray(),
            };
        }
    }
}
