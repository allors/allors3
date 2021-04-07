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
    using Database.Derivations;
    using Meta;
    using DateTime = System.DateTime;

    public class TimeEntryWorkerRule : Rule
    {
        public TimeEntryWorkerRule(MetaPopulation m) : base(m, new Guid("fcacc37e-581a-4c6f-bb77-d06a2987ebcf")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.TimeSheet.TimeEntries),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeEntry>())
            {
                if (@this.ExistTimeSheetWhereTimeEntry)
                {
                    @this.Worker = @this.TimeSheetWhereTimeEntry.Worker;
                }
            }
        }
    }
}
