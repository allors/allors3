// <copyright file="FakeEntraEndpoints.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;

    // The endpoints of the fake, on Microsoft's paths under /fake-entra/{tenant}: the two discovery
    // documents and key sets, the authorize endpoint with a page to pick an account, the token endpoint
    // with the authorization code, client credentials and password grants, and the end-session endpoint.
    // The token endpoint takes a few x_ fields that no real client sends, with which a test asks for a
    // token Microsoft would never issue.
    public static class FakeEntraEndpoints
    {
        public static IEndpointRouteBuilder MapFakeEntra(this IEndpointRouteBuilder endpoints)
        {
            var tenant = endpoints.MapGroup(FakeEntra.PathPrefix + "/{tenant:guid}").AllowAnonymous();

            tenant.MapGet("/v2.0/.well-known/openid-configuration", (Guid tenant, HttpRequest request, FakeEntra fake) => Results.Json(fake.MetadataV2(tenant, Origin(request))));
            tenant.MapGet("/.well-known/openid-configuration", (Guid tenant, HttpRequest request, FakeEntra fake) => Results.Json(fake.MetadataV1(tenant, Origin(request))));
            tenant.MapGet("/discovery/v2.0/keys", (FakeEntra fake) => Results.Json(fake.KeysV2()));
            tenant.MapGet("/discovery/keys", (FakeEntra fake) => Results.Json(fake.KeysV1()));
            tenant.MapGet("/oauth2/v2.0/authorize", Authorize);
            tenant.MapPost("/oauth2/v2.0/token", Token);
            tenant.MapGet("/oauth2/v2.0/logout", Logout);

            return endpoints;
        }

        // Microsoft's sign-in page, reduced to picking an account: a link per account, or the account
        // query parameter a test adds itself. The code goes back to the client as it asked, in the
        // query or in a posted form.
        private static IResult Authorize(Guid tenant, HttpRequest request, FakeEntra fake)
        {
            var query = request.Query;
            var clientId = query["client_id"].ToString();
            var redirectUri = query["redirect_uri"].ToString();

            if (!string.Equals(clientId, fake.ClientId, StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest($"AADSTS700016: Application with identifier '{clientId}' was not found in the directory '{tenant}'.");
            }

            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var redirect) || !redirect.IsLoopback)
            {
                return Results.BadRequest($"AADSTS50011: The redirect URI '{redirectUri}' specified in the request does not match the redirect URIs configured for the application '{clientId}'.");
            }

            if (!query["response_type"].ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("code"))
            {
                return Results.BadRequest($"AADSTS700054: response_type '{query["response_type"]}' is not enabled for the application; the fake issues codes only.");
            }

            var accountId = query["account"].ToString();
            if (accountId.Length == 0)
            {
                return Results.Content(PickerPage(request), "text/html; charset=utf-8");
            }

            var account = FakeEntraAccounts.ById(accountId);
            if (account == null || account.IsProgram)
            {
                return Results.BadRequest($"AADSTS50034: The account '{accountId}' does not exist in the directory '{tenant}'.");
            }

            var code = fake.NewCode(account, clientId, redirectUri, query["code_challenge"], query["nonce"], query["scope"]);
            var state = query["state"].ToString();

            if (query["response_mode"] == "form_post")
            {
                return Results.Content(
                    $"<!doctype html><html><body onload=\"document.forms[0].submit()\"><form method=\"post\" action=\"{WebUtility.HtmlEncode(redirectUri)}\">" +
                    $"<input type=\"hidden\" name=\"code\" value=\"{WebUtility.HtmlEncode(code)}\"/><input type=\"hidden\" name=\"state\" value=\"{WebUtility.HtmlEncode(state)}\"/>" +
                    "<noscript><button>Continue</button></noscript></form></body></html>",
                    "text/html; charset=utf-8");
            }

            return Results.Redirect(QueryHelpers.AddQueryString(redirectUri, new Dictionary<string, string> { ["code"] = code, ["state"] = state }));
        }

        private static async Task<IResult> Token(Guid tenant, HttpRequest request, FakeEntra fake)
        {
            var form = await request.ReadFormAsync();
            var grant = form["grant_type"].ToString();
            var knobs = Knobs(form);
            var clientId = form["client_id"].ToString();

            switch (grant)
            {
                case "authorization_code":
                {
                    if (!string.Equals(clientId, fake.ClientId, StringComparison.OrdinalIgnoreCase) || form["client_secret"] != fake.ClientSecret)
                    {
                        return Error(StatusCodes.Status401Unauthorized, "invalid_client", "AADSTS7000215: Invalid client secret provided.");
                    }

                    var issued = fake.Redeem(form["code"], clientId, form["redirect_uri"], form["code_verifier"], out var error);
                    if (issued == null)
                    {
                        return Error(StatusCodes.Status400BadRequest, "invalid_grant", error);
                    }

                    var scopes = ApiScopes(issued.Scope, fake);
                    return Results.Json(new
                    {
                        token_type = "Bearer",
                        scope = string.Join(' ', scopes),
                        expires_in = 3599,
                        ext_expires_in = 3599,
                        access_token = fake.AccessToken(tenant, issued.Account, clientId, scopes, knobs),
                        id_token = fake.IdToken(tenant, issued.Account, clientId, issued.Nonce, knobs),
                    });
                }

                case "client_credentials":
                {
                    var program = FakeEntraAccounts.ByClientId(clientId);
                    if (program == null || form["client_secret"] != program.ClientSecret)
                    {
                        return Error(StatusCodes.Status401Unauthorized, "invalid_client", "AADSTS7000215: Invalid client secret provided.");
                    }

                    return Results.Json(new
                    {
                        token_type = "Bearer",
                        expires_in = 3599,
                        ext_expires_in = 3599,
                        access_token = fake.AccessToken(tenant, program, program.ClientId, null, knobs),
                    });
                }

                case "password":
                {
                    if (!string.Equals(clientId, fake.ClientId, StringComparison.OrdinalIgnoreCase))
                    {
                        return Error(StatusCodes.Status400BadRequest, "unauthorized_client", $"AADSTS700016: Application with identifier '{clientId}' was not found in the directory '{tenant}'.");
                    }

                    var account = FakeEntraAccounts.ByUserName(form["username"]);
                    if (account == null || form["password"] != account.Password)
                    {
                        return Error(StatusCodes.Status400BadRequest, "invalid_grant", "AADSTS50126: Error validating credentials due to invalid username or password.");
                    }

                    var scopes = ApiScopes(form["scope"], fake);
                    return Results.Json(new
                    {
                        token_type = "Bearer",
                        scope = string.Join(' ', scopes),
                        expires_in = 3599,
                        ext_expires_in = 3599,
                        access_token = fake.AccessToken(tenant, account, clientId, scopes, knobs),
                        id_token = fake.IdToken(tenant, account, clientId, null, knobs),
                    });
                }

                default:
                    return Error(StatusCodes.Status400BadRequest, "unsupported_grant_type", $"AADSTS70003: The app requested an unsupported grant type '{grant}'.");
            }
        }

        private static IResult Logout(HttpRequest request)
        {
            var back = request.Query["post_logout_redirect_uri"].ToString();
            return Uri.TryCreate(back, UriKind.Absolute, out var uri) && uri.IsLoopback
                ? Results.Redirect(back)
                : Results.Content("<!doctype html><html><body><h1>Fake Entra ID</h1><p>You signed out.</p></body></html>", "text/html; charset=utf-8");
        }

        // The scopes of the application's API among the requested ones, without the resource prefix,
        // as Entra puts them in scp: api://{client}/access_as_user becomes access_as_user, and .default
        // stands for every scope the application exposes.
        private static string[] ApiScopes(string requested, FakeEntra fake)
        {
            var prefixes = new[] { $"api://{fake.ClientId}/", $"{fake.ClientId}/" };
            var scopes = (requested ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(scope => prefixes.FirstOrDefault(prefix => scope.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) is { } prefix ? scope[prefix.Length..] : null)
                .Where(scope => scope != null)
                .Select(scope => scope == ".default" ? "access_as_user" : scope)
                .Distinct()
                .ToArray();

            return scopes;
        }

        private static FakeEntra.TokenKnobs Knobs(IFormCollection form) => new()
        {
            Version = form["x_token_version"] == "1" ? 1 : 2,
            Lifetime = int.TryParse(form["x_expires_in"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds) ? TimeSpan.FromSeconds(seconds) : null,
            TenantId = Guid.TryParse(form["x_tid"], out var tid) ? tid : null,
            Audience = form["x_aud"].ToString() is { Length: > 0 } audience ? audience : null,
            UnknownKey = form["x_key"] == "unknown",
        };

        private static IResult Error(int statusCode, string error, string description) =>
            Results.Json(new { error, error_description = description }, statusCode: statusCode);

        private static string Origin(HttpRequest request) => $"{request.Scheme}://{request.Host}";

        private static string PickerPage(HttpRequest request)
        {
            var page = new StringBuilder();
            page.Append("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>Fake Entra ID</title></head><body>");
            page.Append("<h1>Fake Entra ID</h1><p>The test server's stand-in for Microsoft Entra ID. Pick an account to sign in as:</p><ul>");
            foreach (var account in FakeEntraAccounts.All.Where(v => !v.IsProgram))
            {
                var link = request.Path + QueryHelpers.AddQueryString(request.QueryString.Value ?? string.Empty, "account", account.Id);
                var kind = account.IsGuest ? "guest" : "member";
                page.Append(CultureInfo.InvariantCulture, $"<li><a href=\"{WebUtility.HtmlEncode(link)}\">{WebUtility.HtmlEncode(account.DisplayName)}</a> ({WebUtility.HtmlEncode(account.UserName)}, {kind})</li>");
            }

            page.Append("</ul></body></html>");
            return page.ToString();
        }
    }
}
