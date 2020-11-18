// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class ShipmentReceiptDerivation : DomainDerivation
    {
        public ShipmentReceiptDerivation(M m) : base(m, new Guid("BE525828-2AAC-4996-98A0-08293485D7DD")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.ShipmentReceipt.ShipmentItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ShipmentReceipt>())
            {
                @this.ReceivedDateTime = @this.ReceivedDateTime.Date;

                if (@this.ExistOrderItem && @this.ExistShipmentItem)
                {
                    var orderShipmentsWhereShipmentItem = @this.ShipmentItem.OrderShipmentsWhereShipmentItem;
                    orderShipmentsWhereShipmentItem.Filter.AddEquals(this.M.OrderShipment.OrderItem, @this.OrderItem);

                    if (orderShipmentsWhereShipmentItem.First == null)
                    {
                        new OrderShipmentBuilder(@this.Strategy.Session)
                            .WithOrderItem(@this.OrderItem)
                            .WithShipmentItem(@this.ShipmentItem)
                            .WithQuantity(@this.QuantityAccepted)
                            .Build();
                    }
                    else
                    {
                        orderShipmentsWhereShipmentItem.First.Quantity += @this.QuantityAccepted;
                    }
                }

                // AppsOnDeriveInventoryItem
                if (@this.ExistShipmentItem && @this.ExistFacility)
                {
                    if (@this.ShipmentItem.ExistSerialisedItem)
                    {
                        var inventoryItems = @this.ShipmentItem.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, @this.Facility);
                        inventoryItems.Filter.AddEquals(this.M.SerialisedInventoryItem.SerialisedInventoryItemState, new SerialisedInventoryItemStates(@this.Session()).Good);
                        @this.InventoryItem = inventoryItems.First;

                        if (!@this.ExistInventoryItem)
                        {
                            @this.InventoryItem = new SerialisedInventoryItemBuilder(@this.Strategy.Session)
                                .WithPart(@this.ShipmentItem.Part)
                                .WithSerialisedItem(@this.ShipmentItem.SerialisedItem)
                                .WithFacility(@this.Facility)
                                .WithUnitOfMeasure(@this.ShipmentItem.Part.UnitOfMeasure)
                                .Build();
                        }
                    }
                    else
                    {
                        var inventoryItems = @this.ShipmentItem.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, @this.Facility);
                        //inventoryItems.Filter.AddEquals(M.NonSerialisedInventoryItem.NonSerialisedInventoryItemState, new NonSerialisedInventoryItemStates(this.Session()).Good);
                        @this.InventoryItem = inventoryItems.First;

                        if (!@this.ExistInventoryItem)
                        {
                            @this.InventoryItem = new NonSerialisedInventoryItemBuilder(@this.Strategy.Session)
                                .WithPart(@this.ShipmentItem.Part)
                                .WithFacility(@this.Facility)
                                .WithUnitOfMeasure(@this.ShipmentItem.Part.UnitOfMeasure)
                                .Build();
                        }
                    }
                }
            }
        }
    }
}
