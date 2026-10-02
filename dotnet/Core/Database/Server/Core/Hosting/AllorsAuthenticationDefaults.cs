// <copyright file="AllorsAuthenticationDefaults.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    public static class AllorsAuthenticationDefaults
    {
        // The scheme that AddAllorsServer registers to select, per request, between the schemes a
        // plug-in names in AllorsAuthenticationOptions: the bearer scheme for a request that carries
        // a bearer token, the session scheme for every other request. A plug-in with both a browser
        // session and bearer tokens makes it the default scheme:
        //
        //   services.AddAuthentication(AllorsAuthenticationDefaults.AuthenticationScheme)
        //
        // Until a plug-in does, the scheme does nothing.
        public const string AuthenticationScheme = "Allors";
    }
}
