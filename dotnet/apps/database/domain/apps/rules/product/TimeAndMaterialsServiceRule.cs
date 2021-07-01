// <copyright file="TimeAndMaterialsServiceDerivation.cs" company="Allors bvba">
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

    public class TimeAndMaterialsServiceRule : Rule
    {
        public TimeAndMaterialsServiceRule(MetaPopulation m) : base(m, new Guid("60d9b0ad-2078-4921-a689-e15877983bb3")) =>
            this.Patterns = new Pattern[]
            {
                m.TimeAndMaterialsService.RolePattern(v => v.Variants),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.TimeAndMaterialsService),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeAndMaterialsService>())
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
                        foreach (var priceComponent in priceComponents)
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
