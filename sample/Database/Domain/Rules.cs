// <copyright file="Rules.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Derivations.Rules;
    using Meta;

    public static class Rules
    {
        // The rules that the derivation engine runs. A rule that is not in this list never runs, so an
        // application lists the rules of Core next to its own.
        public static Rule[] Create(MetaPopulation m) =>
            new Rule[]
            {
                // Core
                new UserNormalizedUserNameRule(m),
                new UserNormalizedUserEmailRule(m),
                new UserInUserPasswordRule(m),
                new GrantEffectiveUsersRule(m),
                new GrantEffectivePermissionsRule(m),
                new SecurityTokenSecurityStampRule(m),

                // Custom
                new PersonOpenWorkItemCountRule(m),
            };
    }
}
