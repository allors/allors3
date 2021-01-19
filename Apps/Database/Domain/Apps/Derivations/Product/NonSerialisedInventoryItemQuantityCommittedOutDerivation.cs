// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class NonSerialisedInventoryItemQuantityCommittedOutDerivation : DomainDerivation
    {
        public NonSerialisedInventoryItemQuantityCommittedOutDerivation(M m) : base(m, new Guid("2c758908-3644-4ead-b671-5c6f45604f70")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.InventoryItemTransaction.InventoryItem) { Steps = new IPropertyType[] {m.InventoryItemTransaction.InventoryItem }, OfType = m.NonSerialisedInventoryItem.Class },
                new ChangedPattern(m.PickListItem.InventoryItem) { Steps = new IPropertyType[] {m.PickListItem.InventoryItem }, OfType = m.NonSerialisedInventoryItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                var quantityCommittedOut = 0M;

                foreach (InventoryItemTransaction inventoryItemTransaction in @this.InventoryItemTransactionsWhereInventoryItem)
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

                foreach (PickListItem pickListItem in @this.PickListItemsWhereInventoryItem)
                {
                    if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(@this.Strategy.Session).Picked))
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            if (!itemIssuance.ShipmentItem.ShipmentItemState.Shipped)
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

                if (quantityCommittedOut != @this.QuantityCommittedOut)
                {
                    @this.QuantityCommittedOut = quantityCommittedOut;
                }
            }
        }
    }
}
