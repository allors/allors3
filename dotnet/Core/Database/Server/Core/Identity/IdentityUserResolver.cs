// <copyright file="IdentityUserResolver.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System.Globalization;
    using System.Security.Claims;
    using Database;
    using Database.Domain;
    using Services;

    // ASP.NET Core Identity signs a user in with the Allors object id as its user id (see
    // AllorsUserStore), so the NameIdentifier claim of an Identity principal is that id.
    public class IdentityUserResolver : IUserResolver
    {
        public User Resolve(ClaimsPrincipal principal, ITransaction transaction) =>
            long.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.None, CultureInfo.InvariantCulture, out var userId)
                ? transaction.Instantiate(userId) as User
                : null;
    }
}
