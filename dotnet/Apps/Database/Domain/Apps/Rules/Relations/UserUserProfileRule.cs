// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class UserUserProfileRule : Rule
    {
        public UserUserProfileRule(MetaPopulation m) : base(m, new Guid("8427f819-db71-42a8-a752-094dc08c47e0")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v => v.UserName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<User>())
            {
                @this.DeriveUserUserProfile(validation);
            }
        }
    }

    public static class UserUserProfileRuleExtensions
    {
        public static void DeriveUserUserProfile(this User @this, IValidation validation)
        {
            if (@this.ExistUserName && !@this.ExistUserProfile)
            {
                @this.UserProfile = new UserProfileBuilder(@this.Strategy.Transaction).Build();
            }
        }
    }
}
