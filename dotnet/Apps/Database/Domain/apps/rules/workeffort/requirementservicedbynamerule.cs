// <copyright file="RequirementServicedByNameRule.cs" company="Allors bvba">
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

    public class RequirementServicedByNameRule : Rule
    {
        public RequirementServicedByNameRule(MetaPopulation m) : base(m, new Guid("2f8711e3-035d-4044-b3d3-e0a03a655ce7")) =>
            this.Patterns = new Pattern[]
        {
            m.Requirement.RolePattern(v => v.ServicedBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Requirement>())
            {
                @this.ServicedByName = @this.ServicedBy?.PartyName;
            }
        }
    }
}
