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

    public class SalesInvoiceBillingShipmentItemBillingRule : Rule
    {
        public SalesInvoiceBillingShipmentItemBillingRule(MetaPopulation m) : base(m, new Guid("245140bb-ef98-487c-82dd-4ecdf5bc3f71")) =>
            this.Patterns = new Pattern[]
        {
            m.InvoiceItem.AssociationPattern(v => v.ShipmentItemBillingsWhereInvoiceItem, v => v.AsSalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem, m.SalesInvoice)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                foreach (SalesInvoiceItem salesInvoiceItem in @this.SalesInvoiceItems)
                {
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
                }
            }
        }
    }
}
