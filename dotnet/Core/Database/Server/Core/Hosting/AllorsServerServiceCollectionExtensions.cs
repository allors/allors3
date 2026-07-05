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
    using System.Threading.Tasks;
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
        // The default authentication scheme: a policy scheme that forwards Bearer-header requests to
        // JWT and everything else to the Identity application cookie.
        public const string AuthenticationScheme = "AllorsAuthentication";

        // Identity area pages disabled (404) by default — see DisableIdentityPagesConvention.
        // Overridable via the "Identity:DisabledPages" configuration array.
        private static readonly string[] DefaultDisabledIdentityPages =
        {
            "/Account/Register",
            "/Account/RegisterConfirmation",
            "/Account/Manage/PersonalData",
            "/Account/Manage/DeletePersonalData",
            "/Account/Manage/DownloadPersonalData",
            "/Account/Manage/TwoFactorAuthentication",
        };

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

            // Dual-scheme window: a request carrying an "Authorization: Bearer" header authenticates
            // via JWT (existing machine clients, bit-identical); everything else uses the revocable
            // Identity application cookie. JWT is retired once every client is on the cookie.
            services.AddAuthentication(authenticationOptions =>
                {
                    authenticationOptions.DefaultScheme = AuthenticationScheme;
                    authenticationOptions.DefaultAuthenticateScheme = AuthenticationScheme;
                    authenticationOptions.DefaultChallengeScheme = AuthenticationScheme;
                })
                .AddPolicyScheme(AuthenticationScheme, "Bearer header → JWT, else Identity cookie", policySchemeOptions =>
                    policySchemeOptions.ForwardDefaultSelector = context =>
                        context.Request.Headers.Authorization.Any(v => v != null && v.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            ? JwtBearerDefaults.AuthenticationScheme
                            : IdentityConstants.ApplicationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("JwtToken:Key").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    });

            services.ConfigureApplicationCookie(cookieOptions =>
            {
                cookieOptions.Cookie.Name = environment.IsDevelopment() ? "Allors.Auth" : "__Host-Allors.Auth";
                cookieOptions.Cookie.HttpOnly = true;
                cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
                // Development runs over plain http (the C#/Playwright fixtures use CookieContainer,
                // which refuses Secure cookies over http); production is https at the edge.
                cookieOptions.Cookie.SecurePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                cookieOptions.SlidingExpiration = true;
                cookieOptions.ExpireTimeSpan = TimeSpan.TryParse(configuration["Identity:Cookie:ExpireTimeSpan"], out var expireTimeSpan)
                    ? expireTimeSpan
                    : TimeSpan.FromHours(8);

                // JSON API callers get a raw status code, not a login-page redirect.
                cookieOptions.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/allors"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                cookieOptions.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/allors"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            // Revocation lever: the built-in SecurityStampValidator re-checks the persisted security
            // stamp on this interval, so a rotated stamp (disable / "log out everywhere") invalidates
            // live cookies within ~5 minutes.
            services.Configure<SecurityStampValidatorOptions>(securityStampValidatorOptions =>
                securityStampValidatorOptions.ValidationInterval = TimeSpan.FromMinutes(5));

            services.AddResponseCaching();

            var mvcBuilder = options.UseControllersWithViews ? services.AddControllersWithViews() : services.AddControllers();

            var disabledIdentityPages = configuration.GetSection("Identity:DisabledPages").Get<string[]>() ?? DefaultDisabledIdentityPages;
            services.AddRazorPages(razorPagesOptions =>
                razorPagesOptions.Conventions.Add(new DisableIdentityPagesConvention(disabledIdentityPages)));

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
