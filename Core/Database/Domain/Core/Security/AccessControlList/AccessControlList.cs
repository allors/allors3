// <copyright file="AccessControlList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Allors.Meta;

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public class AccessControlList : IAccessControlList
    {
        private readonly Guid classId;

        private AccessControl[] accessControls;
        private HashSet<long> deniedPermissions;
        private bool lazyLoaded;
        private PermissionCacheEntry permissionCacheEntry;

        internal AccessControlList(IAccessControlLists accessControlLists, IObject @object)
        {
            this.AccessControlLists = accessControlLists;
            this.Object = (Object)@object;
            this.classId = this.Object.Strategy.Class.Id;

            var session = @object.Strategy.Session;
            var permissionCache = session.GetCache<PermissionCache, PermissionCache>(() => new PermissionCache(session));
            this.permissionCacheEntry = permissionCache.PermissionCacheEntryByClassId[this.classId];

            this.lazyLoaded = false;
        }

        public AccessControl[] AccessControls
        {
            get
            {
                this.LazyLoad();
                return this.accessControls;
            }
        }

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

        bool IAccessControlList.CanRead(IPropertyType propertyType) => propertyType is IRoleType type ? this.CanRead(type) : this.CanRead((IAssociationType)propertyType);

        public bool CanRead(IRoleType roleType)
        {
            if (this.Object != null)
            {
                if (this.permissionCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
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
                if (this.permissionCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
                {
                    return this.IsPermitted(permissionId);
                }
            }

            return false;
        }

        public bool CanRead(IAssociationType roleType)
        {
            if (this.Object != null)
            {
                if (this.permissionCacheEntry.AssociationReadPermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
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
                if (this.permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(methodType.Id, out var permissionId))
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
                var session = strategy.Session;

                SecurityToken[] securityTokens;
                if (this.Object is DelegatedAccessControlledObject controlledObject)
                {
                    var delegatedAccess = controlledObject.DelegateAccess();
                    securityTokens = delegatedAccess.SecurityTokens;
                    if (securityTokens != null && securityTokens.Any(v => v == null))
                    {
                        securityTokens = securityTokens.Where(v => v != null).ToArray();
                    }

                    var delegatedAccessDeniedPermissions = delegatedAccess.DeniedPermissions;
                    if (delegatedAccessDeniedPermissions != null && delegatedAccessDeniedPermissions.Length > 0)
                    {
                        this.deniedPermissions = this.Object.DeniedPermissions.Count > 0 ?
                                                     new HashSet<long>(this.Object.DeniedPermissions.Union(delegatedAccessDeniedPermissions).Select(v => v.Id)) :
                                                     new HashSet<long>(delegatedAccessDeniedPermissions.Select(v => v.Id));
                    }
                    else if (this.Object.DeniedPermissions.Count > 0)
                    {
                        this.deniedPermissions = new HashSet<long>(this.Object.DeniedPermissions.Select(v => v.Id));
                    }
                }
                else
                {
                    securityTokens = this.Object.SecurityTokens;

                    if (this.Object.DeniedPermissions.Count > 0)
                    {
                        this.deniedPermissions = new HashSet<long>(this.Object.DeniedPermissions.Select(v => v.Id));
                    }
                }

                if (securityTokens == null || securityTokens.Length == 0)
                {
                    var tokens = new SecurityTokens(session);
                    securityTokens = strategy.IsNewInSession
                                          ? new[] { tokens.InitialSecurityToken ?? tokens.DefaultSecurityToken }
                                          : new[] { tokens.DefaultSecurityToken };
                }

                this.accessControls = securityTokens.SelectMany(v => v.AccessControls)
                    .Distinct()
                    .Where(this.AccessControlLists.EffectivePermissionIdsByAccessControl.ContainsKey)
                    .ToArray();

                var permissionCache = session.GetCache<PermissionCache, PermissionCache>(() => new PermissionCache(session));
                this.permissionCacheEntry = permissionCache.PermissionCacheEntryByClassId[this.classId];

                this.lazyLoaded = true;
            }
        }
    }
}
