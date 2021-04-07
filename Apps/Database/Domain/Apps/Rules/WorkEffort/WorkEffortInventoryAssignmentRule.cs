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
    using Database.Derivations;
    using Resources;

    public class WorkEffortInventoryAssignmentRule : Rule
    {
        public WorkEffortInventoryAssignmentRule(MetaPopulation m) : base(m, new Guid("cd533d3e-922c-4938-a12d-cfacd6c3b9d9")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Assignment),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                if (@this.ExistAssignment)
                {
                    @this.Assignment.ResetPrintDocument();
                }
            }
        }
    }
}
