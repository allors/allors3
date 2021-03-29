// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class WorkEffortTypeRule : Rule
    {
        public WorkEffortTypeRule(M m) : base(m, new Guid("765b3252-66a0-4358-8f7b-1765a1d8cf53")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortType, m.WorkEffortType.WorkEffortPartStandards),
            new RolePattern(m.WorkEffortPartStandard, m.WorkEffortPartStandard.FromDate) { Steps = new IPropertyType[] { m.WorkEffortPartStandard .WorkEffortTypeWhereWorkEffortPartStandard } },
            new RolePattern(m.WorkEffortPartStandard, m.WorkEffortPartStandard.ThroughDate) { Steps = new IPropertyType[] { m.WorkEffortPartStandard .WorkEffortTypeWhereWorkEffortPartStandard } },
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
