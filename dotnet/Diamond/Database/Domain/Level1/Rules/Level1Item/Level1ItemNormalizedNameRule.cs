// <copyright file="Level1ItemNormalizedNameRule.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class Level1ItemNormalizedNameRule : Rule
    {
        public Level1ItemNormalizedNameRule(MetaPopulation m) : base(m, new Guid("a998a941-bd7b-4819-80d8-4821db90a1ef")) =>
            this.Patterns = new Pattern[]
            {
                m.Level1Item.RolePattern(v => v.Level1Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var item in matches.Cast<Level1Item>())
            {
                item.Level1NormalizedName = item.Level1Name?.Normalize().ToUpperInvariant();
            }
        }
    }
}
