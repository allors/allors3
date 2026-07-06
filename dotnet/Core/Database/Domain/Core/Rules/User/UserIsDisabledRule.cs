// <copyright file="UserIsDisabledRule.cs" company="Allors bv">
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

    public class UserIsDisabledRule : Rule
    {
        public UserIsDisabledRule(MetaPopulation m) : base(m, new Guid("382036a0-64df-45e4-ba56-a2af9321ee5f")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v => v.IsDisabled),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var user in matches.Cast<User>())
            {
                if (user.IsDisabled)
                {
                    // Lock the account permanently and rotate the security stamp: the framework lockout
                    // gate rejects any sign-in and the stamp validator invalidates any live cookie.
                    user.UserLockoutEnabled = true;
                    user.UserLockoutEnd = System.DateTime.MaxValue;
                    user.UserSecurityStamp = Guid.NewGuid().ToString();
                }
                else if (user.UserLockoutEnd == System.DateTime.MaxValue)
                {
                    // Re-enabled: undo the permanent lockout this rule set, reset the failure count, and
                    // rotate the stamp again. Guarded on MaxValue so an ordinary timed lockout is untouched.
                    user.RemoveUserLockoutEnd();
                    user.UserAccessFailedCount = 0;
                    user.UserSecurityStamp = Guid.NewGuid().ToString();
                }
            }
        }
    }
}
