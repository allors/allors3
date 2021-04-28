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
    using Database.Derivations;

    public class PriceComponentDerivePricedByRule : Rule
    {
        public PriceComponentDerivePricedByRule(MetaPopulation m) : base(m, new Guid("e52fd97f-a6b7-4fc9-8f35-249fde8ad0a6")) =>
            this.Patterns = new Pattern[]
            {
                m.PriceComponent.RolePattern(v => v.PricedBy),
                m.PriceComponent.RolePattern(v => v.FromDate),
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
            }
        }
    }
}
