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

    public class RgsFilterLevel5Rule : Rule
    {
        public RgsFilterLevel5Rule(MetaPopulation m) : base(m, new Guid("a7421482-6555-4a3a-8c3f-c80d212da56d")) =>
            this.Patterns = new Pattern[]
            {
                m.RgsFilter.RolePattern(v => v.ExcludeLevel5),
                m.RgsFilter.RolePattern(v => v.ExcludeLevel5Extension)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RgsFilter>())
            {
                if (@this.ExcludeLevel5.HasValue && !@this.ExcludeLevel5.Value)
                {
                    validation.AddError(@this, @this.Meta.ExcludeLevel5, "ExcludeLevel5 must be True");
                }

                if (@this.ExcludeLevel5Extension.HasValue && !@this.ExcludeLevel5Extension.Value)
                {
                    validation.AddError(@this, @this.Meta.ExcludeLevel5, "ExcludeLevel5Extension must be True");
                }
            }
        }
    }
}
