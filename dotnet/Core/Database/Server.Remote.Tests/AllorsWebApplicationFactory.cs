// <copyright file="AllorsWebApplicationFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System.Collections.Generic;
    using Database;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

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
