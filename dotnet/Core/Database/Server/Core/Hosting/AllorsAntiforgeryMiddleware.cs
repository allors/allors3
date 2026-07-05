// <copyright file="AllorsAntiforgeryMiddleware.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;

    // Antiforgery for the JSON API, scoped to browser (cookie) callers only. Safe /allors responses
    // hand out a readable XSRF-TOKEN cookie; unsafe /allors requests are validated ONLY when the
    // caller authenticated via the Identity application cookie. Bearer, test-header and future
    // API-key clients carry a different authentication type and are therefore exempt by construction,
    // which is what lets this ship during the dual-scheme window without touching non-browser clients.
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

        public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
        {
            if (context.Request.Path.StartsWithSegments("/allors"))
            {
                var method = context.Request.Method;
                if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
                {
                    // Issue the readable token cookie once; keep cacheable responses Set-Cookie-free thereafter.
                    if (!context.Request.Cookies.ContainsKey(XsrfCookieName))
                    {
                        var tokens = antiforgery.GetAndStoreTokens(context);
                        context.Response.Cookies.Append(XsrfCookieName, tokens.RequestToken, new CookieOptions
                        {
                            HttpOnly = false,
                            SameSite = SameSiteMode.Lax,
                            Secure = this.secureCookie,
                            Path = "/",
                        });
                    }
                }
                else if (AuthenticatedViaApplicationCookie(context.User))
                {
                    try
                    {
                        await antiforgery.ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { error = "antiforgery" });
                        return;
                    }
                }
            }

            await this.next(context);
        }

        private static bool AuthenticatedViaApplicationCookie(ClaimsPrincipal user) =>
            user?.Identities.Any(identity => identity.IsAuthenticated && identity.AuthenticationType == IdentityConstants.ApplicationScheme) == true;
    }
}
