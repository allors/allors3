// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class ShipmentReceiptDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("BE525828-2AAC-4996-98A0-08293485D7DD");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.ShipmentReceipt.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var shipmentReceipt in matches.Cast<ShipmentReceipt>())
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

                // BaseOnDeriveInventoryItem
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
    }
}
