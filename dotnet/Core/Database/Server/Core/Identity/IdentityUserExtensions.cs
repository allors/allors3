// <copyright file="IdentityUserExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using Database;
    using Database.Domain;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;

    public static class IdentityUserExtensions
    {
        public static string CreateToken(this IdentityUser @this, IConfiguration configuration) => new JwtTokens(configuration).CreateToken(@this);

        public static User User(this IdentityUser @this, ITransaction transaction) => (User)transaction.Instantiate(@this.Id);
    }
}
