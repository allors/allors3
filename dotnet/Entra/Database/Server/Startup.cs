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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ObjectFactory = Database.ObjectFactory;
    using User = Database.Domain.User;

    // The test-harness server of the Entra tree: Core's server with the Entra plug-in, so users sign in
    // with Microsoft Entra ID, and the Test domain's user factory, which creates a Person for a person
    // and an Agent for a program. By default the server signs in against a fake Entra of its own, see
    // FakeEntra; pointed at a real tenant by configuration (Entra:TenantId, ClientId, ClientSecret and
    // FakeEntra:Enabled = false), it signs in against Microsoft.
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        private bool FakeEntraEnabled => this.Configuration.GetValue("FakeEntra:Enabled", false);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAllorsServer(this.Configuration, this.Environment, new AllorsServerOptions
            {
                ApplicationName = "Allors.Entra",
            });

            services.AddAllorsEntra(this.Configuration, this.Environment);

            // The concrete domain creates the users that the plug-in asks for.
            services.AddSingleton<IUserFactory, TestUserFactory>();

            if (this.FakeEntraEnabled)
            {
                services.AddFakeEntra();
            }
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

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Core maps the Allors API; the plug-in its sign-in and sign-out; the fake its own endpoints.
            app.UseAllorsServer(endpoints =>
            {
                endpoints.MapAllorsEntra();

                // Exercise Core's antiforgery after authorization has joined the two schemes.
                foreach (var scheme in new[] { EntraDefaults.SessionScheme, AllorsAuthenticationDefaults.AuthenticationScheme })
                {
                    endpoints.MapPost("/allors/Test/MultiScheme/" + scheme, () => Results.Ok())
                        .RequireAuthorization(new AuthorizeAttribute
                        {
                            AuthenticationSchemes = scheme + "," + EntraDefaults.BearerScheme,
                        });
                }

                if (this.FakeEntraEnabled)
                {
                    endpoints.MapFakeEntra();
                }
            });
        }
    }
}
