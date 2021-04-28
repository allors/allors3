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

    public class WorkEffortAssignmentRateWorkEffortRule : Rule
    {
        public WorkEffortAssignmentRateWorkEffortRule(MetaPopulation m) : base(m, new Guid("ae6797aa-5d30-4b9b-bc4b-0f8b899f7eaf")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortAssignmentRate.RolePattern(v => v.WorkEffort),
            m.WorkEffortAssignmentRate.RolePattern(v => v.WorkEffortPartyAssignment),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortAssignmentRate>())
            {
                if (!@this.ExistWorkEffort && @this.ExistWorkEffortPartyAssignment)
                {
                    @this.WorkEffort = @this.WorkEffortPartyAssignment.Assignment;
                }
            }
        }
    }
}
