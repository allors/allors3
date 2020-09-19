// <copyright file="PrefetchPolicyBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

namespace Allors
{
    using System.Linq;
    using Allors.Meta;

    public static partial class PrefetchPolicyBuilderExtensions
    {
        static PrefetchPolicyBuilderExtensions()
        {
        }

        public static void WithWorkspaceRules(this PrefetchPolicyBuilder @this, Class @class)
        {
            // TODO: Cache
            foreach (var roleType in @class.RoleTypes.Where(v => v.Workspace))
            {
                @this.WithRule(roleType);
            }
        }

        public static void WithSecurityRules(this PrefetchPolicyBuilder @this, Class @class, M m)
        {
            // TODO: Cache
            var AccessControlPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.AccessControl.CacheId)
                .Build();

            var SecurityTokenPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.SecurityToken.AccessControls, AccessControlPrefetchPolicy)
                .Build();

            if (@class.DelegatedAccessRoleTypes != null)
            {
                var builder = new PrefetchPolicyBuilder()
                    .WithRule(m.Object.SecurityTokens, SecurityTokenPrefetchPolicy)
                    .WithRule(m.Object.DeniedPermissions)
                    .Build();

                var delegatedAccessRoleTypes = @class.DelegatedAccessRoleTypes;
                foreach (var delegatedAccessRoleType in delegatedAccessRoleTypes)
                {
                    @this.WithRule(delegatedAccessRoleType, builder);
                }
            }

            @this.WithRule(m.Object.SecurityTokens, SecurityTokenPrefetchPolicy);
            @this.WithRule(m.Object.DeniedPermissions);
        }
    }
}
