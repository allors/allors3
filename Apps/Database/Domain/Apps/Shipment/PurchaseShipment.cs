// <copyright file="PurchaseShipment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Linq;

    public partial class PurchaseShipment
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PurchaseShipment, this.M.PurchaseShipment.ShipmentState),
        };

        public void AppsReceive(PurchaseShipmentReceive method)
        {
            this.ShipmentState = new ShipmentStates(this.Strategy.Session).Received;
            this.EstimatedArrivalDate = this.Session().Now().Date;

            foreach (ShipmentItem shipmentItem in this.ShipmentItems)
            {
                shipmentItem.ShipmentItemState = new ShipmentItemStates(this.Session()).Received;

                if (!shipmentItem.ExistShipmentReceiptWhereShipmentItem)
                {
                    if (!shipmentItem.ExistOrderShipmentsWhereShipmentItem)
                    {
                        new ShipmentReceiptBuilder(this.Session())
                            .WithQuantityAccepted(shipmentItem.Quantity)
                            .WithShipmentItem(shipmentItem)
                            .WithFacility(shipmentItem.StoredInFacility)
                            .Build();
                    }
                    else
                    {
                        foreach (OrderShipment orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                        {
                            new ShipmentReceiptBuilder(this.Session())
                                .WithQuantityAccepted(orderShipment.Quantity)
                                .WithOrderItem(orderShipment.OrderItem)
                                .WithShipmentItem(shipmentItem)
                                .WithFacility(shipmentItem.StoredInFacility)
                                .Build();
                        }
                    }
                }

                if (shipmentItem.Part.InventoryItemKind.IsSerialised)
                {
                    new InventoryItemTransactionBuilder(this.Session())
                        .WithPart(shipmentItem.Part)
                        .WithSerialisedItem(shipmentItem.SerialisedItem)
                        .WithUnitOfMeasure(shipmentItem.Part.UnitOfMeasure)
                        .WithFacility(shipmentItem.StoredInFacility)
                        .WithReason(new InventoryTransactionReasons(this.Strategy.Session).IncomingShipment)
                        .WithShipmentItem(shipmentItem)
                        .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(this.Session()).Good)
                        .WithQuantity(1)
                        .Build();

                    shipmentItem.SerialisedItem.SerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session()).Available;
                    shipmentItem.SerialisedItem.AvailableForSale = true;

                    if ((this.ShipToParty as InternalOrganisation)?.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(this.Session()).PurchaseshipmentReceive) == true)
                    {
                        shipmentItem.SerialisedItem.OwnedBy = this.ShipToParty;
                        shipmentItem.SerialisedItem.Ownership = new Ownerships(this.Session()).Own;
                    }
                }
                else
                {
                    new InventoryItemTransactionBuilder(this.Session())
                        .WithPart(shipmentItem.Part)
                        .WithUnitOfMeasure(shipmentItem.Part.UnitOfMeasure)
                        .WithFacility(shipmentItem.StoredInFacility)
                        .WithReason(new InventoryTransactionReasons(this.Strategy.Session).IncomingShipment)
                        .WithNonSerialisedInventoryItemState(new NonSerialisedInventoryItemStates(this.Session()).Good)
                        .WithShipmentItem(shipmentItem)
                        .WithQuantity(shipmentItem.Quantity)
                        .WithCost(shipmentItem.UnitPurchasePrice)
                        .Build();
                }
            }
        }
    }
}
