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

    public class SurchargeComponentDerivation : DomainDerivation
    {
        public SurchargeComponentDerivation(M m) : base(m, new Guid("1C8B75D1-3288-4DB7-987E-7A64A3225891")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.SurchargeComponent.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SurchargeComponent>())
            {
                validation.AssertAtLeastOne(@this, this.M.SurchargeComponent.Price, this.M.SurchargeComponent.Percentage);
                validation.AssertExistsAtMostOne(@this, this.M.SurchargeComponent.Price, this.M.SurchargeComponent.Percentage);

                if (@this.ExistPrice)
                {
                    if (!@this.ExistCurrency)
                    {
                        @this.Currency = @this.PricedBy.PreferredCurrency;
                    }

                    validation.AssertExists(@this, this.M.BasePrice.Currency);
                }

                if (@this.ExistProduct)
                {
                    @this.Product.AppsOnDeriveVirtualProductPriceComponent();
                }
            }
        }
    }
}
