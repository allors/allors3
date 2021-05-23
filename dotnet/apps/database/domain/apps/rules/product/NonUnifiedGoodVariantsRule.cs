// <copyright file="NonUnifiedGoodDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class NonUnifiedGoodVariantsRule : Rule
    {
        public NonUnifiedGoodVariantsRule(MetaPopulation m) : base(m, new Guid("438788be-d450-446d-aca6-54f9027fa67f")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedGood.RolePattern(v => v.Variants),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.NonUnifiedGood),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
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
