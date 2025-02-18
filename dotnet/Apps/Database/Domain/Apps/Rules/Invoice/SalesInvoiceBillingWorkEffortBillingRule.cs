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

    public class SalesInvoiceBillingWorkEffortBillingRule : Rule
    {
        public SalesInvoiceBillingWorkEffortBillingRule(MetaPopulation m) : base(m, new Guid("7b7ae700-45ce-432c-b203-31ff37bbf0dc")) =>
            this.Patterns = new Pattern[]
        {
            m.InvoiceItem.AssociationPattern(v => v.WorkEffortBillingsWhereInvoiceItem, v => v.AsSalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem, m.SalesInvoice),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                foreach (var salesInvoiceItem in @this.SalesInvoiceItems)
                {
                    foreach (var workEffortBilling in salesInvoiceItem.WorkEffortBillingsWhereInvoiceItem)
                    {
                        if (!@this.WorkEfforts.Contains(workEffortBilling.WorkEffort))
                        {
                            @this.AddWorkEffort(workEffortBilling.WorkEffort);
                        }
                    }
                }
            }
        }
    }
}
