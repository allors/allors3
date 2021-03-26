// <copyright file="SerialisedItemDisplayProductCategoriesDerivation.cs" company="Allors bvba">
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
    using Resources;

    public class SerialisedItemDisplayProductCategoriesDerivation : DomainDerivation
    {
        public SerialisedItemDisplayProductCategoriesDerivation(M m) : base(m, new Guid("8b6c78f6-f165-4179-acf5-1c3ef96b36b1")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.ProductCategory.AllProducts) { Steps = new IPropertyType[]{ m.Part.SerialisedItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                if (@this.ExistPartWhereSerialisedItem && @this.PartWhereSerialisedItem.GetType().Name == typeof(UnifiedGood).Name)
                {
                    var unifiedGood = @this.PartWhereSerialisedItem as UnifiedGood;
                    @this.DisplayProductCategories = string.Join(", ", unifiedGood.ProductCategoriesWhereProduct.Select(v => v.DisplayName));
                }
            }
        }
    }
}
