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

    public class SalesOrderItemQuantatiesDerivation : DomainDerivation
    {
        public SalesOrderItemQuantatiesDerivation(M m) : base(m, new Guid("5790f640-2435-466e-8f34-6cb817008e3d")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemState),
                new ChangedPattern(m.SalesOrderItem.QuantityPendingShipment),
                new ChangedPattern(m.SalesOrderItem.QuantityShipped),
                new ChangedPattern(m.NonSerialisedInventoryItem.QuantityOnHand) {Steps = new IPropertyType[]{m.NonSerialisedInventoryItem.SalesOrderItemsWhereReservedFromNonSerialisedInventoryItem}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(m.NonSerialisedInventoryItem.QuantityCommittedOut) {Steps = new IPropertyType[]{m.NonSerialisedInventoryItem.SalesOrderItemsWhereReservedFromNonSerialisedInventoryItem}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(m.NonSerialisedInventoryItem.QuantityExpectedIn) {Steps = new IPropertyType[]{m.NonSerialisedInventoryItem.SalesOrderItemsWhereReservedFromNonSerialisedInventoryItem}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(m.NonSerialisedInventoryItem.AvailableToPromise) {Steps = new IPropertyType[]{m.NonSerialisedInventoryItem.SalesOrderItemsWhereReservedFromNonSerialisedInventoryItem}, OfType = m.SalesOrderItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsInProcess && !v.SalesOrderItemShipmentState.IsShipped))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                if (@this.ExistReservedFromNonSerialisedInventoryItem)
                {
                    if ((salesOrder.OrderKind?.ScheduleManually == true && @this.QuantityPendingShipment > 0)
                        || !salesOrder.ExistOrderKind || !salesOrder.OrderKind.ScheduleManually)
                    {
                        var committedOutSameProductOtherItem = salesOrder.SalesOrderItems
                            .Where(v => !Equals(v, @this) && Equals(v.Product, @this.Product))
                            .Sum(v => v.QuantityRequestsShipping);

                        var qoh = @this.ReservedFromNonSerialisedInventoryItem.QuantityOnHand;

                        var atp = @this.ReservedFromNonSerialisedInventoryItem.AvailableToPromise - committedOutSameProductOtherItem > 0 ?
                            @this.ReservedFromNonSerialisedInventoryItem.AvailableToPromise - committedOutSameProductOtherItem :
                            0;

                        var quantityCommittedOut = 0M;

                        foreach (SalesOrderItemInventoryAssignment salesOrderItemInventoryAssignment1 in @this.SalesOrderItemInventoryAssignments)
                        {
                            foreach(InventoryItemTransaction inventoryItemTransaction in salesOrderItemInventoryAssignment1.InventoryItem.InventoryItemTransactionsWhereInventoryItem)
                            {
                                var reason = inventoryItemTransaction.Reason;

                                if (reason.IncreasesQuantityCommittedOut == true)
                                {
                                    quantityCommittedOut += inventoryItemTransaction.Quantity;
                                }
                                else if (reason.IncreasesQuantityCommittedOut == false)
                                {
                                    quantityCommittedOut -= inventoryItemTransaction.Quantity;
                                }
                            }
                        }

                        if (quantityCommittedOut < 0)
                        {
                            quantityCommittedOut = 0;
                        }

                        @this.QuantityCommittedOut = quantityCommittedOut;

                        var wantToShip = @this.QuantityCommittedOut - @this.QuantityPendingShipment;

                        var inventoryAssignment = @this.SalesOrderItemInventoryAssignments.FirstOrDefault(v => v.InventoryItem.Equals(@this.ReservedFromNonSerialisedInventoryItem));
                        if (@this.QuantityCommittedOut > qoh)
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

                        var neededFromInventory = @this.QuantityOrdered - @this.QuantityShipped - @this.QuantityCommittedOut;
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
                                if (@this.QuantityCommittedOut > qoh)
                                {
                                    inventoryAssignment.Quantity = qoh;
                                }
                                else
                                {
                                    inventoryAssignment.Quantity = @this.QuantityCommittedOut + availableFromInventory;
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
