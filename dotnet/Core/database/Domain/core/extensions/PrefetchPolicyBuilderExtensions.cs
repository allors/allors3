// <copyright file="PrefetchPolicyBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System.Linq;
    using Meta;

    public static class PrefetchPolicyBuilderExtensions
    {
        public static void WithWorkspaceRules(this PrefetchPolicyBuilder @this, IClass @class)
        {
            // TODO: Cache
            foreach (var roleType in @class.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Length > 0))
            {
                @this.WithRule(roleType);
            }
        }

        public static void WithSecurityRules(this PrefetchPolicyBuilder @this, Class @class, MetaPopulation m)
        {
            // TODO: Cache
            var AccessControlPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Grant.UniqueId)
                .Build();

            var SecurityTokenPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.SecurityToken.Grants, AccessControlPrefetchPolicy)
                .Build();

            if (@class.DelegatedAccessRoleTypes != null)
            {
                var builder = new PrefetchPolicyBuilder()
                    .WithRule(m.Object.SecurityTokens, SecurityTokenPrefetchPolicy)
                    .WithRule(m.Object.Revocations)
                    .Build();

                foreach (var delegatedAccessRoleType in @class.DelegatedAccessRoleTypes)
                {
                    @this.WithRule(delegatedAccessRoleType, builder);
                }
            }

            @this.WithRule(m.Object.SecurityTokens, SecurityTokenPrefetchPolicy);
            @this.WithRule(m.Object.Revocations);
        }
    }
}
