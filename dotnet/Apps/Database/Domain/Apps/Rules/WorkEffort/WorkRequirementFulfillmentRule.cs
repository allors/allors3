// <copyright file="WorkRequirementFulfillmentRule.cs" company="Allors bv">
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

    public class WorkRequirementFulfillmentRule : Rule
    {
        public WorkRequirementFulfillmentRule(MetaPopulation m) : base(m, new Guid("60cb36bf-6a1d-4060-b1ec-39b24747e55a")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkRequirementFulfillment.RolePattern(v => v.FullfilledBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirementFulfillment>())
            {
                @this.WorkRequirementNumber = @this.FullfilledBy.RequirementNumber;
                @this.WorkRequirementDescription = @this.FullfilledBy.Description;
            }
        }
    }
}
