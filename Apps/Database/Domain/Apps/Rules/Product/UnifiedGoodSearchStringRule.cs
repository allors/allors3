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

    public class UnifiedGoodSearchStringRule : Rule
    {
        public UnifiedGoodSearchStringRule(MetaPopulation m) : base(m, new Guid("e2623257-5b06-4a98-a6bc-0dd9d6049a2c")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.UnifiedGood, m.UnifiedGood.DerivationTrigger),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.ProductIdentifications),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Keywords),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.SerialisedItems),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.ProductType),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Brand),
                new RolePattern(m.UnifiedGood, m.UnifiedGood.Model),
                new AssociationPattern(m.ProductCategory.AllProducts) { OfType = m.UnifiedGood },
                new AssociationPattern(m.PriceComponent.Product) { OfType = m.UnifiedGood },
                new AssociationPattern(m.SupplierOffering.Part) { OfType = m.UnifiedGood },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<UnifiedGood>())
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
