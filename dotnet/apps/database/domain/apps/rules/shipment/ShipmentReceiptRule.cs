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
    using Derivations.Rules;

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
                    var shipment = @this.ShipmentItem.OrderShipmentsWhereShipmentItem.FirstOrDefault(v => Equals(@this.OrderItem, v.OrderItem));

                    if (shipment == null)
                    {
                        new OrderShipmentBuilder(@this.Strategy.Transaction)
                            .WithOrderItem(@this.OrderItem)
                            .WithShipmentItem(@this.ShipmentItem)
                            .WithQuantity(@this.QuantityAccepted)
                            .Build();
                    }
                    else
                    {
                        shipment.Quantity += @this.QuantityAccepted;
                    }
                }

                if (@this.ExistShipmentItem && @this.ExistFacility)
                {
                    if (@this.ShipmentItem.ExistSerialisedItem)
                    {
                        var good = new SerialisedInventoryItemStates(@this.Transaction()).Good;

                        @this.InventoryItem = @this.ShipmentItem.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => Equals(@this.Facility, v.Facility) && Equals(good, v.SerialisedInventoryItemState));

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
                        @this.InventoryItem = @this.ShipmentItem.Part.InventoryItemsWherePart.FirstOrDefault(v => Equals(@this.Facility, v.Facility));

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
