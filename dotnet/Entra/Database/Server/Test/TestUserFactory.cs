// <copyright file="TestUserFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Security.Claims;
    using Allors.Security;
    using Allors.Services;
    using Database;
    using Database.Domain;

    // The user factory of this tree's concrete domain, Test. It reads the claims the plug-in validated
    // and decides, as an application does: a program's own token becomes an Agent, a person becomes a
    // Person, and an account whose user name starts with "refused" is not admitted, so that the tests
    // have an account the application turns away. Nobody gets a group: a new user sees what the
    // access control gives every authenticated user and nothing more.
    public class TestUserFactory : IUserFactory
    {
        public const string RefusedPrefix = "refused";

        public User Create(ITransaction transaction, ClaimsPrincipal principal)
        {
            if (principal.IsApplication())
            {
                return new AgentBuilder(transaction).Build();
            }

            var userName = principal.UserName();
            if (userName == null || userName.StartsWith(RefusedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new PersonBuilder(transaction).Build();
        }
    }
}
