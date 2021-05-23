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

    public class ShipmentReceiptRule : Rule
    {
        public ShipmentReceiptRule(MetaPopulation m) : base(m, new Guid("BE525828-2AAC-4996-98A0-08293485D7DD")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentReceipt.RolePattern(v => v.ShipmentItem),
                m.ShipmentReceipt.RolePattern(v => v.OrderItem),
                m.ShipmentReceipt.RolePattern(v => v.QuantityAccepted),
                m.ShipmentReceipt.RolePattern(v => v.Facility),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentReceiptWhereShipmentItem),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentReceiptWhereShipmentItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ShipmentReceipt>())
            {
                if (@this.ExistOrderItem && @this.ExistShipmentItem)
                {
                    var orderShipmentsWhereShipmentItem = @this.ShipmentItem.OrderShipmentsWhereShipmentItem;
                    orderShipmentsWhereShipmentItem.Filter.AddEquals(this.M.OrderShipment.OrderItem, @this.OrderItem);

                    if (orderShipmentsWhereShipmentItem.First == null)
                    {
                        new OrderShipmentBuilder(@this.Strategy.Transaction)
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

                if (@this.ExistShipmentItem && @this.ExistFacility)
                {
                    if (@this.ShipmentItem.ExistSerialisedItem)
                    {
                        var inventoryItems = @this.ShipmentItem.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, @this.Facility);
                        inventoryItems.Filter.AddEquals(this.M.SerialisedInventoryItem.SerialisedInventoryItemState, new SerialisedInventoryItemStates(@this.Transaction()).Good);
                        @this.InventoryItem = inventoryItems.First;

                        if (!@this.ExistInventoryItem)
                        {
                            @this.InventoryItem = new SerialisedInventoryItemBuilder(@this.Strategy.Transaction)
                                .WithPart(@this.ShipmentItem.Part)
                                .WithSerialisedItem(@this.ShipmentItem.SerialisedItem)
                                .WithFacility(@this.Facility)
                                .WithUnitOfMeasure(@this.ShipmentItem.Part.UnitOfMeasure)
                                .Build();
                        }
                    }

                    if (@this.ShipmentItem.ExistPart && @this.ShipmentItem.Part.InventoryItemKind.IsNonSerialised)
                    {
                        var inventoryItems = @this.ShipmentItem.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, @this.Facility);
                        //inventoryItems.Filter.AddEquals(M.NonSerialisedInventoryItem.NonSerialisedInventoryItemState, new NonSerialisedInventoryItemStates(this.Transaction()).Good);
                        @this.InventoryItem = inventoryItems.First;

                        if (!@this.ExistInventoryItem)
                        {
                            @this.InventoryItem = new NonSerialisedInventoryItemBuilder(@this.Strategy.Transaction)
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
