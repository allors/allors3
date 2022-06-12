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

    public class WorkspaceAccessControl : IAccessControl
    {
        private readonly ISecurity security;
        private readonly IVersionedGrants userGrants;
        private readonly IDictionary<IClass, IRoleType> masks;
        private readonly string workspaceName;

        private readonly Dictionary<IObject, IAccessControlList> aclByObject;

        public WorkspaceAccessControl(ISecurity security, IVersionedGrants userGrants, IDictionary<IClass, IRoleType> masks, string workspaceName)
        {
            this.security = security;
            this.userGrants = userGrants;
            this.masks = masks;
            this.workspaceName = workspaceName;

            this.aclByObject = new Dictionary<IObject, IAccessControlList>();
        }

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (!this.aclByObject.TryGetValue(@object, out var acl))
                {
                    acl = this.GetAccessControlList((Object)@object);
                    this.aclByObject.Add(@object, acl);
                }

                return acl;
            }
        }
        public bool IsMasked(IObject @object)
        {
            if (!this.masks.TryGetValue(@object.Strategy.Class, out var mask))
            {
                return false;
            }

            var acl = this[@object];
            return !acl.CanRead(mask);
        }

        private WorkspaceAccessControlList GetAccessControlList(Object @object)
        {
            var strategy = @object.Strategy;
            var transaction = strategy.Transaction;
            var delegatedAccess = @object is DelegatedAccessObject del ? del.DelegatedAccess : null;

            Grant[] grants = null;
            Revocation[] revocations = null;

            // Grants
            {
                IEnumerable<SecurityToken> tokens = null;
                if (delegatedAccess?.ExistSecurityTokens == true)
                {
                    tokens = @object.ExistSecurityTokens ? delegatedAccess.SecurityTokens.Concat(@object.SecurityTokens) : delegatedAccess.SecurityTokens;
                }
                else if (@object.ExistSecurityTokens)
                {
                    tokens = @object.SecurityTokens;
                }

                if (tokens == null)
                {
                    var securityTokens = new SecurityTokens(transaction);
                    tokens = strategy.IsNewInTransaction
                        ? new[] { securityTokens.InitialSecurityToken ?? securityTokens.DefaultSecurityToken }
                        : new[] { securityTokens.DefaultSecurityToken };
                }

                grants = tokens.SelectMany(v => v.Grants).Distinct().Where(v => this.userGrants.Set.Contains(v.Id)).ToArray();
            }

            // Revocations
            {
                IEnumerable<Revocation> unfilteredRevocations = null;
                if (delegatedAccess?.ExistRevocations == true)
                {
                    unfilteredRevocations = @object.ExistRevocations ? @object.Revocations.Concat(delegatedAccess.Revocations.Where(v => !@object.Revocations.Contains(v))) : delegatedAccess.Revocations;
                }
                else if (@object.ExistRevocations)
                {
                    unfilteredRevocations = @object.Revocations;
                }

                revocations = unfilteredRevocations != null ? unfilteredRevocations.ToArray() : Array.Empty<Revocation>();
            }

            var grantsPermissions = this.security
                .GetGrantPermissions(transaction, grants, this.workspaceName)
                .Where(v => v.Value.Set.Any())
                .Select(v => v.Value)
                .ToArray();

            var revocationsPermissions = this.security
                .GetRevocationPermissions(transaction, revocations, this.workspaceName)
                .Where(v => v.Value.Set.Any())
                .Select(v => v.Value)
                .ToArray();


            return new WorkspaceAccessControlList(
                this,
                @object,
                grantsPermissions,
                revocationsPermissions);
        }
    }
}
