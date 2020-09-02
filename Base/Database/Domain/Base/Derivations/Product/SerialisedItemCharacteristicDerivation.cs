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

    public class SerialisedItemCharacteristicDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("B9EB094F-4E60-4ABD-8AE6-CAA02D38AFA1");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.SerialisedItemCharacteristic.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var serialisedItemCharacteristic in matches.Cast<SerialisedItemCharacteristic>())
            {
                if (serialisedItemCharacteristic.SerialisedItemCharacteristicType.ExistUnitOfMeasure)
                {
                    var existingLocalisedtexts = serialisedItemCharacteristic.LocalisedValues.ToDictionary(d => d.Locale);

                    foreach (Locale locale in serialisedItemCharacteristic.Strategy.Session.GetSingleton().AdditionalLocales)
                    {
                        if (existingLocalisedtexts.TryGetValue(locale, out var localisedText))
                        {
                            localisedText.Text = serialisedItemCharacteristic.Value;
                            existingLocalisedtexts.Remove(locale);
                        }
                        else
                        {
                            localisedText = new LocalisedTextBuilder(serialisedItemCharacteristic.Strategy.Session)
                                .WithLocale(locale)
                                .Build();

                            serialisedItemCharacteristic.AddLocalisedValue(localisedText);
                        }
                    }

                    foreach (var localisedText in existingLocalisedtexts.Values)
                    {
                        serialisedItemCharacteristic.RemoveLocalisedValue(localisedText);
                    }
                }
            }
        }
    }
}
