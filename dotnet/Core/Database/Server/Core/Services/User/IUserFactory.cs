// <copyright file="IUserFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Security.Claims;
    using Database;
    using Database.Domain;

    // How the application's own domain creates a user for an authentication plug-in. Core and the
    // plug-ins know User as an interface only; the concrete domain knows its classes, so it is the
    // one that creates users. Registering a factory is optional: without one no plug-in creates
    // users.
    public interface IUserFactory
    {
        // A new user for the principal, of the class the application chooses, or null when the
        // application does not admit the principal. The principal carries what the plug-in knows
        // about the person; it is authenticated only when the user is created during a sign-in.
        // The plug-in passes a transaction of its own, sets its authentication fields on the new
        // user afterwards, and derives and commits.
        User Create(ITransaction transaction, ClaimsPrincipal principal);
    }
}
