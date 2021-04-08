// <copyright file="SerialisedInventoryItemQuantitiesDerivation.cs" company="Allors bvba">
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

    public class SerialisedInventoryItemQuantitiesRule : Rule
    {
        public SerialisedInventoryItemQuantitiesRule(MetaPopulation m) : base(m, new Guid("0dd99432-c8e6-4278-8f49-fb1a4d7d6ddc")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedInventoryItem.RolePattern(v => v.SerialisedInventoryItemState),
                m.InventoryItemTransaction.RolePattern(v => v.Quantity, v => v.InventoryItem.InventoryItem.AsSerialisedInventoryItem),
                m.InventoryItem.AssociationPattern(v => v.InventoryItemTransactionsWhereInventoryItem, m.SerialisedInventoryItem),
                m.InventoryItem.AssociationPattern(v => v.PickListItemsWhereInventoryItem, m.SerialisedInventoryItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                var settings = @this.Strategy.Transaction.GetSingleton().Settings;

                @this.Quantity = 0;

                if (!settings.InventoryStrategy.OnHandSerialisedStates.Contains(@this.SerialisedInventoryItemState))
                {
                    @this.Quantity = 0;
                }
                else
                {
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
                }

                foreach (PickListItem pickListItem in @this.PickListItemsWhereInventoryItem)
                {
                    if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(@this.Strategy.Transaction).Picked))
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            if (!itemIssuance.ShipmentItem.ShipmentItemState.IsShipped)
                            {
                                @this.Quantity -= (int)pickListItem.QuantityPicked;
                            }
                        }
                    }
                }

                if (@this.Quantity < 0 || @this.Quantity > 1)
                {
                    var message = "Invalid transaction";
                    cycle.Validation.AddError($"{@this} {@this.Meta.Quantity} {message}");
                }
            }
        }
    }
}
