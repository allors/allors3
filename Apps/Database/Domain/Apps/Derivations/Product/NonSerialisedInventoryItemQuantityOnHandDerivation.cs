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

    public class NonSerialisedInventoryItemQuantityOnHandDerivation : DomainDerivation
    {
        public NonSerialisedInventoryItemQuantityOnHandDerivation(M m) : base(m, new Guid("36bb6207-ff7d-4bc1-afaf-a2c12d649c1c")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.NonSerialisedInventoryItem.NonSerialisedInventoryItemState),
                new ChangedPattern(m.InventoryItemTransaction.InventoryItem) { Steps = new IPropertyType[] {m.InventoryItemTransaction.InventoryItem }, OfType = m.NonSerialisedInventoryItem.Class },
                new ChangedPattern(m.PickListItem.InventoryItem) { Steps = new IPropertyType[] {m.PickListItem.InventoryItem }, OfType = m.NonSerialisedInventoryItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                var settings = @this.Strategy.Session.GetSingleton().Settings;

                var quantityOnHand = 0M;

                if (!settings.InventoryStrategy.OnHandNonSerialisedStates.Contains(@this.NonSerialisedInventoryItemState))
                {
                    @this.QuantityOnHand = 0;
                }
                else
                {
                    foreach (InventoryItemTransaction inventoryTransaction in @this.InventoryItemTransactionsWhereInventoryItem)
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

                    foreach (PickListItem pickListItem in @this.PickListItemsWhereInventoryItem)
                    {
                        if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(@this.Strategy.Session).Picked))
                        {
                            foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                            {
                                if (!itemIssuance.ShipmentItem.ShipmentItemState.Shipped)
                                {
                                    quantityOnHand -= pickListItem.QuantityPicked;
                                }
                            }
                        }
                    }

                    if (quantityOnHand != @this.QuantityOnHand)
                    {
                        @this.QuantityOnHand = quantityOnHand;
                    }
                }
            }
        }
    }
}
