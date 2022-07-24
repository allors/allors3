// <copyright file="PrefetchPolicyBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Database.Data;
    using Meta;

    public static class PrefetchPolicyBuilderExtensions
    {
        public static PrefetchPolicyBuilder WithSecurityRules(this PrefetchPolicyBuilder @this, MetaPopulation m)
        {
            // Object
            @this.WithRule(m.Object.SecurityTokens);
            @this.WithRule(m.Object.Revocations);

            // DelegatedAccessObject
            var delegatedAccessPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Object.SecurityTokens)
                .WithRule(m.Object.Revocations)
                .Build();

            @this.WithRule(m.DelegatedAccessObject.DelegatedAccess, delegatedAccessPolicy);

            return @this;
        }

        public static PrefetchPolicyBuilder WithWorkspaceRules(this PrefetchPolicyBuilder @this, MetaPopulation m, ISet<IRoleType> roleTypes)
        {
            @this.WithSecurityRules(m);

            foreach (var roleType in roleTypes)
            {
                if (roleType.ObjectType.IsComposite)
                {
                    var securityPrefetch = new PrefetchPolicyBuilder().WithSecurityRules(m).Build();
                    @this.WithRule(roleType, securityPrefetch);
                }
                else
                {
                    @this.WithRule(roleType);
                }
            }

            return @this;
        }

        public static PrefetchPolicyBuilder WithNodes(this PrefetchPolicyBuilder @this, Node[] treeNodes, MetaPopulation m)
        {
            foreach (var node in treeNodes)
            {
                @this.WithNode(node, m);
            }

            return @this;
        }

        private static PrefetchPolicyBuilder WithNode(this PrefetchPolicyBuilder @this, Node treeNode, MetaPopulation m)
        {
            if (treeNode.Nodes == null || treeNode.Nodes.Length == 0)
            {
                @this.WithRule(treeNode.PropertyType);
            }
            else
            {
                var nestedPrefetchPolicyBuilder = new PrefetchPolicyBuilder();
                foreach (var node in treeNode.Nodes)
                {
                    @this.WithNode(node, m);
                }

                var nestedPrefetchPolicy = nestedPrefetchPolicyBuilder.Build();
                @this.WithRule(treeNode.PropertyType, nestedPrefetchPolicy);
            }

            if (treeNode.PropertyType.ObjectType is Composite)
            {
                @this.WithSecurityRules(m);
            }

            return @this;
        }
    }
}
