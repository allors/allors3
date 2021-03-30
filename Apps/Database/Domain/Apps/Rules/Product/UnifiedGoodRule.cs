// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class UnifiedGoodRule : Rule
    {
        public UnifiedGoodRule(M m) : base(m, new Guid("B1C14106-C300-453D-989B-81E05767CFC4")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.UnifiedGood, m.UnifiedGood.DerivationTrigger),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.ProductIdentifications),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Keywords),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Variants),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.SerialisedItems),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.ProductType),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Brand),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Model),
                new AssociationPattern(m.ProductCategory.AllProducts) { OfType = m.UnifiedGood.Class },
                new AssociationPattern(m.PriceComponent.Product) { OfType = m.UnifiedGood.Class },
                new AssociationPattern(m.SupplierOffering.Part) { OfType = m.UnifiedGood.Class },
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

                var builder = new StringBuilder();
                if (@this.ExistProductIdentifications)
                {
                    builder.Append(string.Join(" ", @this.ProductIdentifications.Select(v => v.Identification)));
                }

                if (@this.ExistProductCategoriesWhereAllProduct)
                {
                    builder.Append(string.Join(" ", @this.ProductCategoriesWhereAllProduct.Select(v => v.Name)));
                }

                if (@this.ExistSupplierOfferingsWherePart)
                {
                    builder.Append(string.Join(" ", @this.SupplierOfferingsWherePart.Select(v => v.Supplier.PartyName)));
                }

                if (@this.ExistSerialisedItems)
                {
                    builder.Append(string.Join(" ", @this.SerialisedItems.Select(v => v.SerialNumber)));
                    builder.Append(string.Join(" ", @this.SerialisedItems.Select(v => v.ItemNumber)));
                }

                if (@this.ExistProductType)
                {
                    builder.Append(string.Join(" ", @this.ProductType.Name));
                }

                if (@this.ExistBrand)
                {
                    builder.Append(string.Join(" ", @this.Brand.Name));
                }

                if (@this.ExistModel)
                {
                    builder.Append(string.Join(" ", @this.Model.Name));
                }

                builder.Append(string.Join(" ", @this.Keywords));

                @this.SearchString = builder.ToString();
            }
        }
    }
}
