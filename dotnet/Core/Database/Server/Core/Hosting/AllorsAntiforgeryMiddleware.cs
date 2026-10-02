// <copyright file="AllorsAntiforgeryMiddleware.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    // Antiforgery for the JSON API, scoped to browser (cookie) callers only. Safe /allors responses
    // hand out a readable XSRF-TOKEN cookie; unsafe /allors requests are validated ONLY when the
    // caller authenticated with a scheme that an authentication plug-in listed as a cookie scheme
    // (AllorsAntiforgeryOptions). Bearer, test-header and API-key clients carry a different
    // authentication type and are therefore exempt by construction.
    public class AllorsAntiforgeryMiddleware
    {
        public const string XsrfCookieName = "XSRF-TOKEN";

        private readonly RequestDelegate next;
        private readonly bool secureCookie;

        public AllorsAntiforgeryMiddleware(RequestDelegate next, bool secureCookie)
        {
            this.next = next;
            this.secureCookie = secureCookie;
        }

        public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery, IOptions<AllorsAntiforgeryOptions> options)
        {
            if (context.Request.Path.StartsWithSegments("/allors"))
            {
                var method = context.Request.Method;
                if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
                {
                    // Issue the readable token cookie once; keep cacheable responses Set-Cookie-free
                    // thereafter. Request tokens are bound to the authenticated identity, so sign-in and
                    // sign-out delete the cookie (a cookie plug-in wires that into its cookie events)
                    // and the next safe GET re-mints it here.
                    if (!context.Request.Cookies.ContainsKey(XsrfCookieName))
                    {
                        var tokens = antiforgery.GetAndStoreTokens(context);
                        context.Response.Cookies.Append(XsrfCookieName, tokens.RequestToken, CookieOptionsFor(this.secureCookie));
                    }
                }
                else if (AuthenticatedWithCookie(context.User, options.Value.AuthenticationTypes))
                {
                    try
                    {
                        await antiforgery.ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException)
                    {
                        // Drop the failing token so the next safe GET re-issues a valid pair — self-heals
                        // an identity-binding mismatch or a token that no longer decrypts (key loss).
                        DeleteCookie(context, this.secureCookie);
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { error = "antiforgery" });
                        return;
                    }
                }
            }

            await this.next(context);
        }

        public static void DeleteCookie(HttpContext context, bool secureCookie) =>
            context.Response.Cookies.Delete(XsrfCookieName, CookieOptionsFor(secureCookie));

        // Mint and delete must agree on these attributes, or the delete misses the cookie.
        private static CookieOptions CookieOptionsFor(bool secureCookie) => new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Secure = secureCookie,
            Path = "/",
        };

        private static bool AuthenticatedWithCookie(ClaimsPrincipal user, ISet<string> cookieAuthenticationTypes) =>
            user?.Identities.Any(identity => identity.IsAuthenticated && cookieAuthenticationTypes.Contains(identity.AuthenticationType)) == true;
    }
}
