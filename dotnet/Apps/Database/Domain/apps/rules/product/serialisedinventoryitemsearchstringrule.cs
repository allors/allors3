// <copyright file="Domain.cs" company="Allors bvba">
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
    using Derivations.Rules;

    public class SerialisedInventoryItemSearchStringRule : Rule
    {
        public SerialisedInventoryItemSearchStringRule(MetaPopulation m) : base(m, new Guid("422d3f47-37fd-45a8-bd19-4c17b513f5d5")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItem.RolePattern(v => v.Part, m.SerialisedInventoryItem),
                m.Part.RolePattern(v => v.DisplayName, v => v.InventoryItemsWherePart.InventoryItem, m.SerialisedInventoryItem),
                m.InventoryItem.RolePattern(v => v.Facility, m.SerialisedInventoryItem),
                m.Facility.RolePattern(v => v.Name, v => v.InventoryItemsWhereFacility.InventoryItem, m.SerialisedInventoryItem),
                m.InventoryItem.RolePattern(v => v.UnitOfMeasure, m.SerialisedInventoryItem),
                m.SerialisedInventoryItem.RolePattern(v => v.SerialisedItem),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.SerialisedInventoryItemsWhereSerialisedItem.SerialisedInventoryItem),
                m.SerialisedInventoryItem.RolePattern(v => v.SerialisedInventoryItemState),

                m.InventoryItem.AssociationPattern(v => v.WorkEffortInventoryAssignmentsWhereInventoryItem, m.SerialisedInventoryItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                var array = new string[] {
                    @this.Part?.DisplayName,
                    @this.Facility?.Name,
                    @this.UnitOfMeasure?.Name,
                    @this.SerialisedItem?.DisplayName,
                    @this.SerialisedInventoryItemState?.Name,
                    @this.ExistWorkEffortInventoryAssignmentsWhereInventoryItem ? string.Join(" ", @this.WorkEffortInventoryAssignmentsWhereInventoryItem?.Select(v => v.Assignment?.WorkEffortNumber ?? string.Empty).ToArray()) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
