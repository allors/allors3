// <copyright file="SyncResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Meta;
    using Allors.Protocol.Json.Api.Sync;
    using Ranges;
    using Security;

    public class SyncResponseBuilder
    {
        private readonly IUnitConvert unitConvert;
        private readonly IRanges ranges;

        private readonly AccessControlsWriter accessControlsWriter;
        private readonly PermissionsWriter permissionsWriter;

        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly Action<IEnumerable<IObject>> prefetch;

        public SyncResponseBuilder(ITransaction transaction, IAccessControlLists accessControlLists, ISet<IClass> allowedClasses, Action<IEnumerable<IObject>> prefetch, IUnitConvert unitConvert, IRanges ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.prefetch = prefetch;
            this.unitConvert = unitConvert;
            this.ranges = ranges;

            this.AccessControlLists = accessControlLists;

            this.accessControlsWriter = new AccessControlsWriter(this.AccessControlLists);
            this.permissionsWriter = new PermissionsWriter(this.AccessControlLists);
        }

        public IAccessControlLists AccessControlLists { get; }

        public SyncResponse Build(SyncRequest syncRequest)
        {
            var objects = this.transaction.Instantiate(syncRequest.o)
                .Where(v => this.allowedClasses?.Contains(v.Strategy.Class) == true)
                .ToArray();

            this.prefetch(objects);

            return new SyncResponse
            {
                o = objects.Select(v =>
                {
                    var @class = v.Strategy.Class;
                    var acl = this.AccessControlLists[v];

                    return new SyncResponseObject
                    {
                        i = v.Id,
                        v = v.Strategy.ObjectVersion,
                        t = v.Strategy.Class.Tag,
                        // TODO: Cache
                        r = @class.DatabaseRoleTypes?.Where(v => v.RelationType.WorkspaceNames.Length > 0)
                            .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                            .Select(w => this.CreateSyncResponseRole(v, w, this.unitConvert))
                            .ToArray(),
                        a = this.ranges.ImportFrom(this.accessControlsWriter.Write(v)).Save(),
                        d = this.ranges.ImportFrom(this.permissionsWriter.Write(v)).Save(),
                    };
                }).ToArray(),
                a = this.AccessControlLists.EffectivePermissionIdsByAccessControl.Keys
                    .Select(v => new[] { v.Strategy.ObjectId, v.Strategy.ObjectVersion })
                    .ToArray(),
            };
        }

        private SyncResponseRole CreateSyncResponseRole(IObject @object, IRoleType roleType, IUnitConvert unitConvert)
        {
            var syncResponseRole = new SyncResponseRole { t = roleType.RelationType.Tag };

            if (roleType.ObjectType.IsUnit)
            {
                syncResponseRole.v = unitConvert.ToJson(@object.Strategy.GetUnitRole(roleType));
            }
            else if (roleType.IsOne)
            {
                syncResponseRole.o = @object.Strategy.GetCompositeRole(roleType)?.Id;
            }
            else
            {
                var roles = @object.Strategy.GetCompositesRole<IObject>(roleType);
                syncResponseRole.c = this.ranges.ImportFrom(roles.Select(roleObject => roleObject.Id)).ToArray();
            }

            return syncResponseRole;
        }
    }
}
