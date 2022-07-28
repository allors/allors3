// <copyright file="WorkEffortInventoryAssignmentDerivedBillableQuantityDerivation.cs" company="Allors bvba">
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

    public class WorkEffortInventoryAssignmentDerivedBillableQuantityRule : Rule
    {
        public WorkEffortInventoryAssignmentDerivedBillableQuantityRule(MetaPopulation m) : base(m, new Guid("f99a8e0d-0272-41d6-ab23-a3894ff3b04e")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.AssignedBillableQuantity),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                @this.DerivedBillableQuantity = @this.AssignedBillableQuantity ?? @this.Quantity;
            }
        }
    }
}
