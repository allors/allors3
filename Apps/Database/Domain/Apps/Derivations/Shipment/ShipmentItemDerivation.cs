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
                new ChangedPattern(this.M.OrderShipment.Quantity) { Steps =  new IPropertyType[] {m.OrderShipment.ShipmentItem} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }

                if ((@this.ShipmentWhereShipmentItem.GetType().Name.Equals(typeof(CustomerShipment).Name) || @this.ShipmentWhereShipmentItem.GetType().Name.Equals(typeof(PurchaseReturn).Name))
                    && @this.ExistSerialisedItem
                    && !@this.ExistNextSerialisedItemAvailability)
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError($"{@this} {@this.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                }

                // AppsOnDeriveCustomerShipmentItem
                if (@this.ShipmentWhereShipmentItem is CustomerShipment)
                {
                    @this.QuantityPicked = 0;
                    foreach (ItemIssuance itemIssuance in @this.ItemIssuancesWhereShipmentItem)
                    {
                        if (itemIssuance.PickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(@this.Strategy.Session).Picked))
                        {
                            @this.QuantityPicked += itemIssuance.Quantity;
                        }
                    }

                    if (Equals(@this.ShipmentWhereShipmentItem.ShipmentState, new ShipmentStates(@this.Strategy.Session).Shipped))
                    {
                        @this.QuantityShipped = 0;
                        foreach (ItemIssuance itemIssuance in @this.ItemIssuancesWhereShipmentItem)
                        {
                            @this.QuantityShipped += itemIssuance.Quantity;
                        }
                    }
                }

                // AppsOnDeriveCustomerShipmentItem
                if (@this.ExistShipmentWhereShipmentItem
                    && @this.ShipmentWhereShipmentItem is PurchaseShipment
                    && @this.ExistPart
                    && @this.Part.InventoryItemKind.IsNonSerialised
                    && !@this.ExistUnitPurchasePrice)
                {
                    validation.AssertExists(@this, @this.Meta.UnitPurchasePrice);
                }

                if (@this.ExistShipmentWhereShipmentItem
                    && @this.ShipmentWhereShipmentItem is PurchaseShipment
                    && !@this.ExistStoredInFacility
                    && @this.ShipmentWhereShipmentItem.ExistShipToFacility)
                {
                    @this.StoredInFacility = @this.ShipmentWhereShipmentItem.ShipToFacility;
                }

                if (@this.ExistShipmentWhereShipmentItem
                    && @this.ShipmentWhereShipmentItem is PurchaseShipment
                    && @this.ExistShipmentReceiptWhereShipmentItem)
                {
                    @this.Quantity = 0;
                    var shipmentReceipt = @this.ShipmentReceiptWhereShipmentItem;
                    @this.Quantity += shipmentReceipt.QuantityAccepted + shipmentReceipt.QuantityRejected;
                }
            }
        }

        //TODO fixs
        //void Sync(Shipment shipment) => this.SyncedShipment = shipment;
    }
}
