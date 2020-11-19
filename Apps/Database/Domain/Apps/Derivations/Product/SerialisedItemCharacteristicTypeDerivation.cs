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

    public class SerialisedItemCharacteristicTypeDerivation : DomainDerivation
    {
        public SerialisedItemCharacteristicTypeDerivation(M m) : base(m, new Guid("D24124E7-12FF-4F12-AC46-364D91570028")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.SerialisedItemCharacteristicType.UnitOfMeasure),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItemCharacteristicType>())
            {
                var defaultLocale = @this.Strategy.Session.GetSingleton().DefaultLocale;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
