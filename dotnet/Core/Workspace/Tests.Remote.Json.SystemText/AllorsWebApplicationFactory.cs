// <copyright file="AllorsWebApplicationFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System.Collections.Generic;
    using Allors.Database;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class AllorsWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["adapter"] = "MEMORY",
                    ["JwtToken:Key"] = "TestSecretKeyForJwtTokenSigningThatIsLongEnough",
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(new WorkspaceConfig(new Dictionary<HostString, string>
                {
                    { new HostString("localhost"), "Default" },
                    { new HostString("localhost", 5000), "Default" },
                }));
            });

            builder.UseEnvironment("Development");
        }

        public IDatabase Database
        {
            get
            {
                var databaseService = this.Services.GetRequiredService<IDatabaseService>();
                return databaseService.Database;
            }
        }
    }
}
