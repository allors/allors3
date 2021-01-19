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

    public class InventoryItemTransactionDerivation : DomainDerivation
    {
        public InventoryItemTransactionDerivation(M m) : base(m, new Guid("E1F9D2DA-9C99-473D-B49F-17465CDEDBC9")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.InventoryItemTransaction.Quantity)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                if (@this.ExistPart && @this.Part.InventoryItemKind.IsSerialised)
                {
                    if (@this.Quantity != 1 && @this.Quantity != -1 && @this.Quantity != 0)
                    {
                        var message = "Serialised Inventory Items only accept Quantities of -1, 0, and 1.";
                        validation.AddError($"{@this} {@this.Meta.Quantity} {message}");
                    }

                    if (!@this.ExistSerialisedItem)
                    {
                        var message = "The Serial Number is required for Inventory Item Transactions involving Serialised Inventory Items.";
                        validation.AddError($"{@this} {@this.Meta.SerialisedItem} {message}");
                    }

                    if (@this.Reason.IncreasesQuantityOnHand == true && (@this.Quantity < -1 || @this.Quantity > 1))
                    {
                        var message = "Invalid transaction";
                        validation.AddError($"{@this} {@this.Meta.Reason} {message}");
                    }

                    if (@this.Reason.IncreasesQuantityOnHand == false && (@this.Quantity < -1 || @this.Quantity > 1))
                    {
                        var message = "Invalid transaction";
                        validation.AddError($"{@this} {@this.Meta.Reason} {message}");
                    }

                    if (@this.Quantity == 1
                        && @this.ExistSerialisedItem
                        && @this.SerialisedItem.ExistSerialisedInventoryItemsWhereSerialisedItem
                        && @this.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem.Any(v => v.Quantity == 1)
                        && @this.ExistReason
                        && @this.Reason.IncreasesQuantityOnHand == true)
                    {
                        var message = "Serialised item already in inventory";
                        validation.AddError($"{@this} {@this.Meta.SerialisedItem} {message}");
                    }
                }
            }
        }
    }
}
