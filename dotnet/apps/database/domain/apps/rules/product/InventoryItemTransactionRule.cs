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
    using Derivations.Rules;
    using Resources;

    public class InventoryItemTransactionRule : Rule
    {
        public InventoryItemTransactionRule(MetaPopulation m) : base(m, new Guid("E1F9D2DA-9C99-473D-B49F-17465CDEDBC9")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.Quantity),
                m.SerialisedInventoryItem.RolePattern(v => v.Quantity, v => v.InventoryItemTransactionsWhereInventoryItem),
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
                        validation.AddError(@this, @this.Meta.Quantity, ErrorMessages.InvalidInventoryItemQuantity);
                    }

                    if (!@this.ExistSerialisedItem)
                    {
                        validation.AddError(@this, @this.Meta.SerialisedItem, ErrorMessages.SerialNumberRequired);
                    }

                    if (@this.Reason.IncreasesQuantityOnHand == true && (@this.Quantity < -1 || @this.Quantity > 1))
                    {
                        validation.AddError(@this, @this.Meta.Reason, ErrorMessages.InvalidTransaction);
                    }

                    if (@this.Reason.IncreasesQuantityOnHand == false && (@this.Quantity < -1 || @this.Quantity > 1))
                    {
                        validation.AddError(@this, @this.Meta.Reason, ErrorMessages.InvalidTransaction);
                    }

                    if (@this.SerialisedItem?.SerialisedInventoryItemsWhereSerialisedItem.Sum(v => v.Quantity) > 1 )
                    {
                        validation.AddError(@this, @this.Meta.SerialisedItem, ErrorMessages.SerializedItemAlreadyInInventory);
                    }
                }
            }
        }
    }
}
