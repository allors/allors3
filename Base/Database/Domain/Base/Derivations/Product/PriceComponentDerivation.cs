// <copyright file="PriceComponentDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PriceComponentDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("59C88605-9799-4849-A0E9-F107DB4BFBD1");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
                new CreatedPattern(M.PriceComponent.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var priceComponentExtensions in matches.Cast<PriceComponent>())
            {
                var internalOrganisations = new Organisations(priceComponentExtensions.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!priceComponentExtensions.ExistPricedBy && internalOrganisations.Count() == 1)
                {
                    priceComponentExtensions.PricedBy = internalOrganisations.First();
                }
            }
        }
    }
}
