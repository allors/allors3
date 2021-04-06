// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class GoodRule : Rule
    {
        public GoodRule(MetaPopulation m) : base(m, new Guid("1e9d2e93-5f3d-4682-9051-a0fd3f89d68e")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Good, m.Good.LocalisedNames),
                new RolePattern(m.Good, m.Good.LocalisedDescriptions),
                new RolePattern(m.LocalisedText, m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedName }, OfType = m.Good},
                new RolePattern(m.LocalisedText, m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedDescription}, OfType = m.Good},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Good>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
