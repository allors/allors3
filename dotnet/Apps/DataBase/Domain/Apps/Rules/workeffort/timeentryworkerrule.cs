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
    using Derivations.Rules;
    using Meta;

    public class TimeEntryWorkerRule : Rule
    {
        public TimeEntryWorkerRule(MetaPopulation m) : base(m, new Guid("fcacc37e-581a-4c6f-bb77-d06a2987ebcf")) =>
            this.Patterns = new Pattern[]
        {
            m.TimeEntry.AssociationPattern(v => v.TimeSheetWhereTimeEntry),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
