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

    public class RgsFilterEzZzpWoCoRule : Rule
    {
        public RgsFilterEzZzpWoCoRule(MetaPopulation m) : base(m, new Guid("0e0cd7ae-1d1b-4ad0-94a2-300a496f6c72")) =>
            this.Patterns = new Pattern[]
            {
                m.RgsFilter.RolePattern(v => v.UseEz),
                m.RgsFilter.RolePattern(v => v.UseZzp),
                m.RgsFilter.RolePattern(v => v.UseWoCo)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RgsFilter>())
            {
                bool[] list = { @this.UseEz, @this.UseZzp, @this.UseWoCo };


                if (list.Count(x => x) > 1)
                {
                    validation.AddError(@this, @this.Meta.UseEz, "UseEz, UseZzp and UseWoCo, There can only be 1 True");
                }
            }
        }
    }
}
