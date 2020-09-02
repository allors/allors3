// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class SerialisedItemCharacteristicTypeDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("D24124E7-12FF-4F12-AC46-364D91570028");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.SerialisedItemCharacteristicType.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var serialisedItemCharacteristicType in matches.Cast<SerialisedItemCharacteristicType>())
            {
                var defaultLocale = serialisedItemCharacteristicType.Strategy.Session.GetSingleton().DefaultLocale;

                if (serialisedItemCharacteristicType.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    serialisedItemCharacteristicType.Name = serialisedItemCharacteristicType.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
