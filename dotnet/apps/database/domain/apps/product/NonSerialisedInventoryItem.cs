
// <copyright file="NonSerialisedInventoryItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class NonSerialisedInventoryItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.NonSerialisedInventoryItem, this.M.NonSerialisedInventoryItem.NonSerialisedInventoryItemState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistNonSerialisedInventoryItemState)
            {
                this.NonSerialisedInventoryItemState = new NonSerialisedInventoryItemStates(this.Strategy.Transaction).Good;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            foreach (InventoryItemVersion version in this.AllVersions)
            {
                version.Delete();
            }
        }

        public decimal CalculateQuantityOnHand(Settings settings)
        {
            var quantityOnHand = 0M;

            if (!settings.InventoryStrategy.OnHandNonSerialisedStates.Contains(this.NonSerialisedInventoryItemState))
            {
                this.QuantityOnHand = 0;
            }
            else
            {
                foreach (InventoryItemTransaction inventoryTransaction in this.InventoryItemTransactionsWhereInventoryItem)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason?.IncreasesQuantityOnHand == true)
                    {
                        quantityOnHand += inventoryTransaction.Quantity;
                    }
                    else if (reason?.IncreasesQuantityOnHand == false)
                    {
                        quantityOnHand -= inventoryTransaction.Quantity;
                    }
                }

                foreach (PickListItem pickListItem in this.PickListItemsWhereInventoryItem)
                {
                    if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(this.Strategy.Transaction).Picked))
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            if (!itemIssuance.ShipmentItem.ShipmentItemState.IsShipped)
                            {
                                quantityOnHand -= pickListItem.QuantityPicked;
                            }
                        }
                    }
                }
            }

            return quantityOnHand;
        }

        public decimal CalculateQuantityCommittedOut()
        {
            var quantityCommittedOut = 0M;

            foreach (InventoryItemTransaction inventoryItemTransaction in this.InventoryItemTransactionsWhereInventoryItem)
            {
                var reason = inventoryItemTransaction.Reason;

                if (reason?.IncreasesQuantityCommittedOut == true)
                {
                    quantityCommittedOut += inventoryItemTransaction.Quantity;
                }
                else if (reason?.IncreasesQuantityCommittedOut == false)
                {
                    quantityCommittedOut -= inventoryItemTransaction.Quantity;
                }
            }

            foreach (PickListItem pickListItem in this.PickListItemsWhereInventoryItem)
            {
                if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(this.Strategy.Transaction).Picked))
                {
                    foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                    {
                        if (!itemIssuance.ShipmentItem.ShipmentItemState.IsShipped)
                        {
                            quantityCommittedOut -= pickListItem.QuantityPicked;
                        }
                    }
                }
            }

            if (quantityCommittedOut < 0)
            {
                quantityCommittedOut = 0;
            }

            return quantityCommittedOut;
        }

        public decimal CalculateAvailableToPromise(Settings settings)
        {
            var quantityOnHand = this.CalculateQuantityOnHand(settings);
            var quantityCommittedOut = this.CalculateQuantityCommittedOut();

            var availableToPromise = quantityOnHand - quantityCommittedOut;

            if (availableToPromise < 0)
            {
                availableToPromise = 0;
            }

            return availableToPromise;
        }
    }
}
