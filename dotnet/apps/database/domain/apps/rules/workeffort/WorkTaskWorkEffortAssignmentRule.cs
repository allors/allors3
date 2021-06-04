
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

    public class WorkTaskWorkEffortAssignmentRule : Rule
    {
        public WorkTaskWorkEffortAssignmentRule(MetaPopulation m) : base(m, new Guid("ece68fb5-8622-4a6b-908e-6a867c895548")) =>
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
                foreach (ServiceEntry serviceEntry in @this.ServiceEntriesWhereWorkEffort)
                {
                    if (serviceEntry is TimeEntry timeEntry)
                    {
                        var from = timeEntry.FromDate;
                        var through = timeEntry.ThroughDate;
                        var worker = timeEntry.TimeSheetWhereTimeEntry?.Worker;
                        var facility = timeEntry.WorkEffort.Facility;

                        var matchingAssignment = @this.WorkEffortPartyAssignmentsWhereAssignment.FirstOrDefault
                            (a => a.Assignment.Equals(@this)
                            && a.Party.Equals(worker)
                            && (a.ExistFacility && a.Facility.Equals(facility) || !a.ExistFacility && facility == null)
                            && (!a.ExistFromDate || a.ExistFromDate && a.FromDate <= @from)
                            && (!a.ExistThroughDate || a.ExistThroughDate && a.ThroughDate >= through));

                        if (matchingAssignment == null)
                        {
                            if (@this.TakenBy?.RequireExistingWorkEffortPartyAssignment == true)
                            {
                                // TODO: Move text to Resources
                                var message = $"No Work Effort Party Assignment matches Worker: {worker}, Facility: {facility}" + $", Work Effort: {@this}, From: {from}, Through {through}";
                                validation.AddError(@this, @this.M.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment, message);
                            }
                            else if (worker != null) // Sync a new WorkEffortPartyAssignment
                            {
                                new WorkEffortPartyAssignmentBuilder(@this.Strategy.Transaction)
                                    .WithAssignment(@this)
                                    .WithParty(worker)
                                    .WithFacility(facility)
                                    .Build();
                            }
                        }
                    }
                }
            }
        }
    }
}
