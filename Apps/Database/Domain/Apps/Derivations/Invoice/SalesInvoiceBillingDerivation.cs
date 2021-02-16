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
    using Resources;

    public class SalesInvoiceBillingDerivation : DomainDerivation
    {
        public SalesInvoiceBillingDerivation(M m) : base(m, new Guid("466ee750-47ad-4db3-bbb8-fce5c7a4b342")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(m.OrderItemBilling.InvoiceItem) { Steps =  new IPropertyType[] {m.OrderItemBilling.InvoiceItem, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem }, OfType = m.SalesInvoice.Class },
            new ChangedPattern(m.WorkEffortBilling.InvoiceItem) { Steps =  new IPropertyType[] {m.WorkEffortBilling.InvoiceItem, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem }, OfType = m.SalesInvoice.Class },
            new ChangedPattern(m.ShipmentItemBilling.InvoiceItem) { Steps =  new IPropertyType[] {m.ShipmentItemBilling.InvoiceItem, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem }, OfType = m.SalesInvoice.Class },
            new ChangedPattern(m.TimeEntryBilling.InvoiceItem) { Steps =  new IPropertyType[] {m.TimeEntryBilling.InvoiceItem, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem }, OfType = m.SalesInvoice.Class },
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

                    foreach (WorkEffortBilling workEffortBilling in salesInvoiceItem.WorkEffortBillingsWhereInvoiceItem)
                    {
                        if (!@this.WorkEfforts.Contains(workEffortBilling.WorkEffort))
                        {
                            @this.AddWorkEffort(workEffortBilling.WorkEffort);
                        }
                    }

                    foreach (ShipmentItemBilling shipmentItemBilling in salesInvoiceItem.ShipmentItemBillingsWhereInvoiceItem)
                    {
                        if (!@this.Shipments.Contains(shipmentItemBilling.ShipmentItem.ShipmentWhereShipmentItem))
                        {
                            @this.AddShipment(shipmentItemBilling.ShipmentItem.ShipmentWhereShipmentItem);

                            foreach (OrderShipment orderShipment in shipmentItemBilling.ShipmentItem.OrderShipmentsWhereShipmentItem)
                            {
                                if (orderShipment.OrderItem is SalesOrderItem salesOrderItem
                                    && !@this.SalesOrders.Contains(salesOrderItem.SalesOrderWhereSalesOrderItem))
                                {
                                    @this.AddSalesOrder(salesOrderItem.SalesOrderWhereSalesOrderItem);
                                }
                            }
                        }
                    }

                    foreach (TimeEntryBilling timeEntryBilling in salesInvoiceItem.TimeEntryBillingsWhereInvoiceItem)
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
