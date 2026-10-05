// <copyright file="AllorsServerServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.RateLimiting;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public static partial class AllorsServerServiceCollectionExtensions
    {
        // What Core decides: the services behind the Allors API. The API endpoints require an
        // authenticated user themselves ([Authorize]), and the selected authentication plug-in tells
        // Core who that user is (IUserResolver). Everything else is a building block below, or plain
        // ASP.NET Core, that the application switches on in its Startup.
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

            if (!string.IsNullOrWhiteSpace(options.ApplicationName))
            {
                services.AddDataProtection().SetApplicationName(options.ApplicationName);
            }

            services.AddAuthorization();

            // The browser session. An authentication plug-in names its session scheme in
            // AllorsAuthenticationOptions, and Core applies the rules of the session to that cookie,
            // whichever plug-in registers it and whenever it does.
            services.AddOptions<AllorsAuthenticationOptions>();
            services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>>(provider =>
                new AllorsSessionCookie(provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>(), environment));
            services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>>(provider =>
                new AllorsSessionCookie(provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>(), environment,
                    provider.GetServices<IAllorsSessionValidator>().ToArray()));
            services.AddSingleton<IValidateOptions<CookieAuthenticationOptions>, AllorsSessionCookieValidation>();

            // The scheme that selects which named scheme authenticates a request: the bearer scheme
            // for a request that carries a bearer token, the session scheme for every other request.
            // A plug-in with both makes it the default scheme; until then it does nothing.
            services.AddAuthentication().AddPolicyScheme(AllorsAuthenticationDefaults.AuthenticationScheme, null, policySchemeOptions =>
                policySchemeOptions.ForwardDefaultSelector = context =>
                {
                    var authentication = context.RequestServices.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value;
                    var bearer = authentication.BearerScheme != null &&
                                 context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
                    return bearer ? authentication.BearerScheme : authentication.SessionScheme ?? authentication.BearerScheme;
                });

            // A browser that signs in with a cookie sends that cookie with every request on its own, so
            // an unsafe API request that the session scheme authenticated must also carry an
            // antiforgery token.
            services.AddAntiforgery(antiforgeryOptions =>
            {
                antiforgeryOptions.HeaderName = "X-XSRF-TOKEN";
                antiforgeryOptions.Cookie.SecurePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            });

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
                    LogInvalidModelState(logger, problemDetails.Title, message);

                    return builtInFactory(context);
                };
            });

            return mvcBuilder;
        }

        // Building block: every endpoint without authorization of its own requires an authenticated
        // user, unless it opts out with [AllowAnonymous]. The Allors API does not depend on it.
        public static IServiceCollection AddAllorsDefaultDeny(this IServiceCollection services) =>
            services.AddAuthorization(authorizationOptions =>
                authorizationOptions.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        // Building block: limits requests to the given paths, typically the paths an authentication
        // plug-in signs in on, per client IP; configured in Security:AuthenticationRateLimit. Pair it
        // with app.UseRateLimiter().
        public static IServiceCollection AddAllorsRateLimiting(this IServiceCollection services, IConfiguration configuration, params string[] paths)
        {
            var settings = AuthenticationRateLimitSettings.From(configuration);
            settings.Paths = settings.Paths.Concat(paths).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            return services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    AuthenticationRateLimitPolicy.Partition(context, settings));
            });
        }

        // Building block: keeps the data protection keys, which protect sign-in cookies and
        // antiforgery tokens, in DataProtection:KeysDirectory, or else in .allors/dataprotection-keys
        // under the content root, so that they survive a restart.
        public static IDataProtectionBuilder AddAllorsDataProtection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var keysDirectory = configuration["DataProtection:KeysDirectory"];
            if (string.IsNullOrWhiteSpace(keysDirectory))
            {
                keysDirectory = Path.Combine(environment.ContentRootPath, ".allors", "dataprotection-keys");
            }

            return services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "{Title} {Errors}")]
        private static partial void LogInvalidModelState(ILogger logger, string title, string errors);
    }
}
