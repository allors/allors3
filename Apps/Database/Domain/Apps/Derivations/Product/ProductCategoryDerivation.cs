// <copyright file="Domain.cs" company="Allors bvba">
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

    public class ProductCategoryDerivation : DomainDerivation
    {
        public ProductCategoryDerivation(M m) : base(m, new Guid("59C88605-9799-4849-A0E9-F107DB4BFBD1")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.ProductCategory.Name),
                new ChangedPattern(m.ProductCategory.PrimaryParent),
                new ChangedPattern(m.ProductCategory.SecondaryParents),
                new ChangedPattern(m.ProductCategory.CategoryImage),
                new ChangedPattern(m.ProductCategory.Products),
                new ChangedPattern(m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.ProductCategoryWhereLocalisedName } },
                new ChangedPattern(m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.ProductCategoryWhereLocalisedDescription } },
                new ChangedPattern(m.ProductCategory.PrimaryParent) {Steps = new IPropertyType[] {m.ProductCategory.PrimaryParent} },
                new ChangedPattern(m.ProductCategory.PrimaryParent) {Steps = new IPropertyType[] {m.ProductCategory.ProductCategoriesWhereDescendant} },
                new ChangedPattern(m.ProductCategory.SecondaryParents) {Steps = new IPropertyType[] {m.ProductCategory.SecondaryParents} },
                new ChangedPattern(m.ProductCategory.SecondaryParents) {Steps = new IPropertyType[] {m.ProductCategory.ProductCategoriesWhereDescendant} },
                new ChangedPattern(m.ProductCategory.AllProducts) {Steps = new IPropertyType[] {m.ProductCategory.ProductCategoriesWhereDescendant} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductCategory>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                @this.DisplayName = @this.Name;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name =
                        @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions
                        .First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (!@this.ExistCategoryImage)
                {
                    @this.CategoryImage =
                        @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                }

                {
                    var primaryAncestors = new List<ProductCategory>();
                    var primaryAncestor = @this.PrimaryParent;
                    while (primaryAncestor != null)
                    {
                        if (primaryAncestors.Contains(primaryAncestor))
                        {
                            var cyclic = string.Join(" -> ",
                                primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                            validation.AddError(
                                $"{@this} {@this.Meta.PrimaryParent}  Cycle detected in " + cyclic);
                            break;
                        }

                        primaryAncestors.Add(primaryAncestor);
                        primaryAncestor = primaryAncestor.PrimaryParent;
                    }

                    @this.PrimaryAncestors = primaryAncestors.ToArray();

                    primaryAncestors.Reverse();
                    primaryAncestors.Add(@this);
                    @this.DisplayName = string.Join("/", primaryAncestors.Select(v => v.Name));
                }

                @this.Children = @this.ProductCategoriesWherePrimaryParent
                    .Union(@this.ProductCategoriesWhereSecondaryParent).ToArray();

                {
                    var descendants = new List<ProductCategory>();
                    var children = @this.Children.ToArray();
                    while (children.Length > 0)
                    {
                        if (children.Any(v => descendants.Contains(v)))
                        {
                            var cyclic = string.Join(" -> ", descendants.Union(children).Select(v => v.Name));
                            validation.AddError(
                                $"{@this} {@this.Meta.Children} Cycle detected in " + cyclic);
                            break;
                        }

                        descendants.AddRange(children);
                        children = children.SelectMany(v => v.Children).ToArray();
                    }

                    @this.Descendants = descendants.ToArray();
                }

                var descendantsAndSelf = @this.Descendants.Append(@this).ToArray();

                @this.AllProducts = descendantsAndSelf
                    .SelectMany(v => v.Products)
                    .ToArray();

                @this.AllParts = @this.AllProducts
                    .Select(v =>
                        v is Part part ? part : v is NonUnifiedGood nonUnifiedGood ? nonUnifiedGood.Part : null)
                    .Where(v => v != null)
                    .ToArray();

                @this.AllSerialisedItemsForSale = descendantsAndSelf
                    .SelectMany(
                        v => @this.AllParts
                            .SelectMany(w => w.SerialisedItems)
                            .Where(w => w.AvailableForSale))
                    .ToArray();

                @this.AllNonSerialisedInventoryItemsForSale = descendantsAndSelf
                    .SelectMany(
                        v => @this.AllParts
                            .SelectMany(w => w.InventoryItemsWherePart)
                            .OfType<NonSerialisedInventoryItem>()
                            .Where(w => w.NonSerialisedInventoryItemState.AvailableForSale))
                    .ToArray();

                @this.PreviousSecondaryParents = @this.SecondaryParents;
            }
        }
    }
}
