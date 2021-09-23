// <copyright file="AccessControlList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
    using Database.Services;
    using Meta;

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public class AccessControlList : IAccessControlList
    {
        private readonly IInternalAccessControl accessControl;
        private readonly IPermissionsCacheEntry permissionsCacheEntry;

        private Grant[] accessControls;
        private Revocation[] revocations;

        private bool lazyLoaded;

        internal AccessControlList(IInternalAccessControl accessControl, IObject @object, IPermissionsCache permissionsCache)
        {
            this.accessControl = accessControl;
            this.Object = (Object)@object;
            this.permissionsCacheEntry = permissionsCache.Get(this.Object.Strategy.Class.Id);

            this.lazyLoaded = false;
        }

        IEnumerable<IGrant> IAccessControlList.Grants => this.Grants;
        public IEnumerable<Grant> Grants
        {
            get
            {
                this.LazyLoad();
                return this.accessControls;
            }
        }

        IEnumerable<IRevocation> IAccessControlList.Revocations => this.Revocations;
        public Revocation[] Revocations
        {
            get
            {
                this.LazyLoad();
                return this.revocations;
            }
        }

        public Object Object { get; }

        public bool CanRead(IRoleType roleType)
        {
            if (this.Object != null && this.permissionsCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
            {
                return this.IsPermitted(permissionId);
            }

            return false;
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (roleType.RelationType.IsDerived)
            {
                return false;
            }

            if (this.Object != null && this.permissionsCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(roleType.RelationType.Id, out var permissionId))
            {
                return this.IsPermitted(permissionId);
            }

            return false;
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (this.Object != null && this.permissionsCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(methodType.Id, out var permissionId))
            {
                return this.IsPermitted(permissionId);
            }

            return false;
        }

        private bool IsPermitted(long permissionId)
        {
            this.LazyLoad();

            if (this.accessControls.Any(v => this.accessControl.GrantedPermissionIds(v).Contains(permissionId)))
            {
                return this.revocations?.Any(v => this.accessControl.RevokedPermissionIds(v).Contains(permissionId)) != true;
            }

            return false;
        }

        private void LazyLoad()
        {
            if (!this.lazyLoaded)
            {
                var strategy = this.Object.Strategy;
                var transaction = strategy.Transaction;

                Revocation[] delegatedAccessDeniedPermissions = null;

                SecurityToken[] securityTokens;
                if (this.Object is DelegatedAccessObject controlledObject)
                {
                    var delegatedAccess = controlledObject.DelegateAccess();
                    securityTokens = delegatedAccess.SecurityTokens;
                    if (securityTokens?.Any(v => v == null) == true)
                    {
                        securityTokens = securityTokens.Where(v => v != null).ToArray();
                    }

                    delegatedAccessDeniedPermissions = delegatedAccess.Revocations;
                }
                else
                {
                    securityTokens = this.Object.SecurityTokens.ToArray();
                }

                var unfilteredRevocations = delegatedAccessDeniedPermissions?.Length > 0
                    ? this.Object.Revocations.Union(delegatedAccessDeniedPermissions)
                    : this.Object.Revocations;

                this.revocations = this.accessControl.Filter(unfilteredRevocations);

                if (securityTokens == null || securityTokens.Length == 0)
                {
                    var tokens = new SecurityTokens(transaction);
                    securityTokens = strategy.IsNewInTransaction
                                          ? new[] { tokens.InitialSecurityToken ?? tokens.DefaultSecurityToken }
                                          : new[] { tokens.DefaultSecurityToken };
                }

                this.accessControls = this.accessControl.Filter(securityTokens.SelectMany(v => v.Grants).Distinct());

                this.lazyLoaded = true;
            }
        }
    }
}
