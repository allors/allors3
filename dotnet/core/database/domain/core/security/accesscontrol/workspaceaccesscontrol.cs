// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Database.Security;
    using Meta;
    using Ranges;
    using Allors.Database.Services;

    public class WorkspaceAccessControl : IInternalAccessControl
    {
        private readonly string workspaceName;
        private readonly IReadOnlyDictionary<IGrant, IRange<long>> permissionIdsByGrant;
        private readonly IDictionary<IRevocation, IRange<long>> permissionIdsByRevocation;
        private readonly Dictionary<IObject, IAccessControlList> aclByObject;

        private readonly IRanges<long> ranges;
        private readonly IPermissionsCache permissionCache;

        public WorkspaceAccessControl(string workspaceName, User user)
        {
            var services = user.Strategy.Transaction.Database.Services;

            this.ranges = services.Get<IRanges<long>>();
            this.permissionCache = services.Get<IPermissionsCache>();
            
            this.workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.User = user ?? throw new ArgumentNullException(nameof(user));
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
                permissionIds = this.ranges.Import(revocation.Permissions.Where(v => v.InWorkspace(this.workspaceName)).Select(v => v.Id));
                this.permissionIdsByRevocation.Add(revocation, permissionIds);
            }

            return permissionIds;
        }

        // TODO: Optimize
        public Grant[] Filter(IEnumerable<Grant> unfilteredGrants) => unfilteredGrants.Where(v => this.permissionIdsByGrant.ContainsKey(v) && v.EffectivePermissions.Any(w => w.InWorkspace(this.workspaceName))).ToArray();

        // TODO: Optimize
        public Revocation[] Filter(IEnumerable<Revocation> unfilteredRevocations) => unfilteredRevocations.Where(v => v.DeniedPermissions.Any(w => w.InWorkspace(this.workspaceName))).ToArray();

        private Dictionary<IGrant, IRange<long>> BuildEffectivePermissionsByGrant()
        {
            var permissionsByAccessControl = new Dictionary<IGrant, IRange<long>>();

            var transaction = this.User.Transaction();
            var database = transaction.Database;
            var accessControlCache = database.Services.Get<IGrantCache>();

            List<Grant> misses = null;
            foreach (var accessControl in this.User.GrantsWhereEffectiveUser)
            {
                var effectivePermissions = accessControlCache.GetPermissions(this.workspaceName, accessControl.Id);
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
                    var m = this.User.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

                    var permissionPrefetch = new PrefetchPolicyBuilder()
                        .WithRule(m.ReadPermission.RelationTypePointer)
                        .WithRule(m.WritePermission.RelationTypePointer)
                        .WithRule(m.ExecutePermission.MethodTypePointer)
                        .Build();

                    var prefetch = new PrefetchPolicyBuilder()
                        .WithRule(m.Grant.EffectivePermissions, permissionPrefetch)
                        .Build();

                    transaction.Prefetch(prefetch, misses);
                }

                foreach (var accessControl in misses)
                {
                    var workspaceEffectivePermissions = accessControl.EffectivePermissions.Where(v => v.InWorkspace(this.workspaceName));
                    var effectivePermissionIds = this.ranges.Import(workspaceEffectivePermissions.Select(v => v.Id));
                    accessControlCache.SetPermissions(this.workspaceName, accessControl.Id, effectivePermissionIds);
                    permissionsByAccessControl.Add(accessControl, effectivePermissionIds);
                }
            }

            return permissionsByAccessControl;
        }
    }
}
