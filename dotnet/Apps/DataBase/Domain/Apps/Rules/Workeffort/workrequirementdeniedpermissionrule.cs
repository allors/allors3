// <copyright file="WorkRequirementDeniedPermissionRule.cs" company="Allors bvba">
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

    public class WorkRequirementDeniedPermissionRule : Rule
    {
        public WorkRequirementDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("619c3afb-7bac-4921-baab-6c2d3a7c253f")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkRequirement.RolePattern(v => v.TransitionalRevocations),
            m.WorkEffort.RolePattern(v => v.WorkEffortState, v => v.WorkRequirementFulfillmentsWhereFullfillmentOf.WorkRequirementFulfillment.FullfilledBy),
            m.WorkRequirementFulfillment.RolePattern(v => v.FullfilledBy, v => v.FullfilledBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var cancelRevocation = new Revocations(@this.Strategy.Transaction).WorkRequirementCancelRevocation;
                var deleteRevocation = new Revocations(@this.Strategy.Transaction).WorkRequirementDeleteRevocation;
                var reopenRevocation = new Revocations(@this.Strategy.Transaction).WorkRequirementReopenRevocation;

                if (@this.ExistWorkRequirementFulfillmentWhereFullfilledBy)
                {
                    @this.AddRevocation(cancelRevocation);
                    @this.AddRevocation(deleteRevocation);
                    @this.AddRevocation(reopenRevocation);
                }
            }
        }
    }
}
