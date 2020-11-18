// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class SerialisedInventoryItemDerivation : DomainDerivation
    {
        public SerialisedInventoryItemDerivation(M m) : base(m, new Guid("29B3C9B5-7BB2-4851-A424-F984E7AE348B")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SerialisedInventoryItem.Facility),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                if (!@this.ExistName)
                {
                    @this.Name = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.SerialisedInventoryItemState?.Name}";
                }

                // AppsOnDeriveQuantity
                @this.Quantity = 0;

                foreach (InventoryItemTransaction inventoryTransaction in @this.InventoryItemTransactionsWhereInventoryItem)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason.IncreasesQuantityOnHand == true)
                    {
                        @this.Quantity += (int)inventoryTransaction.Quantity;
                    }
                    else if (reason.IncreasesQuantityOnHand == false)
                    {
                        @this.Quantity -= (int)inventoryTransaction.Quantity;
                    }
                }

                foreach (PickListItem pickListItem in @this.PickListItemsWhereInventoryItem)
                {
                    if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(@this.Strategy.Session).Picked))
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            if (!itemIssuance.ShipmentItem.ShipmentItemState.Shipped)
                            {
                                @this.Quantity -= (int)pickListItem.QuantityPicked;
                            }
                        }
                    }
                }

                foreach (ShipmentReceipt shipmentReceipt in @this.ShipmentReceiptsWhereInventoryItem)
                {
                    // serialised items are handled via InventoryItemTransactions
                    if (shipmentReceipt.ExistShipmentItem && !shipmentReceipt.ShipmentItem.Part.InventoryItemKind.IsSerialised)
                    {
                        var purchaseShipment = (PurchaseShipment)shipmentReceipt.ShipmentItem.ShipmentWhereShipmentItem;
                        if (purchaseShipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Received))
                        {
                            @this.Quantity += (int)shipmentReceipt.QuantityAccepted;
                        }
                    }
                }

                if (@this.Quantity < 0 || @this.Quantity > 1)
                {
                    var message = "Invalid transaction";
                    cycle.Validation.AddError($"{@this} {@this.Meta.Quantity} {message}");
                }

                // TODO: Remove OnDerive
                //serialisedInventoryItem.Part.OnDerive(x => x.WithDerivation(derivation));
            }
        }
    }
}
