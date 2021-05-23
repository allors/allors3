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

    public class ProductCategoryLocalisedNameRule : Rule
    {
        public ProductCategoryLocalisedNameRule(MetaPopulation m) : base(m, new Guid("be0f1057-f23f-4638-b10c-59618c099f88")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.Name),
                m.LocalisedText.RolePattern(v => v.Text, v => v.ProductCategoryWhereLocalisedName),
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
            }
        }
    }
}
