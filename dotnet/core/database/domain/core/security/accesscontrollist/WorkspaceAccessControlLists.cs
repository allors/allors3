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

    public class WorkspaceAccessControlLists : IAccessControlLists
    {
        public WorkspaceAccessControlLists(string workspaceName, User user)
        {
            this.WorkspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.AclByObject = new Dictionary<IObject, IAccessControlList>();
            this.PermissionIdsByAccessControl = this.EffectivePermissionsByAccessControl();
            this.DeniedPermissionIdsByRestriction = new Dictionary<IRestriction, ISet<long>>();
        }

        public IReadOnlyDictionary<IAccessControl, ISet<long>> PermissionIdsByAccessControl { get; }

        public IDictionary<IRestriction, ISet<long>> DeniedPermissionIdsByRestriction { get; }

        // TODO: Optimize
        public Restriction[] Filter(IEnumerable<Restriction> unfilteredRestrictions) => unfilteredRestrictions.Where(v => v.DeniedPermissions.Any(w => w.InWorkspace(this.WorkspaceName))).ToArray();

        public string WorkspaceName { get; }

        public User User { get; }

        private Dictionary<IObject, IAccessControlList> AclByObject { get; }

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (!this.AclByObject.TryGetValue(@object, out var acl))
                {
                    acl = new AccessControlList(this, @object, this.WorkspaceName);
                    this.AclByObject.Add(@object, acl);
                }

                return acl;
            }
        }

        private Dictionary<IAccessControl, ISet<long>> EffectivePermissionsByAccessControl()
        {
            var permissionsByAccessControl = new Dictionary<IAccessControl, ISet<long>>();

            var transaction = this.User.Transaction();
            var database = transaction.Database;
            var accessControlCache = database.Services.Get<IAccessControlCache>();

            List<AccessControl> misses = null;
            foreach (var accessControl in this.User.AccessControlsWhereEffectiveUser)
            {
                var effectivePermissions = accessControlCache.GetPermissions(this.WorkspaceName, accessControl.Id);
                if (effectivePermissions == null)
                {
                    misses ??= new List<AccessControl>();
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
                        .WithRule(m.AccessControl.EffectivePermissions, permissionPrefetch)
                        .Build();

                    transaction.Prefetch(prefetch, misses);
                }

                foreach (var accessControl in misses)
                {
                    var workspaceEffectivePermissions = accessControl.EffectivePermissions.Where(v => v.InWorkspace(this.WorkspaceName));
                    var effectivePermissionIds = new HashSet<long>(workspaceEffectivePermissions.Select(v => v.Id));
                    accessControlCache.SetPermissions(this.WorkspaceName, accessControl.Id, effectivePermissionIds);
                    permissionsByAccessControl.Add(accessControl, effectivePermissionIds);
                }
            }

            return permissionsByAccessControl;
        }
    }
}
