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

    public class PartCategoryDeriveTextsRule : Rule
    {
        public PartCategoryDeriveTextsRule(MetaPopulation m) : base(m, new Guid("e680c875-e9e3-4d20-86ce-cb36f74ff26f")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PartCategory, m.PartCategory.Name),
                new RolePattern(m.LocalisedText, m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.PartCategoryWhereLocalisedName } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartCategory>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
