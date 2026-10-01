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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ObjectFactory = Database.ObjectFactory;
    using User = Database.Domain.User;

    // The test-harness server of the Identity tree: Core's server with the Identity plug-in, so
    // users sign in with ASP.NET Core Identity.
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
                ApplicationName = "Allors.Identity",
            });

            services.AddAllorsIdentity(this.Configuration, this.Environment);
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

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAllorsServer();
        }
    }
}
