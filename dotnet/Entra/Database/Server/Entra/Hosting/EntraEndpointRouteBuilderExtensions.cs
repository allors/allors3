// <copyright file="EntraEndpointRouteBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class EntraEndpointRouteBuilderExtensions
    {
        // The two endpoints a browser application needs: a sign-in to send the browser to, and a
        // sign-out to post to. Both require an authenticated user, so neither opts out of the
        // application's authorization: a browser without a session that asks to sign in is challenged
        // by Core's session rules, which send it to Entra and back here.
        public static IEndpointRouteBuilder MapAllorsEntra(this IEndpointRouteBuilder endpoints, string openIdConnectScheme = EntraDefaults.OpenIdConnectScheme)
        {
            // Signed in, the browser goes on to the local returnUrl, and nowhere else: an open redirect
            // would let a sign-in link send a person to another site.
            endpoints.MapGet(EntraPaths.SignIn, (string returnUrl) =>
                {
                    returnUrl ??= "/";
                    return IsLocal(returnUrl)
                        ? Results.LocalRedirect(returnUrl)
                        : Results.BadRequest($"returnUrl must be a local path, such as /, and not '{returnUrl}'.");
                })
                .RequireAuthorization();

            // Core's antiforgery protects the Allors API against the session cookie; the sign-out is
            // outside the API and protects itself the same way, so a page of another site cannot sign
            // a person out. The session ends, and so does the sign-in with Entra.
            endpoints.MapPost(EntraPaths.SignOut, async (HttpContext context) =>
                {
                    try
                    {
                        await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException e)
                    {
                        return Results.BadRequest($"The sign-out request carries no valid antiforgery token: {e.Message}");
                    }

                    var sessionScheme = context.RequestServices.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionScheme;
                    return Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, new[] { sessionScheme, openIdConnectScheme });
                })
                .RequireAuthorization();

            return endpoints;
        }

        // A path of this site: it starts with one slash, so it is neither absolute nor protocol-relative.
        private static bool IsLocal(string url) =>
            url.Length > 0 && url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
    }
}
