// <copyright file="ServerFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;

    // Runs the server in the process of a test, with its Program and Startup. The settings of the test come
    // after the configuration root and the environment, and so override them.
    public class ServerFactory : WebApplicationFactory<Program>
    {
        private readonly IDictionary<string, string> settings;

        public ServerFactory(IDictionary<string, string> settings) => this.settings = settings;

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddInMemoryCollection(this.settings));
    }
}
