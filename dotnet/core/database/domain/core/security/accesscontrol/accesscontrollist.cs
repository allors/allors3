// <copyright file="AccessControlList.cs" company="Allors bvba">
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

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public class AccessControlList : IAccessControlList
    {
        private readonly IInternalAccessControl accessControl;
        private readonly IReadOnlyDictionary<Guid, long> readPermissionIdByRelationTypeId;
        private readonly IReadOnlyDictionary<Guid, long> writePermissionIdByRelationTypeId;
        private readonly IReadOnlyDictionary<Guid, long> executePermissionIdByMethodTypeId;

        private Grant[] grants;
        private Revocation[] revocations;

        private bool lazyLoaded;

        internal AccessControlList(IInternalAccessControl accessControl, IObject @object)
        {
            this.accessControl = accessControl;
            this.Object = (Object)@object;

            if (this.Object != null)
            {
                var @class = this.Object.Strategy.Class;
                this.readPermissionIdByRelationTypeId = @class.ReadPermissionIdByRelationTypeId;
                this.writePermissionIdByRelationTypeId = @class.WritePermissionIdByRelationTypeId;
                this.executePermissionIdByMethodTypeId = @class.ExecutePermissionIdByMethodTypeId;
            }

            this.lazyLoaded = false;
        }

        public Object Object { get; }



        IEnumerable<IGrant> IAccessControlList.Grants => this.Grants;
        public IEnumerable<Grant> Grants
        {
            get
            {
                this.LazyLoad();
                return this.grants;
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

        public bool CanRead(IRoleType roleType) => this.readPermissionIdByRelationTypeId?.TryGetValue(roleType.RelationType.Id, out var permissionId) == true && this.IsPermitted(permissionId);

        public bool CanWrite(IRoleType roleType) => !roleType.RelationType.IsDerived && this.writePermissionIdByRelationTypeId?.TryGetValue(roleType.RelationType.Id, out var permissionId) == true && this.IsPermitted(permissionId);

        public bool CanExecute(IMethodType methodType) => this.executePermissionIdByMethodTypeId?.TryGetValue(methodType.Id, out var permissionId) == true && this.IsPermitted(permissionId);

        public bool IsMasked() => this.accessControl.IsMasked(this.Object);

        private bool IsPermitted(long permissionId)
        {
            this.LazyLoad();

            if (this.grants.Any(v => this.accessControl.GrantedPermissionIds(v).Contains(permissionId)))
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
                var delegatedAccess = this.Object is DelegatedAccessObject del ? del.DelegatedAccess : null;

                // Grants
                {
                    IEnumerable<SecurityToken> tokens = null;
                    if (delegatedAccess?.ExistSecurityTokens == true)
                    {
                        tokens = this.Object.ExistSecurityTokens ? delegatedAccess.SecurityTokens.Concat(this.Object.SecurityTokens) : delegatedAccess.SecurityTokens;
                    }
                    else if (this.Object.ExistSecurityTokens)
                    {
                        tokens = this.Object.SecurityTokens;
                    }

                    if (tokens == null)
                    {
                        var securityTokens = new SecurityTokens(transaction);
                        tokens = strategy.IsNewInTransaction
                            ? new[] { securityTokens.InitialSecurityToken ?? securityTokens.DefaultSecurityToken }
                            : new[] { securityTokens.DefaultSecurityToken };
                    }

                    this.grants = this.accessControl.Filter(tokens.SelectMany(v => v.Grants)).Distinct().ToArray();
                }

                // Revocations
                {
                    IEnumerable<Revocation> unfilteredRevocations = null;
                    if (delegatedAccess?.ExistRevocations == true)
                    {
                        unfilteredRevocations = this.Object.ExistRevocations ? this.Object.Revocations.Concat(delegatedAccess.Revocations.Where(v => !this.Object.Revocations.Contains(v))) : delegatedAccess.Revocations;
                    }
                    else if (this.Object.ExistRevocations)
                    {
                        unfilteredRevocations = this.Object.Revocations;
                    }

                    this.revocations = unfilteredRevocations != null ? this.accessControl.Filter(unfilteredRevocations) : Array.Empty<Revocation>();
                }

                this.lazyLoaded = true;
            }
        }
    }
}
