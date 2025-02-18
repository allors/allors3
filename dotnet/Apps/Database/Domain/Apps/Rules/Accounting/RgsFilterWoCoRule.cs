// <copyright file="Domain.cs" company="Allors bvba">
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

    public class RgsFilterWoCoRule : Rule
    {
        public RgsFilterWoCoRule(MetaPopulation m) : base(m, new Guid("f46875ad-2166-4997-ba4a-07e798b6c897")) =>
            this.Patterns = new Pattern[]
            {
                m.RgsFilter.RolePattern(v => v.UseWoCo),
                m.RgsFilter.RolePattern(v => v.ExcludeWoCo),
            };
        
        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RgsFilter>())
            {
                if (@this.ExcludeWoCo.HasValue && @this.UseWoCo && @this.ExcludeWoCo.Value)
                {
                    validation.AddError(@this, @this.Meta.UseBase, "UseWoCo and ExcludeWoCo can't be both True");
                }
            }
        }
    }
}
