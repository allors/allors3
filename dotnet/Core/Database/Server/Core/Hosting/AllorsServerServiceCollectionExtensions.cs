// <copyright file="AllorsServerServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.RateLimiting;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    public static class AllorsServerServiceCollectionExtensions
    {
        public static IMvcBuilder AddAllorsServer(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, AllorsServerOptions options)
        {
            ProductionSecretsGuard.Validate(configuration, environment.IsDevelopment());

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

            var authenticationRateLimitSettings = AuthenticationRateLimitSettings.From(configuration);
            services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    AuthenticationRateLimitPolicy.Partition(context, authenticationRateLimitSettings));
            });

            var dataProtectionKeysDirectory = configuration["DataProtection:KeysDirectory"];
            if (string.IsNullOrWhiteSpace(dataProtectionKeysDirectory))
            {
                dataProtectionKeysDirectory = Path.Combine(environment.ContentRootPath, ".allors", "dataprotection-keys");
            }

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDirectory))
                .SetApplicationName(options.ApplicationName);

            services.AddDefaultIdentity<IdentityUser>(identityOptions =>
                {
                    // Bounded auto-unlock over hair-trigger hard locks: a permanent/low-threshold
                    // lockout is a denial-of-service lever against known usernames.
                    identityOptions.Lockout.AllowedForNewUsers = true;
                    identityOptions.Lockout.MaxFailedAccessAttempts = 10;
                    identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                    // Length over composition (NIST 800-63B / OWASP ASVS): composition rules push
                    // predictable substitutions without adding entropy.
                    identityOptions.Password.RequiredLength = 12;
                    identityOptions.Password.RequireDigit = false;
                    identityOptions.Password.RequireUppercase = false;
                    identityOptions.Password.RequireLowercase = false;
                    identityOptions.Password.RequireNonAlphanumeric = false;
                    identityOptions.Password.RequiredUniqueChars = 4;
                })
                .AddAllorsStores();

            services.Configure<IdentityOptions>(configuration.GetSection("Identity"));

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
