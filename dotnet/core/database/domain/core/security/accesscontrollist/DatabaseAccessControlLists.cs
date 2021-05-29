// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
   

    public class DatabaseAccessControlLists : IAccessControlLists
    {
        public DatabaseAccessControlLists(User user)
        {
            this.User = user;
            this.AclByObject = new Dictionary<IObject, IAccessControlList>();
            this.EffectivePermissionIdsByAccessControl = this.EffectivePermissionsByAccessControl();
        }

        public IEnumerable<IAccessControl> AccessControls => this.AclByObject.SelectMany(v => v.Value.AccessControls).Distinct();

        public IReadOnlyDictionary<IAccessControl, ISet<long>> EffectivePermissionIdsByAccessControl { get;  }

        public User User { get; }

        private Dictionary<IObject, IAccessControlList> AclByObject { get; }

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (!this.AclByObject.TryGetValue(@object, out var acl))
                {
                    acl = new AccessControlList(this, @object);
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
            var accessControlCache = database.Services().AccessControlCache;
            
            List<AccessControl> misses = null;
            foreach (AccessControl accessControl in this.User.AccessControlsWhereEffectiveUser)
            {
                var effectivePermissions = accessControlCache.GetPermissions(accessControl.Id);
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
                    var m = database.Services().M;
                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.AccessControl.EffectivePermissions)
                        .Build();

                    transaction.Prefetch(prefetchPolicy, misses);
                }

                foreach (var accessControl in misses)
                {
                    var effectivePermissionIds = new HashSet<long>(accessControl.EffectivePermissions.Select(v => v.Id));
                    accessControlCache.SetPermissions(accessControl.Id, effectivePermissionIds);
                    permissionsByAccessControl.Add(accessControl, effectivePermissionIds);
                }
            }

            return permissionsByAccessControl;
        }
    }
}
