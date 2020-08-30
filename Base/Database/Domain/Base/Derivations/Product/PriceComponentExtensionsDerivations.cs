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
        public class PriceComponentExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdPriceComponentExtensions = changeSet.Created.Select(v=>v.GetObject()).OfType<PriceComponent>();

                foreach(var priceComponentExtensions in createdPriceComponentExtensions)
                {
                    var internalOrganisations = new Organisations(priceComponentExtensions.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                    if (!priceComponentExtensions.ExistPricedBy && internalOrganisations.Count() == 1)
                    {
                        priceComponentExtensions.PricedBy = internalOrganisations.First();
                    }
                }
            }
        }

        public static void PriceComponentExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("f390757e-0375-4729-8b11-ec697d8106bb")] = new PriceComponentExtensionsCreationDerivation();
        }
    }
}
