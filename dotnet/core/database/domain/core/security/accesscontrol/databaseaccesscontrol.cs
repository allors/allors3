// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
    using Meta;
    using Ranges;
    using Allors.Database.Services;

    public class DatabaseAccessControl : IInternalAccessControl
    {
        private readonly IReadOnlyDictionary<IGrant, IRange<long>> permissionIdsByGrant;
        private readonly IDictionary<IRevocation, IRange<long>> permissionIdsByRevocation;
        private readonly Dictionary<IObject, IAccessControlList> aclByObject;

        private readonly IRanges<long> ranges;
        private readonly IPermissionsCache permissionCache;

        public DatabaseAccessControl(User user)
        {
            var services = user.Strategy.Transaction.Database.Services;

            this.ranges = services.Get<IRanges<long>>();
            this.permissionCache = services.Get<IPermissionsCache>();

            this.User = user;
            this.aclByObject = new Dictionary<IObject, IAccessControlList>();
            this.permissionIdsByGrant = this.BuildEffectivePermissionsByGrant();
            this.permissionIdsByRevocation = new Dictionary<IRevocation, IRange<long>>();
        }

        public User User { get; }

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (!this.aclByObject.TryGetValue(@object, out var acl))
                {
                    acl = new AccessControlList(this, @object, this.permissionCache);
                    this.aclByObject.Add(@object, acl);
                }

                return acl;
            }
        }

        public IRange<long> GrantedPermissionIds(IGrant grant) => this.permissionIdsByGrant[grant];

        // TODO: Optimize
        public IRange<long> RevokedPermissionIds(IRevocation revocation)
        {
            if (!this.permissionIdsByRevocation.TryGetValue(revocation, out var permissionIds))
            {
                permissionIds = this.ranges.Import(revocation.Permissions.Select(v => v.Id));
                this.permissionIdsByRevocation.Add(revocation, permissionIds);
            }

            return permissionIds;
        }

        public Grant[] Filter(IEnumerable<Grant> unfilteredGrants) => unfilteredGrants.Where(v => this.permissionIdsByGrant.ContainsKey(v)).ToArray();

        public Revocation[] Filter(IEnumerable<Revocation> unfilteredRevocations) => unfilteredRevocations.ToArray();

        private Dictionary<IGrant, IRange<long>> BuildEffectivePermissionsByGrant()
        {
            var permissionsByAccessControl = new Dictionary<IGrant, IRange<long>>();

            var transaction = this.User.Transaction();
            var database = transaction.Database;
            var accessControlCache = database.Services.Get<IGrantCache>();

            List<Grant> misses = null;
            foreach (var accessControl in this.User.GrantsWhereEffectiveUser)
            {
                var effectivePermissions = accessControlCache.GetPermissions(accessControl.Id);
                if (effectivePermissions == null)
                {
                    misses ??= new List<Grant>();
                    misses.Add(accessControl);
                }
                else
                {
                    permissionsByAccessControl.Add(accessControl, effectivePermissions);
                }
            }

            if (misses != null)
            {
                if (misses.Count > 1)
                {
                    // TODO: Cache
                    var m = database.Services.Get<MetaPopulation>();
                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.Grant.EffectivePermissions)
                        .Build();

                    transaction.Prefetch(prefetchPolicy, misses);
                }

                foreach (var range in misses)
                {
                    var effectivePermissionIds = this.ranges.Import(range.EffectivePermissions.Select(v => v.Id));
                    accessControlCache.SetPermissions(range.Id, effectivePermissionIds);
                    permissionsByAccessControl.Add(range, effectivePermissionIds);
                }
            }

            return permissionsByAccessControl;
        }
    }
}
