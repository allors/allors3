// <copyright file="WorkEffortInventoryAssignmentDerivedBillableQuantityDerivation.cs" company="Allors bvba">
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

    public class WorkEffortInventoryAssignmentDerivedBillableQuantityDerivation : DomainDerivation
    {
        public WorkEffortInventoryAssignmentDerivedBillableQuantityDerivation(M m) : base(m, new Guid("fb0411a2-00ec-4e19-bb4d-f58f4aefef14")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(m.WorkEffortInventoryAssignment.Quantity),
            new ChangedPattern(m.WorkEffortInventoryAssignment.AssignedBillableQuantity),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
