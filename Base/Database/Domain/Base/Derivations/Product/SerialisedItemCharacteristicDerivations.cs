// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class SerialisedItemCharacteristicCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdSerialisedItemCharacteristics = changeSet.Created.Select(v=>v.GetObject()).OfType<SerialisedItemCharacteristic>();

                foreach(var serialisedItemCharacteristic in createdSerialisedItemCharacteristics)
                {
                    if (serialisedItemCharacteristic.SerialisedItemCharacteristicType.ExistUnitOfMeasure)
                    {
                        Sync(serialisedItemCharacteristic);
                    }
                }

                void Sync(SerialisedItemCharacteristic serialisedItemCharacteristic)
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

        public static void SerialisedItemCharacteristicRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2a1b5544-11f0-4fa4-8699-afbf27d6875f")] = new SerialisedItemCharacteristicCreationDerivation();
        }
    }
}
