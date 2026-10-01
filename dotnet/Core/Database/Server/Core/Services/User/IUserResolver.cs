// <copyright file="IUserResolver.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Security.Claims;
    using Database;
    using Database.Domain;

    // How an authentication plug-in connects to Core: it tells Core which Allors user a signed-in
    // principal stands for. Core authorizes that user; it never reads how the user signed in.
    // Exactly one resolver is registered, by the selected plug-in.
    public interface IUserResolver
    {
        // The user the signed-in principal stands for, or null when there is none. A plug-in that
        // creates or updates users does so in a transaction of its own and commits it: the request's
        // transaction is rolled back when the request ends.
        User Resolve(ClaimsPrincipal principal, ITransaction transaction);
    }
}
