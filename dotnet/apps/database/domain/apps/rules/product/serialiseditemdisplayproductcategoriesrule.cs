// <copyright file="SerialisedItemDisplayProductCategoriesDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SerialisedItemDisplayProductCategoriesRule : Rule
    {
        public SerialisedItemDisplayProductCategoriesRule(MetaPopulation m) : base(m, new Guid("8b6c78f6-f165-4179-acf5-1c3ef96b36b1")) =>
            this.Patterns = new Pattern[]
            {
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, v => v.ProductCategoriesWhereAllProduct.ProductCategory.AllParts.Part.SerialisedItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                if (@this.ExistPartWhereSerialisedItem && @this.PartWhereSerialisedItem.GetType().Name == typeof(UnifiedGood).Name)
                {
                    var unifiedGood = @this.PartWhereSerialisedItem as UnifiedGood;
                    @this.ProductCategoriesDisplayName = string.Join(", ", unifiedGood.ProductCategoriesWhereProduct.Select(v => v.DisplayName));
                }
            }
        }
    }
}
