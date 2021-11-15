
// <copyright file="PullController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using Data;
    using Domain;
    using Meta;

    public class PullResponsePrefetcher : IPullPrefetchers
    {
        private readonly ITransaction transaction;
        private readonly MetaPopulation metaPopulation;

        public PullResponsePrefetcher(ITransaction transaction, MetaPopulation metaPopulation)
        {
            this.transaction = transaction;
            this.metaPopulation = metaPopulation;
        }

        public PrefetchPolicy ForInclude(IComposite composite, Node[] tree)
        {
            var builder = new PrefetchPolicyBuilder();
            builder.WithSecurityRules(composite, this.metaPopulation);
            if (tree != null)
            {
                builder.WithNodes(tree, this.metaPopulation);
            }

            return builder.Build();
        }

        public PrefetchPolicy ForDependency(IComposite composite, ISet<IPropertyType> propertyTypes)
        {
            var builder = new PrefetchPolicyBuilder();
            builder.WithSecurityRules(composite, this.metaPopulation);
            foreach (var propertyType in propertyTypes)
            {
                if (propertyType.ObjectType.IsComposite)
                {
                    var securityBuilder = new PrefetchPolicyBuilder();
                    securityBuilder.WithSecurityRules((Composite)propertyType.ObjectType, this.metaPopulation);
                    var security = securityBuilder.Build();
                    builder.WithRule(propertyType, security);
                }
                else
                {
                    builder.WithRule(propertyType);
                }

            }

            return builder.Build();
        }
    }
}
