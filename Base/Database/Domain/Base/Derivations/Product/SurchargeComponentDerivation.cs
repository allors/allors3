// <copyright file="SurchargeComponentDerivation.cs" company="Allors bvba">
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
    using Resources;

    public class SurchargeComponentDerivation : DomainDerivation
    {
        public SurchargeComponentDerivation(M m) : base(m, new Guid("1C8B75D1-3288-4DB7-987E-7A64A3225891")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.SurchargeComponent.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var surchargeComponent in matches.Cast<SurchargeComponent>())
            {
                validation.AssertAtLeastOne(surchargeComponent, M.SurchargeComponent.Price, M.SurchargeComponent.Percentage);
                validation.AssertExistsAtMostOne(surchargeComponent, M.SurchargeComponent.Price, M.SurchargeComponent.Percentage);

                if (surchargeComponent.ExistPrice)
                {
                    if (!surchargeComponent.ExistCurrency)
                    {
                        surchargeComponent.Currency = surchargeComponent.PricedBy.PreferredCurrency;
                    }

                    validation.AssertExists(surchargeComponent, M.BasePrice.Currency);
                }

                if (surchargeComponent.ExistProduct)
                {
                    surchargeComponent.Product.BaseOnDeriveVirtualProductPriceComponent();
                }
            }
        }
    }
}
