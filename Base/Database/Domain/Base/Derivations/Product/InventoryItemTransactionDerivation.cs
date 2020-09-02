// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class InventoryItemTransactionCreationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("E1F9D2DA-9C99-473D-B49F-17465CDEDBC9");

        public IEnumerable<Pattern> Patterns { get; } = new[] { new CreatedPattern(M.InventoryItemTransaction.Class) };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var inventoryItemTransaction in matches.Cast<InventoryItemTransaction>())
            {

                if (inventoryItemTransaction.Part.InventoryItemKind.IsSerialised)
                {
                    if (inventoryItemTransaction.Quantity != 1 && inventoryItemTransaction.Quantity != -1 && inventoryItemTransaction.Quantity != 0)
                    {
                        var message = "Serialised Inventory Items only accept Quantities of -1, 0, and 1.";
                        validation.AddError($"{inventoryItemTransaction} {inventoryItemTransaction.Meta.Quantity} {message}");
                    }

                    if (!inventoryItemTransaction.ExistSerialisedItem)
                    {
                        var message = "The Serial Number is required for Inventory Item Transactions involving Serialised Inventory Items.";
                        validation.AddError($"{inventoryItemTransaction} {inventoryItemTransaction.Meta.SerialisedItem} {message}");
                    }

                    if (inventoryItemTransaction.Reason.IncreasesQuantityOnHand == true && (inventoryItemTransaction.Quantity < -1 || inventoryItemTransaction.Quantity > 1))
                    {
                        var message = "Invalid transaction";
                        validation.AddError($"{inventoryItemTransaction} {inventoryItemTransaction.Meta.Reason} {message}");
                    }

                    if (inventoryItemTransaction.Reason.IncreasesQuantityOnHand == false && (inventoryItemTransaction.Quantity < -1 || inventoryItemTransaction.Quantity > 1))
                    {
                        var message = "Invalid transaction";
                        validation.AddError($"{inventoryItemTransaction} {inventoryItemTransaction.Meta.Reason} {message}");
                    }

                    if (inventoryItemTransaction.Quantity == 1
                        && inventoryItemTransaction.SerialisedItem.ExistSerialisedInventoryItemsWhereSerialisedItem
                        && inventoryItemTransaction.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem.Any(v => v.Quantity == 1)
                        && inventoryItemTransaction.Reason.IncreasesQuantityOnHand == true)
                    {
                        var message = "Serialised item already in inventory";
                        validation.AddError($"{inventoryItemTransaction} {inventoryItemTransaction.Meta.SerialisedItem} {message}");
                    }
                }
            }

        }
    }
}
