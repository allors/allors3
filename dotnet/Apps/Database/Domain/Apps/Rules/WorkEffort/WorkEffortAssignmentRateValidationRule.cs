// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class WorkEffortAssignmentRateValidationRule : Rule
    {
        public WorkEffortAssignmentRateValidationRule(MetaPopulation m) : base(m, new Guid("a733672e-cbd8-4816-b628-0ab9bdd55703")) => this.Patterns = new Pattern[]
        {
            m.WorkEffortAssignmentRate.RolePattern(v => v.WorkEffortPartyAssignment),
            m.WorkEffortAssignmentRate.RolePattern(v => v.RateType),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortAssignmentRate>())
            {
                if (@this.ExistWorkEffort && @this.ExistRateType)
                {
                    var workEfforts = @this.WorkEffort.WorkEffortAssignmentRatesWhereWorkEffort.Where(v => Equals(@this.RateType, v.RateType));
                    if (workEfforts.Count() > 1)
                    {
                        validation.AddError(@this, @this.Meta.RateType, ErrorMessages.WorkEffortRateError);
                    }
                }
            }
        }
    }
}
