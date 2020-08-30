// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class CatalogueCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCatalogues = changeSet.Created.Select(v=>v.GetObject()).OfType<Catalogue>();

                foreach(var catalogue in createdCatalogues)
                {
                    var defaultLocale = catalogue.Strategy.Session.GetSingleton().DefaultLocale;

                    if (catalogue.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                    {
                        catalogue.Name = catalogue.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                    }

                    if (catalogue.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                    {
                        catalogue.Description = catalogue.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                    }

                    if (!catalogue.ExistCatalogueImage)
                    {
                        catalogue.CatalogueImage = catalogue.Strategy.Session.GetSingleton().Settings.NoImageAvailableImage;
                    }
                }

            }

        }

        public static void CatalogueRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("855299a7-727a-4208-aa2f-806f8721075b")] = new CatalogueCreationDerivation();
        }
    }
}
