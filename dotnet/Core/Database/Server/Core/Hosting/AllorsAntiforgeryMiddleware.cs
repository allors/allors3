// <copyright file="AllorsAntiforgeryMiddleware.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    // Antiforgery for the JSON API, scoped to browser (cookie) callers only. Safe /allors responses
    // hand out a readable XSRF-TOKEN cookie; unsafe /allors requests are validated ONLY when the
    // session scheme authenticated the caller, the cookie scheme an authentication plug-in names in
    // AllorsAuthenticationOptions. Bearer, test-header and API-key clients are authenticated by
    // another scheme and are therefore exempt by construction. The scheme decides, not the type of
    // the identity: a session that signed in with OpenID Connect and a bearer token carry the same
    // identity type.
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

        public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery, IOptions<AllorsAuthenticationOptions> options)
        {
            if (context.Request.Path.StartsWithSegments("/allors"))
            {
                var method = context.Request.Method;
                if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
                {
                    // Issue the readable token cookie once; keep cacheable responses Set-Cookie-free
                    // thereafter. Request tokens are bound to the authenticated identity, so sign-in and
                    // sign-out delete the cookie (AllorsSessionCookie wires that into the events of the
                    // session scheme) and the next safe GET re-mints it here.
                    if (!context.Request.Cookies.ContainsKey(XsrfCookieName))
                    {
                        var tokens = antiforgery.GetAndStoreTokens(context);
                        context.Response.Cookies.Append(XsrfCookieName, tokens.RequestToken, CookieOptionsFor(this.secureCookie));
                    }
                }
                else if (AuthenticatedBySession(context, options.Value.SessionScheme))
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

        // The authentication middleware records which scheme authenticated the request.
        private static bool AuthenticatedBySession(HttpContext context, string sessionScheme) =>
            sessionScheme != null &&
            string.Equals(context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Ticket?.AuthenticationScheme, sessionScheme, StringComparison.Ordinal);
    }
}
