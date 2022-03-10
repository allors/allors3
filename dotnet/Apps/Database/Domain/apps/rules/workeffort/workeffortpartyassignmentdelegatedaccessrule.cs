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
    using Resources;

    public class WorkEffortPartyAssignmentDelegatedAccessRule : Rule
    {
        public WorkEffortPartyAssignmentDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("7407eab3-b776-41d2-90f6-2e9a0d5c6c40")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortPartyAssignment.RolePattern(v => v.AssignmentRates),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortPartyAssignment>())
            {
                @this.DelegatedAccess = @this.Assignment;
            }
        }
    }
}
