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

    public class DeliverableBasedServiceSearchStringRule : Rule
    {
        public DeliverableBasedServiceSearchStringRule(MetaPopulation m) : base(m, new Guid("957ea986-2db5-42e7-8821-bfb52be13da1")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedProduct.RolePattern(v => v.Name, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.Description, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.Comment, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.InternalComment, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.LocalisedNames, m.DeliverableBasedService),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName.UnifiedProduct, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.LocalisedDescriptions, m.DeliverableBasedService),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedDescription.UnifiedProduct, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.Keywords, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.LocalisedKeywords, m.DeliverableBasedService),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedKeyword.UnifiedProduct, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.ProductIdentifications, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.PublicElectronicDocuments, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.PrivateElectronicDocuments, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.UnitOfMeasure, m.DeliverableBasedService),
                m.UnifiedProduct.RolePattern(v => v.Scope, m.DeliverableBasedService),
                m.Product.RolePattern(v => v.IntrastatCode, m.DeliverableBasedService),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, m.DeliverableBasedService),
                m.Product.AssociationPattern(v => v.PriceComponentsWhereProduct, m.DeliverableBasedService),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<DeliverableBasedService>())
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
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text)) : null,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text)) : null,
                    @this.ExistLocalisedKeywords ? string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text)) : null,
                    @this.ExistProductIdentifications ? string.Join(" ", @this.ProductIdentifications?.Select(v => v.Identification)) : null,
                    @this.ExistProductCategoriesWhereAllProduct ? string.Join(" ", @this.ProductCategoriesWhereAllProduct?.Select(v => v.DisplayName)) : null,
                    @this.ExistPublicElectronicDocuments ? string.Join(" ", @this.PublicElectronicDocuments?.Select(v => v.Name)) : null,
                    @this.ExistPrivateElectronicDocuments ? string.Join(" ", @this.PrivateElectronicDocuments?.Select(v => v.Name)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
