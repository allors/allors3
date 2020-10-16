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
    using Resources;

    public class ShipmentItemDerivation : DomainDerivation
    {
        public ShipmentItemDerivation(M m) : base(m, new Guid("472FF004-E087-4237-8BE4-D9B9194D3BB3")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.ShipmentItem.Class),
                new ChangedRolePattern(this.M.OrderShipment.Quantity),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var shipmentItem in matches.Cast<ShipmentItem>())
            {
                if (!shipmentItem.ExistDerivationTrigger)
                {
                    shipmentItem.DerivationTrigger = Guid.NewGuid();
                }

                if ((shipmentItem.ShipmentWhereShipmentItem.GetType().Name.Equals(typeof(CustomerShipment).Name) || shipmentItem.ShipmentWhereShipmentItem.GetType().Name.Equals(typeof(PurchaseReturn).Name))
                    && shipmentItem.ExistSerialisedItem
                    && !shipmentItem.ExistNextSerialisedItemAvailability)
                {
                    validation.AssertExists(shipmentItem, shipmentItem.Meta.NextSerialisedItemAvailability);
                }

                if (shipmentItem.ExistSerialisedItem && shipmentItem.Quantity != 1)
                {
                    validation.AddError($"{shipmentItem} {shipmentItem.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                }

                // AppsOnDeriveCustomerShipmentItem
                if (shipmentItem.ShipmentWhereShipmentItem is CustomerShipment)
                {
                    shipmentItem.QuantityPicked = 0;
                    foreach (ItemIssuance itemIssuance in shipmentItem.ItemIssuancesWhereShipmentItem)
                    {
                        if (itemIssuance.PickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(shipmentItem.Strategy.Session).Picked))
                        {
                            shipmentItem.QuantityPicked += itemIssuance.Quantity;
                        }
                    }

                    if (Equals(shipmentItem.ShipmentWhereShipmentItem.ShipmentState, new ShipmentStates(shipmentItem.Strategy.Session).Shipped))
                    {
                        shipmentItem.QuantityShipped = 0;
                        foreach (ItemIssuance itemIssuance in shipmentItem.ItemIssuancesWhereShipmentItem)
                        {
                            shipmentItem.QuantityShipped += itemIssuance.Quantity;
                        }
                    }
                }

                // AppsOnDeriveCustomerShipmentItem
                if (shipmentItem.ExistShipmentWhereShipmentItem
                    && shipmentItem.ShipmentWhereShipmentItem is PurchaseShipment
                    && shipmentItem.ExistPart
                    && shipmentItem.Part.InventoryItemKind.IsNonSerialised
                    && !shipmentItem.ExistUnitPurchasePrice)
                {
                    validation.AssertExists(shipmentItem, shipmentItem.Meta.UnitPurchasePrice);
                }

                if (shipmentItem.ExistShipmentWhereShipmentItem
                    && shipmentItem.ShipmentWhereShipmentItem is PurchaseShipment
                    && !shipmentItem.ExistStoredInFacility
                    && shipmentItem.ShipmentWhereShipmentItem.ExistShipToFacility)
                {
                    shipmentItem.StoredInFacility = shipmentItem.ShipmentWhereShipmentItem.ShipToFacility;
                }

                if (shipmentItem.ExistShipmentWhereShipmentItem
                    && shipmentItem.ShipmentWhereShipmentItem is PurchaseShipment
                    && shipmentItem.ExistShipmentReceiptWhereShipmentItem)
                {
                    shipmentItem.Quantity = 0;
                    var shipmentReceipt = shipmentItem.ShipmentReceiptWhereShipmentItem;
                    shipmentItem.Quantity += shipmentReceipt.QuantityAccepted + shipmentReceipt.QuantityRejected;
                }
            }
        }

        //TODO fixs
        //void Sync(Shipment shipment) => this.SyncedShipment = shipment;
    }
}
