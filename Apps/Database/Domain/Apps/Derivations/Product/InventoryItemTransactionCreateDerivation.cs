// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class InventoryItemTransactionCreateDerivation : DomainDerivation
    {
        public InventoryItemTransactionCreateDerivation(M m) : base(m, new Guid("e6a947d1-c908-4a2c-9dc8-05b2c53d515e")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.InventoryItemTransaction.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                if (!@this.ExistTransactionDate)
                {
                    @this.TransactionDate = @this.Session().Now();
                }

                if (!@this.ExistPart)
                {
                    @this.Part = @this.SerialisedItem?.PartWhereSerialisedItem;
                }

                if (!@this.ExistFacility)
                {
                    @this.Facility = @this.Part?.DefaultFacility;
                }

                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = @this.Part?.UnitOfMeasure;
                }

                if (@this.ExistPart && @this.Part.InventoryItemKind.IsSerialised)
                {
                    if (!@this.ExistSerialisedInventoryItemState)
                    {
                        @this.SerialisedInventoryItemState = @this.Reason.DefaultSerialisedInventoryItemState;
                    }
                }
                else
                {
                    if (!@this.ExistNonSerialisedInventoryItemState)
                    {
                        @this.NonSerialisedInventoryItemState = @this.Reason.DefaultNonSerialisedInventoryItemState;
                    }
                }

                if (!@this.ExistInventoryItem)
                {
                    SyncInventoryItem(@this, validation);
                }

                // Match on required properties
                var matched = false;
                var matchingItems = @this.Part.InventoryItemsWherePart.ToArray();
                var possibleMatches = matchingItems.Length > 0;

                if (possibleMatches)
                {
                    matchingItems = matchingItems.Where(i => i.Facility.Equals(@this.Facility)).ToArray();
                    possibleMatches = (matchingItems != null) && (matchingItems.Length > 0);
                }

                if (possibleMatches)
                {
                    matchingItems = matchingItems.Where(m => m.UnitOfMeasure.Equals(@this.UnitOfMeasure)).ToArray();
                    possibleMatches = matchingItems.Length > 0;
                }

                // Match on optional properties
                if (possibleMatches && @this.ExistLot)
                {
                    matchingItems = matchingItems.Where(m => m.Lot.Equals(@this.Lot)).ToArray();
                    possibleMatches = matchingItems.Length > 0;
                }

                if (possibleMatches && @this.ExistSerialisedInventoryItemState)
                {
                    if (matchingItems is SerialisedInventoryItem[] matchingSerialisedItems)
                    {
                        matchingItems = matchingSerialisedItems.Where(m => m.InventoryItemState.Equals(@this.SerialisedInventoryItemState)).ToArray();
                        possibleMatches = matchingItems.Length > 0;
                    }
                }

                if (possibleMatches && @this.ExistNonSerialisedInventoryItemState)
                {
                    if (matchingItems is NonSerialisedInventoryItem[] matchingNonSerialisedItems)
                    {
                        matchingItems = matchingNonSerialisedItems.Where(m => m.InventoryItemState.Equals(@this.NonSerialisedInventoryItemState)).ToArray();
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
                            if (nonSerialItem.NonSerialisedInventoryItemState.Equals(@this.NonSerialisedInventoryItemState))
                            {
                                @this.InventoryItem = item;
                                matched = true;
                                break;
                            }
                        }
                        else if (item is SerialisedInventoryItem serialItem)
                        {
                            if (serialItem.SerialisedItem.Equals(@this.SerialisedItem))
                            {
                                @this.InventoryItem = item;
                                matched = true;
                                break;
                            }
                        }
                    }
                }

                if (matched)
                {
                    if (@this.InventoryItem is SerialisedInventoryItem serialItem)
                    {
                        serialItem.SerialisedInventoryItemState = @this.SerialisedInventoryItemState;
                    }

                    if (@this.InventoryItem is NonSerialisedInventoryItem nonSerialItem)
                    {
                        nonSerialItem.NonSerialisedInventoryItemState = @this.NonSerialisedInventoryItemState;
                    }
                }

                static void SyncInventoryItem(InventoryItemTransaction inventoryItemTransaction, IDomainValidation validation)
                {
                    var facility = inventoryItemTransaction.Facility ?? inventoryItemTransaction.Part.DefaultFacility;
                    var unitOfMeasure = inventoryItemTransaction.UnitOfMeasure ?? inventoryItemTransaction.Part.UnitOfMeasure;

                    if (inventoryItemTransaction.Part.InventoryItemKind.IsSerialised && (inventoryItemTransaction.Quantity < -1 || inventoryItemTransaction.Quantity > 1))
                    {
                        var message = "Invalid transaction";
                        validation.AddError($"{inventoryItemTransaction}, {inventoryItemTransaction.Meta.SerialisedItem}, {message}");
                    }

                    if (inventoryItemTransaction.Part.InventoryItemKind.IsSerialised)
                    {
                        var inventoryItems = inventoryItemTransaction.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem;
                        inventoryItems.Filter.AddEquals(inventoryItemTransaction.M.InventoryItem.Part, inventoryItemTransaction.Part);
                        inventoryItems.Filter.AddEquals(inventoryItemTransaction.M.InventoryItem.Facility, facility);
                        var inventoryItem = inventoryItems.First;

                        if (inventoryItem == null)
                        {
                            var builder = new SerialisedInventoryItemBuilder(inventoryItemTransaction.Strategy.Session)
                                .WithFacility(facility)
                                .WithUnitOfMeasure(unitOfMeasure)
                                .WithSerialisedItem(inventoryItemTransaction.SerialisedItem)
                                .WithPart(inventoryItemTransaction.Part)
                                .WithSerialisedInventoryItemState(inventoryItemTransaction.SerialisedInventoryItemState);

                            if (inventoryItemTransaction.ExistLot)
                            {
                                builder.WithLot(inventoryItemTransaction.Lot);
                            }

                            inventoryItemTransaction.InventoryItem = builder.Build();
                        }
                    }
                    else if (inventoryItemTransaction.Part.InventoryItemKind.IsNonSerialised)
                    {
                        var inventoryItems = inventoryItemTransaction.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(inventoryItemTransaction.M.InventoryItem.Facility, facility);
                        var inventoryItem = inventoryItems.First;

                        if (inventoryItem == null)
                        {
                            var builder = new NonSerialisedInventoryItemBuilder(inventoryItemTransaction.Strategy.Session)
                                .WithFacility(facility)
                                .WithUnitOfMeasure(unitOfMeasure)
                                .WithPart(inventoryItemTransaction.Part)
                                .WithNonSerialisedInventoryItemState(inventoryItemTransaction.NonSerialisedInventoryItemState);

                            if (inventoryItemTransaction.ExistLot)
                            {
                                builder.WithLot(inventoryItemTransaction.Lot);
                            }

                            inventoryItemTransaction.InventoryItem = builder.Build();
                        }
                    }
                }
            }
        }
    }
}
