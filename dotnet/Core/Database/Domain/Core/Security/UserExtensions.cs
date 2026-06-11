// <copyright file="UserExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class UserExtensions
    {
        public static bool IsAdministrator(this User @this)
        {
            var administrators = new UserGroups(@this.Transaction()).Administrators;
            return administrators.Members.Contains(@this);
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
        }

        public static void CoreDelete(this User @this, DeletableDelete method)
        {
            @this.OwnerGrant?.CascadingDelete();
            @this.OwnerSecurityToken?.CascadingDelete();
        }
    }
}
