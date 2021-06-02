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
    using Derivations.Rules;
    using Resources;

    public class ShipmentItemRule : Rule
    {
        public ShipmentItemRule(MetaPopulation m) : base(m, new Guid("472FF004-E087-4237-8BE4-D9B9194D3BB3")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentItem.RolePattern(v => v.NextSerialisedItemAvailability),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem),
                m.ShipmentItem.RolePattern(v => v.Quantity),
                m.ShipmentItem.RolePattern(v => v.UnitPurchasePrice),
                m.ShipmentItem.RolePattern(v => v.Part),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility),
                m.ItemIssuance.RolePattern(v => v.Quantity, v => v.ShipmentItem),
                m.PickList.RolePattern(v => v.PickListState, v => v.PickListItems.PickListItem.ItemIssuancesWherePickListItem.ItemIssuance.ShipmentItem),
                m.Shipment.RolePattern(v => v.ShipmentState, v => v.ShipmentItems),
                m.Shipment.RolePattern(v => v.ShipToFacility, v => v.ShipmentItems),
                m.ShipmentReceipt.RolePattern(v => v.QuantityAccepted, v => v.ShipmentItem),
                m.ShipmentReceipt.RolePattern(v => v.QuantityRejected, v => v.ShipmentItem),
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
                    validation.AddError(@this, @this.Meta.Quantity, ErrorMessages.SerializedItemQuantity);
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
