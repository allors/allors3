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
    using Resources;

    public class UserInUserPasswordRule : Rule
    {
        public UserInUserPasswordRule(MetaPopulation m) : base(m, new Guid("AF93DA46-1C9A-47C4-9E5F-6A04751F5259")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v=>v.InExistingUserPassword),
                m.User.RolePattern(v=>v.InUserPassword),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<User>())
            {
                var passwordHasher = @this.Transaction().Database.Services.Get<IPasswordHasher>();
                var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

                try
                {
                    if (!string.IsNullOrWhiteSpace(@this.InExistingUserPassword) && !passwordHasher.VerifyHashedPassword(@this.UserName, @this.UserPasswordHash, @this.InExistingUserPassword))
                    {
                        cycle.Validation.AddError(@this, m.User.InExistingUserPassword, DomainErrors.InvalidPassword);
                        continue;
                    }

                    if (@this.ExistInUserPassword && !passwordHasher.CheckStrength(@this.InUserPassword))
                    {
                        cycle.Validation.AddError(@this, m.User.InUserPassword, DomainErrors.InvalidNewPassword);
                        continue;
                    }

                    if (@this.ExistInUserPassword)
                    {
                        @this.UserPasswordHash = passwordHasher.HashPassword(@this.UserName, @this.InUserPassword);
                    }
                }
                finally
                {
                    @this.RemoveInExistingUserPassword();
                    @this.RemoveInUserPassword();
                }
            }
        }
    }
}
