// <copyright file="SerialisedItemDisplayProductCategoriesDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class SerialisedItemProductCategoriesDisplayNameRule : Rule
    {
        public SerialisedItemProductCategoriesDisplayNameRule(MetaPopulation m) : base(m, new Guid("8b6c78f6-f165-4179-acf5-1c3ef96b36b1")) =>
            this.Patterns = new Pattern[]
            {
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, v => v.ProductCategoriesWhereAllProduct.ObjectType.AllParts.ObjectType.SerialisedItems),
                m.SerialisedItem.RolePattern(v => v.DerivationTrigger),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemProductCategoriesDisplayName(validation);
            }
        }
    }

    public static class SerialisedItemProductCategoriesDisplayNameRuleExtensions
    {
        public static void DeriveSerialisedItemProductCategoriesDisplayName(this SerialisedItem @this, IValidation validation)
        {
            if (@this.ExistPartWhereSerialisedItem && @this.PartWhereSerialisedItem.GetType().Name == typeof(UnifiedGood).Name)
            {
                var unifiedGood = @this.PartWhereSerialisedItem as UnifiedGood;
                @this.ProductCategoriesDisplayName = string.Join(", ", unifiedGood.ProductCategoriesWhereProduct.Select(v => v.DisplayName));
            }
        }
    }
}
