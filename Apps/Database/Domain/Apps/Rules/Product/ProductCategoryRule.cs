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

    public class ProductCategoryRule : Rule
    {
        public ProductCategoryRule(MetaPopulation m) : base(m, new Guid("59C88605-9799-4849-A0E9-F107DB4BFBD1")) =>
            this.Patterns = new Pattern[]
            {
                //new RolePattern(m.ProductCategory, m.ProductCategory.Name),
                //new RolePattern(m.ProductCategory, m.ProductCategory.PrimaryParent),
                //new RolePattern(m.ProductCategory, m.ProductCategory.SecondaryParents),
                //new RolePattern(m.ProductCategory, m.ProductCategory.CategoryImage),
                //new RolePattern(m.ProductCategory, m.ProductCategory.Products),
                //new AssociationPattern(m.ProductCategory.PrimaryParent),
                //new RolePattern(m.ProductCategory, m.ProductCategory.PrimaryParent) {Steps = new IPropertyType[] {m.ProductCategory.ProductCategoriesWhereDescendant} },
                //new AssociationPattern(m.ProductCategory.SecondaryParents),
                //new RolePattern(m.ProductCategory, m.ProductCategory.SecondaryParents) {Steps = new IPropertyType[] {m.ProductCategory.ProductCategoriesWhereDescendant} },
                //new RolePattern(m.ProductCategory, m.ProductCategory.AllProducts) {Steps = new IPropertyType[] {m.ProductCategory.ProductCategoriesWhereDescendant} },

                m.ProductCategory.RolePattern(v => v.Name),
                m.ProductCategory.RolePattern(v => v.PrimaryParent),
                m.ProductCategory.RolePattern(v => v.SecondaryParents),
                m.ProductCategory.RolePattern(v => v.Products),
                m.ProductCategory.RolePattern(v => v.PrimaryParent, v => v.ProductCategoriesWhereDescendant),
                m.ProductCategory.RolePattern(v => v.SecondaryParents, v => v.ProductCategoriesWhereDescendant),
                m.ProductCategory.RolePattern(v => v.AllProducts, v => v.ProductCategoriesWhereDescendant),
                m.ProductCategory.AssociationPattern(v => v.ProductCategoriesWherePrimaryParent),
                m.ProductCategory.AssociationPattern(v => v.ProductCategoriesWhereSecondaryParent),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductCategory>())
            {
                //if (!@this.ExistCategoryImage)
                //{
                //    @this.CategoryImage =
                //        @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                //}

                // TODO: Met bovenstaande if werkt ProductCategoriesWhereDescendant wel, zonder niet

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
