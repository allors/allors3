// <copyright file="InventoryItemTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class InventoryItemTransaction
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistTransactionDate)
            {
                this.TransactionDate = this.Session().Now();
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            if (!this.ExistPart)
            {
                this.Part = this.SerialisedItem?.PartWhereSerialisedItem;
            }

            if (!this.ExistFacility)
            {
                this.Facility = this.Part?.DefaultFacility;
            }

            if (!this.ExistUnitOfMeasure)
            {
                this.UnitOfMeasure = this.Part?.UnitOfMeasure;
            }

            if (this.ExistPart && this.Part.InventoryItemKind.IsSerialised)
            {
                if (!this.ExistSerialisedInventoryItemState)
                {
                    this.SerialisedInventoryItemState = this.Reason.DefaultSerialisedInventoryItemState;
                }
            }
            else
            {
                if (!this.ExistNonSerialisedInventoryItemState)
                {
                    this.NonSerialisedInventoryItemState = this.Reason.DefaultNonSerialisedInventoryItemState;
                }
            }

            if (!this.ExistInventoryItem)
            {
                this.SyncInventoryItem();
            }

            // Match on required properties
            var matched = false;
            var matchingItems = this.Part.InventoryItemsWherePart.ToArray();
            var possibleMatches = matchingItems.Length > 0;

            if (possibleMatches)
            {
                matchingItems = matchingItems.Where(i => i.Facility.Equals(this.Facility)).ToArray();
                possibleMatches = (matchingItems != null) && (matchingItems.Length > 0);
            }

            if (possibleMatches)
            {
                matchingItems = matchingItems.Where(m => m.UnitOfMeasure.Equals(this.UnitOfMeasure)).ToArray();
                possibleMatches = matchingItems.Length > 0;
            }

            // Match on optional properties
            if (possibleMatches && this.ExistLot)
            {
                matchingItems = matchingItems.Where(m => m.Lot.Equals(this.Lot)).ToArray();
                possibleMatches = matchingItems.Length > 0;
            }

            if (possibleMatches && this.ExistSerialisedInventoryItemState)
            {
                if (matchingItems is SerialisedInventoryItem[] matchingSerialisedItems)
                {
                    matchingItems = matchingSerialisedItems.Where(m => m.InventoryItemState.Equals(this.SerialisedInventoryItemState)).ToArray();
                    possibleMatches = matchingItems.Length > 0;
                }
            }

            if (possibleMatches && this.ExistNonSerialisedInventoryItemState)
            {
                if (matchingItems is NonSerialisedInventoryItem[] matchingNonSerialisedItems)
                {
                    matchingItems = matchingNonSerialisedItems.Where(m => m.InventoryItemState.Equals(this.NonSerialisedInventoryItemState)).ToArray();
                    possibleMatches = matchingItems.Length > 0;
                }
            }

            if (possibleMatches)
            {
                // Match on Non/SerialisedInventoryItemState
                foreach (var item in matchingItems)
                {
                    if (item is NonSerialisedInventoryItem nonSerialItem)
                    {
                        if (nonSerialItem.NonSerialisedInventoryItemState.Equals(this.NonSerialisedInventoryItemState))
                        {
                            this.InventoryItem = item;
                            matched = true;
                            break;
                        }
                    }
                    else if (item is SerialisedInventoryItem serialItem)
                    {
                        if (serialItem.SerialisedItem.Equals(this.SerialisedItem))
                        {
                            this.InventoryItem = item;
                            matched = true;
                            break;
                        }
                    }
                }
            }

            if (matched)
            {
                if (this.InventoryItem is SerialisedInventoryItem serialItem)
                {
                    serialItem.SerialisedInventoryItemState = this.SerialisedInventoryItemState;
                }

                if (this.InventoryItem is NonSerialisedInventoryItem nonSerialItem)
                {
                    nonSerialItem.NonSerialisedInventoryItemState = this.NonSerialisedInventoryItemState;
                }
            }
        }

        private void SyncInventoryItem()
        {
            var derivation = new Derivations.Default.DefaultDerivation(this.Strategy.Session);
            var facility = this.Facility ?? this.Part.DefaultFacility;
            var unitOfMeasure = this.UnitOfMeasure ?? this.Part.UnitOfMeasure;

            if (this.Part.InventoryItemKind.IsSerialised && (this.Quantity < -1 || this.Quantity > 1))
            {
                var message = "Invalid transaction";
                derivation.Validation.AddError(this, this.Meta.SerialisedItem, message);
            }

            if (this.Part.InventoryItemKind.IsSerialised)
            {
                var inventoryItems = this.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem;
                inventoryItems.Filter.AddEquals(this.M.InventoryItem.Part, this.Part);
                inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, facility);
                var inventoryItem = inventoryItems.First;

                if (inventoryItem == null)
                {
                    var builder = new SerialisedInventoryItemBuilder(this.Strategy.Session)
                        .WithFacility(facility)
                        .WithUnitOfMeasure(unitOfMeasure)
                        .WithSerialisedItem(this.SerialisedItem)
                        .WithPart(this.Part)
                        .WithSerialisedInventoryItemState(this.SerialisedInventoryItemState);

                    if (this.ExistLot)
                    {
                        builder.WithLot(this.Lot);
                    }

                    this.InventoryItem = builder.Build();
                }
            }
            else if (this.Part.InventoryItemKind.IsNonSerialised)
            {
                var inventoryItems = this.Part.InventoryItemsWherePart;
                inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, facility);
                var inventoryItem = inventoryItems.First;

                if (inventoryItem == null)
                {
                    var builder = new NonSerialisedInventoryItemBuilder(this.Strategy.Session)
                        .WithFacility(facility)
                        .WithUnitOfMeasure(unitOfMeasure)
                        .WithPart(this.Part)
                        .WithNonSerialisedInventoryItemState(this.NonSerialisedInventoryItemState);

                    if (this.ExistLot)
                    {
                        builder.WithLot(this.Lot);
                    }

                    this.InventoryItem = builder.Build();
                }
            }
        }
    }
}
