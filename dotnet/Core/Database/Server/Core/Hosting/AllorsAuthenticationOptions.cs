// <copyright file="AllorsAuthenticationOptions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    // What an authentication plug-in tells Core about the schemes it registers. The plug-in only
    // names them; Core applies the rules that every plug-in shares, in one place.
    public class AllorsAuthenticationOptions
    {
        // The cookie scheme that keeps a browser signed in, or null when the application has no
        // browser session. Core gives that cookie its defaults, answers its challenges for the
        // Allors API with a status code, and requires an antiforgery token of an unsafe API request
        // that the scheme authenticated: a browser sends such a cookie with every request on its own.
        public string SessionScheme { get; set; }
    }
}
