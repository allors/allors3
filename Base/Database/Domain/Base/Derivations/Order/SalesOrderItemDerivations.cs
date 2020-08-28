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

    public static partial class DabaseExtensions
    {
        public class SalesOrderItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var empty = Array.Empty<SalesOrderItem>();

                var createdSalesOrderItem = changeSet.Created.Select(session.Instantiate).OfType<SalesOrderItem>();

                changeSet.AssociationsByRoleType.TryGetValue(M.SalesOrderItem.SalesOrderItemState, out var changedSalesOrderState);
                var salesOrdersWhereStateChanged = changedSalesOrderState?.Select(session.Instantiate).OfType<SalesOrderItem>();



                var allPurchaseOrders = createdSalesOrderItem
                    .Union(salesOrdersWhereStateChanged ?? empty);

                foreach (var salesOrderItem in allPurchaseOrders.Where(v => v != null))
                {
                    var salesOrder = salesOrderItem.SalesOrderWhereSalesOrderItem;
                    var shipped = new ShipmentStates(salesOrderItem.Session()).Shipped;

                    if (salesOrderItem.ExistSerialisedItem && !salesOrderItem.ExistNextSerialisedItemAvailability)
                    {
                        validation.AssertExists(salesOrderItem, salesOrderItem.Meta.NextSerialisedItemAvailability);
                    }

                    if (salesOrderItem.Part != null && salesOrderItem.Part.InventoryItemKind.IsSerialised && salesOrderItem.QuantityOrdered != 1)
                    {
                        validation.AddError($"{salesOrderItem} {M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                    }

                    if (salesOrderItem.Part != null && salesOrderItem.Part.InventoryItemKind.IsNonSerialised && salesOrderItem.QuantityOrdered == 0)
                    {
                        validation.AddError($"{salesOrderItem} {M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                    }

                    if (salesOrderItem.ExistInvoiceItemType && salesOrderItem.InvoiceItemType.MaxQuantity.HasValue && salesOrderItem.QuantityOrdered > salesOrderItem.InvoiceItemType.MaxQuantity.Value)
                    {
                        validation.AddError($"{salesOrderItem} {M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                    }

                    var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(session);
                    var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(session);
                    var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(session);
                    var salesOrderItemStates = new SalesOrderItemStates(session);

                    // Shipments
                    salesOrderItem.QuantityPendingShipment = salesOrderItem.OrderShipmentsWhereOrderItem
                        .Where(v => v.ExistShipmentItem && !((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.Equals(shipped))
                        .Sum(v => v.Quantity);

                    salesOrderItem.QuantityShipped = salesOrderItem.OrderShipmentsWhereOrderItem
                        .Where(v => v.ExistShipmentItem && ((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.Equals(shipped))
                        .Sum(v => v.Quantity);

                    if (salesOrderItem.SalesOrderItemState.IsInProcess
                        && salesOrderItem.ExistPreviousReservedFromNonSerialisedInventoryItem
                        && salesOrderItem.ReservedFromNonSerialisedInventoryItem != salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem)
                    {
                        validation.AddError($"{salesOrderItem} {salesOrderItem.Meta.ReservedFromNonSerialisedInventoryItem} {ErrorMessages.ReservedFromNonSerialisedInventoryItem}");
                    }

                    if (salesOrderItem.ExistSerialisedItem && salesOrderItem.QuantityOrdered != 1)
                    {
                        validation.AddError($"{salesOrderItem} {salesOrderItem.Meta.QuantityOrdered} {ErrorMessages.SerializedItemQuantity}");
                    }

                    if (salesOrderItem.QuantityOrdered < salesOrderItem.QuantityPendingShipment || salesOrderItem.QuantityOrdered < salesOrderItem.QuantityShipped)
                    {
                        validation.AddError($"{salesOrderItem} {M.SalesOrderItem.QuantityOrdered} {ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining}");
                    }

                    if (salesOrderItem.SalesOrderItemInventoryAssignments.FirstOrDefault() != null)
                    {
                        salesOrderItem.QuantityCommittedOut = salesOrderItem.SalesOrderItemInventoryAssignments
                            .SelectMany(v => v.InventoryItemTransactions)
                            .Where(t => t.Reason.Equals(new InventoryTransactionReasons(salesOrderItem.Session()).Reservation))
                            .Sum(v => v.Quantity);
                    }
                    else
                    {
                        salesOrderItem.QuantityCommittedOut = 0;
                    }

                    // SalesOrderItem States
                    if (salesOrderItem.IsValid)
                    {
                        if (salesOrder.SalesOrderState.IsProvisional)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.Provisional;
                        }

                        if (salesOrder.SalesOrderState.IsReadyForPosting &&
                            (salesOrderItem.SalesOrderItemState.IsProvisional || salesOrderItem.SalesOrderItemState.IsRequestsApproval || salesOrderItem.SalesOrderItemState.IsOnHold))
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.ReadyForPosting;
                        }

                        if (salesOrder.SalesOrderState.IsRequestsApproval &&
                            (salesOrderItem.SalesOrderItemState.IsProvisional || salesOrderItem.SalesOrderItemState.IsOnHold))
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.RequestsApproval;
                        }

                        if (salesOrder.SalesOrderState.IsAwaitingAcceptance
                            && salesOrderItem.SalesOrderItemState.IsReadyForPosting)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.AwaitingAcceptance;
                        }

                        if (salesOrder.SalesOrderState.IsInProcess
                            && salesOrderItem.SalesOrderItemState.IsAwaitingAcceptance || salesOrderItem.SalesOrderItemState.IsOnHold)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.InProcess;
                        }

                        if (salesOrder.SalesOrderState.IsOnHold && salesOrderItem.SalesOrderItemState.IsInProcess)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.OnHold;
                        }

                        if (salesOrder.SalesOrderState.IsFinished)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.Finished;
                        }

                        if (salesOrder.SalesOrderState.IsCancelled)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.Cancelled;
                        }

                        if (salesOrder.SalesOrderState.IsRejected)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.Rejected;
                        }
                    }

                    if (salesOrderItem.IsValid)
                    {
                        // ShipmentState
                        if (!salesOrderItem.ExistOrderShipmentsWhereOrderItem)
                        {
                            salesOrderItem.SalesOrderItemShipmentState = salesOrderItemShipmentStates.NotShipped;
                        }
                        else if (salesOrderItem.QuantityShipped == 0 && salesOrderItem.QuantityPendingShipment > 0)
                        {
                            salesOrderItem.SalesOrderItemShipmentState = salesOrderItemShipmentStates.InProgress;
                        }
                        else
                        {
                            salesOrderItem.SalesOrderItemShipmentState = salesOrderItem.QuantityShipped < salesOrderItem.QuantityOrdered ?
                                                                             salesOrderItemShipmentStates.PartiallyShipped :
                                                                             salesOrderItemShipmentStates.Shipped;
                        }

                        // PaymentState
                        var orderBilling = salesOrderItem.OrderItemBillingsWhereOrderItem.Select(v => v.InvoiceItem).OfType<SalesInvoiceItem>().ToArray();

                        if (orderBilling.Any())
                        {
                            if (orderBilling.All(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                            {
                                salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.Paid;
                            }
                            else if (orderBilling.All(v => !v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                            {
                                salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                            }
                            else
                            {
                                salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.PartiallyPaid;
                            }
                        }
                        else
                        {
                            var shipmentBilling = salesOrderItem.OrderShipmentsWhereOrderItem.SelectMany(v => v.ShipmentItem.ShipmentItemBillingsWhereShipmentItem).Select(v => v.InvoiceItem).OfType<SalesInvoiceItem>().ToArray();
                            if (shipmentBilling.Any())
                            {
                                if (shipmentBilling.All(v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                                {
                                    salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.Paid;
                                }
                                else if (shipmentBilling.All(v => !v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceState.IsPaid))
                                {
                                    salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                                }
                                else
                                {
                                    salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.PartiallyPaid;
                                }
                            }
                            else
                            {
                                salesOrderItem.SalesOrderItemPaymentState = salesOrderItemPaymentStates.NotPaid;
                            }
                        }

                        // InvoiceState
                        var amountAlreadyInvoiced = salesOrderItem.OrderItemBillingsWhereOrderItem?.Sum(v => v.Amount);
                        if (amountAlreadyInvoiced == 0)
                        {
                            amountAlreadyInvoiced = salesOrderItem.OrderShipmentsWhereOrderItem
                                .SelectMany(orderShipment => orderShipment.ShipmentItem.ShipmentItemBillingsWhereShipmentItem)
                                .Aggregate(amountAlreadyInvoiced, (current, shipmentItemBilling) => current + shipmentItemBilling.Amount);
                        }

                        var leftToInvoice = salesOrderItem.TotalExVat - amountAlreadyInvoiced;

                        if (amountAlreadyInvoiced == 0)
                        {
                            salesOrderItem.SalesOrderItemInvoiceState = salesOrderItemInvoiceStates.NotInvoiced;
                        }
                        else if (amountAlreadyInvoiced > 0 && leftToInvoice > 0)
                        {
                            salesOrderItem.SalesOrderItemInvoiceState = salesOrderItemInvoiceStates.PartiallyInvoiced;
                        }
                        else
                        {
                            salesOrderItem.SalesOrderItemInvoiceState = salesOrderItemInvoiceStates.Invoiced;
                        }

                        // SalesOrderItem States
                        if (salesOrderItem.SalesOrderItemShipmentState.Shipped && salesOrderItem.SalesOrderItemInvoiceState.Invoiced)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.Completed;
                        }

                        if (salesOrderItem.SalesOrderItemState.IsCompleted && salesOrderItem.SalesOrderItemPaymentState.Paid)
                        {
                            salesOrderItem.SalesOrderItemState = salesOrderItemStates.Finished;
                        }
                    }

                    CalculatePrice(salesOrderItem, salesOrder);

                    if (!salesOrderItem.IsValid && salesOrderItem.WasValid)
                    {
                        if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem && salesOrderItem.ExistQuantityCommittedOut)
                        {
                            var inventoryAssignment = salesOrderItem.SalesOrderItemInventoryAssignments.FirstOrDefault();
                            if (inventoryAssignment != null)
                            {
                                inventoryAssignment.Quantity = 0 - salesOrderItem.QuantityCommittedOut;
                            }
                        }
                    }

                    // TODO: Move to Custom
                    if (changeSet.IsCreated(salesOrderItem) && !salesOrderItem.ExistDescription)
                    {
                        if (salesOrderItem.ExistSerialisedItem)
                        {
                            var builder = new StringBuilder();
                            var part = salesOrderItem.SerialisedItem.PartWhereSerialisedItem;

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

                            builder.Append($", SN: {salesOrderItem.SerialisedItem.SerialNumber}");

                            if (salesOrderItem.SerialisedItem.ExistManufacturingYear)
                            {
                                builder.Append($", YOM: {salesOrderItem.SerialisedItem.ManufacturingYear}");
                            }

                            foreach (SerialisedItemCharacteristic characteristic in salesOrderItem.SerialisedItem.SerialisedItemCharacteristics)
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

                            salesOrderItem.Description = details;

                        }
                        else if (salesOrderItem.ExistProduct && salesOrderItem.Product is UnifiedGood unifiedGood)
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

                            salesOrderItem.Description = details;
                        }
                    }

                    Sync(salesOrderItem);

                    salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem = salesOrderItem.ReservedFromNonSerialisedInventoryItem;
                    salesOrderItem.PreviousQuantity = salesOrderItem.QuantityOrdered;
                    salesOrderItem.PreviousProduct = salesOrderItem.Product;

                    if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem)
                    {
                        if (!salesOrderItem.ReservedFromNonSerialisedInventoryItem.Equals(salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem))
                        {
                            //derivation.Mark(salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem);
                        }
                    }

                    if (!salesOrderItem.SalesOrderItemInvoiceState.NotInvoiced || !salesOrderItem.SalesOrderItemShipmentState.NotShipped)
                    {
                        var deniablePermissionByOperandTypeId = new Dictionary<Guid, Permission>();

                        foreach (Permission permission in salesOrderItem.Session().Extent<Permission>())
                        {
                            if (permission.ConcreteClassPointer == salesOrderItem.Strategy.Class.Id
                                && (permission.Operation == Operations.Write || permission.Operation == Operations.Execute))
                            {
                                deniablePermissionByOperandTypeId.Add(permission.OperandTypePointer, permission);
                            }
                        }

                        foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                        {
                            salesOrderItem.AddDeniedPermission(keyValuePair.Value);
                        }
                    }

                    var deletePermission = new Permissions(salesOrderItem.Strategy.Session).Get(salesOrderItem.Meta.ObjectType, salesOrderItem.Meta.Delete, Operations.Execute);
                    if (salesOrderItem.IsDeletable)
                    {
                        salesOrderItem.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        salesOrderItem.AddDeniedPermission(deletePermission);
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
                                        inventoryItems.Filter.AddEquals(M.InventoryItem.Facility, salesOrder.OriginFacility);
                                        salesOrderItem.ReservedFromSerialisedInventoryItem = inventoryItems.FirstOrDefault() as SerialisedInventoryItem;
                                    }
                                }
                            }
                            else
                            {
                                if (!salesOrderItem.ExistReservedFromNonSerialisedInventoryItem)
                                {
                                    var inventoryItems = salesOrderItem.Part.InventoryItemsWherePart;
                                    inventoryItems.Filter.AddEquals(M.InventoryItem.Facility, salesOrder.OriginFacility);
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
                                        .Where(v => !Equals(v, this) && Equals(v.Product, salesOrderItem.Product))
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

                    var orderPriceComponents = new PriceComponents(salesOrderItem.Session()).CurrentPriceComponents(salesOrder.OrderDate);
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
                        v => PriceComponents.BaseIsApplicable(
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
                            validation.AddError($"{salesOrderItem} {M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
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

        public static void SalesOrderItemsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("11c7947f-7fd6-4932-937d-352a8701020d")] = new SalesOrderItemCreationDerivation();
        }
    }
}
