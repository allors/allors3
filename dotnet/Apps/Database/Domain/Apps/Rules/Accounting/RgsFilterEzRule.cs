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

    public class RgsFilterEzRule : Rule
    {
        public RgsFilterEzRule(MetaPopulation m) : base(m, new Guid("5df472d9-2e62-4485-99d1-2458c0fab30d")) =>
            this.Patterns = new Pattern[]
            {
                m.RgsFilter.RolePattern(v => v.UseEz),
                m.RgsFilter.RolePattern(v => v.ExcludeEz),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RgsFilter>())
            {
                if (@this.ExcludeEz.HasValue && @this.ExcludeEz.Value && @this.UseEz)
                {
                    validation.AddError(@this, this.M.RgsFilter.UseEz, "UseEz and ExcludeEz cannot be both true");
                }
            }
        }
    }
}
