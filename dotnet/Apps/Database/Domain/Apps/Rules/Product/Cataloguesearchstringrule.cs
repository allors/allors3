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

    public class CatalogueSearchStringRule : Rule
    {
        public CatalogueSearchStringRule(MetaPopulation m) : base(m, new Guid("d7db9fd9-81a0-4cd6-ad66-c0686e5cae84")) =>
            this.Patterns = new Pattern[]
        {
            m.Catalogue.RolePattern(v => v.Name),
            m.Catalogue.RolePattern(v => v.Description),
            m.Catalogue.RolePattern(v => v.LocalisedNames),
            m.LocalisedText.RolePattern(v => v.Text, v => v.CatalogueWhereLocalisedName.ObjectType),
            m.Catalogue.RolePattern(v => v.LocalisedDescriptions),
            m.LocalisedText.RolePattern(v => v.Text, v => v.CatalogueWhereLocalisedDescription.ObjectType),
            m.Catalogue.RolePattern(v => v.CatScope),
            m.Catalogue.RolePattern(v => v.ProductCategories),
            m.ProductCategory.RolePattern(v => v.DisplayName, v => v.CatalogueWhereProductCategory.ObjectType),

            m.Catalogue.AssociationPattern(v => v.StoreWhereCatalogue),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Catalogue>())
            {
                @this.DeriveCatalogueSearchString(validation);
            }
        }
    }

    public static class CatalogueSearchStringRuleExtensions
    {
        public static void DeriveCatalogueSearchString(this Catalogue @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Name,
                    @this.Description,
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.CatScope?.Name,
                    @this.ExistProductCategories ? string.Join(" ", @this.ProductCategories?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.StoreWhereCatalogue?.Name,
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
