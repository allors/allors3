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
    using Derivations.Rules;

    public class ProductCategoryLocalisedDescriptionRule : Rule
    {
        public ProductCategoryLocalisedDescriptionRule(MetaPopulation m) : base(m, new Guid("3a03ae65-d6f5-4bdd-8b91-960f446c5f36")) =>
            this.Patterns = new Pattern[]
            {
                m.LocalisedText.RolePattern(v => v.Text, v => v.ProductCategoryWhereLocalisedDescription),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductCategory>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions
                        .First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
