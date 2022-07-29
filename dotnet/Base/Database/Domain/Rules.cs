// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Derivations.Rules;
    using Meta;

    public static partial class Rules
    {
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

                // Base
                new MediaRule(m),
                new TransitionalDeniedPermissionRule(m),
                new NotificationListRule(m),

                // Custom
                new DataRule(m),
                new OrganisationEmployementRule(m),
                new PersonAddressRule(m),
                new PersonFullNameRule(m),
                new PersonGreetingRule(m),
            };
    }
}
