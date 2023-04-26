// <copyright file="SyncResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
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
        private readonly IDictionary<IClass, PrefetchPolicy> prefetchPolicyByClass;

        private readonly HashSet<IObject> maskedObjects;

        public SyncResponseBuilder(ITransaction transaction,
            IAccessControl accessControl,
            ISet<IClass> allowedClasses,
            IDictionary<IClass, ISet<IRoleType>> roleTypesByClass,
            IDictionary<IClass, PrefetchPolicy> prefetchPolicyByClass,
            IUnitConvert unitConvert,
            IRanges<long> ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.roleTypesByClass = roleTypesByClass;
            this.prefetchPolicyByClass = prefetchPolicyByClass;
            this.unitConvert = unitConvert;
            this.ranges = ranges;
            this.maskedObjects = new HashSet<IObject>();

            this.AccessControl = accessControl;
        }

        public IAccessControl AccessControl { get; }

        public SyncResponse Build(SyncRequest syncRequest)
        {
            var requestObjects = this.transaction.Instantiate(syncRequest.o);

            foreach (var groupBy in requestObjects.GroupBy(v => v.Strategy.Class, v => v))
            {
                var prefetchClass = groupBy.Key;
                var prefetchObjects = groupBy;
                if (this.prefetchPolicyByClass.TryGetValue(prefetchClass, out var prefetchPolicy))
                {
                    this.transaction.Prefetch(prefetchPolicy, prefetchObjects);
                }
            }

            return new SyncResponse
            {
                o = requestObjects.Where(this.Include).Select(v =>
                {
                    var @class = v.Strategy.Class;
                    var acl = this.AccessControl[v];
                    var roleTypes = this.roleTypesByClass[@class];

                    return new SyncResponseObject
                    {
                        i = v.Id,
                        v = v.Strategy.ObjectVersion,
                        c = v.Strategy.Class.Tag,
                        ro = roleTypes
                            .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                            .Select(w => this.CreateSyncResponseRole(v, w, this.unitConvert))
                            .ToArray(),
                        g = this.ranges.Import(acl.Grants.Select(w => w.Id)).Save(),
                        r = this.ranges.Import(acl.Revocations.Select(w => w.Id)).Save(),
                    };
                }).ToArray(),
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
                var role = @object.Strategy.GetCompositeRole(roleType);
                if (this.Include(role))
                {
                    syncResponseRole.o = role.Id;
                }
            }
            else
            {
                var roles = @object.Strategy.GetCompositesRole<IObject>(roleType).Where(this.Include);
                syncResponseRole.c = this.ranges.Import(roles.Select(roleObject => roleObject.Id)).ToArray();
            }

            return syncResponseRole;
        }

        public bool Include(IObject @object)
        {
            if (@object == null || this.allowedClasses?.Contains(@object.Strategy.Class) != true || this.maskedObjects.Contains(@object))
            {
                return false;
            }

            if (this.AccessControl[@object].IsMasked())
            {
                this.maskedObjects.Add(@object);
                return false;
            }

            return true;
        }
    }
}
