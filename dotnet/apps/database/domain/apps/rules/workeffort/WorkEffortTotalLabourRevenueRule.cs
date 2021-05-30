// <copyright file="WorkEffortTotalLabourRevenueDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Database.Derivations;
    using Derivations.Rules;

    public class WorkEffortTotalLabourRevenueRule : Rule
    {
        public WorkEffortTotalLabourRevenueRule(MetaPopulation m) : base(m, new Guid("fb6c7fc1-e090-4c17-a799-1c562b258ec7")) =>
            this.Patterns = new[]
            {
                m.TimeEntry.RolePattern(v => v.WorkEffort, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort.ServiceEntry.WorkEffort),
                m.TimeEntry.RolePattern(v => v.BillingAmount, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort.ServiceEntry.WorkEffort),
                m.TimeEntry.RolePattern(v => v.IsBillable, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort.ServiceEntry.WorkEffort),
                m.TimeEntry.RolePattern(v => v.AmountOfTime, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort.ServiceEntry.WorkEffort),
                m.TimeEntry.RolePattern(v => v.BillableAmountOfTime, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort.ServiceEntry.WorkEffort),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalLabourRevenue = Rounder.RoundDecimal(@this.BillableTimeEntries().Sum(v => v.BillingAmount), 2);
            }
        }
    }
}
