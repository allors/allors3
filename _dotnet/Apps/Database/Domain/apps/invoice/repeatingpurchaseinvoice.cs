// <copyright file="RepeatingPurchaseInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class RepeatingPurchaseInvoice
    {
        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistInternalOrganisation && internalOrganisations.Count() == 1)
            {
                this.InternalOrganisation = internalOrganisations.First();
            }
        }

        public void Repeat()
        {
            var now = this.Strategy.Transaction.Now().Date;
            var monthly = new TimeFrequencies(this.Strategy.Transaction).Month;
            var weekly = new TimeFrequencies(this.Strategy.Transaction).Week;

            if (this.Frequency.Equals(monthly))
            {
                var nextDate = now.AddMonths(1).Date;
                this.Repeat(now, nextDate);
            }

            if (this.Frequency.Equals(weekly))
            {
                var nextDate = now.AddDays(7).Date;
                this.Repeat(now, nextDate);
            }
        }

        private void Repeat(DateTime now, DateTime nextDate)
        {
            if (!this.ExistFinalExecutionDate || nextDate <= this.FinalExecutionDate.Value.Date)
            {
                this.NextExecutionDate = nextDate.Date;
            }

            var orderCandidates = this.Supplier.PurchaseOrdersWhereTakenViaSupplier
                .Where(v => v.OrderedBy.Equals(this.InternalOrganisation) &&
                            (v.PurchaseOrderState.IsSent || v.PurchaseOrderState.IsCompleted) &&
                            (v.PurchaseOrderShipmentState.IsReceived || v.PurchaseOrderShipmentState.IsPartiallyReceived));

            var orderItemsToBill = new List<PurchaseOrderItem>();
            foreach (var purchaseOrder in orderCandidates)
            {
                foreach (PurchaseOrderItem purchaseOrderItem in purchaseOrder.ValidOrderItems)
                {
                    if (!purchaseOrderItem.ExistOrderItemBillingsWhereOrderItem &&
                        purchaseOrderItem.PurchaseOrderItemShipmentState.IsReceived || purchaseOrderItem.PurchaseOrderItemShipmentState.IsPartiallyReceived || !purchaseOrderItem.ExistPart && purchaseOrderItem.QuantityReceived == 1)
                    {
                        orderItemsToBill.Add(purchaseOrderItem);
                    }
                }
            }

            if (orderItemsToBill.Any())
            {
                var purchaseInvoice = new PurchaseInvoiceBuilder(this.Strategy.Transaction)
                    .WithBilledFrom(this.Supplier)
                    .WithBilledTo(this.InternalOrganisation)
                    .WithInvoiceDate(this.Transaction().Now())
                    .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction()).PurchaseInvoice)
                    .Build();

                foreach (var orderItem in orderItemsToBill)
                {
                    var invoiceItem = new PurchaseInvoiceItemBuilder(this.Strategy.Transaction)
                        .WithAssignedUnitPrice(orderItem.UnitPrice)
                        .WithPart(orderItem.Part)
                        .WithQuantity(orderItem.QuantityOrdered)
                        .WithAssignedVatRegime(orderItem.AssignedVatRegime)
                        .WithAssignedIrpfRegime(orderItem.AssignedIrpfRegime)
                        .WithDescription(orderItem.Description)
                        .WithInternalComment(orderItem.InternalComment)
                        .WithMessage(orderItem.Message)
                        .Build();

                    if (invoiceItem.ExistPart)
                    {
                        invoiceItem.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).PartItem;
                    }
                    else
                    {
                        invoiceItem.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).Service;
                    }

                    purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);

                    new OrderItemBillingBuilder(this.Strategy.Transaction)
                        .WithQuantity(orderItem.QuantityOrdered)
                        .WithAmount(orderItem.TotalBasePrice)
                        .WithOrderItem(orderItem)
                        .WithInvoiceItem(invoiceItem)
                        .Build();
                }
            }

            this.PreviousExecutionDate = now.Date;
        }
    }
}
