// <copyright file="InventoryItemSearchStringDerivation.cs" company="Allors bvba">
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

    public class ProductCategorySearchStringRule : Rule
    {
        public ProductCategorySearchStringRule(MetaPopulation m) : base(m, new Guid("49c1afc6-8646-43e8-82d4-f03e7f580565")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.DisplayName),
                m.ProductCategory.RolePattern(v => v.Name),
                m.ProductCategory.RolePattern(v => v.Description),
                m.ProductCategory.RolePattern(v => v.Code),
                m.ProductCategory.RolePattern(v => v.CatScope),
                m.ProductCategory.RolePattern(v => v.LocalisedNames),
                m.LocalisedText.RolePattern(v => v.Text, v => v.ProductCategoryWhereLocalisedName.ProductCategory),
                m.ProductCategory.RolePattern(v => v.LocalisedDescriptions),
                m.LocalisedText.RolePattern(v => v.Text, v => v.ProductCategoryWhereLocalisedDescription.ProductCategory),

                m.ProductCategory.AssociationPattern(v => v.CatalogueWhereProductCategory),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductCategory>())
            {
                var array = new string[] {
                    @this.DisplayName,
                    @this.Name,
                    @this.Description,
                    @this.Code,
                    @this.CatScope?.Name,
                    @this.CatalogueWhereProductCategory?.Name,
                    @this.CatalogueWhereProductCategory?.Description,
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
