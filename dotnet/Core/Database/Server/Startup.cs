// <copyright file="Startup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
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

        // Default scheme for this (test-harness) server: routes the X-Allors-TestUser header to the
        // test handler and everything else to the Identity application cookie.
        private const string TestUserOrCookieScheme = "AllorsTestUserOrCookie";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAllorsServer(this.Configuration, this.Environment, new AllorsServerOptions
            {
                ApplicationName = "Allors.Core",
                UseControllersWithViews = true,
            });

            // Test-harness only (jest + the remote C# suites hit this abstract server): a request with
            // the X-Allors-TestUser header authenticates as that user. A policy scheme becomes the
            // default and routes the header to the test handler, everything else to the Identity
            // cookie. Registered here, in the abstract server's Startup, not the inherited seam, so a
            // downstream inheritor never gets it.
            services.AddAuthentication(authenticationOptions =>
                    authenticationOptions.DefaultScheme = TestUserOrCookieScheme)
                .AddScheme<AuthenticationSchemeOptions, TestUserAuthenticationHandler>(TestUserAuthenticationHandler.SchemeName, null)
                .AddPolicyScheme(TestUserOrCookieScheme, "X-Allors-TestUser header, else Identity cookie", policySchemeOptions =>
                    policySchemeOptions.ForwardDefaultSelector = context =>
                        context.Request.Headers.ContainsKey(TestUserAuthenticationHandler.HeaderName)
                            ? TestUserAuthenticationHandler.SchemeName
                            : IdentityConstants.ApplicationScheme);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) =>
            app.UseAllorsServer(this.Environment, loggerFactory);
    }
}
