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

    public class PriceComponentDerivation : DomainDerivation
    {
        public PriceComponentDerivation(M m) : base(m, new Guid("34F7833F-170D-45C3-92F0-B8AD33C3A028")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.PriceComponent.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
