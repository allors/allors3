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

    public class RoleMany2ManyRule : Rule
    {
        public RoleMany2ManyRule(MetaPopulation m) : base(m, new Guid("4383159F-258D-4FCB-833C-55D2B91109A1")) =>
            this.Patterns = new[]
            {
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBsWhereMany2Many.BB.AAsWhereMany2Many),
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBsWhereUnusedMany2Many.BB.AAsWhereUnusedMany2Many)
            };


        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.Many2Many.FirstOrDefault()?.Many2Many.FirstOrDefault()?.Assigned;
            }
        }
    }
}
