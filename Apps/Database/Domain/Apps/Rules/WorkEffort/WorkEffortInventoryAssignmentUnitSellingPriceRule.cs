// <copyright file="WorkEffortInventoryAssignmentUnitSellingPriceDerivation.cs" company="Allors bvba">
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
    using Resources;

    public class WorkEffortInventoryAssignmentUnitSellingPriceRule : Rule
    {
        public WorkEffortInventoryAssignmentUnitSellingPriceRule(M m) : base(m, new Guid("06422d32-7174-423c-897d-629bd8b878ad")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Assignment),
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.InventoryItem),
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.AssignedUnitSellingPrice),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                if (@this.AssignedUnitSellingPrice.HasValue)
                {
                    @this.UnitSellingPrice = @this.AssignedUnitSellingPrice.Value;
                }
                else
                {
                    var part = @this.InventoryItem.Part;

                    var currentPriceComponents = @this.Assignment?.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.FromDate <= @this.Assignment.ScheduledStart && (!v.ExistThroughDate || v.ThroughDate >= @this.Assignment.ScheduledStart))
                        .ToArray();

                    if (currentPriceComponents != null)
                    {
                        var currentPartPriceComponents = part.GetPriceComponents(currentPriceComponents);

                        var price = currentPartPriceComponents.OfType<BasePrice>().Max(v => v.Price);
                        @this.UnitSellingPrice = price ?? 0M;
                    }
                    else
                    {
                        @this.UnitSellingPrice = 0M;
                    }
                }
            }
        }
    }
}
