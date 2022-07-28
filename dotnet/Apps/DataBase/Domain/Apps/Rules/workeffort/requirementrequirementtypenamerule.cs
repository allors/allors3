// <copyright file="RequirementTypeNameRule.cs" company="Allors bvba">
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

    public class RequirementRequirementTypeNameRule : Rule
    {
        public RequirementRequirementTypeNameRule(MetaPopulation m) : base(m, new Guid("cc63761c-0ebb-4f58-853a-11f1f65a1a09")) =>
            this.Patterns = new Pattern[]
        {
            m.Requirement.RolePattern(v => v.RequirementType),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Requirement>())
            {
                @this.RequirementTypeName = @this.RequirementType?.Name;
            }
        }
    }
}
