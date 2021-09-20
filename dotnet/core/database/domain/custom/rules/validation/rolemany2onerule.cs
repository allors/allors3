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

    public class RoleMany2OneRule : Rule
    {
        public RoleMany2OneRule(MetaPopulation m) : base(m, new Guid("cbebe35e-9931-4701-8b05-8ed61b266bb2")) =>
            this.Patterns = new[]
            {
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBsWhereMany2One.BB.AAsWhereMany2One),
                m.CC.RolePattern(v=>v.Assigned, v=>v.BBsWhereUnusedMany2One.BB.AAsWhereUnusedMany2One)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.Many2One?.Many2One?.Assigned;
            }
        }
    }
}
