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
    using Derivations.Rules;

    public class SalesInvoiceBillingOrderItemBillingRule : Rule
    {
        public SalesInvoiceBillingOrderItemBillingRule(MetaPopulation m) : base(m, new Guid("521a616a-6cd1-4610-9a72-1c41218145c5")) =>
            this.Patterns = new Pattern[]
        {
            m.InvoiceItem.AssociationPattern(v => v.OrderItemBillingsWhereInvoiceItem, v => v.AsSalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem, m.SalesInvoice),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                foreach (SalesInvoiceItem salesInvoiceItem in @this.SalesInvoiceItems)
                {
                    foreach (OrderItemBilling orderItemBilling in salesInvoiceItem.OrderItemBillingsWhereInvoiceItem)
                    {
                        if (orderItemBilling.OrderItem is SalesOrderItem salesOrderItem
                            && !@this.SalesOrders.Contains(salesOrderItem.SalesOrderWhereSalesOrderItem))
                        {
                            @this.AddSalesOrder(salesOrderItem.SalesOrderWhereSalesOrderItem);
                        }
                    }
                }
            }
        }
    }
}
