// <copyright file="RequirementRequirementStateNameRule.cs" company="Allors bv">
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

    public class RequirementRequirementStateNameRule : Rule
    {
        public RequirementRequirementStateNameRule(MetaPopulation m) : base(m, new Guid("f19b1eff-2021-4941-a8ab-679a901eed1a")) =>
            this.Patterns = new Pattern[]
        {
            m.Requirement.RolePattern(v => v.RequirementState),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Requirement>())
            {
                @this.RequirementStateName = @this.RequirementState?.Name;
            }
        }
    }
}
