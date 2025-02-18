// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;
    using Resources;

    public class SalesOrderItemInventoryItemRule : Rule
    {
        public SalesOrderItemInventoryItemRule(MetaPopulation m) : base(m, new Guid("76dcdd1b-33ab-4d51-9c5e-66eb085ba6fb")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.SerialisedItem),
                m.SalesOrderItem.RolePattern(v => v.ReservedFromNonSerialisedInventoryItem),
                m.SalesOrderItem.RolePattern(v => v.ReservedFromSerialisedInventoryItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState),
                m.SerialisedInventoryItem.RolePattern(v => v.Quantity, v => v.SerialisedItem.ObjectType.SalesOrderItemsWhereSerialisedItem),
                m.Part.AssociationPattern(v => v.InventoryItemTransactionsWherePart, v => v.AsUnifiedGood.SalesOrderItemsWhereProduct),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                if (@this.SalesOrderItemState.IsInProcess
                     && @this.ExistPreviousReservedFromNonSerialisedInventoryItem
                    && !Equals(@this.ReservedFromNonSerialisedInventoryItem, @this.PreviousReservedFromNonSerialisedInventoryItem))
                {
                    validation.AddError(@this, @this.Meta.ReservedFromNonSerialisedInventoryItem, ErrorMessages.ReservedFromNonSerialisedInventoryItem);
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
                                var inventoryItems = @this.Part.InventoryItemsWherePart.Where(v => Equals(salesOrder.OriginFacility, v.Facility));
                                @this.ReservedFromSerialisedInventoryItem = inventoryItems.FirstOrDefault() as SerialisedInventoryItem;
                            }
                        }
                    }
                    else if (!@this.ExistReservedFromNonSerialisedInventoryItem)
                    {
                        var inventoryItems = @this.Part.InventoryItemsWherePart.Where(v => Equals(salesOrder.OriginFacility, v.Facility));
                        @this.ReservedFromNonSerialisedInventoryItem = inventoryItems.FirstOrDefault() as NonSerialisedInventoryItem;
                    }
                }

                @this.PreviousReservedFromNonSerialisedInventoryItem = @this.ReservedFromNonSerialisedInventoryItem;
            }
        }
    }
}
