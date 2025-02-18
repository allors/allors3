// <copyright file="Domain.cs" company="Allors bv">
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

    public class NonUnifiedPartSearchStringRule : Rule
    {
        public NonUnifiedPartSearchStringRule(MetaPopulation m) : base(m, new Guid("ebc7bac7-733d-44cf-9850-77d3a3308bfa")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedPart.RolePattern(v => v.SupplierReferenceNumbers),
                m.Part.RolePattern(v => v.Name, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.Description, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.Comment, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.InternalComment, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.LocalisedNames, m.NonUnifiedPart),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName.ObjectType, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.LocalisedDescriptions, m.NonUnifiedPart),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedDescription.ObjectType, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.Keywords, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.LocalisedKeywords, m.NonUnifiedPart),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedKeyword.ObjectType, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.ProductIdentifications, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.PublicElectronicDocuments, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.PrivateElectronicDocuments, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.UnitOfMeasure, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.Scope, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.HsCode, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.SerialisedItems, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.InventoryItemKind, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.ProductType, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.Brand, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.Model, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.ManufacturedBy, m.NonUnifiedPart),
                m.Party.RolePattern(v => v.DisplayName, v => v.PartsWhereManufacturedBy.ObjectType, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.SuppliedBy, m.NonUnifiedPart),
                m.Party.RolePattern(v => v.DisplayName, v => v.PartsWhereSuppliedBy.ObjectType, m.NonUnifiedPart),
                m.Part.RolePattern(v => v.DefaultFacility, m.NonUnifiedPart),
                m.Facility.RolePattern(v => v.Name, v => v.PartsWhereDefaultFacility.ObjectType, m.NonUnifiedPart),
                m.Part.AssociationPattern(v => v.PartCategoriesWherePart, m.NonUnifiedPart),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, m.NonUnifiedPart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                @this.DeriveNonUnifiedPartSearchString(validation);
            }
        }
    }

    public static class NonUnifiedPartSearchStringRuleExtensions
    {
        public static void DeriveNonUnifiedPartSearchString(this NonUnifiedPart @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Name,
                    @this.SupplierReferenceNumbers,
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
                    @this.HsCode,
                    @this.ManufacturedBy?.DisplayName,
                    @this.InventoryItemKind?.Name,
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedKeywords ? string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistProductIdentifications ? string.Join(" ", @this.ProductIdentifications?.Select(v => v.Identification ?? string.Empty).ToArray()) : null,
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
