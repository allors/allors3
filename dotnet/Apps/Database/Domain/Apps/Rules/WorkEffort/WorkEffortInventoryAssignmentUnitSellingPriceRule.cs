// <copyright file="WorkEffortInventoryAssignmentUnitSellingPriceDerivation.cs" company="Allors bv">
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

    public class WorkEffortInventoryAssignmentUnitSellingPriceRule : Rule
    {
        public WorkEffortInventoryAssignmentUnitSellingPriceRule(MetaPopulation m) : base(m, new Guid("06422d32-7174-423c-897d-629bd8b878ad")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.InventoryItem),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.AssignedUnitSellingPrice),
            m.WorkEffort.RolePattern(v => v.ScheduledStart, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkEffort.RolePattern(v => v.ActualHours, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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

                    var startDate = @this.Assignment?.ActualStart ?? @this.Assignment?.ScheduledStart ?? transaction.Now().Date;

                    var currentPriceComponents = @this.Assignment?.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.ExistPrice && v.FromDate <= startDate && (!v.ExistThroughDate || v.ThroughDate >= startDate))
                        .ToArray();

                    if (@this.ExistAssignment && part != null && currentPriceComponents.Length > 0)
                    {
                        var currentPartPriceComponents = part.GetPriceComponents(currentPriceComponents);

                        var maxPrice = currentPartPriceComponents.OfType<BasePrice>().Max(v => Rounder.RoundDecimal(Currencies.ConvertCurrency(v.Price.Value, startDate, v.Currency, @this.Assignment.Currency), 2));

                        @this.UnitSellingPrice = maxPrice;
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
