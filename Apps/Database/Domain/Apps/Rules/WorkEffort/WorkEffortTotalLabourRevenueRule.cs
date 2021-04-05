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

    public class WorkEffortTotalLabourRevenueRule : Rule
    {
        public WorkEffortTotalLabourRevenueRule(MetaPopulation m) : base(m, new Guid("fb6c7fc1-e090-4c17-a799-1c562b258ec7")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.TimeEntry, m.TimeEntry.WorkEffort) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new RolePattern(m.TimeEntry, m.TimeEntry.BillingAmount) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new RolePattern(m.TimeEntry, m.TimeEntry.IsBillable) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new RolePattern(m.TimeEntry, m.TimeEntry.AmountOfTime) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new RolePattern(m.TimeEntry, m.TimeEntry.BillableAmountOfTime) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalLabourRevenue = Math.Round(@this.BillableTimeEntries().Sum(v => v.BillingAmount), 2);
            }
        }
    }
}
