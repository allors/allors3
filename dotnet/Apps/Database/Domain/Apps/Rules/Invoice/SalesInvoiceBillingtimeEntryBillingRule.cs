// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SalesInvoiceBillingtimeEntryBillingRule : Rule
    {
        public SalesInvoiceBillingtimeEntryBillingRule(MetaPopulation m) : base(m, new Guid("0f32d928-4a86-4421-9249-dcb3bb0a5940")) =>
            this.Patterns = new Pattern[]
        {
            m.InvoiceItem.AssociationPattern(v => v.TimeEntryBillingsWhereInvoiceItem, v => v.AsSalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem, m.SalesInvoice),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                foreach (var salesInvoiceItem in @this.SalesInvoiceItems)
                {
                    foreach (var timeEntryBilling in salesInvoiceItem.TimeEntryBillingsWhereInvoiceItem)
                    {
                        if (!@this.WorkEfforts.Contains(timeEntryBilling.TimeEntry.WorkEffort))
                        {
                            @this.AddWorkEffort(timeEntryBilling.TimeEntry.WorkEffort);
                        }
                    }
                }
            }
        }
    }
}
