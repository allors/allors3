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

        public void Prepare(IEnumerable<IObject> objects)
        {
            ITransaction transaction = null;

            var grantsByObject = new Dictionary<Object, Grant[]>();
            var revocationsByObject = new Dictionary<Object, Revocation[]>();

            var allGrants = new HashSet<Grant>();
            var allRevocations = new HashSet<Revocation>();

            foreach (Object @object in objects)
            {
                transaction ??= @object.Strategy.Transaction;

                IEnumerable<SecurityToken> tokens = null;

                var delegatedAccess = @object is DelegatedAccessObject del ? del.DelegatedAccess : null;
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
                    tokens = @object.Strategy.IsNewInTransaction
                        ? new[] { securityTokens.InitialSecurityToken ?? securityTokens.DefaultSecurityToken }
                        : new[] { securityTokens.DefaultSecurityToken };
                }

                var grants = tokens.SelectMany(v => v.Grants).Distinct().Where(v => this.userGrants.Set.Contains(v.Id)).ToArray();
                grantsByObject.Add(@object, grants);

                allGrants.UnionWith(grants);

                IEnumerable<Revocation> revocationEnum = null;
                if (delegatedAccess?.ExistRevocations == true)
                {
                    revocationEnum = @object.ExistRevocations ? @object.Revocations.Concat(delegatedAccess.Revocations.Where(v => !@object.Revocations.Contains(v))) : delegatedAccess.Revocations;
                }
                else if (@object.ExistRevocations)
                {
                    revocationEnum = @object.Revocations;
                }

                var revocations = revocationEnum != null ? revocationEnum.ToArray() : Array.Empty<Revocation>();
                revocationsByObject.Add(@object, revocations);

                allRevocations.UnionWith(revocations);
            }

            var allGrantsPermissions = this.security.GetGrantPermissions(transaction, allGrants, this.workspaceName);
            var allRevocationsPermissions = this.security.GetRevocationPermissions(transaction, allRevocations, this.workspaceName);

            foreach (Object @object in objects)
            {
                var grants = grantsByObject[@object];
                var revocations = revocationsByObject[@object];

                var grantsPermissions = grants.Select(v => allGrantsPermissions[v]).Where(v => v.Set.Any()).ToArray();
                var revocationsPermission = revocations.Select(v => allRevocationsPermissions[v]).Where(v => v.Set.Any()).ToArray();

                var acl = new WorkspaceAccessControlList(this, @object, grantsPermissions, revocationsPermission);
                this.aclByObject[@object] = acl;
            }
        }

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (this.aclByObject.TryGetValue(@object, out var acl))
                {
                    return acl;
                }

                this.Prepare(new[] { @object });
                return this.aclByObject[@object];
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
    }
}
