// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class SalesOrderItemDerivation : DomainDerivation
    {
        public SalesOrderItemDerivation(M m) : base(m, new Guid("FEF4E104-A0F0-4D83-A248-A1A606D93E41")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemState),
                new ChangedPattern(m.SalesOrderItem.QuantityOrdered),
                new ChangedPattern(m.SalesOrderItem.ReservedFromNonSerialisedInventoryItem),
                new ChangedPattern(m.SalesOrderItem.ReservedFromSerialisedInventoryItem),
                new ChangedPattern(m.SalesOrder.SalesOrderState) {Steps = new IPropertyType[]{ m.SalesOrder.SalesOrderItems} },
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
                var shipped = new ShipmentStates(@this.Session()).Shipped;

                if (@this.ExistProduct && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Session()).ProductItem;
                }

                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistSerialisedItem && !@this.ExistNextSerialisedItemAvailability)
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsSerialised && @this.QuantityOrdered != 1)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsNonSerialised && @this.QuantityOrdered == 0)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistInvoiceItemType && @this.InvoiceItemType.MaxQuantity.HasValue && @this.QuantityOrdered > @this.InvoiceItemType.MaxQuantity.Value)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(session);
                var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(session);
                var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(session);
                var salesOrderItemStates = new SalesOrderItemStates(session);

                // Shipments
                @this.QuantityPendingShipment = @this.OrderShipmentsWhereOrderItem
                    .Where(v => v.ExistShipmentItem && !((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.Equals(shipped))
                    .Sum(v => v.Quantity);

                @this.QuantityShipped = @this.OrderShipmentsWhereOrderItem
                    .Where(v => v.ExistShipmentItem && ((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.Equals(shipped))
                    .Sum(v => v.Quantity);

                if (@this.SalesOrderItemState.IsInProcess
                    && @this.ExistPreviousReservedFromNonSerialisedInventoryItem
                    && !Equals(@this.ReservedFromNonSerialisedInventoryItem, @this.PreviousReservedFromNonSerialisedInventoryItem))
                {
                    validation.AddError($"{@this} {@this.Meta.ReservedFromNonSerialisedInventoryItem} {ErrorMessages.ReservedFromNonSerialisedInventoryItem}");
                }

                if (@this.ExistSerialisedItem && @this.QuantityOrdered != 1)
                {
                    validation.AddError($"{@this} {@this.Meta.QuantityOrdered} {ErrorMessages.SerializedItemQuantity}");
                }

                if (@this.QuantityOrdered < @this.QuantityPendingShipment || @this.QuantityOrdered < @this.QuantityShipped)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining}");
                }

                if (@this.SalesOrderItemInventoryAssignments.FirstOrDefault() != null)
                {
                    @this.QuantityCommittedOut = @this.SalesOrderItemInventoryAssignments
                        .SelectMany(v => v.InventoryItemTransactions)
                        .Where(t => t.Reason.Equals(new InventoryTransactionReasons(@this.Session()).Reservation))
                        .Sum(v => v.Quantity);
                }
                else
                {
                    @this.QuantityCommittedOut = 0;
                }

                // SalesOrderItem States
                if (@this.IsValid && salesOrder.ExistSalesOrderState)
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
                        else if (orderBilling.All(v => !v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                        }
                        else
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.PartiallyPaid;
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
                            else if (shipmentBilling.All(v => !v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                            {
                                @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                            }
                            else
                            {
                                @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.PartiallyPaid;
                            }
                        }
                        else
                        {
                            @this.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                        }
                    }

                    // InvoiceState
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
                }

                CalculatePrice(@this, salesOrder);

                if (!@this.IsValid && @this.WasValid)
                {
                    if (@this.ExistReservedFromNonSerialisedInventoryItem && @this.ExistQuantityCommittedOut)
                    {
                        var inventoryAssignment = @this.SalesOrderItemInventoryAssignments.FirstOrDefault();
                        if (inventoryAssignment != null)
                        {
                            inventoryAssignment.Quantity = 0 - @this.QuantityCommittedOut;
                        }
                    }
                }

                // TODO: Move to Custom
                if (cycle.ChangeSet.IsCreated(@this) && !@this.ExistDescription)
                {
                    if (@this.ExistSerialisedItem)
                    {
                        var builder = new StringBuilder();
                        var part = @this.SerialisedItem.PartWhereSerialisedItem;

                        if (part != null && part.ExistManufacturedBy)
                        {
                            builder.Append($", Manufacturer: {part.ManufacturedBy.PartyName}");
                        }

                        if (part != null && part.ExistBrand)
                        {
                            builder.Append($", Brand: {part.Brand.Name}");
                        }

                        if (part != null && part.ExistModel)
                        {
                            builder.Append($", Model: {part.Model.Name}");
                        }

                        builder.Append($", SN: {@this.SerialisedItem.SerialNumber}");

                        if (@this.SerialisedItem.ExistManufacturingYear)
                        {
                            builder.Append($", YOM: {@this.SerialisedItem.ManufacturingYear}");
                        }

                        foreach (SerialisedItemCharacteristic characteristic in @this.SerialisedItem.SerialisedItemCharacteristics)
                        {
                            if (characteristic.ExistValue)
                            {
                                var characteristicType = characteristic.SerialisedItemCharacteristicType;
                                if (characteristicType.ExistUnitOfMeasure)
                                {
                                    var uom = characteristicType.UnitOfMeasure.ExistAbbreviation
                                                    ? characteristicType.UnitOfMeasure.Abbreviation
                                                    : characteristicType.UnitOfMeasure.Name;
                                    builder.Append(
                                        $", {characteristicType.Name}: {characteristic.Value} {uom}");
                                }
                                else
                                {
                                    builder.Append($", {characteristicType.Name}: {characteristic.Value}");
                                }
                            }
                        }

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Description = details;

                    }
                    else if (@this.ExistProduct && @this.Product is UnifiedGood unifiedGood)
                    {
                        var builder = new StringBuilder();

                        if (unifiedGood != null && unifiedGood.ExistManufacturedBy)
                        {
                            builder.Append($", Manufacturer: {unifiedGood.ManufacturedBy.PartyName}");
                        }

                        if (unifiedGood != null && unifiedGood.ExistBrand)
                        {
                            builder.Append($", Brand: {unifiedGood.Brand.Name}");
                        }

                        if (unifiedGood != null && unifiedGood.ExistModel)
                        {
                            builder.Append($", Model: {unifiedGood.Model.Name}");
                        }

                        foreach (SerialisedItemCharacteristic characteristic in unifiedGood.SerialisedItemCharacteristics)
                        {
                            if (characteristic.ExistValue)
                            {
                                var characteristicType = characteristic.SerialisedItemCharacteristicType;
                                if (characteristicType.ExistUnitOfMeasure)
                                {
                                    var uom = characteristicType.UnitOfMeasure.ExistAbbreviation
                                                    ? characteristicType.UnitOfMeasure.Abbreviation
                                                    : characteristicType.UnitOfMeasure.Name;
                                    builder.Append($", {characteristicType.Name}: {characteristic.Value} {uom}");
                                }
                                else
                                {
                                    builder.Append($", {characteristicType.Name}: {characteristic.Value}");
                                }
                            }
                        }

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Description = details;
                    }
                }

                Sync(@this);

                @this.PreviousReservedFromNonSerialisedInventoryItem = @this.ReservedFromNonSerialisedInventoryItem;
                @this.PreviousQuantity = @this.QuantityOrdered;
                @this.PreviousProduct = @this.Product;

                if (@this.ExistReservedFromNonSerialisedInventoryItem)
                {
                    if (!@this.ReservedFromNonSerialisedInventoryItem.Equals(@this.PreviousReservedFromNonSerialisedInventoryItem))
                    {
                        //derivation.Mark(salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem);
                    }
                }

                if (!@this.SalesOrderItemInvoiceState.NotInvoiced || !@this.SalesOrderItemShipmentState.NotShipped)
                {
                    var deniablePermissionByOperandTypeId = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in @this.Session().Extent<Permission>())
                    {
                        if (permission.ClassPointer == @this.Strategy.Class.Id
                            && (permission.Operation == Operations.Write || permission.Operation == Operations.Execute))
                        {
                            deniablePermissionByOperandTypeId.Add(permission.OperandType, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                    {
                        @this.AddDeniedPermission(keyValuePair.Value);
                    }
                }

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }

            void Sync(SalesOrderItem salesOrderItem)
            {
                if (salesOrderItem.IsValid)
                {
                    var salesOrder = salesOrderItem.SalesOrderWhereSalesOrderItem;

                    if (salesOrderItem.Part != null && salesOrder?.TakenBy != null)
                    {
                        if (salesOrderItem.Part.InventoryItemKind.IsSerialised)
                        {
                            if (!salesOrderItem.ExistReservedFromSerialisedInventoryItem)
                            {
                                if (salesOrderItem.ExistSerialisedItem)
                                {
                                    if (salesOrderItem.SerialisedItem.ExistSerialisedInventoryItemsWhereSerialisedItem)
                                    {
                                        salesOrderItem.ReservedFromSerialisedInventoryItem = salesOrderItem.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => v.Quantity == 1);
                                    }
                                }
                                else
                                {
                                    var inventoryItems = salesOrderItem.Part.InventoryItemsWherePart;
                                    inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, salesOrder.OriginFacility);
                                    salesOrderItem.ReservedFromSerialisedInventoryItem = inventoryItems.FirstOrDefault() as SerialisedInventoryItem;
                                }
                            }
                        }
                        else
                        {
                            if (!salesOrderItem.ExistReservedFromNonSerialisedInventoryItem)
                            {
                                var inventoryItems = salesOrderItem.Part.InventoryItemsWherePart;
                                inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, salesOrder.OriginFacility);
                                salesOrderItem.ReservedFromNonSerialisedInventoryItem = inventoryItems.FirstOrDefault() as NonSerialisedInventoryItem;
                            }
                        }
                    }

                    if (salesOrderItem.SalesOrderItemState.IsInProcess && !salesOrderItem.SalesOrderItemShipmentState.Shipped)
                    {
                        if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem)
                        {
                            if ((salesOrder.OrderKind?.ScheduleManually == true && salesOrderItem.QuantityPendingShipment > 0)
                                || !salesOrder.ExistOrderKind || salesOrder.OrderKind.ScheduleManually == false)
                            {
                                var committedOutSameProductOtherItem = salesOrder.SalesOrderItems
                                    .Where(v => !Equals(v, salesOrderItem) && Equals(v.Product, salesOrderItem.Product))
                                    .Sum(v => v.QuantityRequestsShipping);

                                var qoh = salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand;

                                var atp = salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise - committedOutSameProductOtherItem > 0 ?
                                    salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise - committedOutSameProductOtherItem :
                                    0;

                                var quantityCommittedOut = salesOrderItem.SalesOrderItemInventoryAssignments
                                    .SelectMany(v => v.InventoryItemTransactions)
                                    .Where(t => t.Reason.Equals(new InventoryTransactionReasons(salesOrderItem.Session()).Reservation))
                                    .Sum(v => v.Quantity);

                                var wantToShip = quantityCommittedOut - salesOrderItem.QuantityPendingShipment;

                                var inventoryAssignment = salesOrderItem.SalesOrderItemInventoryAssignments.FirstOrDefault(v => v.InventoryItem.Equals(salesOrderItem.ReservedFromNonSerialisedInventoryItem));
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

                                var neededFromInventory = salesOrderItem.QuantityOrdered - salesOrderItem.QuantityShipped - quantityCommittedOut;
                                var availableFromInventory = neededFromInventory < atp ? neededFromInventory : atp;

                                if (neededFromInventory != 0 || salesOrderItem.QuantityShortFalled > 0)
                                {
                                    if (inventoryAssignment == null)
                                    {
                                        var salesOrderItemInventoryAssignment = new SalesOrderItemInventoryAssignmentBuilder(salesOrderItem.Session())
                                            .WithInventoryItem(salesOrderItem.ReservedFromNonSerialisedInventoryItem)
                                            .WithQuantity(wantToShip + availableFromInventory)
                                            .Build();

                                        salesOrderItem.AddSalesOrderItemInventoryAssignment(salesOrderItemInventoryAssignment);
                                    }
                                    else
                                    {
                                        inventoryAssignment.InventoryItem = salesOrderItem.ReservedFromNonSerialisedInventoryItem;
                                        if (quantityCommittedOut > qoh)
                                        {
                                            inventoryAssignment.Quantity = qoh;
                                        }
                                        else
                                        {
                                            inventoryAssignment.Quantity = quantityCommittedOut + availableFromInventory;
                                        }
                                    }

                                    salesOrderItem.QuantityRequestsShipping = wantToShip + availableFromInventory;

                                    if (salesOrderItem.QuantityRequestsShipping > qoh)
                                    {
                                        salesOrderItem.QuantityRequestsShipping = qoh;
                                    }

                                    if (salesOrder.OrderKind?.ScheduleManually == true)
                                    {
                                        salesOrderItem.QuantityRequestsShipping = 0;
                                    }

                                    salesOrderItem.QuantityReserved = salesOrderItem.QuantityOrdered - salesOrderItem.QuantityShipped;
                                    salesOrderItem.QuantityShortFalled = neededFromInventory - availableFromInventory > 0 ? neededFromInventory - availableFromInventory : 0;
                                }
                            }
                        }

                        if (salesOrderItem.ExistReservedFromSerialisedInventoryItem)
                        {
                            var inventoryAssignment = salesOrderItem.SalesOrderItemInventoryAssignments.FirstOrDefault(v => v.InventoryItem.Equals(salesOrderItem.ReservedFromSerialisedInventoryItem));
                            if (inventoryAssignment == null)
                            {
                                var salesOrderItemInventoryAssignment = new SalesOrderItemInventoryAssignmentBuilder(salesOrderItem.Session())
                                     .WithInventoryItem(salesOrderItem.ReservedFromSerialisedInventoryItem)
                                     .WithQuantity(1)
                                     .Build();

                                salesOrderItem.AddSalesOrderItemInventoryAssignment(salesOrderItemInventoryAssignment);

                                salesOrderItem.QuantityRequestsShipping = 1;
                            }

                            salesOrderItem.QuantityReserved = 1;
                        }
                    }
                }
            }

            void CalculatePrice(SalesOrderItem salesOrderItem, SalesOrder salesOrder, bool useValueOrdered = false)
            {
                var sameProductItems = salesOrder.SalesOrderItems
                    .Where(v => v.IsValid && v.ExistProduct && v.Product.Equals(salesOrderItem.Product))
                    .ToArray();

                var quantityOrdered = sameProductItems.Sum(w => w.QuantityOrdered);
                var valueOrdered = useValueOrdered ? sameProductItems.Sum(w => w.TotalBasePrice) : 0;

                var orderPriceComponents = salesOrder.TakenBy?.PriceComponentsWherePricedBy
                    .Where(v => v.FromDate <= salesOrder.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= salesOrder.OrderDate))
                    .ToArray();

                var orderItemPriceComponents = Array.Empty<PriceComponent>();
                if (salesOrderItem.ExistProduct)
                {
                    orderItemPriceComponents = salesOrderItem.Product.GetPriceComponents(orderPriceComponents);
                }
                else if (salesOrderItem.ExistProductFeature)
                {
                    orderItemPriceComponents = salesOrderItem.ProductFeature.GetPriceComponents(salesOrderItem.SalesOrderItemWhereOrderedWithFeature.Product, orderPriceComponents);
                }

                var priceComponents = orderItemPriceComponents.Where(
                    v => PriceComponents.AppsIsApplicable(
                        new PriceComponents.IsApplicable
                        {
                            PriceComponent = v,
                            Customer = salesOrder.BillToCustomer,
                            Product = salesOrderItem.Product,
                            SalesOrder = salesOrder,
                            QuantityOrdered = quantityOrdered,
                            ValueOrdered = valueOrdered,
                        })).ToArray();

                var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

                // Calculate Unit Price (with Discounts and Surcharges)
                if (salesOrderItem.AssignedUnitPrice.HasValue)
                {
                    salesOrderItem.UnitBasePrice = unitBasePrice ?? salesOrderItem.AssignedUnitPrice.Value;
                    salesOrderItem.UnitDiscount = 0;
                    salesOrderItem.UnitSurcharge = 0;
                    salesOrderItem.UnitPrice = salesOrderItem.AssignedUnitPrice.Value;
                }
                else
                {
                    if (!unitBasePrice.HasValue)
                    {
                        validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
                        return;
                    }

                    salesOrderItem.UnitBasePrice = unitBasePrice.Value;

                    salesOrderItem.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                        v => v.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                            : v.Price ?? 0);

                    salesOrderItem.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                        v => v.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                            : v.Price ?? 0);

                    salesOrderItem.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;

                    foreach (OrderAdjustment orderAdjustment in salesOrderItem.DiscountAdjustments)
                    {
                        salesOrderItem.UnitDiscount += orderAdjustment.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                            : orderAdjustment.Amount ?? 0;
                    }

                    foreach (OrderAdjustment orderAdjustment in salesOrderItem.SurchargeAdjustments)
                    {
                        salesOrderItem.UnitSurcharge += orderAdjustment.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                            : orderAdjustment.Amount ?? 0;
                    }

                    salesOrderItem.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;
                }

                foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                {
                    salesOrderItem.UnitBasePrice += featureItem.UnitBasePrice;
                    salesOrderItem.UnitPrice += featureItem.UnitPrice;
                    salesOrderItem.UnitDiscount += featureItem.UnitDiscount;
                    salesOrderItem.UnitSurcharge += featureItem.UnitSurcharge;
                }

                salesOrderItem.UnitVat = salesOrderItem.ExistVatRate ? salesOrderItem.UnitPrice * salesOrderItem.VatRate.Rate / 100 : 0;
                salesOrderItem.UnitIrpf = salesOrderItem.ExistIrpfRate ? salesOrderItem.UnitPrice * salesOrderItem.IrpfRate.Rate / 100 : 0;

                // Calculate Totals
                salesOrderItem.TotalBasePrice = salesOrderItem.UnitBasePrice * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalDiscount = salesOrderItem.UnitDiscount * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalSurcharge = salesOrderItem.UnitSurcharge * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalOrderAdjustment = salesOrderItem.TotalSurcharge - salesOrderItem.TotalDiscount;

                if (salesOrderItem.TotalBasePrice > 0)
                {
                    salesOrderItem.TotalDiscountAsPercentage = Math.Round(salesOrderItem.TotalDiscount / salesOrderItem.TotalBasePrice * 100, 2);
                    salesOrderItem.TotalSurchargeAsPercentage = Math.Round(salesOrderItem.TotalSurcharge / salesOrderItem.TotalBasePrice * 100, 2);
                }
                else
                {
                    salesOrderItem.TotalDiscountAsPercentage = 0;
                    salesOrderItem.TotalSurchargeAsPercentage = 0;
                }

                salesOrderItem.TotalExVat = salesOrderItem.UnitPrice * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalVat = salesOrderItem.UnitVat * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalIncVat = salesOrderItem.TotalExVat + salesOrderItem.TotalVat;
                salesOrderItem.TotalIrpf = salesOrderItem.UnitIrpf * salesOrderItem.QuantityOrdered;
                salesOrderItem.GrandTotal = salesOrderItem.TotalIncVat - salesOrderItem.TotalIrpf;
            }
        }
    }
}
