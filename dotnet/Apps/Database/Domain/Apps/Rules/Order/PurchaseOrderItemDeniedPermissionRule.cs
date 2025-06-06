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

    public class PurchaseOrderItemDeniedPermissionRule : Rule
    {
        public PurchaseOrderItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("68b556f7-00ae-49a7-8d51-49c52ae18b4d")) =>
            this.Patterns = new Pattern[]
        {
            m.PurchaseOrderItem.RolePattern(v => v.TransitionalRevocations),
            m.PurchaseOrderItem.RolePattern(v => v.QuantityReturned),
            m.OrderItemBilling.RolePattern(v => v.OrderItem, v => v.OrderItem, m.PurchaseOrderItem),
            m.OrderRequirementCommitment.RolePattern(v => v.OrderItem, v => v.OrderItem, m.PurchaseOrderItem),
            m.OrderItem.AssociationPattern(v => v.WorkEffortsWhereOrderItemFulfillment, m.PurchaseOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderShipmentsWhereOrderItem, m.PurchaseOrderItem),
            m.OrderShipment.RolePattern(v => v.OrderItem, v => v.OrderItem, m.PurchaseOrderItem),
            m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand, v => v.Part.ObjectType.PurchaseOrderItemsWherePart),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.DerivePurchaseOrderItemDeniedPermission(validation);
            }
        }
    }

    public static class PurchaseOrderItemDeniedPermissionRuleExtensions
    {
        public static void DerivePurchaseOrderItemDeniedPermission(this PurchaseOrderItem @this, IValidation validation)
        {
            @this.Revocations = @this.TransitionalRevocations;
            var m = @this.Transaction().Database.Services.Get<MetaPopulation>();

            var deleteRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderItemDeleteRevocation;
            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }

            var writeRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderItemWriteRevocation;
            var executeRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderItemExecuteRevocation;
            var quickReceiveRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderItemQuickReceiveRevocation;
            var returnRevocation = new Revocations(@this.Strategy.Transaction).PurchaseOrderItemReturnRevocation;

            // Should be possible to change item price even after shipment is present.
            // When revising order then OrderItemState.IsCreated
            if (!@this.PurchaseOrderItemState.IsCreated
                && @this.ExistPurchaseOrderItemShipmentState
                && !@this.PurchaseOrderItemShipmentState.IsNotReceived
                && !@this.PurchaseOrderItemShipmentState.IsNa)
            {
                @this.AddRevocation(writeRevocation);
                @this.AddRevocation(executeRevocation);
                @this.RemoveRevocation(returnRevocation);
            }

            // If not IsAutomaticallyReceived there could be shipment waiting to be received. Can not execute quickreceive again.
            foreach (var orderShipment in @this.OrderShipmentsWhereOrderItem)
            {
                if (orderShipment.ExistShipmentItem && orderShipment.ShipmentItem.ShipmentWhereShipmentItem.Strategy.Class.Equals(m.PurchaseShipment))
                {
                    @this.AddRevocation(quickReceiveRevocation);
                }

                if (orderShipment.ExistShipmentItem && orderShipment.ShipmentItem.ShipmentWhereShipmentItem.Strategy.Class.Equals(m.PurchaseReturn))
                {
                    @this.AddRevocation(returnRevocation);
                }
            }

            if (@this.QuantityReceived == @this.QuantityReturned)
            {
                @this.AddRevocation(returnRevocation);
            }

            if (@this.ExistPart && @this.QuantityReceived - @this.QuantityReturned > 0)
            {
                var quantityPossibleToQuickReturn = @this.QuantityReceived - @this.QuantityReturned;
                if (@this.Part.InventoryItemKind.IsSerialised)
                {
                    if (@this.Part.InventoryItemsWherePart.FirstOrDefault(v => v.Facility.Equals(@this.StoredInFacility)) is SerialisedInventoryItem inventoryItem && inventoryItem.Quantity == 0)
                    {
                        @this.AddRevocation(returnRevocation);
                    }
                }
                else
                {
                    if (@this.Part.InventoryItemsWherePart.FirstOrDefault(v => v.Facility.Equals(@this.StoredInFacility)) is NonSerialisedInventoryItem inventoryItem && inventoryItem.QuantityOnHand < quantityPossibleToQuickReturn)
                    {
                        @this.AddRevocation(returnRevocation);
                    }
                }
            }
        }
    }
}
