// <copyright file="Startup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server
{
    using System;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Security;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HostFiltering;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using User = Allors.Database.Domain.User;

    // The services and the pipeline of the server, with the JSON API, the sign-in endpoint and the services of
    // Core's server.
    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            this.Configuration.Require("adapter", "JwtToken:Key");
            if (DatabaseBuilder.HasDatabaseServer(this.Configuration["adapter"]))
            {
                this.Configuration.Require("ConnectionStrings:DefaultConnection");
            }

            services.Configure<HostFilteringOptions>(options =>
            {
                var hosts = this.Configuration["AllowedHosts"]?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                options.AllowedHosts = hosts is { Length: > 0 } ? hosts : new[] { "localhost" };
                options.AllowEmptyHosts = false;
            });

            services.AddSingleton<IPolicyService, PolicyService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IWorkspaceService, DefaultWorkspaceService>();
            services.AddScoped<IClaimsPrincipalService, ClaimsPrincipalService>();
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddDefaultIdentity<IdentityUser>()
                .AddAllorsStores()
                .AddAllorsJwtBearer(this.Configuration);

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
            var databaseBuilder = new DatabaseBuilder(new DefaultDatabaseServices(engine), this.Configuration, objectFactory);
            app.ApplicationServices.GetRequiredService<IDatabaseService>().Database = databaseBuilder.Build();

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.ConfigureExceptionHandler(env);

            // The transaction of a request takes its user from the claims of the token.
            app.UseMiddleware<ClaimsPrincipalServiceMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                // The sign-in endpoint, allors/Authentication/Token, follows this route; the endpoints of the
                // JSON API, such as allors/pull, have routes of their own.
                endpoints.MapControllerRoute(name: "default", pattern: "allors/{controller}/{action}/{id?}");
                endpoints.MapControllers();
            });
        }

        private sealed class DefaultWorkspaceService : IWorkspaceService
        {
            public string Name => "Default";
        }
    }
}
