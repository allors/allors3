// <copyright file="UserExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

            // Rotate the security stamp so any live session is invalidated on the next revalidation.
            // (Done here rather than in a derive rule on UserPasswordHash: Identity's own UpdateAsync
            // already rotates the stamp when it changes the hash, and a rule would clobber that.)
            @this.UserSecurityStamp = Guid.NewGuid().ToString();

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

            if (!@this.ExistIsDisabled)
            {
                @this.IsDisabled = false;
            }
        }

        public static void CoreDelete(this User @this, DeletableDelete method)
        {
            @this.OwnerGrant?.CascadingDelete();
            @this.OwnerSecurityToken?.CascadingDelete();

            foreach (var login in @this.Logins)
            {
                login.CascadingDelete();
            }
        }
    }
}
