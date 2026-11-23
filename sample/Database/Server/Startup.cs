// <copyright file="Startup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server
{
    using System.Collections.Generic;
    using System.Text;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Security;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
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

            // The workspace of a request is the one of the host that the request names. A client that reaches
            // the server through another host name or port needs an entry of its own.
            var workspaceConfig = new WorkspaceConfig(new Dictionary<HostString, string>
            {
                { new HostString("localhost", 5000), "Default" },
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPolicyService, PolicyService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton(workspaceConfig);
            services.AddScoped<IClaimsPrincipalService, ClaimsPrincipalService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IWorkspaceService, WorkspaceService>();

            // ASP.NET Core Identity finds and checks the users in the database.
            services.AddDefaultIdentity<IdentityUser>()
                .AddAllorsStores();

            // A request authenticates with the bearer token of the sign-in endpoint, which signs it with the
            // UTF-8 bytes of the setting JwtToken:Key.
            services.AddAuthentication(options => options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JwtToken:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                });

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
    }
}
