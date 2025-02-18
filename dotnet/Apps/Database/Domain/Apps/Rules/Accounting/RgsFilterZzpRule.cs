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

    public class RgsFilterZzpRule : Rule
    {
        public RgsFilterZzpRule(MetaPopulation m) : base(m, new Guid("c35e24b2-0e1b-411a-8eb4-ac344a7d8c3f")) =>
            this.Patterns = new Pattern[]
            {
                m.RgsFilter.RolePattern(v => v.UseZzp),
                m.RgsFilter.RolePattern(v => v.ExcludeZzp),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RgsFilter>())
            {
                if (@this.ExcludeZzp.HasValue && @this.ExcludeZzp.Value && @this.UseZzp)
                {
                    validation.AddError(@this, this.M.RgsFilter.UseZzp, "UseZzp and ExcludeZzp cannot be both true");
                }
            }
        }
    }
}
