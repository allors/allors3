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
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class EntraEndpointRouteBuilderExtensions
    {
        // The two endpoints a browser application needs: a sign-in to send the browser to, and a
        // sign-out to post to. Sign-in requires authentication: Core's session rules send a browser
        // without a session to Entra and back here. Sign-out must never start a new sign-in.
        public static IEndpointRouteBuilder MapAllorsEntra(this IEndpointRouteBuilder endpoints, string openIdConnectScheme = EntraDefaults.OpenIdConnectScheme)
        {
            // Signed in, the browser goes on to the local returnUrl, and nowhere else: an open redirect
            // would let a sign-in link send a person to another site.
            endpoints.MapGet(EntraPaths.SignIn, (string returnUrl) =>
                {
                    returnUrl ??= "/";
                    return RedirectHttpResult.IsLocalUrl(returnUrl)
                        ? Results.LocalRedirect(returnUrl)
                        : Results.BadRequest($"returnUrl must be a local path, such as /, and not '{returnUrl}'.");
                })
                .RequireAuthorization();

            // Core's antiforgery protects the Allors API against the session cookie; the sign-out is
            // outside the API and protects itself the same way, so a page of another site cannot sign
            // a person out. The session ends, and so does the sign-in with Entra.
            endpoints.MapPost(EntraPaths.SignOut, async (HttpContext context) =>
                {
                    // The default identity may come from a bearer token. Only a valid browser
                    // session needs to end; an absent or expired one is already signed out locally.
                    var sessionScheme = context.RequestServices.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionScheme;
                    var session = await context.AuthenticateAsync(sessionScheme);
                    if (!session.Succeeded)
                    {
                        return Results.NoContent();
                    }

                    // The antiforgery token belongs to the cookie's user, even when the request
                    // also carried a bearer token that authenticated a different principal.
                    context.User = session.Principal;
                    try
                    {
                        await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException e)
                    {
                        return Results.BadRequest($"The sign-out request carries no valid antiforgery token: {e.Message}");
                    }

                    return Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, new[] { sessionScheme, openIdConnectScheme });
                })
                .AllowAnonymous();

            return endpoints;
        }
    }
}
