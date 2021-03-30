// <copyright file="PriceComponentDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations;
    using Database.Derivations;

    public class PriceComponentRule : Rule
    {
        public PriceComponentRule(M m) : base(m, new Guid("34F7833F-170D-45C3-92F0-B8AD33C3A028")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PriceComponent, m.PriceComponent.FromDate),
                new RolePattern(m.PriceComponent, m.PriceComponent.ThroughDate),
                new RolePattern(m.PriceComponent, m.PriceComponent.PricedBy),
                new RolePattern(m.PriceComponent, m.PriceComponent.Price),
                new RolePattern(m.PriceComponent, m.PriceComponent.Product),
                new RolePattern(m.PriceComponent, m.PriceComponent.Part),
                new RolePattern(m.PriceComponent, m.PriceComponent.ProductFeature),
                new RolePattern(m.DiscountComponent, m.DiscountComponent.Percentage),
                new RolePattern(m.SurchargeComponent, m.SurchargeComponent.Percentage)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PriceComponent>())
            {
                foreach (var salesInvoice in (@this.PricedBy as InternalOrganisation)?.SalesInvoicesWhereBilledFrom.Where(v => v.ExistSalesInvoiceState && v.SalesInvoiceState.IsReadyForPosting))
                {
                    salesInvoice.DerivationTrigger = Guid.NewGuid();
                }

                foreach (var salesOrder in (@this.PricedBy as InternalOrganisation)?.SalesOrdersWhereTakenBy.Where(v => v.ExistSalesOrderState && v.SalesOrderState.IsProvisional))
                {
                    salesOrder.DerivationTrigger = Guid.NewGuid();
                }

                foreach (var quote in (@this.PricedBy as InternalOrganisation)?.QuotesWhereIssuer.Where(v => v.ExistQuoteState && v.QuoteState.IsCreated))
                {
                    quote.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistPrice)
                {
                    if (!@this.ExistCurrency && @this.ExistPricedBy)
                    {
                        @this.Currency = @this.PricedBy.PreferredCurrency;
                    }

                    validation.AssertExists(@this, this.M.BasePrice.Currency);
                }
            }
        }
    }
}
