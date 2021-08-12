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

    public class I1ChangedRoleRule : Rule
    {
        public I1ChangedRoleRule(MetaPopulation m) : base(m, new Guid("475E8B38-21BB-40F9-AD67-9A7432F73CDD")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.I1, m.S12.ChangedRolePingI1)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var i1 in matches.Cast<I1>())
            {
                i1.ChangedRolePongI1 = i1.ChangedRolePingI1;
            }
        }
    }
}
