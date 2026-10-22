// <copyright file="AccessControlListFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;

    public class WorkspaceAccessControl : IAccessControl
    {
        private readonly ISecurity security;
        private readonly IUser user;
        private readonly string workspaceName;

        private readonly Dictionary<IObject, IAccessControlList> aclByObject;

        public WorkspaceAccessControl(ISecurity security, IUser user, string workspaceName)
        {
            this.security = security;
            this.user = user;
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

        private WorkspaceAccessControlList GetAccessControlList(Object @object)
        {
            var strategy = @object.Strategy;
            var transaction = strategy.Transaction;
            var delegatedAccess = @object is DelegatedAccessObject del ? del.DelegatedAccess : null;

            IVersionedGrant[] versionedGrants;
            IVersionedRevocation[] versionedRevocations;

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

                versionedGrants = this.security.GetVersionedGrants(transaction, this.user, tokens.ToArray(), this.workspaceName);
            }

            // Revocations
            {
                IEnumerable<Revocation> revocations = null;
                if (delegatedAccess?.ExistRevocations == true)
                {
                    revocations = @object.ExistRevocations ? @object.Revocations.Union(delegatedAccess.Revocations) : delegatedAccess.Revocations;
                }
                else if (@object.ExistRevocations)
                {
                    revocations = @object.Revocations;
                }

                versionedRevocations = this.security
                    .GetVersionedRevocations(transaction, this.user, revocations?.ToArray() ?? Array.Empty<IRevocation>(), this.workspaceName)
                    .Where(v => v.PermissionSet.Any())
                    .ToArray();
            }

            return new WorkspaceAccessControlList(@object, versionedGrants, versionedRevocations);
        }

        private WorkspaceAccessControlList Create(IObject @object, IVersionedGrant[] grants, IVersionedRevocation[] revocations) => new WorkspaceAccessControlList(@object, grants, revocations);
    }
}
