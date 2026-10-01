// <copyright file="AllorsAntiforgeryOptions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Collections.Generic;

    public class AllorsAntiforgeryOptions
    {
        // The authentication types (schemes) that sign a browser in with a cookie. The browser sends
        // such a cookie with every request on its own, so an unsafe /allors request authenticated by
        // one of them must also carry a valid antiforgery token. An authentication plug-in that signs
        // in with a cookie adds its scheme here.
        public ISet<string> AuthenticationTypes { get; } = new HashSet<string>(StringComparer.Ordinal);
    }
}
