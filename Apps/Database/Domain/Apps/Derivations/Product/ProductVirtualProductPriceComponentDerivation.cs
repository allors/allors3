// <copyright file="ProductVirtualProductPriceComponentDerivation.cs" company="Allors bvba">
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

    public class ProductVirtualProductPriceComponentDerivation : DomainDerivation
    {
        public ProductVirtualProductPriceComponentDerivation(M m) : base(m, new Guid("6738e693-fd1c-46fb-b207-167b0fc3d1e1")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PriceComponent.Product) { Steps = new IPropertyType[] {m.PriceComponent.Product } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Product>())
            {
                if (!@this.ExistProductWhereVariant)
                {
                    @this.RemoveVirtualProductPriceComponents();
                }

                if (@this.ExistVariants)
                {
                    @this.RemoveVirtualProductPriceComponents();

                    var priceComponents = @this.PriceComponentsWhereProduct;

                    foreach (Product product in @this.Variants)
                    {
                        foreach (PriceComponent priceComponent in priceComponents)
                        {
                            product.AddVirtualProductPriceComponent(priceComponent);

                            if (priceComponent is BasePrice basePrice && !priceComponent.ExistProductFeature)
                            {
                                product.AddBasePrice(basePrice);
                            }
                        }
                    }
                }
            }
        }
    }
}
