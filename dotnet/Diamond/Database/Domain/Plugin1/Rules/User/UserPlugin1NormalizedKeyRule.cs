// <copyright file="UserPlugin1NormalizedKeyRule.cs" company="Allors bv">
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

    public class UserPlugin1NormalizedKeyRule : Rule
    {
        public UserPlugin1NormalizedKeyRule(MetaPopulation m) : base(m, new Guid("fed6a3ab-8f6c-4f13-ac29-9199d3727614")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v => v.Plugin1Key),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var user in matches.Cast<User>())
            {
                user.Plugin1NormalizedKey = user.Plugin1Key?.Normalize().ToUpperInvariant();
            }
        }
    }
}
