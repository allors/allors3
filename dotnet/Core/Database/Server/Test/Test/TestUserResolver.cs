// <copyright file="TestUserResolver.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Globalization;
    using System.Security.Claims;
    using Allors.Services;
    using Database;
    using Database.Domain;

    // The test-harness server's IUserResolver. The test handler signs a test user in with the Allors
    // object id as the NameIdentifier claim (see TestUserAuthenticationHandler), and this resolver
    // turns that id back into the user. Registered only in the abstract test-harness server's Startup,
    // like the handler, so a downstream inheritor never gets it.
    public class TestUserResolver : IUserResolver
    {
        public User Resolve(ClaimsPrincipal principal, ITransaction transaction) =>
            long.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.None, CultureInfo.InvariantCulture, out var userId)
                ? transaction.Instantiate(userId) as User
                : null;
    }
}
