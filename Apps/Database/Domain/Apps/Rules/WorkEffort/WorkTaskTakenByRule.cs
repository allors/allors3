
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
        public WorkTaskTakenByRule(MetaPopulation m) : base(m, new Guid("02be092e-04ca-4cce-9255-19b562ee6dd4")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.TakenBy),
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
