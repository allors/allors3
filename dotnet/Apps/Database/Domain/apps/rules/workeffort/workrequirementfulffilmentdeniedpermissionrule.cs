// <copyright file="WorkRequirementFulfillmentDeniedPermissionRule.cs" company="Allors bvba">
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

    public class WorkRequirementFulfillmentDeniedPermissionRule : Rule
    {
        public WorkRequirementFulfillmentDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("0e1774f5-289a-4202-a077-53567786a9e8")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.TransitionalRevocations, v => v.WorkRequirementFulfillmentsWhereFullfillmentOf),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirementFulfillment>())
            {
                var deleteRevocation = new Revocations(@this.Strategy.Transaction).WorkRequirementFulfillmentDeleteRevocation;

                if (@this.ExistFullfillmentOf)
                {
                    if (@this.FullfillmentOf.WorkEffortState.IsCancelled
                        || @this.FullfillmentOf.WorkEffortState.IsCompleted
                        || @this.FullfillmentOf.WorkEffortState.IsFinished)
                    {
                        @this.AddRevocation(deleteRevocation);
                    }
                    else
                    {
                        @this.RemoveRevocation(deleteRevocation);
                    }
                }
            }
        }
    }
}
