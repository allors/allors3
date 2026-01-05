// <copyright file="Startup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Server.Admin;

namespace Allors.Server
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Controllers;
    using Database.Domain;
    using Services;
    using Security;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using ObjectFactory = Database.ObjectFactory;
    using Database.Adapters;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Meta;
    using User = Database.Domain.User;
    using Microsoft.AspNetCore.Mvc;

    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this.Configuration);

            // Allors
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPolicyService, PolicyService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton(new WorkspaceConfig(new Dictionary<HostString, string>
            {
                {new HostString("localhost", 4010), "Default"}
            }));
            services.AddScoped<IWorkspaceService, WorkspaceService>();
            // Allors Scoped
            services.AddScoped<IClaimsPrincipalService, ClaimsPrincipalService>();
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddCors(options =>
                options.AddDefaultPolicy(
                    builder => builder
                        .WithOrigins(
                            "http://localhost",
                            "http://localhost:4000",
                            "http://localhost:4010",
                            "http://localhost:4020",
                            "http://localhost:4200",
                            "https://localhost:9876")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()));

            services.AddDefaultIdentity<IdentityUser>()
                .AddAllorsStores();

            services.AddAuthentication(option => option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.Configuration.GetSection("JwtToken:Key").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    });

            services.AddResponseCaching();
            services.AddControllersWithViews();

            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                var builtInFactory = options.InvalidModelStateResponseFactory;

                options.InvalidModelStateResponseFactory = context =>
                {
                    return builtInFactory(context);
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            // Allors
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
            var databaseScope = new DefaultDatabaseServices(engine);
            var databaseBuilder = new DatabaseBuilder(databaseScope, this.Configuration, objectFactory, null, 60);
            var databaseService = app.ApplicationServices.GetRequiredService<IDatabaseService>();
            databaseService.Build = () => databaseBuilder.Build();
            databaseService.Database = databaseService.Build();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors();

            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.ConfigureExceptionHandler(env);

            app.UseResponseCaching();

            app.UseMiddleware<ClaimsPrincipalServiceMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "allors/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
            });
        }
    }
}
