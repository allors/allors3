// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class ShipmentReceiptCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdShipmentReceipt = changeSet.Created.Select(session.Instantiate).OfType<ShipmentReceipt>();

                foreach (var shipmentReceipt in createdShipmentReceipt)
                {
                    shipmentReceipt.ReceivedDateTime = shipmentReceipt.ReceivedDateTime.Date;

                    if (shipmentReceipt.ExistOrderItem && shipmentReceipt.ExistShipmentItem)
                    {
                        var orderShipmentsWhereShipmentItem = shipmentReceipt.ShipmentItem.OrderShipmentsWhereShipmentItem;
                        orderShipmentsWhereShipmentItem.Filter.AddEquals(M.OrderShipment.OrderItem, shipmentReceipt.OrderItem);

                        if (orderShipmentsWhereShipmentItem.First == null)
                        {
                            new OrderShipmentBuilder(shipmentReceipt.Strategy.Session)
                                .WithOrderItem(shipmentReceipt.OrderItem)
                                .WithShipmentItem(shipmentReceipt.ShipmentItem)
                                .WithQuantity(shipmentReceipt.QuantityAccepted)
                                .Build();
                        }
                        else
                        {
                            orderShipmentsWhereShipmentItem.First.Quantity += shipmentReceipt.QuantityAccepted;
                        }
                    }

                    this.BaseOnDeriveInventoryItem(shipmentReceipt);
                }
            }

            public void BaseOnDeriveInventoryItem(ShipmentReceipt shipmentReceipt)
            {
                if (shipmentReceipt.ExistShipmentItem && shipmentReceipt.ExistFacility)
                {
                    if (shipmentReceipt.ShipmentItem.ExistSerialisedItem)
                    {
                        var inventoryItems = shipmentReceipt.ShipmentItem.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem;
                        inventoryItems.Filter.AddEquals(M.InventoryItem.Facility, shipmentReceipt.Facility);
                        inventoryItems.Filter.AddEquals(M.SerialisedInventoryItem.SerialisedInventoryItemState, new SerialisedInventoryItemStates(shipmentReceipt.Session()).Good);
                        shipmentReceipt.InventoryItem = inventoryItems.First;

                        if (!shipmentReceipt.ExistInventoryItem)
                        {
                            shipmentReceipt.InventoryItem = new SerialisedInventoryItemBuilder(shipmentReceipt.Strategy.Session)
                                                .WithPart(shipmentReceipt.ShipmentItem.Part)
                                                .WithSerialisedItem(shipmentReceipt.ShipmentItem.SerialisedItem)
                                                .WithFacility(shipmentReceipt.Facility)
                                                .WithUnitOfMeasure(shipmentReceipt.ShipmentItem.Part.UnitOfMeasure)
                                                .Build();
                        }
                    }
                    else
                    {
                        var inventoryItems = shipmentReceipt.ShipmentItem.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(M.InventoryItem.Facility, shipmentReceipt.Facility);
                        //inventoryItems.Filter.AddEquals(M.NonSerialisedInventoryItem.NonSerialisedInventoryItemState, new NonSerialisedInventoryItemStates(this.Session()).Good);
                        shipmentReceipt.InventoryItem = inventoryItems.First;

                        if (!shipmentReceipt.ExistInventoryItem)
                        {
                            shipmentReceipt.InventoryItem = new NonSerialisedInventoryItemBuilder(shipmentReceipt.Strategy.Session)
                                                .WithPart(shipmentReceipt.ShipmentItem.Part)
                                                .WithFacility(shipmentReceipt.Facility)
                                                .WithUnitOfMeasure(shipmentReceipt.ShipmentItem.Part.UnitOfMeasure)
                                                .Build();
                        }
                    }
                }
            }
        }

        public static void shipmentReceiptRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("c61dccfa-3787-4307-a1d4-2723392971b2")] = new ShipmentReceiptCreationDerivation();
        }
    }
}
