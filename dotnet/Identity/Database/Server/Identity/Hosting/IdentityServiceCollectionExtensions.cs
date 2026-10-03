// <copyright file="IdentityServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    // Authentication with ASP.NET Core Identity: users sign in with a user name and a password on the
    // Identity pages, and the Identity application cookie keeps them signed in.
    public static class IdentityServiceCollectionExtensions
    {
        // Identity area pages disabled (404) by default — see DisableIdentityPagesConvention.
        // Overridable via the "Identity:DisabledPages" configuration array.
        private static readonly string[] DefaultDisabledIdentityPages =
        {
            "/Account/Register",
            "/Account/RegisterConfirmation",
            "/Account/LoginWith2fa",
            "/Account/LoginWithRecoveryCode",
            "/Account/Manage/PersonalData",
            "/Account/Manage/DeletePersonalData",
            "/Account/Manage/DownloadPersonalData",
            "/Account/Manage/TwoFactorAuthentication",
            "/Account/Manage/EnableAuthenticator",
            "/Account/Manage/ResetAuthenticator",
            "/Account/Manage/GenerateRecoveryCodes",
            "/Account/Manage/ShowRecoveryCodes",
            "/Account/Manage/Disable2fa",
        };

        public static IServiceCollection AddAllorsIdentity(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
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

            // Authentication is the ASP.NET Core Identity application cookie, configured as the default
            // scheme by AddDefaultIdentity above. It is the browser session of this plug-in: Core
            // applies the rules of the session to it. Identity:Cookie:ExpireTimeSpan sets its
            // lifetime, in place of Core's default.
            if (TimeSpan.TryParse(configuration["Identity:Cookie:ExpireTimeSpan"], out var expireTimeSpan))
            {
                services.ConfigureApplicationCookie(cookieOptions => cookieOptions.ExpireTimeSpan = expireTimeSpan);
            }

            // Revocation lever: the built-in SecurityStampValidator re-checks the persisted security
            // stamp on this interval, so a rotated stamp (disable / "log out everywhere") invalidates
            // live cookies within ~5 minutes.
            services.Configure<SecurityStampValidatorOptions>(securityStampValidatorOptions =>
                // Development (and the test rigs) revalidate the stamp every request, so disabling a
                // user or rotating the stamp takes effect immediately; production uses 5 minutes.
                securityStampValidatorOptions.ValidationInterval = environment.IsDevelopment() ? TimeSpan.Zero : TimeSpan.FromMinutes(5));

            var disabledIdentityPages = configuration.GetSection("Identity:DisabledPages").Get<string[]>() ?? DefaultDisabledIdentityPages;
            services.AddRazorPages(razorPagesOptions =>
                razorPagesOptions.Conventions.Add(new DisableIdentityPagesConvention(disabledIdentityPages)));

            // How Identity connects to Core: it tells Core who the signed-in user is, and names its
            // application cookie as the session, so that Core hardens that cookie, answers the Allors
            // API with a status code and protects it against the cookie with antiforgery.
            services.AddSingleton<IUserResolver, IdentityUserResolver>();
            services.Configure<AllorsAuthenticationOptions>(authenticationOptions =>
                authenticationOptions.SessionScheme = IdentityConstants.ApplicationScheme);

            return services;
        }
    }
}
