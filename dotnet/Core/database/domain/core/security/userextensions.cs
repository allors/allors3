// <copyright file="UserExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public static partial class UserExtensions
    {
        public static bool IsAdministrator(this User @this)
        {
            var administrators = new UserGroups(@this.Transaction()).Administrators;
            return administrators.Members.Contains(@this);
        }

        public static T SetPassword<T>(this T @this, string clearTextPassword)
            where T : User
        {
            var passwordService = @this.Transaction().Database.Services.Get<IPasswordHasher>();
            @this.UserPasswordHash = passwordService.HashPassword(@this.UserName, clearTextPassword);
            return @this;
        }

        public static bool VerifyPassword(this User @this, string clearTextPassword)
        {
            if (string.IsNullOrWhiteSpace(clearTextPassword))
            {
                return false;
            }

            var passwordService = @this.Transaction().Database.Services.Get<IPasswordHasher>();
            return passwordService.VerifyHashedPassword(@this.UserName, @this.UserPasswordHash, clearTextPassword);
        }

        public static void CoreOnPostBuild(this User @this, ObjectOnPostBuild method)
        {
            if (!@this.ExistOwnerGrant)
            {
                var ownerRole = new Roles(@this.Strategy.Transaction).Owner;
                @this.OwnerGrant = new GrantBuilder(@this.Strategy.Transaction)
                    .WithRole(ownerRole)
                    .WithSubject(@this)
                    .Build();
            }

            if (!@this.ExistOwnerSecurityToken)
            {
                @this.OwnerSecurityToken = new SecurityTokenBuilder(@this.Strategy.Transaction)
                    .WithGrant(@this.OwnerGrant)
                    .Build();
            }

            if (!@this.ExistUserSecurityStamp)
            {
                @this.UserSecurityStamp = Guid.NewGuid().ToString();
            }
        }

        public static void CoreDelete(this User @this, DeletableDelete method)
        {
            @this.OwnerGrant?.Delete();
            @this.OwnerSecurityToken?.Delete();

            foreach (var login in @this.Logins)
            {
                login.Delete();
            }
        }
    }
}
