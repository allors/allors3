
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

    public class WorkTaskTakenByRule : Rule
    {
        public WorkTaskTakenByRule(MetaPopulation m) : base(m, new Guid("12794dc5-8a79-4983-b480-4324602ae717")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkTask, m.WorkTask.TakenBy),
            new RolePattern(m.WorkTask, m.WorkTask.ExecutedBy),
            new RolePattern(m.WorkTask, m.WorkTask.ActualStart),
            new RolePattern(m.WorkTask, m.WorkTask.WorkEffortState),
            new RolePattern(m.TimeEntry, m.TimeEntry.FromDate) { Steps = new IPropertyType[] { m.TimeEntry.WorkEffort} },
            new RolePattern(m.TimeEntry, m.TimeEntry.ThroughDate) { Steps = new IPropertyType[] { m.TimeEntry.WorkEffort} },
            new RolePattern(m.TimeEntry, m.TimeEntry.WorkEffort) { Steps = new IPropertyType[] { m.TimeEntry.WorkEffort} },
            new RolePattern(m.TimeSheet, m.TimeSheet.TimeEntries) { Steps = new IPropertyType[] { m.TimeSheet.TimeEntries, m.TimeEntry.WorkEffort} },
            new AssociationPattern(m.WorkEffortInventoryAssignment.Assignment),
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Quantity) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.InventoryItem) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistTakenBy
                    && @this.TakenBy != @this.CurrentVersion.TakenBy)
                {
                    validation.AddError($"{@this} {this.M.WorkTask.TakenBy} {ErrorMessages.InternalOrganisationChanged}");
                }

                @this.ResetPrintDocument();

                if (!@this.ExistWorkEffortNumber && @this.ExistTakenBy)
                {
                    var year = @this.Transaction().Now().Year;
                    @this.WorkEffortNumber = @this.TakenBy.NextWorkEffortNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.TakenBy.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.TakenBy.WorkEffortSequence.IsEnforcedSequence ? @this.TakenBy.WorkEffortNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.WorkEffortNumberPrefix;
                    @this.SortableWorkEffortNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.WorkEffortNumber, year.ToString());
                }
            }
        }
    }
}
