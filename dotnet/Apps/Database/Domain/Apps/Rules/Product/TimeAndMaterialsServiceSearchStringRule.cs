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

    public class TimeAndMaterialsServiceSearchStringRule : Rule
    {
        public TimeAndMaterialsServiceSearchStringRule(MetaPopulation m) : base(m, new Guid("20bae1e4-a033-4dd7-b714-c3f8b73b8138")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedProduct.RolePattern(v => v.Name, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.Description, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.Comment, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.InternalComment, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.LocalisedNames, m.TimeAndMaterialsService),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName.ObjectType, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.LocalisedDescriptions, m.TimeAndMaterialsService),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedDescription.ObjectType, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.Keywords, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.LocalisedKeywords, m.TimeAndMaterialsService),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedKeyword.ObjectType, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.ProductIdentifications, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.PublicElectronicDocuments, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.PrivateElectronicDocuments, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.UnitOfMeasure, m.TimeAndMaterialsService),
                m.UnifiedProduct.RolePattern(v => v.Scope, m.TimeAndMaterialsService),
                m.Product.RolePattern(v => v.IntrastatCode, m.TimeAndMaterialsService),

                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, m.TimeAndMaterialsService),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.TimeAndMaterialsService),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeAndMaterialsService>())
            {
                @this.DeriveTimeAndMaterialsServiceSearchString(validation);
            }
        }
    }

    public static class TimeAndMaterialsServiceSearchStringRuleExtensions
    {
        public static void DeriveTimeAndMaterialsServiceSearchString(this TimeAndMaterialsService @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Name,
                    @this.Description,
                    @this.Keywords,
                    @this.Comment,
                    @this.InternalComment,
                    @this.UnitOfMeasure?.Abbreviation,
                    @this.UnitOfMeasure?.Name,
                    @this.Scope?.Name,
                    @this.IntrastatCode,
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedKeywords ? string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistProductIdentifications ? string.Join(" ", @this.ProductIdentifications?.Select(v => v.Identification ?? string.Empty).ToArray()) : null,
                    @this.ExistProductCategoriesWhereAllProduct ? string.Join(" ", @this.ProductCategoriesWhereAllProduct?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPublicElectronicDocuments ? string.Join(" ", @this.PublicElectronicDocuments?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistPrivateElectronicDocuments ? string.Join(" ", @this.PrivateElectronicDocuments?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
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
