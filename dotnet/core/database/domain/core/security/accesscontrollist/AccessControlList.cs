// <copyright file="AccessControlList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Security;

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public class AccessControlList : IAccessControlList
    {
        private readonly IPermissionsCacheEntry permissionsCacheEntry;

        private AccessControl[] accessControls;
        private HashSet<long> deniedPermissions;
        private bool lazyLoaded;

        internal AccessControlList(IAccessControlLists accessControlLists, IObject @object)
        {
            this.AccessControlLists = accessControlLists;
            this.Object = (Object)@object;

            var permissionsCache = @object.DatabaseServices().PermissionsCache;
            this.permissionsCacheEntry = permissionsCache.Get(this.Object.Strategy.Class.Id);

            this.lazyLoaded = false;
        }

        IEnumerable<IAccessControl> IAccessControlList.AccessControls => this.AccessControls;
        public IEnumerable<AccessControl> AccessControls
        {
            get
            {
                this.LazyLoad();
                return this.accessControls;
            }
        }

        ISet<long> IAccessControlList.DeniedPermissionIds => this.DeniedPermissionIds;
        public HashSet<long> DeniedPermissionIds
        {
            get
            {
                this.LazyLoad();
                return this.deniedPermissions;
            }
        }

        public Object Object { get; }

        private IAccessControlLists AccessControlLists { get; }

        public bool CanRead(IRoleType roleType)
        {
            if (this.Object != null)
            {
                if (this.permissionsCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
                {
                    return this.IsPermitted(permissionId);
                }
            }

            return false;
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (this.Object != null)
            {
                if (this.permissionsCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
                {
                    return this.IsPermitted(permissionId);
                }
            }

            return false;
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (this.Object != null)
            {
                if (this.permissionsCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(methodType.Id, out var permissionId))
                {
                    return this.IsPermitted(permissionId);
                }
            }

            return false;
        }

        private bool IsPermitted(long permissionId)
        {
            this.LazyLoad();

            if (this.deniedPermissions?.Contains(permissionId) == true)
            {
                return false;
            }

            return this.accessControls.Any(v => this.AccessControlLists.EffectivePermissionIdsByAccessControl[v].Contains(permissionId));
        }

        private void LazyLoad()
        {
            if (!this.lazyLoaded)
            {
                var strategy = this.Object.Strategy;
                var transaction = strategy.Transaction;

                SecurityToken[] securityTokens;
                if (this.Object is DelegatedAccessControlledObject controlledObject)
                {
                    var delegatedAccess = controlledObject.DelegateAccess();
                    securityTokens = delegatedAccess.SecurityTokens;
                    if (securityTokens?.Any(v => v == null) == true)
                    {
                        securityTokens = securityTokens.Where(v => v != null).ToArray();
                    }

                    var delegatedAccessDeniedPermissions = delegatedAccess.DeniedPermissions;
                    if (delegatedAccessDeniedPermissions?.Length > 0)
                    {
                        this.deniedPermissions = this.Object.DeniedPermissions.Any() ?
                                                     new HashSet<long>(this.Object.DeniedPermissions.Union(delegatedAccessDeniedPermissions).Select(v => v.Id)) :
                                                     new HashSet<long>(delegatedAccessDeniedPermissions.Select(v => v.Id));
                    }
                    else if (this.Object.DeniedPermissions.Any())
                    {
                        this.deniedPermissions = new HashSet<long>(this.Object.DeniedPermissions.Select(v => v.Id));
                    }
                }
                else
                {
                    securityTokens = this.Object.SecurityTokens.ToArray();

                    if (this.Object.DeniedPermissions.Any())
                    {
                        this.deniedPermissions = new HashSet<long>(this.Object.DeniedPermissions.Select(v => v.Id));
                    }
                }

                if (securityTokens == null || securityTokens.Length == 0)
                {
                    var tokens = new SecurityTokens(transaction);
                    securityTokens = strategy.IsNewInTransaction
                                          ? new[] { tokens.InitialSecurityToken ?? tokens.DefaultSecurityToken }
                                          : new[] { tokens.DefaultSecurityToken };
                }

                this.accessControls = securityTokens.SelectMany(v => v.AccessControls)
                    .Distinct()
                    .Where(this.AccessControlLists.EffectivePermissionIdsByAccessControl.ContainsKey)
                    .ToArray();

                this.lazyLoaded = true;
            }
        }
    }
}
