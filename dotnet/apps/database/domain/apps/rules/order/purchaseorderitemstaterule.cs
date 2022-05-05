// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class PurchaseOrderItemStateRule : Rule
    {
        public PurchaseOrderItemStateRule(MetaPopulation m) : base(m, new Guid("046a8987-0a6a-4678-8959-2d1136a2b8f8")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.IsReceivable),
                m.PurchaseOrderItem.RolePattern(v => v.QuantityReturned),
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState, v => v.PurchaseOrderItems),
                m.ShipmentReceipt.RolePattern(v => v.QuantityAccepted, v => v.OrderItem, m.PurchaseOrderItem),
                m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, m.PurchaseOrderItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.DerivePurchaseOrderItemState(validation);
            }
        }
    }

    public static class PurchaseOrderItemStateRuleExtensions
    {
        public static void DerivePurchaseOrderItemState(this PurchaseOrderItem @this, IValidation validation)
        {
            var purchaseOrder = @this.PurchaseOrderWherePurchaseOrderItem;
            var states = new PurchaseOrderItemStates(@this.Transaction());
            var purchaseOrderState = @this.PurchaseOrderWherePurchaseOrderItem?.PurchaseOrderState;

            if (purchaseOrderState != null)
            {
                // when revising
                if (purchaseOrderState.IsCreated
                    && (@this.PurchaseOrderItemState.IsInProcess || @this.PurchaseOrderItemState.IsSent)
                    && @this.ExistPurchaseOrderItemShipmentState
                    && !@this.PurchaseOrderItemShipmentState.IsReceived)
                {
                    @this.PurchaseOrderItemState = states.Created;
                }

                if (purchaseOrderState.IsInProcess &&
                    (@this.PurchaseOrderItemState.IsCreated || @this.PurchaseOrderItemState.IsOnHold))
                {
                    @this.PurchaseOrderItemState = states.InProcess;
                }

                if (purchaseOrderState.IsOnHold && @this.PurchaseOrderItemState.IsInProcess)
                {
                    @this.PurchaseOrderItemState = states.OnHold;
                }

                if (purchaseOrderState.IsSent && @this.PurchaseOrderItemState.IsInProcess)
                {
                    @this.PurchaseOrderItemState = states.Sent;
                }

                if (@this.IsValid && purchaseOrderState.IsFinished)
                {
                    @this.PurchaseOrderItemState = states.Finished;
                }

                if (@this.IsValid && purchaseOrderState.IsCancelled)
                {
                    @this.PurchaseOrderItemState = states.Cancelled;
                }

                if (@this.IsValid && purchaseOrderState.IsRejected)
                {
                    @this.PurchaseOrderItemState = states.Rejected;
                }
            }

            var purchaseOrderItemShipmentStates = new PurchaseOrderItemShipmentStates(@this.Transaction());
            var purchaseOrderItemPaymentStates = new PurchaseOrderItemPaymentStates(@this.Transaction());
            var purchaseOrderItemStates = new PurchaseOrderItemStates(@this.Transaction());

            if (@this.IsValid)
            {
                // ShipmentState
                if (@this.IsReceivable)
                {
                    var quantityReceived = 0M;
                    foreach (var shipmentReceipt in @this.ShipmentReceiptsWhereOrderItem)
                    {
                        quantityReceived += shipmentReceipt.QuantityAccepted;
                    }

                    @this.QuantityReceived = quantityReceived;
                }

                if (!@this.IsReceivable)
                {
                    @this.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(@this.Strategy.Transaction).Na;
                }
                else if (@this.QuantityReceived == 0 && @this.IsReceivable)
                {
                    @this.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(@this.Strategy.Transaction).NotReceived;
                }
                else
                {
                    @this.PurchaseOrderItemShipmentState = @this.QuantityReceived < @this.QuantityOrdered ?
                        purchaseOrderItemShipmentStates.PartiallyReceived :
                        purchaseOrderItemShipmentStates.Received;
                }

                // PaymentState
                var orderBilling = @this.OrderItemBillingsWhereOrderItem.Select(v => v.InvoiceItem).OfType<PurchaseInvoiceItem>().ToArray();

                if (orderBilling.Any())
                {
                    if (orderBilling.All(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPaid))
                    {
                        @this.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.Paid;
                    }
                    else if (orderBilling.Any(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPartiallyPaid))
                    {
                        @this.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.PartiallyPaid;
                    }
                    else
                    {
                        @this.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.NotPaid;
                    }
                }

                // PurchaseOrderItem States
                if (@this.PurchaseOrderItemState.IsSent
                    && (@this.PurchaseOrderItemShipmentState.IsReceived || @this.PurchaseOrderItemShipmentState.IsNa))
                {
                    @this.PurchaseOrderItemState = purchaseOrderItemStates.Completed;
                }

                if (@this.PurchaseOrderItemState.IsCompleted && @this.PurchaseOrderItemPaymentState.IsPaid)
                {
                    @this.PurchaseOrderItemState = purchaseOrderItemStates.Finished;
                }
            }
        }
    }
}
