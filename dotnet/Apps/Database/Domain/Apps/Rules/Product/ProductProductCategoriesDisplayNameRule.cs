// <copyright file="ProductProductCategoriesDisplayNameRule.cs" company="Allors bv">
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

    public class ProductProductCategoriesDisplayNameRule : Rule
    {
        public ProductProductCategoriesDisplayNameRule(MetaPopulation m) : base(m, new Guid("5c004eaa-282a-4f0d-8017-47a9e51fe698")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.Name, v => v.Products),
                m.ProductCategory.RolePattern(v => v.PrimaryParent, v => v.Products),
                m.ProductCategory.RolePattern(v => v.Products, v => v.Products)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Product>())
            {
                @this.DeriveProductProductCategoriesDisplayName(validation);
            }
        }
    }

    public static class ProductProductCategoriesDisplayNameExtensions
    {
        public static void DeriveProductProductCategoriesDisplayName(this Product @this, IValidation validation)
        {
            foreach (var category in @this.ProductCategoriesWhereProduct)
            {
                var primaryAncestors = new List<ProductCategory>();
                var primaryAncestor = category.PrimaryParent;
                while (primaryAncestor != null)
                {
                    if (primaryAncestors.Contains(primaryAncestor))
                    {
                        var loop = string.Join(" -> ", primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                        validation.AddError(category, category.Meta.PrimaryParent, "Cycle detected in " + loop);
                        break;
                    }

                    primaryAncestors.Add(primaryAncestor);
                    primaryAncestor = primaryAncestor.PrimaryParent;
                }

                primaryAncestors.Reverse();
                primaryAncestors.Add(category);
                @this.ProductCategoriesDisplayName = string.Join("/", primaryAncestors.Select(v => v.Name));
            }
        }
    }
}
