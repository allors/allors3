// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public class WorkspaceAccessControlLists : IAccessControlLists
    {
        public WorkspaceAccessControlLists(string workspaceName, User user)
        {
            this.WorkspaceName = workspaceName;
            this.User = user;
            this.AclByObject = new Dictionary<IObject, IAccessControlList>();
            this.EffectivePermissionIdsByAccessControl = this.EffectivePermissionsByAccessControl();
        }

        public IReadOnlyDictionary<IAccessControl, ISet<long>> EffectivePermissionIdsByAccessControl { get; set; }

        public string WorkspaceName { get; }

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
            var effectivePermissionsByAccessControl = new Dictionary<IAccessControl, ISet<long>>();

            var session = this.User.Session();
            var database = session.Database;
            var effectivePermissionCache = database.State().WorkspaceEffectivePermissionCache;

            List<AccessControl> misses = null;
            foreach (AccessControl accessControl in this.User.AccessControlsWhereEffectiveUser)
            {
                var effectivePermissions = effectivePermissionCache.Get(this.WorkspaceName, accessControl.Id);
                if (effectivePermissions == null)
                {
                    misses ??= new List<AccessControl>();
                    misses.Add(accessControl);
                }
                else
                {
                    effectivePermissionsByAccessControl.Add(accessControl, effectivePermissions);
                }
            }

            if (misses != null)
            {
                if (misses.Count > 1)
                {
                    // TODO: Cache
                    var m = this.User.DatabaseState().M;

                    var permissionPrefetch = new PrefetchPolicyBuilder()
                        .WithRule(m.ReadPermission.RelationTypePointer)
                        .WithRule(m.WritePermission.RelationTypePointer)
                        .WithRule(m.ExecutePermission.MethodTypePointer)
                        .Build();

                    var prefetch = new PrefetchPolicyBuilder()
                        .WithRule(m.AccessControl.EffectivePermissions, permissionPrefetch)
                        .Build();

                    session.Prefetch(prefetch, misses);
                }

                foreach (var accessControl in misses)
                {
                    var workspaceEffectivePermissions = accessControl.EffectivePermissions.Where(v => v.InWorkspace(this.WorkspaceName));
                    var effectivePermissionIds = new HashSet<long>(workspaceEffectivePermissions.Select(v => v.Id));
                    effectivePermissionCache.Set(this.WorkspaceName, accessControl.Id, effectivePermissionIds);
                    effectivePermissionsByAccessControl.Add(accessControl, effectivePermissionIds);
                }
            }

            return effectivePermissionsByAccessControl;
        }
    }
}
