
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

    public class WorkTaskActualHoursRule : Rule
    {
        public WorkTaskActualHoursRule(MetaPopulation m) : base(m, new Guid("ebe5b853-4b95-4b4f-b952-09bfe441eb8f")) =>
            this.Patterns = new Pattern[]
        {
            m.TimeEntry.RolePattern(v => v.FromDate, v => v.WorkEffort),
            m.TimeEntry.RolePattern(v => v.ThroughDate, v => v.WorkEffort),
            m.TimeEntry.RolePattern(v => v.WorkEffort, v => v.WorkEffort),
            m.TimeSheet.RolePattern(v => v.TimeEntries, v => v.TimeEntries.TimeEntry.WorkEffort)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.ActualHours = 0M;

                foreach (ServiceEntry serviceEntry in @this.ServiceEntriesWhereWorkEffort)
                {
                    if (serviceEntry is TimeEntry timeEntry)
                    {
                        @this.ActualHours += timeEntry.ActualHours;

                        if (!@this.ExistActualStart)
                        {
                            @this.ActualStart = timeEntry.FromDate;
                        }
                        else if (timeEntry.FromDate < @this.ActualStart)
                        {
                            @this.ActualStart = timeEntry.FromDate;
                        }

                        if (!@this.ExistActualCompletion)
                        {
                            @this.ActualCompletion = timeEntry.ThroughDate;
                        }
                        else if (timeEntry.ThroughDate > @this.ActualCompletion)
                        {
                            @this.ActualCompletion = timeEntry.ThroughDate;
                        }
                    }
                }
            }
        }
    }
}
