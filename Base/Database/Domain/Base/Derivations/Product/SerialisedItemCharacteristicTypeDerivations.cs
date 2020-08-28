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
        public class SerialisedItemCharacteristicTypeCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdSerialisedItemCharacteristicTypes = changeSet.Created.Select(session.Instantiate).OfType<SerialisedItemCharacteristicType>();

                foreach(var serialisedItemCharacteristicType in createdSerialisedItemCharacteristicTypes)
                {
                    var defaultLocale = serialisedItemCharacteristicType.Strategy.Session.GetSingleton().DefaultLocale;

                    if (serialisedItemCharacteristicType.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                    {
                        serialisedItemCharacteristicType.Name = serialisedItemCharacteristicType.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                    }
                }
               
            }
        }

        public static void SerialisedItemCharacteristicTypeRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("b7646f0e-94f3-4bfe-848b-f1fb7044bd63")] = new SerialisedItemCharacteristicTypeCreationDerivation();
        }
    }
}
