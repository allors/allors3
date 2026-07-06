// <copyright file="Startup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAllorsServer(this.Configuration, this.Environment, new AllorsServerOptions
            {
                ApplicationName = "Allors.Core",
                CorsOrigins = new[]
                {
                    "http://localhost",
                    "http://localhost:4000",
                    "http://localhost:4200",
                    "http://localhost:9876",
                },
                UseControllersWithViews = true,
            });

            // Test-harness only (jest + the remote C# suites hit this abstract server): a request with
            // the X-Allors-TestUser header authenticates as that user. Registered here, in the abstract
            // server's Startup, rather than the inherited seam, so a downstream inheritor never gets it.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestUserAuthenticationHandler>(TestUserAuthenticationHandler.SchemeName, null);

            services.PostConfigure<PolicySchemeOptions>(
                AllorsServerServiceCollectionExtensions.AuthenticationScheme,
                policySchemeOptions =>
                {
                    var forwardToBearerOrCookie = policySchemeOptions.ForwardDefaultSelector;
                    policySchemeOptions.ForwardDefaultSelector = context =>
                        context.Request.Headers.ContainsKey(TestUserAuthenticationHandler.HeaderName)
                            ? TestUserAuthenticationHandler.SchemeName
                            : forwardToBearerOrCookie?.Invoke(context);
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) =>
            app.UseAllorsServer(this.Environment, loggerFactory);
    }
}
