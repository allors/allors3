// <copyright file="UserExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public static partial class UserExtensions
    {
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

        public static void IdentityOnPostBuild(this User @this, ObjectOnPostBuild method)
        {
            if (!@this.ExistUserSecurityStamp)
            {
                @this.UserSecurityStamp = Guid.NewGuid().ToString();
            }
        }

        public static void IdentityDelete(this User @this, DeletableDelete method)
        {
            foreach (var login in @this.Logins)
            {
                login.CascadingDelete();
            }
        }
    }
}
