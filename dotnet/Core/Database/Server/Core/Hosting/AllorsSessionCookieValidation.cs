// <copyright file="AllorsSessionCookieValidation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Extensions.Options;

    // Validation runs after every PostConfigure, including application registrations made after
    // Core. Keep the exact handlers Core installed so a later replacement cannot silently remove
    // the session rules. Weak keys allow replaced options instances to be collected on reload.
    internal sealed class AllorsSessionCookieValidation : IValidateOptions<CookieAuthenticationOptions>
    {
        private static readonly ConditionalWeakTable<CookieAuthenticationOptions, RequiredEvents> CapturedEvents = new();
        private readonly IOptions<AllorsAuthenticationOptions> authenticationOptions;

        public AllorsSessionCookieValidation(IOptions<AllorsAuthenticationOptions> authenticationOptions) =>
            this.authenticationOptions = authenticationOptions;

        internal static void Capture(CookieAuthenticationOptions options, bool lifetime, bool validateSession) =>
            CapturedEvents.AddOrUpdate(options, new RequiredEvents(options.Events, lifetime, validateSession));

        public ValidateOptionsResult Validate(string name, CookieAuthenticationOptions options)
        {
            if (!string.Equals(name, this.authenticationOptions.Value.SessionScheme, StringComparison.Ordinal))
            {
                return ValidateOptionsResult.Skip;
            }

            if (options.EventsType != null)
            {
                return Fail(name, nameof(CookieAuthenticationOptions.EventsType));
            }

            if (!CapturedEvents.TryGetValue(options, out var required) || !ReferenceEquals(options.Events, required.Events))
            {
                return Fail(name, nameof(CookieAuthenticationOptions.Events));
            }

            var overridden = OverriddenHandlers(options.Events.GetType(), required.Lifetime, required.ValidateSession);
            if (overridden.Count != 0)
            {
                return ValidateOptionsResult.Fail(
                    $"The session scheme '{name}' overrides {string.Join(", ", overridden)} on {nameof(CookieAuthenticationEvents)}, " +
                    "so Core cannot ensure its session rules run. Configure the On... callbacks with services.Configure instead of overriding these methods.");
            }

            var changed = new List<string>();
            if (!ReferenceEquals(options.Events.OnRedirectToLogin, required.RedirectToLogin))
            {
                changed.Add(nameof(CookieAuthenticationEvents.OnRedirectToLogin));
            }

            if (!ReferenceEquals(options.Events.OnRedirectToAccessDenied, required.RedirectToAccessDenied))
            {
                changed.Add(nameof(CookieAuthenticationEvents.OnRedirectToAccessDenied));
            }

            if (!ReferenceEquals(options.Events.OnSignedIn, required.SignedIn))
            {
                changed.Add(nameof(CookieAuthenticationEvents.OnSignedIn));
            }

            if (!ReferenceEquals(options.Events.OnSigningOut, required.SigningOut))
            {
                changed.Add(nameof(CookieAuthenticationEvents.OnSigningOut));
            }

            if (required.Lifetime && !ReferenceEquals(options.Events.OnSigningIn, required.SigningIn))
            {
                changed.Add(nameof(CookieAuthenticationEvents.OnSigningIn));
            }

            if (required.ValidateSession && !ReferenceEquals(options.Events.OnValidatePrincipal, required.ValidatePrincipal))
            {
                changed.Add(nameof(CookieAuthenticationEvents.OnValidatePrincipal));
            }

            return changed.Count == 0 ? ValidateOptionsResult.Success : Fail(name, string.Join(", ", changed));
        }

        private static ValidateOptionsResult Fail(string name, string members) =>
            ValidateOptionsResult.Fail(
                $"The session scheme '{name}' replaces {members} after Core installs its session rules, so those rules would not run. " +
                $"Leave {nameof(CookieAuthenticationOptions.EventsType)} unset and customize {nameof(CookieAuthenticationOptions)}.{nameof(CookieAuthenticationOptions.Events)} " +
                "with services.Configure instead of replacing the session handlers in PostConfigure.");

        private static HashSet<string> OverriddenHandlers(Type eventsType, bool lifetime, bool validateSession)
        {
            var overridden = new HashSet<string>(StringComparer.Ordinal);
            for (var type = eventsType; type != typeof(CookieAuthenticationEvents); type = type.BaseType)
            {
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var definition = method.GetBaseDefinition();
                    if (definition.DeclaringType == typeof(CookieAuthenticationEvents) &&
                        (definition.Name is nameof(CookieAuthenticationEvents.RedirectToLogin) or nameof(CookieAuthenticationEvents.RedirectToAccessDenied) or
                             nameof(CookieAuthenticationEvents.SignedIn) or nameof(CookieAuthenticationEvents.SigningOut) ||
                         (lifetime && definition.Name == nameof(CookieAuthenticationEvents.SigningIn)) ||
                         (validateSession && definition.Name == nameof(CookieAuthenticationEvents.ValidatePrincipal))))
                    {
                        overridden.Add(definition.Name);
                    }
                }
            }

            return overridden;
        }

        private sealed class RequiredEvents
        {
            internal RequiredEvents(CookieAuthenticationEvents events, bool lifetime, bool validateSession)
            {
                this.Events = events;
                this.Lifetime = lifetime;
                this.ValidateSession = validateSession;
                this.RedirectToLogin = events.OnRedirectToLogin;
                this.RedirectToAccessDenied = events.OnRedirectToAccessDenied;
                this.SignedIn = events.OnSignedIn;
                this.SigningOut = events.OnSigningOut;
                this.SigningIn = events.OnSigningIn;
                this.ValidatePrincipal = events.OnValidatePrincipal;
            }

            internal CookieAuthenticationEvents Events { get; }

            internal bool Lifetime { get; }

            internal bool ValidateSession { get; }

            internal Func<RedirectContext<CookieAuthenticationOptions>, Task> RedirectToLogin { get; }

            internal Func<RedirectContext<CookieAuthenticationOptions>, Task> RedirectToAccessDenied { get; }

            internal Func<CookieSignedInContext, Task> SignedIn { get; }

            internal Func<CookieSigningOutContext, Task> SigningOut { get; }

            internal Func<CookieSigningInContext, Task> SigningIn { get; }

            internal Func<CookieValidatePrincipalContext, Task> ValidatePrincipal { get; }
        }
    }
}
