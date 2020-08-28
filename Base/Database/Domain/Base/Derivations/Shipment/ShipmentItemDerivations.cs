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
    using Resources;

    public static partial class DabaseExtensions
    {
        public class ShipmentItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdShipmentItem = changeSet.Created.Select(session.Instantiate).OfType<ShipmentItem>();

                foreach(var shipmentItem in createdShipmentItem)
                {
                    if (shipmentItem.ExistSerialisedItem && !shipmentItem.ExistNextSerialisedItemAvailability)
                    {
                        validation.AssertExists(shipmentItem, shipmentItem.Meta.NextSerialisedItemAvailability);
                    }

                    if (shipmentItem.ExistSerialisedItem && shipmentItem.Quantity != 1)
                    {
                        validation.AddError($"{shipmentItem} {shipmentItem.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                    }

                    BaseOnDeriveCustomerShipmentItem(shipmentItem);

                    BaseOnDerivePurchaseShipmentItem(shipmentItem);
                }

                void BaseOnDerivePurchaseShipmentItem(ShipmentItem shipmentItem)
                {
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

                void BaseOnDeriveCustomerShipmentItem(ShipmentItem shipmentItem)
                {
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
                }
            }


            //TODO fixs
            //void Sync(Shipment shipment) => this.SyncedShipment = shipment;
        }

        public static void ShipmentItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("4b0541b0-8530-4540-91ae-e123c7b21320")] = new ShipmentItemCreationDerivation();
        }
    }
}
