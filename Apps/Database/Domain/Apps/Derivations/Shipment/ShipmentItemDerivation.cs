// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class ShipmentItemDerivation : DomainDerivation
    {
        public ShipmentItemDerivation(M m) : base(m, new Guid("472FF004-E087-4237-8BE4-D9B9194D3BB3")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.ShipmentItem, m.ShipmentItem.NextSerialisedItemAvailability),
                new RolePattern(m.ShipmentItem, m.ShipmentItem.SerialisedItem),
                new RolePattern(m.ShipmentItem, m.ShipmentItem.Quantity),
                new RolePattern(m.ShipmentItem, m.ShipmentItem.UnitPurchasePrice),
                new RolePattern(m.ShipmentItem, m.ShipmentItem.Part),
                new RolePattern(m.ShipmentItem, m.ShipmentItem.StoredInFacility),
                new RolePattern(m.ItemIssuance, m.ItemIssuance.Quantity) { Steps = new IPropertyType[] {m.ItemIssuance.ShipmentItem } },
                new RolePattern(m.PickList, m.PickList.PickListState) { Steps = new IPropertyType[] {m.PickList.PickListItems, m.PickListItem.ItemIssuancesWherePickListItem, m.ItemIssuance.ShipmentItem } },
                new RolePattern(m.Shipment, m.Shipment.ShipmentState) { Steps = new IPropertyType[] {m.Shipment.ShipmentItems } },
                new RolePattern(m.Shipment, m.Shipment.ShipToFacility) { Steps = new IPropertyType[] {m.Shipment.ShipmentItems } },
                new RolePattern(m.ShipmentReceipt, m.ShipmentReceipt.QuantityAccepted) { Steps = new IPropertyType[] {m.ShipmentReceipt.ShipmentItem } },
                new RolePattern(m.ShipmentReceipt, m.ShipmentReceipt.QuantityRejected) { Steps = new IPropertyType[] {m.ShipmentReceipt.ShipmentItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                if (@this.ExistShipmentWhereShipmentItem
                    && (@this.ShipmentWhereShipmentItem.GetType().Name.Equals(typeof(CustomerShipment).Name) || @this.ShipmentWhereShipmentItem.GetType().Name.Equals(typeof(PurchaseReturn).Name))
                    && @this.ExistSerialisedItem
                    && !@this.ExistNextSerialisedItemAvailability)
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError($"{@this}, {@this.Meta.Quantity}, {ErrorMessages.SerializedItemQuantity}");
                }

                if (@this.ShipmentWhereShipmentItem is CustomerShipment)
                {
                    @this.QuantityPicked = 0;
                    foreach (ItemIssuance itemIssuance in @this.ItemIssuancesWhereShipmentItem)
                    {
                        if (itemIssuance.PickListItem.PickListWherePickListItem.PickListState.IsPicked)
                        {
                            @this.QuantityPicked += itemIssuance.Quantity;
                        }
                    }

                    if (@this.ShipmentWhereShipmentItem.ShipmentState.IsShipped)
                    {
                        @this.QuantityShipped = 0;
                        foreach (ItemIssuance itemIssuance in @this.ItemIssuancesWhereShipmentItem)
                        {
                            @this.QuantityShipped += itemIssuance.Quantity;
                        }
                    }
                }

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
                    var quantity = 0M;
                    var shipmentReceipt = @this.ShipmentReceiptWhereShipmentItem;
                    quantity += shipmentReceipt.QuantityAccepted + shipmentReceipt.QuantityRejected;

                    if (quantity != @this.Quantity)
                    {
                        @this.Quantity = quantity;
                    }
                }
            }
        }
    }
}
