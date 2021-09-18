// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
    using Meta;

    public class WorkspaceAccessControlLists : IInternalAccessControlLists
    {
        private readonly string workspaceName;
        private readonly IReadOnlyDictionary<IGrant, ISet<long>> permissionIdsByGrant;
        private readonly IDictionary<IRevocation, ISet<long>> permissionIdsByRevocation;
        private readonly Dictionary<IObject, IAccessControlList> aclByObject;

        public WorkspaceAccessControlLists(string workspaceName, User user)
        {
            this.workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.aclByObject = new Dictionary<IObject, IAccessControlList>();
            this.permissionIdsByGrant = this.BuildEffectivePermissionsByGrant();
            this.permissionIdsByRevocation = new Dictionary<IRevocation, ISet<long>>();
        }

        public User User { get; }

        // TODO: Optimize
        public IEnumerable<IGrant> Grants => this.aclByObject.SelectMany(v => v.Value.Grants).Distinct();

        // TODO: Optimize
        public IEnumerable<IRevocation> Revocations => this.aclByObject.SelectMany(v => v.Value.Revocations).Distinct();

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (!this.aclByObject.TryGetValue(@object, out var acl))
                {
                    acl = new AccessControlList(this, @object);
                    this.aclByObject.Add(@object, acl);
                }

                return acl;
            }
        }

        public ISet<long> GrantedPermissionIds(IGrant grant) => this.permissionIdsByGrant[grant];

        // TODO: Optimize
        public ISet<long> RevokedPermissionIds(IRevocation revocation)
        {
            if (!this.permissionIdsByRevocation.TryGetValue(revocation, out var permissionIds))
            {
                permissionIds = new HashSet<long>(revocation.DeniedPermissions.Where(v => v.InWorkspace(this.workspaceName)).Select(v => v.Id));
                this.permissionIdsByRevocation.Add(revocation, permissionIds);
            }

            return permissionIds;
        }

        // TODO: Optimize
        public Grant[] Filter(IEnumerable<Grant> unfilteredGrants) => unfilteredGrants.Where(v => this.permissionIdsByGrant.ContainsKey(v) && v.EffectivePermissions.Any(w => w.InWorkspace(this.workspaceName))).ToArray();

        // TODO: Optimize
        public Revocation[] Filter(IEnumerable<Revocation> unfilteredRevocations) => unfilteredRevocations.Where(v => v.DeniedPermissions.Any(w => w.InWorkspace(this.workspaceName))).ToArray();

        private Dictionary<IGrant, ISet<long>> BuildEffectivePermissionsByGrant()
        {
            var permissionsByAccessControl = new Dictionary<IGrant, ISet<long>>();

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
                    var effectivePermissionIds = new HashSet<long>(workspaceEffectivePermissions.Select(v => v.Id));
                    accessControlCache.SetPermissions(this.workspaceName, accessControl.Id, effectivePermissionIds);
                    permissionsByAccessControl.Add(accessControl, effectivePermissionIds);
                }
            }

            return permissionsByAccessControl;
        }
    }
}
