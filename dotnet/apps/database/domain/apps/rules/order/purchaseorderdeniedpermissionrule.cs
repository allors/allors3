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

    public class PurchaseOrderDeniedPermissionRule : Rule
    {
        public PurchaseOrderDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("23ec4c76-b156-406f-ad16-4b83a17db3c6")) =>
            this.Patterns = new Pattern[]
        {
            m.PurchaseOrder.RolePattern(v => v.TransitionalRevocations),
            m.PurchaseOrder.RolePattern(v => v.PurchaseOrderShipmentState),
            m.PurchaseOrder.AssociationPattern(v => v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder),
            m.PurchaseOrder.AssociationPattern(v => v.PurchaseInvoicesWherePurchaseOrder),
            m.PurchaseOrder.AssociationPattern(v => v.SerialisedItemsWherePurchaseOrder),
            m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState, v => v.PurchaseOrderWherePurchaseOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
            m.OrderItem.AssociationPattern(v => v.OrderShipmentsWhereOrderItem, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
            m.OrderItem.AssociationPattern(v => v.OrderRequirementCommitmentsWhereOrderItem, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
            m.OrderItem.AssociationPattern(v => v.WorkEffortsWhereOrderItemFulfillment, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var deleteRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderDeleteRevocation;
                var invoiceRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderInvoiceRevocation;
                var quickReceiveRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderQuickReceiveRevocation;
                var returnRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderReturnRevocation;
                var reviseRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderReviseRevocation;
                var receivedRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderReceivedRevocation;
                var reopenRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderReopenRevocation;
                var writeRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderWriteRevocation;

                if (@this.CanInvoice)
                {
                    @this.RemoveRevocation(invoiceRevocation);
                }
                else
                {
                    @this.AddRevocation(invoiceRevocation);
                }

                if (@this.CanRevise)
                {
                    @this.RemoveRevocation(reviseRevocation);
                }
                else
                {
                    @this.AddRevocation(reviseRevocation);
                }

                if (@this.IsReceivable)
                {
                    @this.RemoveRevocation(quickReceiveRevocation);
                }
                else
                {
                    @this.AddRevocation(quickReceiveRevocation);
                }

                var returnItemRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderItemReturnRevocation;
                if (@this.ValidOrderItems.All(v => ((PurchaseOrderItem)v).Revocations.Contains(returnItemRevocation)))
                {
                    @this.AddRevocation(returnRevocation);
                }

                if (@this.IsDeletable)
                {
                    @this.RemoveRevocation(deleteRevocation);
                }
                else
                {
                    @this.AddRevocation(deleteRevocation);
                }

                if (!@this.PurchaseOrderShipmentState.IsNotReceived && !@this.PurchaseOrderShipmentState.IsNa)
                {
                    @this.AddRevocation(receivedRevocation);
                    @this.AddRevocation(writeRevocation);
                }

                if (@this.PurchaseOrderState.IsCompleted && @this.PurchaseOrderPaymentState.IsNotPaid && @this.PurchaseOrderShipmentState.IsNa)
                {
                    @this.RemoveRevocation(reopenRevocation);
                }
            }
        }
    }
}
