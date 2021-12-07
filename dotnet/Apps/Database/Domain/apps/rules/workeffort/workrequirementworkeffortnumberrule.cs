
// <copyright file="WorkRequirementWorkEffortNumberRule.cs" company="Allors bvba">
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

    public class WorkRequirementWorkEffortNumberRule : Rule
    {
        public WorkRequirementWorkEffortNumberRule(MetaPopulation m) : base(m, new Guid("b9be0fbf-cb5f-40fc-ba30-63f1e22234d2")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkRequirementFulfillment.RolePattern(v => v.FullfillmentOf, v => v.FullfilledBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.WorkEffortNumber = @this.WorkRequirementFulfillmentWhereFullfilledBy.FullfillmentOf.WorkEffortNumber;
            }
        }
    }
}
