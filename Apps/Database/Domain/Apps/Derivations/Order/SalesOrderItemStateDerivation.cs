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

    public class SalesOrderItemStateDerivation : DomainDerivation
    {
        public SalesOrderItemStateDerivation(M m) : base(m, new Guid("3d2c70e3-3751-4ffb-bfaa-f9c2b81b7a70")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItem.ReservedFromNonSerialisedInventoryItem),
                new ChangedPattern(m.SalesOrderItem.ReservedFromSerialisedInventoryItem),
                new ChangedPattern(m.SalesOrderItem.QuantityPendingShipment),
                new ChangedPattern(m.SalesOrder.SalesOrderState) {Steps = new IPropertyType[]{ m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(m.OrderItemBilling.OrderItem) {Steps = new IPropertyType[]{ m.OrderItemBilling.OrderItem}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(m.OrderShipment.Quantity) {Steps = new IPropertyType[]{ m.OrderShipment.OrderItem}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(m.NonSerialisedInventoryItem.QuantityOnHand) {Steps = new IPropertyType[]{ m.NonSerialisedInventoryItem.SalesOrderItemsWhereReservedFromNonSerialisedInventoryItem} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(session);
                var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(session);
                var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(session);
                var salesOrderItemStates = new SalesOrderItemStates(session);

                if (@this.IsValid && salesOrder != null && salesOrder.ExistSalesOrderState)
                {
                    if (salesOrder.SalesOrderState.IsProvisional)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Provisional;
                    }

                    if (salesOrder.SalesOrderState.IsReadyForPosting &&
                        (@this.SalesOrderItemState.IsProvisional || @this.SalesOrderItemState.IsRequestsApproval || @this.SalesOrderItemState.IsOnHold))
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.ReadyForPosting;
                    }

                    if (salesOrder.SalesOrderState.IsRequestsApproval &&
                        (@this.SalesOrderItemState.IsProvisional || @this.SalesOrderItemState.IsOnHold))
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.RequestsApproval;
                    }

                    if (salesOrder.SalesOrderState.IsAwaitingAcceptance
                        && @this.SalesOrderItemState.IsReadyForPosting)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.AwaitingAcceptance;
                    }

                    if (salesOrder.SalesOrderState.IsInProcess
                        && @this.SalesOrderItemState.IsAwaitingAcceptance || @this.SalesOrderItemState.IsOnHold)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.InProcess;
                    }

                    if (salesOrder.SalesOrderState.IsOnHold && @this.SalesOrderItemState.IsInProcess)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.OnHold;
                    }

                    if (salesOrder.SalesOrderState.IsFinished)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Finished;
                    }

                    if (salesOrder.SalesOrderState.IsCancelled)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Cancelled;
                    }

                    if (salesOrder.SalesOrderState.IsRejected)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Rejected;
                    }
                }

                if (@this.IsValid)
                {
                    // ShipmentState
                    if (!@this.ExistOrderShipmentsWhereOrderItem)
                    {
                        @this.SalesOrderItemShipmentState = salesOrderItemShipmentStates.NotShipped;
                    }
                    else if (@this.QuantityShipped == 0 && @this.QuantityPendingShipment > 0)
                    {
                        @this.SalesOrderItemShipmentState = salesOrderItemShipmentStates.InProgress;
                    }
                    else
                    {
                        @this.SalesOrderItemShipmentState = @this.QuantityShipped < @this.QuantityOrdered ?
                                                                         salesOrderItemShipmentStates.PartiallyShipped :
                                                                         salesOrderItemShipmentStates.Shipped;
                    }

                    // PaymentState
                    var orderBilling = @this.OrderItemBillingsWhereOrderItem.Select(v => v.InvoiceItem).OfType<SalesInvoiceItem>().ToArray();

                    if (orderBilling.Any())
                    {
                        if (orderBilling.All(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.Paid;
                        }
                        else if (orderBilling.Any(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPartiallyPaid))
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.PartiallyPaid;
                        }
                        else
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                        }
                    }
                    else
                    {
                        var shipmentBilling = @this.OrderShipmentsWhereOrderItem.SelectMany(v => v.ShipmentItem.ShipmentItemBillingsWhereShipmentItem).Select(v => v.InvoiceItem).OfType<SalesInvoiceItem>().ToArray();
                        if (shipmentBilling.Any())
                        {
                            if (shipmentBilling.All(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                            {
                                @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.Paid;
                            }
                            else if (shipmentBilling.Any(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPartiallyPaid))
                            {
                                @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.PartiallyPaid;
                            }
                            else
                            {
                                @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                            }
                        }
                        else
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                        }
                    }

                    var amountAlreadyInvoiced = @this.OrderItemBillingsWhereOrderItem?.Sum(v => v.Amount);
                    if (amountAlreadyInvoiced == 0)
                    {
                        amountAlreadyInvoiced = @this.OrderShipmentsWhereOrderItem
                            .SelectMany(orderShipment => orderShipment.ShipmentItem.ShipmentItemBillingsWhereShipmentItem)
                            .Aggregate(amountAlreadyInvoiced, (current, shipmentItemBilling) => current + shipmentItemBilling.Amount);
                    }

                    var leftToInvoice = @this.TotalExVat - amountAlreadyInvoiced;

                    if (amountAlreadyInvoiced == 0)
                    {
                        @this.SalesOrderItemInvoiceState = salesOrderItemInvoiceStates.NotInvoiced;
                    }
                    else if (amountAlreadyInvoiced > 0 && leftToInvoice > 0)
                    {
                        @this.SalesOrderItemInvoiceState = salesOrderItemInvoiceStates.PartiallyInvoiced;
                    }
                    else
                    {
                        @this.SalesOrderItemInvoiceState = salesOrderItemInvoiceStates.Invoiced;
                    }

                    // SalesOrderItem States
                    if (@this.SalesOrderItemShipmentState.Shipped && @this.SalesOrderItemInvoiceState.Invoiced)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Completed;
                    }

                    if (@this.SalesOrderItemState.IsCompleted && @this.SalesOrderItemPaymentState.Paid)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Finished;
                    }

                    if (@this.SalesOrderItemState.IsInProcess && !@this.SalesOrderItemShipmentState.Shipped)
                    {
                        if (@this.ExistReservedFromNonSerialisedInventoryItem)
                        {
                            if ((salesOrder.OrderKind?.ScheduleManually == true && @this.QuantityPendingShipment > 0)
                                || !salesOrder.ExistOrderKind || salesOrder.OrderKind.ScheduleManually == false)
                            {
                                var committedOutSameProductOtherItem = salesOrder.SalesOrderItems
                                    .Where(v => !Equals(v, @this) && Equals(v.Product, @this.Product))
                                    .Sum(v => v.QuantityRequestsShipping);

                                var qoh = @this.ReservedFromNonSerialisedInventoryItem.QuantityOnHand;

                                var atp = @this.ReservedFromNonSerialisedInventoryItem.AvailableToPromise - committedOutSameProductOtherItem > 0 ?
                                    @this.ReservedFromNonSerialisedInventoryItem.AvailableToPromise - committedOutSameProductOtherItem :
                                    0;

                                var quantityCommittedOut = @this.SalesOrderItemInventoryAssignments
                                    .SelectMany(v => v.InventoryItemTransactions)
                                    .Where(t => t.Reason.Equals(new InventoryTransactionReasons(session).Reservation))
                                    .Sum(v => v.Quantity);

                                var wantToShip = quantityCommittedOut - @this.QuantityPendingShipment;

                                var inventoryAssignment = @this.SalesOrderItemInventoryAssignments.FirstOrDefault(v => v.InventoryItem.Equals(@this.ReservedFromNonSerialisedInventoryItem));
                                if (quantityCommittedOut > qoh)
                                {
                                    wantToShip = qoh;
                                }

                                //if (salesOrderItem.ExistPreviousReservedFromNonSerialisedInventoryItem
                                //    && !Equals(salesOrderItem.ReservedFromNonSerialisedInventoryItem, salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem))
                                //{
                                //    var previousInventoryAssignment = salesOrderItem.SalesOrderItemInventoryAssignments.FirstOrDefault(v => v.InventoryItem.Equals(salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem));
                                //    previousInventoryAssignment.Quantity = 0;

                                //    foreach (OrderShipment orderShipment in salesOrderItem.OrderShipmentsWhereOrderItem)
                                //    {
                                //        orderShipment.Delete();
                                //    }

                                //    quantityCommittedOut = 0;
                                //    wantToShip = 0;
                                //}

                                var neededFromInventory = @this.QuantityOrdered - @this.QuantityShipped - quantityCommittedOut;
                                var availableFromInventory = neededFromInventory < atp ? neededFromInventory : atp;

                                if (neededFromInventory != 0 || @this.QuantityShortFalled > 0)
                                {
                                    if (inventoryAssignment == null)
                                    {
                                        var salesOrderItemInventoryAssignment = new SalesOrderItemInventoryAssignmentBuilder(session)
                                            .WithInventoryItem(@this.ReservedFromNonSerialisedInventoryItem)
                                            .WithQuantity(wantToShip + availableFromInventory)
                                            .Build();

                                        @this.AddSalesOrderItemInventoryAssignment(salesOrderItemInventoryAssignment);
                                    }
                                    else
                                    {
                                        inventoryAssignment.InventoryItem = @this.ReservedFromNonSerialisedInventoryItem;
                                        if (quantityCommittedOut > qoh)
                                        {
                                            inventoryAssignment.Quantity = qoh;
                                        }
                                        else
                                        {
                                            inventoryAssignment.Quantity = quantityCommittedOut + availableFromInventory;
                                        }
                                    }

                                    @this.QuantityRequestsShipping = wantToShip + availableFromInventory;

                                    if (@this.QuantityRequestsShipping > qoh)
                                    {
                                        @this.QuantityRequestsShipping = qoh;
                                    }

                                    if (salesOrder.OrderKind?.ScheduleManually == true)
                                    {
                                        @this.QuantityRequestsShipping = 0;
                                    }

                                    @this.QuantityReserved = @this.QuantityOrdered - @this.QuantityShipped;
                                    @this.QuantityShortFalled = neededFromInventory - availableFromInventory > 0 ? neededFromInventory - availableFromInventory : 0;
                                }
                            }
                        }

                        if (@this.ExistReservedFromSerialisedInventoryItem)
                        {
                            var inventoryAssignment = @this.SalesOrderItemInventoryAssignments.FirstOrDefault(v => v.InventoryItem.Equals(@this.ReservedFromSerialisedInventoryItem));
                            if (inventoryAssignment == null)
                            {
                                var salesOrderItemInventoryAssignment = new SalesOrderItemInventoryAssignmentBuilder(session)
                                     .WithInventoryItem(@this.ReservedFromSerialisedInventoryItem)
                                     .WithQuantity(1)
                                     .Build();

                                @this.AddSalesOrderItemInventoryAssignment(salesOrderItemInventoryAssignment);

                                @this.QuantityRequestsShipping = 1;
                            }

                            @this.QuantityReserved = 1;
                        }
                    }
                }
            }
        }
    }
}
