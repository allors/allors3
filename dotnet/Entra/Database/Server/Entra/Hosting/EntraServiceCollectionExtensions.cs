// <copyright file="EntraServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;

    // Authentication with Microsoft Entra ID: a browser signs in with OpenID Connect and keeps a cookie
    // session, a program or a client calls the Allors API with an access token of the tenant. The
    // protocol and the token rules are Microsoft.Identity.Web's; the plug-in names its schemes to Core,
    // which selects between session and token per request and owns the session, and connects every
    // validated principal to an Allors user.
    public static class EntraServiceCollectionExtensions
    {
        // Registers Microsoft.Identity.Web from the Entra section with the plug-in's scheme names and
        // defaults, and the plug-in's users. An application that registers Microsoft.Identity.Web
        // itself calls AddAllorsEntraUsers with its scheme names instead.
        public static IServiceCollection AddAllorsEntra(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var section = configuration.GetSection(EntraDefaults.ConfigurationSection);
            RequireTenant(section);

            // The authorization code flow with PKCE, as RFC 9700 and Microsoft recommend; the public
            // cloud unless the section names another instance. The section may override either.
            services.AddAuthentication(AllorsAuthenticationDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(
                    options =>
                    {
                        options.Instance = EntraDefaults.Instance;
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        section.Bind(options);
                    },
                    null,
                    EntraDefaults.OpenIdConnectScheme,
                    EntraDefaults.SessionScheme);

            services.AddAuthentication()
                .AddMicrosoftIdentityWebApi(
                    _ => { },
                    options =>
                    {
                        options.Instance = EntraDefaults.Instance;
                        section.Bind(options);
                    },
                    EntraDefaults.BearerScheme);

            // The code comes back as a query of a GET, so the correlation and nonce cookies need not be
            // sent cross-site and can be Lax; Development runs over plain http, as Core's session does.
            var securePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            services.PostConfigure<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme, options =>
            {
                options.ResponseMode = OpenIdConnectResponseMode.Query;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = securePolicy;
                options.NonceCookie.SameSite = SameSiteMode.Lax;
                options.NonceCookie.SecurePolicy = securePolicy;
            });

            // How Entra connects to Core: the names of its schemes, and a lifetime of the session, as
            // Entra cannot end the application's session.
            services.Configure<AllorsAuthenticationOptions>(options =>
            {
                options.SessionScheme = EntraDefaults.SessionScheme;
                options.BearerScheme = EntraDefaults.BearerScheme;
                options.ChallengeScheme = EntraDefaults.OpenIdConnectScheme;
                options.SessionLifetime = TimeSpan.TryParse(section["SessionLifetime"], CultureInfo.InvariantCulture, out var lifetime)
                    ? lifetime
                    : EntraDefaults.SessionLifetime;
            });

            return services.AddAllorsEntraUsers(EntraDefaults.OpenIdConnectScheme, EntraDefaults.BearerScheme);
        }

        // Connects the principals of the named schemes to Allors users: a validated token admits its
        // principal, which finds or creates the user, and the resolver tells Core which user a request
        // is for. Either scheme may be null when the application has no such scheme.
        public static IServiceCollection AddAllorsEntraUsers(this IServiceCollection services, string openIdConnectScheme, string bearerScheme)
        {
            services.TryAddSingleton<EntraAdmission>();
            services.AddSingleton<IUserResolver, EntraUserResolver>();

            if (openIdConnectScheme != null)
            {
                services.PostConfigure<OpenIdConnectOptions>(openIdConnectScheme, options =>
                {
                    // After the handlers of the application and of Microsoft.Identity.Web: the sign-in
                    // admits the principal, and the session keeps only its Entra identity.
                    var tokenValidated = options.Events.OnTokenValidated;
                    options.Events.OnTokenValidated = async context =>
                    {
                        await tokenValidated(context);
                        if (context.Result?.Failure != null)
                        {
                            return;
                        }

                        var reason = context.HttpContext.RequestServices.GetRequiredService<EntraAdmission>().Admit(context.Principal, signIn: true);
                        if (reason != null)
                        {
                            context.Fail(new EntraAdmission.NotAdmittedException(reason));
                            return;
                        }

                        context.Principal = EntraAdmission.SessionPrincipal(context.Principal);
                    };

                    // A browser whose account is not admitted gets an answer, not an exception page,
                    // unless the application answered already.
                    var remoteFailure = options.Events.OnRemoteFailure;
                    options.Events.OnRemoteFailure = async context =>
                    {
                        await remoteFailure(context);
                        if (context.Result?.Handled == true || context.Failure is not EntraAdmission.NotAdmittedException)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        await context.Response.WriteAsync("The application does not admit this account.");
                        context.HandleResponse();
                    };
                });
            }

            if (bearerScheme != null)
            {
                services.PostConfigure<JwtBearerOptions>(bearerScheme, options =>
                {
                    // Core's API reads the user name from the identity's name; an access token names
                    // the person in preferred_username. An application that chose another claim keeps it.
                    if (options.TokenValidationParameters.NameClaimType == ClaimsIdentity.DefaultNameClaimType)
                    {
                        options.TokenValidationParameters.NameClaimType = EntraClaims.PreferredUserNameClaim;
                    }

                    options.Events ??= new JwtBearerEvents();
                    var tokenValidated = options.Events.OnTokenValidated;
                    options.Events.OnTokenValidated = async context =>
                    {
                        await tokenValidated(context);
                        if (context.Result?.Failure != null)
                        {
                            return;
                        }

                        var reason = context.HttpContext.RequestServices.GetRequiredService<EntraAdmission>().Admit(context.Principal, signIn: false);
                        if (reason != null)
                        {
                            context.Fail(reason);
                        }
                    };
                });
            }

            return services;
        }

        // The settings without which no sign-in can work, checked at start-up rather than at the first
        // request. One tenant: the plug-in signs in the members and guests of the application's own
        // tenant, so the tenant id is a GUID, not common, organizations or consumers.
        private static void RequireTenant(IConfigurationSection section)
        {
            var tenantId = section["TenantId"];
            if (!Guid.TryParse(tenantId, out var tenant) || tenant == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"{section.Path}:TenantId is '{tenantId}', but the Entra plug-in needs the id of the application's tenant, a GUID. " +
                    "Set it to the Directory (tenant) ID of the app registration; 'common', 'organizations' and 'consumers' are not accepted, " +
                    "as the plug-in signs in the members and guests of one tenant.");
            }

            if (string.IsNullOrWhiteSpace(section["ClientId"]))
            {
                throw new InvalidOperationException(
                    $"{section.Path}:ClientId is not set. Set it to the Application (client) ID of the app registration.");
            }

            if (string.IsNullOrWhiteSpace(section["ClientSecret"]) && !section.GetSection("ClientCredentials").GetChildren().Any())
            {
                throw new InvalidOperationException(
                    $"{section.Path}:ClientSecret is not set. The authorization code flow redeems its code with a client credential: " +
                    $"set {section.Path}:ClientSecret to a client secret of the app registration, or {section.Path}:ClientCredentials " +
                    "to a certificate as Microsoft.Identity.Web documents.");
            }
        }
    }
}
