// <copyright file="Domain.cs" company="Allors bvba">
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

    public class SalesInvoiceItemPaymentApplicationAmountAppliedRule : Rule
    {
        public SalesInvoiceItemPaymentApplicationAmountAppliedRule(MetaPopulation m) : base(m, new Guid("a91631d0-433f-46d0-b841-8a79a6cfdd99")) =>
            this.Patterns = new Pattern[]
            {
                m.PaymentApplication.RolePattern(v => v.AmountApplied, v => v.InvoiceItem, m.SalesInvoiceItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var amountPaid = 0M;
                foreach (PaymentApplication paymentApplication in @this.PaymentApplicationsWhereInvoiceItem)
                {
                    amountPaid += paymentApplication.AmountApplied;
                }

                if (amountPaid != @this.AmountPaid)
                {
                    @this.AmountPaid = amountPaid;
                }
            }
        }
    }
}
