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

    public class PriceComponentDerivation : DomainDerivation
    {
        public PriceComponentDerivation(M m) : base(m, new Guid("34F7833F-170D-45C3-92F0-B8AD33C3A028")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PriceComponent.FromDate),
                new ChangedPattern(m.PriceComponent.ThroughDate),
                new ChangedPattern(m.PriceComponent.Price),
                new ChangedPattern(m.PriceComponent.Product),
                new ChangedPattern(m.PriceComponent.Part),
                new ChangedPattern(m.PriceComponent.ProductFeature),
                new ChangedPattern(m.DiscountComponent.Percentage),
                new ChangedPattern(m.SurchargeComponent.Percentage)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
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
            }
        }
    }
}
