// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class WorkEffortAssignmentRateDerivation : DomainDerivation
    {
        public WorkEffortAssignmentRateDerivation(M m) : base(m, new Guid("a733672e-cbd8-4816-b628-0ab9bdd55703")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.WorkEffortAssignmentRate.WorkEffort),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortAssignmentRate>())
            {
                if (!@this.ExistWorkEffort && @this.ExistWorkEffortPartyAssignment)
                {
                    @this.WorkEffort = @this.WorkEffortPartyAssignment.Assignment;
                }

                if (@this.ExistRateType)
                {
                    var extent = @this.WorkEffort.WorkEffortAssignmentRatesWhereWorkEffort;
                    extent.Filter.AddEquals(@this.M.WorkEffortAssignmentRate.RateType, @this.RateType);
                    if (extent.Count > 1)
                    {
                        validation.AddError($"{@this}, {@this.Meta.RateType}, {ErrorMessages.WorkEffortRateError}");
                    }
                }
            }
        }
    }
}
