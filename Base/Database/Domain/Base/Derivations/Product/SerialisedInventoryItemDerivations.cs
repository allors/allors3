// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class SerialisedInventoryItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdSerialisedInventoryItems = changeSet.Created.Select(v=>v.GetObject()).OfType<SerialisedInventoryItem>();

                foreach(var serialisedInventoryItem in createdSerialisedInventoryItems)
                {
                    if (!serialisedInventoryItem.ExistName)
                    {
                        serialisedInventoryItem.Name = $"{serialisedInventoryItem.Part?.Name} at {serialisedInventoryItem.Facility?.Name} with state {serialisedInventoryItem.SerialisedInventoryItemState?.Name}";
                    }

                    BaseOnDeriveQuantity(serialisedInventoryItem);

                    if (serialisedInventoryItem.Quantity < 0 || serialisedInventoryItem.Quantity > 1)
                    {
                        var message = "Invalid transaction";
                        validation.AddError($"{serialisedInventoryItem} {serialisedInventoryItem.Meta.Quantity} {message}");
                    }

                    // TODO: Remove OnDerive
                    //serialisedInventoryItem.Part.OnDerive(x => x.WithDerivation(derivation));
                }

                void BaseOnDeriveQuantity(SerialisedInventoryItem serialisedInventoryItem)
                {
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
                }

            }
        }

        public static void SerialisedInventoryItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("8565efc7-9fc7-4f1a-99f2-6cb157bd8ed7")] = new SerialisedInventoryItemCreationDerivation();
        }
    }
}
