// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class PurchaseOrderStateDerivation : DomainDerivation
    {
        public PurchaseOrderStateDerivation(M m) : base(m, new Guid("96a20d70-69d3-4750-bc44-5551ca5b1c78")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.PurchaseOrder.PurchaseOrderState),
                new AssociationPattern(m.PurchaseOrder.PurchaseOrderItems),
                new AssociationPattern(m.PurchaseOrderItem.DerivationTrigger) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(m.PurchaseOrderItem.PurchaseOrderItemShipmentState) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(m.PurchaseOrderItem.PurchaseOrderItemPaymentState) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                var purchaseOrderShipmentStates = new PurchaseOrderShipmentStates(@this.Strategy.Transaction);
                var purchaseOrderPaymentStates = new PurchaseOrderPaymentStates(@this.Strategy.Transaction);
                var purchaseOrderItemStates = new PurchaseOrderItemStates(cycle.Transaction);

                // PurchaseOrder Shipment State
                if (@this.ValidOrderItems.Any())
                {
                    if (@this.ValidOrderItems.Any(v => ((PurchaseOrderItem)v).IsReceivable))
                    {
                        if (@this.ValidOrderItems.Where(v => ((PurchaseOrderItem)v).IsReceivable).All(v => ((PurchaseOrderItem)v).ExistPurchaseOrderItemShipmentState && ((PurchaseOrderItem)v).PurchaseOrderItemShipmentState.IsReceived))
                        {
                            @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.Received;
                        }
                        else if (@this.ValidOrderItems.Where(v => ((PurchaseOrderItem)v).IsReceivable).All(v => ((PurchaseOrderItem)v).ExistPurchaseOrderItemShipmentState && ((PurchaseOrderItem)v).PurchaseOrderItemShipmentState.IsNotReceived))
                        {
                            @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.NotReceived;
                        }
                        else
                        {
                            @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.PartiallyReceived;
                        }
                    }
                    else
                    {
                        @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.Na;
                    }

                    // PurchaseOrder Payment State
                    if (@this.ValidOrderItems.All(v => ((PurchaseOrderItem)v).ExistPurchaseOrderItemPaymentState && ((PurchaseOrderItem)v).PurchaseOrderItemPaymentState.IsPaid))
                    {
                        @this.PurchaseOrderPaymentState = purchaseOrderPaymentStates.Paid;
                    }
                    else if (@this.ValidOrderItems.All(v => ((PurchaseOrderItem)v).ExistPurchaseOrderItemPaymentState && ((PurchaseOrderItem)v).PurchaseOrderItemPaymentState.IsNotPaid))
                    {
                        @this.PurchaseOrderPaymentState = purchaseOrderPaymentStates.NotPaid;
                    }
                    else
                    {
                        @this.PurchaseOrderPaymentState = purchaseOrderPaymentStates.PartiallyPaid;
                    }

                    // PurchaseOrder OrderState
                    if (@this.PurchaseOrderState.IsSent
                        && (@this.PurchaseOrderShipmentState.IsReceived || @this.PurchaseOrderShipmentState.IsNa))
                    {
                        @this.PurchaseOrderState = new PurchaseOrderStates(@this.Strategy.Transaction).Completed;
                    }

                    if (@this.PurchaseOrderState.IsCompleted && @this.PurchaseOrderPaymentState.IsPaid)
                    {
                        @this.PurchaseOrderState = new PurchaseOrderStates(@this.Strategy.Transaction).Finished;
                    }
                }
            }
        }
    }
}
