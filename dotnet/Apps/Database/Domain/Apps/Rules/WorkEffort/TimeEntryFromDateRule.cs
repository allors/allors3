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
    using Meta;
    using Derivations.Rules;
    using Resources;

    public class TimeEntryFromDateRule : Rule
    {
        public TimeEntryFromDateRule(MetaPopulation m) : base(m, new Guid("3a84a4a7-1e38-4cac-b210-3326af198301")) =>
        this.Patterns = new Pattern[]
        {
            m.TimeEntry.RolePattern(v => v.FromDate),
            m.TimeEntry.RolePattern(v => v.Worker),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeEntry>())
            {
                @this.DeriveTimeEntryFromDate(validation);
            }
        }
    }

    public static class TimeEntryFromDateRuleExtensions
    {
        public static void DeriveTimeEntryFromDate(this TimeEntry @this, IValidation validation)
        {
            if (@this.ExistWorker)
            {
                if (@this.Worker.TimeEntriesWhereWorker.Except(new[] { @this })
                    .FirstOrDefault(v => v.FromDate <= @this.FromDate
                                        && (!v.ExistThroughDate || v.ThroughDate.Value >= @this.FromDate))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }

                if (@this.Worker.TimeEntriesWhereWorker.Except(new[] { @this })
                    .FirstOrDefault(v => @this.FromDate <= v.FromDate
                                        && (!@this.ExistThroughDate || @this.ThroughDate.Value >= v.FromDate))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }
            }
        }
    }
}
