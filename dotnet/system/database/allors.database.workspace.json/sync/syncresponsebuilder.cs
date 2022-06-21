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
        private readonly IRanges<long> ranges;

        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly IDictionary<IClass, ISet<IRoleType>> roleTypesByClass;
        private readonly Action<IEnumerable<IObject>> prefetch;

        private readonly HashSet<IObject> maskedObjects;

        public SyncResponseBuilder(ITransaction transaction,
            IAccessControl accessControl,
            ISet<IClass> allowedClasses,
            IDictionary<IClass, ISet<IRoleType>> roleTypesByClass,
            Action<IEnumerable<IObject>> prefetch,
            IUnitConvert unitConvert,
            IRanges<long> ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.roleTypesByClass = roleTypesByClass;
            this.prefetch = prefetch;
            this.unitConvert = unitConvert;
            this.ranges = ranges;
            this.maskedObjects = new HashSet<IObject>();

            this.AccessControl = accessControl;
        }

        public IAccessControl AccessControl { get; }

        public SyncResponse Build(SyncRequest syncRequest)
        {
            var requestObjects = this.transaction.Instantiate(syncRequest.o);

            this.prefetch(requestObjects);

            var acls = this.AccessControl.GetAccessControlLists(this.transaction, requestObjects);
            var objects = requestObjects.Where(v => this.Include(v, acls)).ToArray();

            var roles = new HashSet<IObject>();
            foreach (var @object in objects)
            {
                var @class = @object.Strategy.Class;
                var acl = acls[@object];
                var roleTypes = this.roleTypesByClass[@class];
                foreach (var roleType in roleTypes.Where(v => v.ObjectType.IsComposite))
                {
                    if (acl.CanRead(roleType) && @object.Strategy.ExistRole(roleType))
                    {
                        if (roleType.IsOne)
                        {
                            roles.Add(@object.Strategy.GetCompositeRole(roleType));
                        }
                        else
                        {
                            roles.UnionWith(@object.Strategy.GetCompositesRole<IObject>(roleType));
                        }
                    }
                }
            }

            var roleAcls = this.AccessControl.GetAccessControlLists(this.transaction, roles);

            return new SyncResponse
            {
                o = objects.Select(v =>
                {
                    var @class = v.Strategy.Class;
                    var acl = acls[v];
                    var roleTypes = this.roleTypesByClass[@class];

                    return new SyncResponseObject
                    {
                        i = v.Id,
                        v = v.Strategy.ObjectVersion,
                        c = v.Strategy.Class.Tag,
                        ro = roleTypes
                            .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                            .Select(w => this.CreateSyncResponseRole(v, w, this.unitConvert, roleAcls))
                            .ToArray(),
                        g = this.ranges.Import(acl.Grants.Select(v => v.Id)).Save(),
                        r = this.ranges.Import(acl.Revocations.Select(v => v.Id)).Save(),
                    };
                }).ToArray(),
            };
        }

        private SyncResponseRole CreateSyncResponseRole(IObject @object, IRoleType roleType, IUnitConvert unitConvert, IDictionary<IObject, IAccessControlList> accessControlLists)
        {
            var syncResponseRole = new SyncResponseRole { t = roleType.RelationType.Tag };

            if (roleType.ObjectType.IsUnit)
            {
                syncResponseRole.v = unitConvert.ToJson(@object.Strategy.GetUnitRole(roleType));
            }
            else if (roleType.IsOne)
            {
                var role = @object.Strategy.GetCompositeRole(roleType);
                if (this.Include(role, accessControlLists))
                {
                    syncResponseRole.o = role.Id;
                }
            }
            else
            {
                var roles = @object.Strategy.GetCompositesRole<IObject>(roleType).Where(v => this.Include(v, accessControlLists));
                syncResponseRole.c = this.ranges.Import(roles.Select(roleObject => roleObject.Id)).ToArray();
            }

            return syncResponseRole;
        }

        public bool Include(IObject @object, IDictionary<IObject, IAccessControlList> accessControlLists)
        {
            if (@object == null || this.allowedClasses?.Contains(@object.Strategy.Class) != true || this.maskedObjects.Contains(@object))
            {
                return false;
            }

            if (accessControlLists[@object].IsMasked())
            {
                this.maskedObjects.Add(@object);
                return false;
            }

            return true;
        }
    }
}
