// <copyright file="WorkEffortInventoryAssignmentCostOfGoodsSoldDerivation.cs" company="Allors bvba">
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

    public class WorkEffortInventoryAssignmentCostOfGoodsSoldRule : Rule
    {
        public WorkEffortInventoryAssignmentCostOfGoodsSoldRule(M m) : base(m, new Guid("a6ce76fb-e8dd-4c87-b265-0ff8d0b2b049")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.InventoryItem),
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Quantity),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                @this.CostOfGoodsSold = @this.Quantity * @this.InventoryItem.Part.PartWeightedAverage.AverageCost;
            }
        }
    }
}
