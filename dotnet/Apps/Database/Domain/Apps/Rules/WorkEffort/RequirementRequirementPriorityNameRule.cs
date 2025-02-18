// <copyright file="RequirementPriorityNameRule.cs" company="Allors bv">
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

    public class RequirementRequirementPriorityNameRule : Rule
    {
        public RequirementRequirementPriorityNameRule(MetaPopulation m) : base(m, new Guid("a7423c7f-91cb-4aa5-8b9e-8b8a10a0b4d2")) =>
            this.Patterns = new Pattern[]
        {
            m.Requirement.RolePattern(v => v.Priority),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Requirement>())
            {
                @this.PriorityName = @this.Priority?.Name;
            }
        }
    }
}
