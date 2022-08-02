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
    using Derivations.Rules;
    using Meta;

    public class RoleOne2OneRule : Rule
    {
        public RoleOne2OneRule(MetaPopulation m) : base(m, new Guid("1C369F4C-CC12-4064-9261-BF899205E251")) =>
            this.Patterns = new[]
            {
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBWhereOne2One.BB.AAWhereOne2One),
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBWhereUnusedOne2One.BB.AAWhereUnusedOne2One)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.One2One?.One2One?.Assigned;
            }
        }
    }
}
