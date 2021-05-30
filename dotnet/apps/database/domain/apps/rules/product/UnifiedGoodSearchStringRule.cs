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
    using Meta;
    using Derivations.Rules;

    public class UnifiedGoodSearchStringRule : Rule
    {
        public UnifiedGoodSearchStringRule(MetaPopulation m) : base(m, new Guid("e2623257-5b06-4a98-a6bc-0dd9d6049a2c")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedGood.RolePattern(v => v.DerivationTrigger),
                m.UnifiedGood.RolePattern(v => v.ProductIdentifications),
                m.UnifiedGood.RolePattern(v => v.Keywords),
                m.UnifiedGood.RolePattern(v => v.SerialisedItems),
                m.UnifiedGood.RolePattern(v => v.ProductType),
                m.UnifiedGood.RolePattern(v => v.Brand),
                m.UnifiedGood.RolePattern(v => v.Model),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, m.UnifiedGood),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.UnifiedGood),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, m.UnifiedGood),
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
