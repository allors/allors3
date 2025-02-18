// <copyright file="Domain.cs" company="Allors bv">
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

    public class RgsFilterBaseRule : Rule
    {
        public RgsFilterBaseRule(MetaPopulation m) : base(m, new Guid("4897952b-b01e-4044-8563-00d98a0ff611")) =>
            this.Patterns = new Pattern[]
            {
                m.RgsFilter.RolePattern(v => v.UseBase),
                m.RgsFilter.RolePattern(v => v.UseExtended)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RgsFilter>())
            {
                if (@this.UseBase && @this.UseExtended)
                {
                    validation.AddError(@this, @this.Meta.UseBase, "UseBase and UseExtended can't be both True");
                }
            }
        }
    }
}
