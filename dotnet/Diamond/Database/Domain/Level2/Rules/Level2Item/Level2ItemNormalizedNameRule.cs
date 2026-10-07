// <copyright file="Level2ItemNormalizedNameRule.cs" company="Allors bv">
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

    public class Level2ItemNormalizedNameRule : Rule
    {
        public Level2ItemNormalizedNameRule(MetaPopulation m) : base(m, new Guid("a82c2018-5a08-45a9-932a-0d025d9ca437")) =>
            this.Patterns = new Pattern[]
            {
                m.Level2Item.RolePattern(v => v.Level2Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var item in matches.Cast<Level2Item>())
            {
                item.Level2NormalizedName = item.Level2Name?.Normalize().ToUpperInvariant();
            }
        }
    }
}
