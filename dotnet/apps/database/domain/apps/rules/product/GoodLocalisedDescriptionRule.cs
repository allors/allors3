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

    public class GoodLocalisedDescriptionRule : Rule
    {
        public GoodLocalisedDescriptionRule(MetaPopulation m) : base(m, new Guid("1e9d2e93-5f3d-4682-9051-a0fd3f89d68e")) =>
            this.Patterns = new Pattern[]
            {
                m.Good.RolePattern(v => v.LocalisedDescriptions),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedDescription, m.Good),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Good>())
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
