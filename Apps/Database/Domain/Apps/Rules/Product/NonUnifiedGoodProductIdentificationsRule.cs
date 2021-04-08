// <copyright file="NonUnifiedGoodDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;
    using Derivations;

    public class NonUnifiedGoodProductIdentificationsRule : Rule
    {
        public NonUnifiedGoodProductIdentificationsRule(MetaPopulation m) : base(m, new Guid("1D67AC19-4D77-441D-AC98-3F274FADFB2C")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedGood.RolePattern(v => v.ProductIdentifications),
                m.NonUnifiedGood.RolePattern(v => v.Keywords),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, m.NonUnifiedGood),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
            {
                var builder = new StringBuilder();
                if (@this.ExistProductIdentifications)
                {
                    builder.Append(string.Join(" ", @this.ProductIdentifications.Select(v => v.Identification)));
                }

                if (@this.ExistProductCategoriesWhereAllProduct)
                {
                    builder.Append(string.Join(" ", @this.ProductCategoriesWhereAllProduct.Select(v => v.Name)));
                }

                builder.Append(string.Join(" ", @this.Keywords));

                @this.SearchString = builder.ToString();
            }
        }
    }
}
