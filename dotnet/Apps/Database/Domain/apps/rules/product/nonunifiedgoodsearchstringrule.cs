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

    public class NonUnifiedGoodSearchStringRule : Rule
    {
        public NonUnifiedGoodSearchStringRule(MetaPopulation m) : base(m, new Guid("2e2ff11f-3182-4826-993f-c3d756b6e145")) =>
            this.Patterns = new Pattern[]
            {
                m.Good.RolePattern(v => v.Name, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.Description, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.Comment, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.InternalComment, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.LocalisedNames, m.NonUnifiedGood),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName.UnifiedProduct, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.LocalisedDescriptions, m.NonUnifiedGood),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedDescription.UnifiedProduct, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.Keywords, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.LocalisedKeywords, m.NonUnifiedGood),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedKeyword.UnifiedProduct, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.ProductIdentifications, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.PublicElectronicDocuments, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.PrivateElectronicDocuments, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.UnitOfMeasure, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.Scope, m.NonUnifiedGood),
                m.Good.RolePattern(v => v.IntrastatCode, m.NonUnifiedGood),
                m.Good.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, m.NonUnifiedGood),
                m.NonUnifiedGood.RolePattern(v => v.Part),
                m.Part.RolePattern(v => v.DisplayName, v => v.NonUnifiedGoodsWherePart.NonUnifiedGood),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
            {
                @this.DeriveNonUnifiedGoodSearchString(validation);
            }
        }
    }

    public static class NonUnifiedGoodSearchStringRuleExtensions
    {
        public static void DeriveNonUnifiedGoodSearchString(this NonUnifiedGood @this, IValidation validation)
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
                    @this.Part?.DisplayName,
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
        }
    }
}
