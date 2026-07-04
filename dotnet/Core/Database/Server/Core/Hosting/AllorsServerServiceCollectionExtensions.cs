// <copyright file="AllorsServerServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Linq;
    using System.Text;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    public static class AllorsServerServiceCollectionExtensions
    {
        public static IMvcBuilder AddAllorsServer(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, AllorsServerOptions options)
        {
            services.AddSingleton(configuration);

            // Allors
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPolicyService, PolicyService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton(new WorkspaceConfig(options.WorkspaceNameByHost));
            // Allors Scoped
            services.AddScoped<IClaimsPrincipalService, ClaimsPrincipalService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IWorkspaceService, WorkspaceService>();

            services.AddCors(corsOptions =>
                corsOptions.AddDefaultPolicy(
                    builder => builder
                        .WithOrigins(options.CorsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()));

            services.AddDefaultIdentity<IdentityUser>()
                .AddAllorsStores();

            services.AddAuthentication(authenticationOptions => authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("JwtToken:Key").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    });

            services.AddResponseCaching();

            var mvcBuilder = options.UseControllersWithViews ? services.AddControllersWithViews() : services.AddControllers();

            services.PostConfigure<ApiBehaviorOptions>(apiBehaviorOptions =>
            {
                var builtInFactory = apiBehaviorOptions.InvalidModelStateResponseFactory;

                apiBehaviorOptions.InvalidModelStateResponseFactory = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Allors.Server");

                    var problemDetails = new ValidationProblemDetails(context.ModelState);
                    var message = string.Join("; ", problemDetails.Errors.Select(v => $"{string.Join(",", v.Value)}"));
                    logger.LogError(problemDetails.Title, message);

                    return builtInFactory(context);
                };
            });

            return mvcBuilder;
        }
    }
}
