// <copyright file="WorkRequirementFulfillmentWorkEffortRule.cs" company="Allors bvba">
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

    public class WorkRequirementFulfillmentWorkEffortRule : Rule
    {
        public WorkRequirementFulfillmentWorkEffortRule(MetaPopulation m) : base(m, new Guid("06b26e72-ef71-48ac-8ed4-3d0ad7758743")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkRequirementFulfillment.RolePattern(v => v.FullfillmentOf),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirementFulfillment>())
            {
                @this.WorkEffortNumber = @this.FullfillmentOf.WorkEffortNumber;
                @this.WorkEffortName = @this.FullfillmentOf.Name;
            }
        }
    }
}
