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

    public class WorkEffortAssignmentRateDelegatedAccessRule : Rule
    {
        public WorkEffortAssignmentRateDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("2b49c161-18c7-4c47-9c7b-5c9310c85822")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortAssignmentRate.RolePattern(v => v.WorkEffort),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortAssignmentRate>())
            {
                @this.DelegatedAccess = @this.WorkEffort;
            }
        }
    }
}
