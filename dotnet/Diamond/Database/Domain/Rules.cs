// <copyright file="Rules.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Derivations.Rules;
    using Meta;

    // The rules of every domain of the population, which the concrete domain lists by hand.
    public static class Rules
    {
        public static Rule[] Create(MetaPopulation m) =>
            new Rule[]
            {
                // Plugin1
                new UserPlugin1NormalizedKeyRule(m),

                // Level2
                new Level2ItemNormalizedNameRule(m),

                // Level1
                new Level1ItemNormalizedNameRule(m),

                // Core
                new GrantEffectiveUsersRule(m),
                new GrantEffectivePermissionsRule(m),
                new SecurityTokenSecurityStampRule(m),
            };
    }
}
