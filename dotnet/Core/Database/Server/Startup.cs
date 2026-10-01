// <copyright file="Startup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using Allors.Services;
    using Database.Adapters;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Domain;
    using Database.Meta;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ObjectFactory = Database.ObjectFactory;
    using User = Database.Domain.User;

    // The test-harness server of the Core test domain. It switches on every building block Core
    // offers, so the platform tests all of them. It selects no authentication plug-in: its only
    // credential is the test sign-in header.
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
                UseControllersWithViews = true,
            });

            services.AddAllorsDefaultDeny();
            services.AddAllorsDataProtection(this.Configuration, this.Environment);
            // No sign-in page to limit: the paths, if any, come from Security:AuthenticationRateLimit.
            services.AddAllorsRateLimiting(this.Configuration);
            services.AddResponseCaching();

            // Test-harness only (jest + the remote C# suites hit this abstract server): a request with
            // the X-Allors-TestUser header authenticates as that user, and the test resolver tells Core
            // who that user is. Registered here, in the abstract server's Startup, not in an inherited
            // seam, so a downstream inheritor never gets it.
            services.AddAuthentication(TestUserAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestUserAuthenticationHandler>(TestUserAuthenticationHandler.SchemeName, null);
            services.AddSingleton<IUserResolver, TestUserResolver>();
        }

        public void Configure(IApplicationBuilder app)
        {
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
            var databaseBuilder = new DatabaseBuilder(new DefaultDatabaseServices(engine, this.Configuration), this.Configuration, objectFactory, null, 60);
            var databaseService = app.ApplicationServices.GetRequiredService<IDatabaseService>();
            databaseService.Build = () => databaseBuilder.Build();
            databaseService.Database = databaseService.Build();

            app.UseAllorsForwardedHeaders(this.Configuration);
            app.UseAllorsSecurityHeaders();

            if (this.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.ConfigureExceptionHandler(this.Environment);
            app.UseResponseCaching();

            app.UseAllorsServer();
        }
    }
}
