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

    public class PartCategoryLocalisedDescriptionsRule : Rule
    {
        public PartCategoryLocalisedDescriptionsRule(MetaPopulation m) : base(m, new Guid("9609e824-cb74-4045-9537-ecb926cf6303")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.LocalisedText, m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.PartCategoryWhereLocalisedDescription } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartCategory>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

            }
        }
    }
}
