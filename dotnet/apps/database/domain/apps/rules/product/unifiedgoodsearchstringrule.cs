// <copyright file="Domain.cs" company="Allors bvba">
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

    public class UnifiedGoodSearchStringRule : Rule
    {
        public UnifiedGoodSearchStringRule(MetaPopulation m) : base(m, new Guid("e2623257-5b06-4a98-a6bc-0dd9d6049a2c")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedProduct.RolePattern(v => v.Name, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.Description, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.Comment, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.InternalComment, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.LocalisedNames, m.UnifiedGood),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName.UnifiedProduct, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.LocalisedDescriptions, m.UnifiedGood),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedDescription.UnifiedProduct, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.Keywords, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.LocalisedKeywords, m.UnifiedGood),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedKeyword.UnifiedProduct, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.ProductIdentifications, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.PublicElectronicDocuments, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.PrivateElectronicDocuments, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.UnitOfMeasure, m.UnifiedGood),
                m.UnifiedProduct.RolePattern(v => v.Scope, m.UnifiedGood),
                m.Product.RolePattern(v => v.IntrastatCode, m.UnifiedGood),
                m.Part.RolePattern(v => v.HsCode, m.UnifiedGood),
                m.Part.RolePattern(v => v.SerialisedItems, m.UnifiedGood),
                m.Part.RolePattern(v => v.InventoryItemKind, m.UnifiedGood),
                m.Part.RolePattern(v => v.ProductType, m.UnifiedGood),
                m.Part.RolePattern(v => v.Brand, m.UnifiedGood),
                m.Part.RolePattern(v => v.Model, m.UnifiedGood),
                m.Part.RolePattern(v => v.ManufacturedBy, m.UnifiedGood),
                m.Party.RolePattern(v => v.DisplayName, v => v.PartsWhereManufacturedBy.Part, m.UnifiedGood),
                m.Part.RolePattern(v => v.SuppliedBy, m.UnifiedGood),
                m.Party.RolePattern(v => v.DisplayName, v => v.PartsWhereSuppliedBy.Part, m.UnifiedGood),
                m.Part.RolePattern(v => v.DefaultFacility, m.UnifiedGood),
                m.Facility.RolePattern(v => v.Name, v => v.PartsWhereDefaultFacility.Part, m.UnifiedGood),

                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, m.UnifiedGood),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.UnifiedGood),
                m.Part.AssociationPattern(v => v.PartCategoriesWherePart, m.UnifiedGood),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, m.UnifiedGood),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                @this.DeriveUnifiedGoodSearchString(validation);
            }
        }
    }

    public static class UnifiedGoodSearchStringRuleExtensions
    {
        public static void DeriveUnifiedGoodSearchString(this UnifiedGood @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Name,
                    @this.Description,
                    @this.Keywords,
                    @this.Comment,
                    @this.InternalComment,
                    @this.ProductType?.Name,
                    @this.Brand?.Name,
                    @this.Model?.Name,
                    @this.UnitOfMeasure?.Abbreviation,
                    @this.UnitOfMeasure?.Name,
                    @this.Scope?.Name,
                    @this.IntrastatCode,
                    @this.HsCode,
                    @this.ManufacturedBy?.DisplayName,
                    @this.InventoryItemKind?.Name,
                    @this.DefaultFacility?.Name,
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedKeywords ? string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistProductIdentifications ? string.Join(" ", @this.ProductIdentifications?.Select(v => v.Identification ?? string.Empty).ToArray()) : null,
                    @this.ExistProductCategoriesWhereAllProduct ? string.Join(" ", @this.ProductCategoriesWhereAllProduct?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPartCategoriesWherePart ? string.Join(" ", @this.PartCategoriesWherePart?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistSupplierOfferingsWherePart ? string.Join(" ", @this.SupplierOfferingsWherePart?.Select(v => v.Supplier?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistSerialisedItems ? string.Join(" ", @this.SerialisedItems?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPublicElectronicDocuments ? string.Join(" ", @this.PublicElectronicDocuments?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistPrivateElectronicDocuments ? string.Join(" ", @this.PrivateElectronicDocuments?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistSuppliedBy ? string.Join(" ", @this.SuppliedBy?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
