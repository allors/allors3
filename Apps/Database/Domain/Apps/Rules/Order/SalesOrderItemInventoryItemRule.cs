// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesOrderItemInventoryItemRule : Rule
    {
        public SalesOrderItemInventoryItemRule(MetaPopulation m) : base(m, new Guid("FEF4E104-A0F0-4D83-A248-A1A606D93E41")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.SerialisedItem),
                m.SalesOrderItem.RolePattern(v => v.ReservedFromNonSerialisedInventoryItem),
                m.SalesOrderItem.RolePattern(v => v.ReservedFromSerialisedInventoryItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState),
                m.SerialisedInventoryItem.RolePattern(v => v.Quantity, v => v.SerialisedItem.SerialisedItem.SalesOrderItemsWhereSerialisedItem),
                m.Part.AssociationPattern(v => v.InventoryItemTransactionsWherePart, v => v.AsUnifiedGood.SalesOrderItemsWhereProduct),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                if (@this.SalesOrderItemState.IsInProcess
                     && @this.ExistPreviousReservedFromNonSerialisedInventoryItem
                    && !Equals(@this.ReservedFromNonSerialisedInventoryItem, @this.PreviousReservedFromNonSerialisedInventoryItem))
                {
                    validation.AddError($"{@this} {@this.Meta.ReservedFromNonSerialisedInventoryItem} {ErrorMessages.ReservedFromNonSerialisedInventoryItem}");
                }

                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                if (@this.IsValid && @this.Part != null && salesOrder?.TakenBy != null)
                {
                    if (@this.Part.InventoryItemKind.IsSerialised)
                    {
                        if (!@this.ExistReservedFromSerialisedInventoryItem)
                        {
                            if (@this.ExistSerialisedItem)
                            {
                                if (@this.SerialisedItem.ExistSerialisedInventoryItemsWhereSerialisedItem)
                                {
                                    @this.ReservedFromSerialisedInventoryItem = @this.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => v.Quantity == 1);
                                }
                            }
                            else
                            {
                                var inventoryItems = @this.Part.InventoryItemsWherePart;
                                inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, salesOrder.OriginFacility);
                                @this.ReservedFromSerialisedInventoryItem = inventoryItems.FirstOrDefault() as SerialisedInventoryItem;
                            }
                        }
                    }
                    else
                    {
                        if (!@this.ExistReservedFromNonSerialisedInventoryItem)
                        {
                            var inventoryItems = @this.Part.InventoryItemsWherePart;
                            inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, salesOrder.OriginFacility);
                            @this.ReservedFromNonSerialisedInventoryItem = inventoryItems.FirstOrDefault() as NonSerialisedInventoryItem;
                        }
                    }
                }

                @this.PreviousReservedFromNonSerialisedInventoryItem = @this.ReservedFromNonSerialisedInventoryItem;
            }
        }
    }
}
