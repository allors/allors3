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
                    // Changing your own password requires proving the current one. Without this the
                    // check below is opt-in: a push carrying only InUserPassword replaces the hash
                    // outright. Scoped to self-service so it cannot break provisioning, where an
                    // administrator sets a worker's password and cannot know the old one.
                    if (@this.ExistInUserPassword
                        && @this.ExistUserPasswordHash
                        && string.IsNullOrWhiteSpace(@this.InExistingUserPassword)
                        && Equals(@this.Strategy.Transaction.Services.Get<Allors.Database.Services.IUserService>().User, @this))
                    {
                        cycle.Validation.AddError(@this, m.User.InExistingUserPassword, DomainErrors.InvalidPassword);
                        continue;
                    }

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
                        // SetPassword also rotates the security stamp, which signs other sessions out.
                        @this.SetPassword(@this.InUserPassword);
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
