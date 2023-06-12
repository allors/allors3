
// <copyright file="WorkRequirementStateRule.cs" company="Allors bvba">
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

    public class WorkRequirementStateRule : Rule
    {
        public WorkRequirementStateRule(MetaPopulation m) : base(m, new Guid("46237c5c-f159-4d8d-b4b0-5fc5b4cc3b89")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.WorkEffortState, v => v.WorkRequirementFulfillmentsWhereFullfillmentOf.ObjectType.FullfilledBy),
            m.WorkRequirementFulfillment.RolePattern(v => v.FullfilledBy, v => v.FullfilledBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                var workEffort = @this.WorkRequirementFulfillmentWhereFullfilledBy.FullfillmentOf;

                if (workEffort.WorkEffortState.IsCompleted || workEffort.WorkEffortState.IsFinished)
                {
                    @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).Finished;
                }
                else if (workEffort.WorkEffortState.IsCancelled)
                {
                    @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).Cancelled;
                }
                else
                {
                    @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).InProgress;
                }
            }
        }
    }
}
