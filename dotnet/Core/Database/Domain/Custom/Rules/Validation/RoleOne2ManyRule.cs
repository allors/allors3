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

    public class RoleOne2ManyRule : Rule
    {
        public RoleOne2ManyRule(MetaPopulation m) : base(m, new Guid("d40ab5c5-c248-4455-bad4-8c825f48e080")) =>
            this.Patterns = new Pattern[]
            {
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBWhereOne2Many.BB.AAWhereOne2Many),
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBWhereUnusedOne2Many.BB.AAWhereUnusedOne2Many)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.One2Many.FirstOrDefault()?.One2Many.FirstOrDefault()?.Assigned;
            }
        }
    }
}
