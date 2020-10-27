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
                new ChangedPattern(this.M.BasePrice.FromDate),
                new ChangedPattern(this.M.BasePrice.ThroughDate),
                new ChangedPattern(this.M.BasePrice.Price),
                new ChangedPattern(this.M.BasePrice.Product),
                new ChangedPattern(this.M.BasePrice.Part),
                new ChangedPattern(this.M.BasePrice.ProductFeature),

                new ChangedPattern(this.M.DiscountComponent.FromDate),
                new ChangedPattern(this.M.DiscountComponent.ThroughDate),
                new ChangedPattern(this.M.DiscountComponent.Price),
                new ChangedPattern(this.M.DiscountComponent.Product),
                new ChangedPattern(this.M.DiscountComponent.Part),
                new ChangedPattern(this.M.DiscountComponent.ProductFeature),

                new ChangedPattern(this.M.SurchargeComponent.FromDate),
                new ChangedPattern(this.M.SurchargeComponent.ThroughDate),
                new ChangedPattern(this.M.SurchargeComponent.Price),
                new ChangedPattern(this.M.SurchargeComponent.Product),
                new ChangedPattern(this.M.SurchargeComponent.Part),
                new ChangedPattern(this.M.SurchargeComponent.ProductFeature),

                new ChangedPattern(this.M.DiscountComponent.Percentage),
                new ChangedPattern(this.M.SurchargeComponent.Percentage),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var priceComponent in matches.Cast<PriceComponent>())
            {
                var internalOrganisations = new Organisations(priceComponent.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!priceComponent.ExistPricedBy && internalOrganisations.Count() == 1)
                {
                    priceComponent.PricedBy = internalOrganisations.First();
                }

                var salesInvoices = (priceComponent.PricedBy as InternalOrganisation)?.SalesInvoicesWhereBilledFrom.Where(v => v.ExistSalesInvoiceState && v.SalesInvoiceState.IsReadyForPosting);
                foreach (var salesInvoice in salesInvoices)
                {
                    salesInvoice.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}
