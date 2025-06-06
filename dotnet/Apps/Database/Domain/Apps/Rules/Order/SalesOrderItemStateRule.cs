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

    public class SalesOrderItemStateRule : Rule
    {
        public SalesOrderItemStateRule(MetaPopulation m) : base(m, new Guid("3d2c70e3-3751-4ffb-bfaa-f9c2b81b7a70")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.DerivationTrigger),
                m.SalesOrderItem.RolePattern(v => v.ReservedFromNonSerialisedInventoryItem),
                m.SalesOrderItem.RolePattern(v => v.ReservedFromSerialisedInventoryItem),
                m.SalesOrderItem.RolePattern(v => v.QuantityPendingShipment),
                m.SalesOrderItem.RolePattern(v => v.QuantityShipped),
                m.SalesOrderItem.RolePattern(v => v.TotalExVat),
                m.SalesOrder.RolePattern(v => v.SalesOrderState, v => v.SalesOrderItems),
                m.OrderItemBilling.RolePattern(v => v.OrderItem, v => v.OrderItem, m.SalesOrderItem),
                m.ShipmentItem.AssociationPattern(v => v.ShipmentItemBillingsWhereShipmentItem,v => v.OrderShipmentsWhereShipmentItem.ObjectType.OrderItem, m.SalesOrderItem),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand, v => v.SalesOrderItemsWhereReservedFromNonSerialisedInventoryItem),
                m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.OrderItemBillingsWhereInvoiceItem.ObjectType.OrderItem, m.SalesOrderItem),
                m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.ShipmentItemBillingsWhereInvoiceItem.ObjectType.ShipmentItem.ObjectType.OrderShipmentsWhereShipmentItem.ObjectType.OrderItem, m.SalesOrderItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(transaction);
                var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(transaction);
                var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(transaction);
                var salesOrderItemStates = new SalesOrderItemStates(transaction);

                if (salesOrder != null
                    && salesOrder.ExistSalesOrderState
                    && (@this.IsValid || (!@this.IsValid && salesOrder.ExistLastSalesOrderState && salesOrder.LastSalesOrderState.IsCancelled && !salesOrder.SalesOrderState.IsCancelled)))
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
                        && (@this.SalesOrderItemState.IsAwaitingAcceptance || @this.SalesOrderItemState.IsOnHold))
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
                    SalesOrderItemShipmentState shipmentState;

                    // ShipmentState
                    if (!@this.ExistProduct)
                    {
                        shipmentState = salesOrderItemShipmentStates.NotApplicable;
                    }
                    else if (@this.QuantityShipped == 0 && @this.QuantityPendingShipment == 0)
                    {
                        shipmentState = salesOrderItemShipmentStates.NotShipped;
                    }
                    else if (@this.QuantityShipped == 0 && @this.QuantityPendingShipment > 0)
                    {
                        shipmentState = salesOrderItemShipmentStates.InProgress;
                    }
                    else
                    {
                        shipmentState = @this.QuantityShipped < @this.QuantityOrdered ?
                                                                         salesOrderItemShipmentStates.PartiallyShipped :
                                                                         salesOrderItemShipmentStates.Shipped;
                    }

                    if (@this.SalesOrderItemShipmentState != shipmentState)
                    {
                        @this.SalesOrderItemShipmentState = shipmentState;
                    }

                    // PaymentState
                    var orderBilling = @this.OrderItemBillingsWhereOrderItem.Select(v => v.InvoiceItem).OfType<SalesInvoiceItem>().ToArray();
                    SalesOrderItemPaymentState paymentState;

                    if (orderBilling.Any())
                    {
                        if (orderBilling.All(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                        {
                            paymentState = salesOrderItemPaymentStates.Paid;
                        }
                        else if (orderBilling.Any(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPartiallyPaid))
                        {
                            paymentState = salesOrderItemPaymentStates.PartiallyPaid;
                        }
                        else
                        {
                            paymentState = salesOrderItemPaymentStates.NotPaid;
                        }
                    }
                    else
                    {
                        var shipmentBilling = @this.OrderShipmentsWhereOrderItem.SelectMany(v => v.ShipmentItem.ShipmentItemBillingsWhereShipmentItem).Select(v => v.InvoiceItem).OfType<SalesInvoiceItem>().ToArray();
                        if (shipmentBilling.Any())
                        {
                            if (shipmentBilling.All(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                            {
                                paymentState = salesOrderItemPaymentStates.Paid;
                            }
                            else if (shipmentBilling.Any(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPartiallyPaid))
                            {
                                paymentState = salesOrderItemPaymentStates.PartiallyPaid;
                            }
                            else
                            {
                                paymentState = salesOrderItemPaymentStates.NotPaid;
                            }
                        }
                        else
                        {
                            paymentState = salesOrderItemPaymentStates.NotPaid;
                        }
                    }

                    if (@this.SalesOrderItemPaymentState != paymentState)
                    {
                        @this.SalesOrderItemPaymentState = paymentState;
                    }

                    var amountAlreadyInvoiced = @this.OrderItemBillingsWhereOrderItem?.Sum(v => v.Amount);
                    if (amountAlreadyInvoiced == 0)
                    {
                        amountAlreadyInvoiced = @this.OrderShipmentsWhereOrderItem
                            .SelectMany(orderShipment => orderShipment.ShipmentItem.ShipmentItemBillingsWhereShipmentItem)
                            .Aggregate(amountAlreadyInvoiced, (current, shipmentItemBilling) => current + shipmentItemBilling.Amount);
                    }

                    var leftToInvoice = @this.TotalExVat - amountAlreadyInvoiced;
                    SalesOrderItemInvoiceState invoiceState;

                    if (amountAlreadyInvoiced == 0)
                    {
                        invoiceState = salesOrderItemInvoiceStates.NotInvoiced;
                    }
                    else if (amountAlreadyInvoiced > 0 && leftToInvoice > 0)
                    {
                        invoiceState = salesOrderItemInvoiceStates.PartiallyInvoiced;
                    }
                    else
                    {
                        invoiceState = salesOrderItemInvoiceStates.Invoiced;
                    }

                    if (@this.SalesOrderItemInvoiceState != invoiceState)
                    {
                        @this.SalesOrderItemInvoiceState = invoiceState;
                    }

                    // SalesOrderItem States
                    if (@this.SalesOrderItemShipmentState.IsShipped && @this.SalesOrderItemInvoiceState.IsInvoiced)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Completed;
                    }

                    if (@this.SalesOrderItemState.IsCompleted && @this.SalesOrderItemPaymentState.IsPaid)
                    {
                        @this.SalesOrderItemState = salesOrderItemStates.Finished;
                    }
                }
            }
        }
    }
}
