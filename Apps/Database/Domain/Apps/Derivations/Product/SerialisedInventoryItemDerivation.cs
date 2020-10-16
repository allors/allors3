// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class SerialisedInventoryItemDerivation : DomainDerivation
    {
        public SerialisedInventoryItemDerivation(M m) : base(m, new Guid("29B3C9B5-7BB2-4851-A424-F984E7AE348B")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.SerialisedInventoryItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var serialisedInventoryItem in matches.Cast<SerialisedInventoryItem>())
            {
                if (!serialisedInventoryItem.ExistName)
                {
                    serialisedInventoryItem.Name = $"{serialisedInventoryItem.Part?.Name} at {serialisedInventoryItem.Facility?.Name} with state {serialisedInventoryItem.SerialisedInventoryItemState?.Name}";
                }

                // AppsOnDeriveQuantity
                serialisedInventoryItem.Quantity = 0;

                foreach (InventoryItemTransaction inventoryTransaction in serialisedInventoryItem.InventoryItemTransactionsWhereInventoryItem)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason.IncreasesQuantityOnHand == true)
                    {
                        serialisedInventoryItem.Quantity += (int)inventoryTransaction.Quantity;
                    }
                    else if (reason.IncreasesQuantityOnHand == false)
                    {
                        serialisedInventoryItem.Quantity -= (int)inventoryTransaction.Quantity;
                    }
                }

                foreach (PickListItem pickListItem in serialisedInventoryItem.PickListItemsWhereInventoryItem)
                {
                    if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(serialisedInventoryItem.Strategy.Session).Picked))
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            if (!itemIssuance.ShipmentItem.ShipmentItemState.Shipped)
                            {
                                serialisedInventoryItem.Quantity -= (int)pickListItem.QuantityPicked;
                            }
                        }
                    }
                }

                foreach (ShipmentReceipt shipmentReceipt in serialisedInventoryItem.ShipmentReceiptsWhereInventoryItem)
                {
                    // serialised items are handled via InventoryItemTransactions
                    if (shipmentReceipt.ExistShipmentItem && !shipmentReceipt.ShipmentItem.Part.InventoryItemKind.IsSerialised)
                    {
                        var purchaseShipment = (PurchaseShipment)shipmentReceipt.ShipmentItem.ShipmentWhereShipmentItem;
                        if (purchaseShipment.ShipmentState.Equals(new ShipmentStates(serialisedInventoryItem.Strategy.Session).Received))
                        {
                            serialisedInventoryItem.Quantity += (int)shipmentReceipt.QuantityAccepted;
                        }
                    }
                }

                if (serialisedInventoryItem.Quantity < 0 || serialisedInventoryItem.Quantity > 1)
                {
                    var message = "Invalid transaction";
                    cycle.Validation.AddError($"{serialisedInventoryItem} {serialisedInventoryItem.Meta.Quantity} {message}");
                }

                // TODO: Remove OnDerive
                //serialisedInventoryItem.Part.OnDerive(x => x.WithDerivation(derivation));
            }
        }
    }
}
