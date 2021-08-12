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

    public class S12ChangedRoleRule : Rule
    {
        public S12ChangedRoleRule(MetaPopulation m) : base(m, new Guid("475E8B38-21BB-40F9-AD67-9A7432F73CDD")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.S12, m.S12.ChangedRolePingS12)
            };


        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var s12 in matches.Cast<S12>())
            {
                s12.ChangedRolePongS12 = s12.ChangedRolePingS12;
            }
        }
    }
}
