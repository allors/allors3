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
    using Derivations.Rules;

    public class WorkEffortTypeRule : Rule
    {
        public WorkEffortTypeRule(MetaPopulation m) : base(m, new Guid("765b3252-66a0-4358-8f7b-1765a1d8cf53")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortType.RolePattern(v => v.WorkEffortPartStandards),
            m.WorkEffortPartStandard.RolePattern(v => v.FromDate, v => v.WorkEffortTypeWhereWorkEffortPartStandard),
            m.WorkEffortPartStandard.RolePattern(v => v.ThroughDate, v => v.WorkEffortTypeWhereWorkEffortPartStandard),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                @this.CurrentWorkEffortPartStandards = @this.WorkEffortPartStandards
                    .Where(v => v.FromDate <= @this.Transaction().Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Transaction().Now()))
                    .ToArray();

                @this.InactiveWorkEffortPartStandards = @this.WorkEffortPartStandards
                    .Except(@this.CurrentWorkEffortPartStandards)
                    .ToArray();
            }
        }
    }
}
