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

    public class GoodLocalisedNamesRule : Rule
    {
        public GoodLocalisedNamesRule(MetaPopulation m) : base(m, new Guid("797142eb-b474-4872-b426-b1e7cd728ffa")) =>
            this.Patterns = new Pattern[]
            {
                m.Good.RolePattern(v => v.LocalisedNames),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName, m.Good),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Good>())
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
