// <copyright file="AllorsServerOptions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public class AllorsServerOptions
    {
        public string ApplicationName { get; set; }

        public string[] CorsOrigins { get; set; }

        public IReadOnlyDictionary<HostString, string> WorkspaceNameByHost { get; set; } = new Dictionary<HostString, string>
        {
            { new HostString("localhost", 5000), "Default" },
        };

        public bool UseControllersWithViews { get; set; } = true;
    }
}
