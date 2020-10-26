// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class ProductCategoryDerivation : DomainDerivation
    {
        public ProductCategoryDerivation(M m) : base(m, new Guid("59C88605-9799-4849-A0E9-F107DB4BFBD1")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.ProductCategory.Class),
                new ChangedPattern(m.ProductCategory.PrimaryParent),
                new ChangedPattern(m.ProductCategory.SecondaryParents),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var productCategory in matches.Cast<ProductCategory>())
            {
                var defaultLocale = productCategory.Strategy.Session.GetSingleton().DefaultLocale;

                productCategory.DisplayName = productCategory.Name;

                if (productCategory.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    productCategory.Name =
                        productCategory.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (productCategory.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    productCategory.Description = productCategory.LocalisedDescriptions
                        .First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (!productCategory.ExistCategoryImage)
                {
                    productCategory.CategoryImage =
                        productCategory.Strategy.Session.GetSingleton().Settings.NoImageAvailableImage;
                }

                {
                    var primaryAncestors = new List<ProductCategory>();
                    var primaryAncestor = productCategory.PrimaryParent;
                    while (primaryAncestor != null)
                    {
                        if (primaryAncestors.Contains(primaryAncestor))
                        {
                            var cyclic = string.Join(" -> ",
                                primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                            validation.AddError(
                                $"{productCategory} {productCategory.Meta.PrimaryParent}  Cycle detected in " + cyclic);
                            break;
                        }

                        primaryAncestors.Add(primaryAncestor);
                        primaryAncestor = primaryAncestor.PrimaryParent;
                    }

                    productCategory.PrimaryAncestors = primaryAncestors.ToArray();

                    primaryAncestors.Reverse();
                    primaryAncestors.Add(productCategory);
                    productCategory.DisplayName = string.Join("/", primaryAncestors.Select(v => v.Name));
                }

                productCategory.Children = productCategory.ProductCategoriesWherePrimaryParent
                    .Union(productCategory.ProductCategoriesWhereSecondaryParent).ToArray();
                {
                    var descendants = new List<ProductCategory>();
                    var children = productCategory.Children.ToArray();
                    while (children.Length > 0)
                    {
                        if (children.Any(v => descendants.Contains(v)))
                        {
                            var cyclic = string.Join(" -> ", descendants.Union(children).Select(v => v.Name));
                            validation.AddError(
                                $"{productCategory} {productCategory.Meta.Children} Cycle detected in " + cyclic);
                            break;
                        }

                        descendants.AddRange(children);
                        children = children.SelectMany(v => v.Children).ToArray();
                    }

                    productCategory.Descendants = descendants.ToArray();
                }

                var descendantsAndSelf = productCategory.Descendants.Append(productCategory).ToArray();

                productCategory.AllProducts = descendantsAndSelf
                    .SelectMany(v => v.Products)
                    .ToArray();

                productCategory.AllParts = productCategory.AllProducts
                    .Select(v =>
                        v is Part part ? part : v is NonUnifiedGood nonUnifiedGood ? nonUnifiedGood.Part : null)
                    .Where(v => v != null)
                    .ToArray();

                productCategory.AllSerialisedItemsForSale = descendantsAndSelf
                    .SelectMany(
                        v => productCategory.AllParts
                            .SelectMany(w => w.SerialisedItems)
                            .Where(w => w.AvailableForSale))
                    .ToArray();

                productCategory.AllNonSerialisedInventoryItemsForSale = descendantsAndSelf
                    .SelectMany(
                        v => productCategory.AllParts
                            .SelectMany(w => w.InventoryItemsWherePart)
                            .OfType<NonSerialisedInventoryItem>()
                            .Where(w => w.NonSerialisedInventoryItemState.AvailableForSale))
                    .ToArray();

                productCategory.PreviousSecondaryParents = productCategory.SecondaryParents;
            }
        }
    }
}
