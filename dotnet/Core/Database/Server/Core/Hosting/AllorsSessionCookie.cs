// <copyright file="AllorsSessionCookie.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    // The rules of the browser session, for the cookie scheme a plug-in names as its session in
    // AllorsAuthenticationOptions. Core owns them, so every plug-in gets the same session.
    //
    // The values of the cookie are defaults, set when the options are configured: what the plug-in
    // or the application configures afterwards wins. The rules on the events of the cookie are set
    // after all configuration, and wrap the events that are there. A plug-in registers its cookie
    // after Core and may replace the events, as ASP.NET Core Identity does for its security stamp
    // validator; wrapping keeps both. Two rules apply only when a plug-in or the application asks
    // for them: the challenge of a named scheme, and an absolute lifetime of the session.
    public class AllorsSessionCookie : IConfigureNamedOptions<CookieAuthenticationOptions>, IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IOptions<AllorsAuthenticationOptions> authenticationOptions;
        private readonly IWebHostEnvironment environment;
        private readonly IAllorsSessionValidator[] validators;

        public AllorsSessionCookie(IOptions<AllorsAuthenticationOptions> authenticationOptions, IWebHostEnvironment environment)
            : this(authenticationOptions, environment, Array.Empty<IAllorsSessionValidator>())
        {
        }

        internal AllorsSessionCookie(IOptions<AllorsAuthenticationOptions> authenticationOptions, IWebHostEnvironment environment, IAllorsSessionValidator[] validators)
        {
            this.authenticationOptions = authenticationOptions;
            this.environment = environment;
            this.validators = validators;
        }

        public void Configure(CookieAuthenticationOptions options) => this.Configure(Options.DefaultName, options);

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (!this.IsSession(name))
            {
                return;
            }

            var development = this.environment.IsDevelopment();

            options.Cookie.Name = development ? "Allors.Auth" : "__Host-Allors.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;

            // Development runs over plain http, and a cookie container refuses a Secure cookie
            // there; production is https at the edge.
            options.Cookie.SecurePolicy = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        }

        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            if (!this.IsSession(name))
            {
                return;
            }

            if (options.EventsType != null)
            {
                throw new InvalidOperationException(
                    $"The session scheme '{name}' takes its cookie events from {nameof(CookieAuthenticationOptions.EventsType)} ({options.EventsType.Name}), " +
                    "so the rules Core sets on the events of the session would not run. " +
                    $"Set the events on {nameof(CookieAuthenticationOptions)}.{nameof(CookieAuthenticationOptions.Events)} instead.");
            }

            var authentication = this.authenticationOptions.Value;

            // A caller of the Allors API gets a status code, not a redirect to a sign-in page. Outside
            // the API a plug-in that signs in elsewhere, with OpenID Connect for instance, names the
            // scheme to challenge; without one the cookie redirects to its own sign-in page.
            var challengeScheme = authentication.ChallengeScheme;
            var challenge = options.Events.OnRedirectToLogin;
            options.Events.OnRedirectToLogin = context =>
                IsApi(context) ? Answer(context, StatusCodes.Status401Unauthorized)
                : challengeScheme != null ? context.HttpContext.ChallengeAsync(challengeScheme, context.Properties)
                : challenge(context);

            var forbid = options.Events.OnRedirectToAccessDenied;
            options.Events.OnRedirectToAccessDenied = context =>
                IsApi(context) ? Answer(context, StatusCodes.Status403Forbidden) : forbid(context);

            // An antiforgery token minted for one identity does not validate for the next: drop the
            // token cookie on sign-in and sign-out, so that the next safe request to the API issues
            // one that is bound to the new authentication state.
            var secureAntiforgeryCookie = !this.environment.IsDevelopment();

            var signedIn = options.Events.OnSignedIn;
            options.Events.OnSignedIn = context =>
            {
                AllorsAntiforgeryMiddleware.DeleteCookie(context.HttpContext, secureAntiforgeryCookie);
                return signedIn(context);
            };

            var signingOut = options.Events.OnSigningOut;
            options.Events.OnSigningOut = context =>
            {
                AllorsAntiforgeryMiddleware.DeleteCookie(context.HttpContext, secureAntiforgeryCookie);
                return signingOut(context);
            };

            // A session may get an absolute lifetime next to its sliding one: the start is stamped at
            // sign-in and travels with the ticket, which a renewal copies, and a cookie past the
            // lifetime is refused and signed out. Stamp sign-in only when a lifetime is set.
            var lifetime = authentication.SessionLifetime;
            if (lifetime != null)
            {
                var signingIn = options.Events.OnSigningIn;
                options.Events.OnSigningIn = context =>
                {
                    context.Properties.Items[SessionStartKey] = Now(context.Options).ToString("o", CultureInfo.InvariantCulture);
                    return signingIn(context);
                };
            }

            // Plug-in checks run on the principal left by the application's callback. Keep them
            // inside this protected wrapper even when the session has no absolute lifetime.
            var validateSession = lifetime != null || this.validators.Length != 0;
            if (validateSession)
            {
                var validatePrincipal = options.Events.OnValidatePrincipal;
                options.Events.OnValidatePrincipal = async context =>
                {
                    if (lifetime != null && (!context.Properties.Items.TryGetValue(SessionStartKey, out var value) ||
                        !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var start) ||
                        Now(context.Options) - start > lifetime.Value))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(context.Scheme.Name);
                        return;
                    }

                    await validatePrincipal(context);
                    foreach (var validator in this.validators)
                    {
                        if (context.Principal == null)
                        {
                            return;
                        }

                        await validator.ValidateAsync(context);
                    }
                };
            }

            AllorsSessionCookieValidation.Capture(options, lifetime != null, validateSession);
        }

        // The sign-in time of a session, in the authentication properties of its ticket.
        public const string SessionStartKey = ".allors.session.start";

        // The cookie handler's clock, which a test can set.
        private static DateTimeOffset Now(CookieAuthenticationOptions options) => (options.TimeProvider ?? TimeProvider.System).GetUtcNow();

        private static bool IsApi(RedirectContext<CookieAuthenticationOptions> context) =>
            context.Request.Path.StartsWithSegments("/allors");

        private static Task Answer(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        }

        private bool IsSession(string name)
        {
            var sessionScheme = this.authenticationOptions.Value.SessionScheme;
            return sessionScheme != null && string.Equals(name, sessionScheme, StringComparison.Ordinal);
        }
    }
}
