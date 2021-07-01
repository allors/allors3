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

    public class SerialisedItemCharacteristicRule : Rule
    {
        public SerialisedItemCharacteristicRule(MetaPopulation m) : base(m, new Guid("B9EB094F-4E60-4ABD-8AE6-CAA02D38AFA1")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value),
                m.SerialisedItemCharacteristic.RolePattern(v => v.SerialisedItemCharacteristicType),
                m.SerialisedItemCharacteristicType.RolePattern(v => v.UnitOfMeasure, v => v.SerialisedItemCharacteristicsWhereSerialisedItemCharacteristicType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItemCharacteristic>())
            {
                if (@this.SerialisedItemCharacteristicType.ExistUnitOfMeasure)
                {
                    var existingLocalisedtexts = @this.LocalisedValues.ToDictionary(d => d.Locale);

                    foreach (var locale in @this.Strategy.Transaction.GetSingleton().AdditionalLocales)
                    {
                        if (existingLocalisedtexts.TryGetValue(locale, out var localisedText))
                        {
                            localisedText.Text = @this.Value;
                            existingLocalisedtexts.Remove(locale);
                        }
                        else
                        {
                            localisedText = new LocalisedTextBuilder(@this.Strategy.Transaction)
                                .WithLocale(locale)
                                .Build();

                            @this.AddLocalisedValue(localisedText);
                        }
                    }

                    foreach (var localisedText in existingLocalisedtexts.Values)
                    {
                        @this.RemoveLocalisedValue(localisedText);
                    }
                }
            }
        }
    }
}
