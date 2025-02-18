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

    public class SerialisedItemCharacteristicTypeRule : Rule
    {
        public SerialisedItemCharacteristicTypeRule(MetaPopulation m) : base(m, new Guid("D24124E7-12FF-4F12-AC46-364D91570028")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItemCharacteristicType.RolePattern(v => v.LocalisedNames),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItemCharacteristicType>())
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
