// <copyright file="PrefetchPolicyBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System.Linq;
    using Database.Data;
    using Meta;

    public static class PrefetchPolicyBuilderExtensions
    {
        public static PrefetchPolicyBuilder WithWorkspaceRules(this PrefetchPolicyBuilder @this, IClass @class)
        {
            // TODO: Cache
            foreach (var roleType in @class.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Length > 0))
            {
                @this.WithRule(roleType);
            }

            return @this;
        }

        public static PrefetchPolicyBuilder WithSecurityRules(this PrefetchPolicyBuilder @this, IComposite composite, MetaPopulation m)
        {
            // TODO: Cache

            var securityTokenPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.SecurityToken.Grants, new PrefetchPolicyBuilder()
                    .WithRule(m.Grant.UniqueId)
                    .Build())
                .Build();

            foreach (Class @class in composite.Classes)
            {
                if (@class.DelegatedAccessRoleTypes != null)
                {
                    var builder = new PrefetchPolicyBuilder()
                        .WithRule(m.Object.SecurityTokens, securityTokenPrefetchPolicy)
                        .WithRule(m.Object.Revocations)
                        .Build();

                    foreach (var delegatedAccessRoleType in @class.DelegatedAccessRoleTypes)
                    {
                        @this.WithRule(delegatedAccessRoleType, builder);
                    }
                }
            }

            @this.WithRule(m.Object.SecurityTokens, securityTokenPrefetchPolicy);
            @this.WithRule(m.Object.Revocations);

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

        public static PrefetchPolicyBuilder WithNode(this PrefetchPolicyBuilder @this, Node treeNode, MetaPopulation m)
        {
            if (treeNode.Nodes == null || treeNode.Nodes.Length == 0)
            {
                @this.WithRule(treeNode.PropertyType);

                var @class = ((Composite)treeNode.PropertyType.ObjectType).Classes.FirstOrDefault() as Class;
                @this.WithSecurityRules(@class, m);
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

            return @this;
        }
    }
}
