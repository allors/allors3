// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class DiscountComponentDerivation : DomainDerivation
    {
        public DiscountComponentDerivation(M m) : base(m, new Guid("C395DB2E-C4A6-4974-BE35-EF2CC70D347D")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.DiscountComponent.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var discountComponent in matches.Cast<DiscountComponent>())
            {
                var internalOrganisations = new Organisations(discountComponent.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!discountComponent.ExistPricedBy && internalOrganisations.Count() == 1)
                {
                    discountComponent.PricedBy = internalOrganisations.First();
                }

                validation.AssertAtLeastOne(discountComponent, M.DiscountComponent.Price, M.DiscountComponent.Percentage);
                validation.AssertExistsAtMostOne(discountComponent, M.DiscountComponent.Price, M.DiscountComponent.Percentage);

                if (discountComponent.ExistPrice)
                {
                    if (!discountComponent.ExistCurrency && discountComponent.ExistPricedBy)
                    {
                        discountComponent.Currency = discountComponent.PricedBy.PreferredCurrency;
                    }

                    validation.AssertExists(discountComponent, M.BasePrice.Currency);
                }

                if (((DiscountComponent)discountComponent).ExistProduct)
                {
                    ((DiscountComponent)discountComponent).Product.BaseOnDeriveVirtualProductPriceComponent();
                }
            }

        }
    }
}
