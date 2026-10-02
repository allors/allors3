// <copyright file="AllorsAuthenticationOptions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;

    // What an authentication plug-in tells Core about the schemes it registers. The plug-in only
    // names them; Core applies the rules that every plug-in shares, in one place.
    public class AllorsAuthenticationOptions
    {
        // The cookie scheme that keeps a browser signed in, or null when the application has no
        // browser session. Core gives that cookie its defaults, answers its challenges for the
        // Allors API with a status code, and requires an antiforgery token of an unsafe API request
        // that the scheme authenticated: a browser sends such a cookie with every request on its own.
        public string SessionScheme { get; set; }

        // The scheme that authenticates a request carrying a bearer token, or null when the
        // application takes none. Core's selecting scheme forwards a request with an Authorization
        // header of the Bearer kind to it, and every other request to the session scheme, once a
        // plug-in makes the selecting scheme the default (see AllorsAuthenticationDefaults).
        public string BearerScheme { get; set; }

        // The scheme that signs a browser in when the session has nobody, an OpenID Connect scheme
        // for instance, or null when the session cookie redirects to a sign-in page of its own. Core
        // challenges it for a request outside the Allors API; a request to the API gets 401 either way.
        public string ChallengeScheme { get; set; }

        // How long a session may last from its sign-in, however often its sliding expiration renews
        // it, or null for no such limit. A plug-in whose identity provider cannot end the
        // application's session sets one.
        public TimeSpan? SessionLifetime { get; set; }
    }
}
