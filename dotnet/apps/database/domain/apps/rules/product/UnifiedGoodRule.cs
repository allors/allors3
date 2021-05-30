// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class UnifiedGoodRule : Rule
    {
        public UnifiedGoodRule(MetaPopulation m) : base(m, new Guid("B1C14106-C300-453D-989B-81E05767CFC4")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedGood.RolePattern(v => v.DerivationTrigger),
                m.UnifiedGood.RolePattern(v => v.Variants),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.UnifiedGood),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                if (@this.ExistCurrentVersion)
                {
                    foreach (Good variant in @this.CurrentVersion.Variants)
                    {
                        if (!@this.Variants.Contains(variant))
                        {
                            variant.RemoveVirtualProductPriceComponents();
                        }
                    }
                }

                if (@this.ExistVariants)
                {
                    @this.RemoveVirtualProductPriceComponents();

                    var priceComponents = @this.PriceComponentsWhereProduct;

                    foreach (Good variant in @this.Variants)
                    {
                        foreach (PriceComponent priceComponent in priceComponents)
                        {
                            variant.AddVirtualProductPriceComponent(priceComponent);

                            if (priceComponent is BasePrice basePrice && !priceComponent.ExistProductFeature)
                            {
                                variant.AddBasePrice(basePrice);
                            }
                        }
                    }
                }
            }
        }
    }
}
