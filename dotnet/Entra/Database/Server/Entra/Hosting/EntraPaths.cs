// <copyright file="EntraPaths.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    // The endpoints of the Entra plug-in, mapped by MapAllorsEntra. Outside the Allors API: a request
    // to the API gets a status code, these two take a browser through a sign-in and a sign-out.
    public static class EntraPaths
    {
        // GET with a returnUrl: signs the browser in with Entra when it has no session, then sends
        // it to the local returnUrl.
        public const string SignIn = "/entra/sign-in";

        // POST with the antiforgery header: ends the session and the sign-in with Entra.
        public const string SignOut = "/entra/sign-out";
    }
}
