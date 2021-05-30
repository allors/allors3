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
    using Derivations.Rules;

    public class DeliverableBasedServiceRule : Rule
    {
        public DeliverableBasedServiceRule(MetaPopulation m) : base(m, new Guid("f0400e44-4b4b-4899-87dc-874038ceece3")) =>
            this.Patterns = new Pattern[]
            {
                m.DeliverableBasedService.RolePattern(v => v.Variants),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.DeliverableBasedService),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<DeliverableBasedService>())
            {
                if (@this.ExistCurrentVersion)
                {
                    foreach (Service variant in @this.CurrentVersion.Variants)
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

                    foreach (Service variant in @this.Variants)
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
