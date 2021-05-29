// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class UserInUserPasswordRule : Rule
    {
        public UserInUserPasswordRule(MetaPopulation m) : base(m, new Guid("AF93DA46-1C9A-47C4-9E5F-6A04751F5259")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v=>v.InUserPassword),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<User>().Where(v => v.ExistInUserPassword))
            {
                var passwordService = @this.Transaction().Database.Services().PasswordHasher;
                @this.UserPasswordHash = passwordService.HashPassword(@this.UserName, @this.InUserPassword);
                @this.RemoveInUserPassword();
            }
        }
    }
}
