// <copyright file="FakeEntra.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft.IdentityModel.Tokens;

    // A stand-in for Microsoft Entra ID inside the test server, for the tests and for a developer
    // without a tenant. It serves Microsoft's documents under Microsoft's issuer, with its endpoints on
    // this server, and signs tokens of Microsoft's shape with a key of its own. The plug-in and
    // Microsoft.Identity.Web validate them as they validate Microsoft's, because the test server routes
    // their requests for Microsoft's host here (AddFakeEntra); the plug-in itself knows nothing of the
    // fake. The shapes follow Microsoft's live documents of 2026-10-03: the v2.0 keys name their issuer
    // as a template, the v1.0 keys do not, and a v1.0 token comes from sts.windows.net with the api://
    // audience. Test scaffolding: it lives in the server's non-inherited Test folder.
    public sealed class FakeEntra
    {
        public const string MicrosoftHost = "login.microsoftonline.com";
        public const string PathPrefix = "/fake-entra";
        public const string HttpClientName = "FakeEntra";

        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
        private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

        private readonly RsaSecurityKey key;

        // Never published: a token signed with it has a signature Microsoft's keys cannot verify.
        private readonly RsaSecurityKey unknownKey;

        private readonly ConcurrentDictionary<string, AuthorizationCode> codes = new();

        public FakeEntra(IConfiguration configuration)
        {
            var section = configuration.GetSection(EntraDefaults.ConfigurationSection);
            this.TenantId = Guid.Parse(section["TenantId"]);
            this.ClientId = section["ClientId"];
            this.ClientSecret = section["ClientSecret"];
            this.key = NewKey();
            this.unknownKey = NewKey();
        }

        // The tenant and the registration of the application, as the plug-in is configured.
        public Guid TenantId { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public static string IssuerV2(Guid tenant) => $"https://{MicrosoftHost}/{tenant}/v2.0";

        public static string IssuerV1(Guid tenant) => $"https://sts.windows.net/{tenant}/";

        public object MetadataV2(Guid tenant, string origin) => new Dictionary<string, object>
        {
            ["token_endpoint"] = $"{origin}{PathPrefix}/{tenant}/oauth2/v2.0/token",
            ["token_endpoint_auth_methods_supported"] = new[] { "client_secret_post", "private_key_jwt", "client_secret_basic" },
            ["jwks_uri"] = $"https://{MicrosoftHost}/{tenant}/discovery/v2.0/keys",
            ["response_modes_supported"] = new[] { "query", "fragment", "form_post" },
            ["subject_types_supported"] = new[] { "pairwise" },
            ["id_token_signing_alg_values_supported"] = new[] { "RS256" },
            ["response_types_supported"] = new[] { "code", "id_token", "code id_token", "id_token token" },
            ["scopes_supported"] = new[] { "openid", "profile", "email", "offline_access" },
            ["issuer"] = IssuerV2(tenant),
            ["request_uri_parameter_supported"] = false,
            ["authorization_endpoint"] = $"{origin}{PathPrefix}/{tenant}/oauth2/v2.0/authorize",
            ["http_logout_supported"] = true,
            ["frontchannel_logout_supported"] = true,
            ["end_session_endpoint"] = $"{origin}{PathPrefix}/{tenant}/oauth2/v2.0/logout",
            ["claims_supported"] = new[] { "sub", "iss", "cloud_instance_name", "aud", "exp", "iat", "auth_time", "acr", "nonce", "preferred_username", "name", "tid", "ver", "at_hash", "c_hash", "email" },
            ["tenant_region_scope"] = "EU",
            ["cloud_instance_name"] = "microsoftonline.com",
        };

        public object MetadataV1(Guid tenant, string origin) => new Dictionary<string, object>
        {
            ["token_endpoint"] = $"{origin}{PathPrefix}/{tenant}/oauth2/token",
            ["token_endpoint_auth_methods_supported"] = new[] { "client_secret_post", "private_key_jwt", "client_secret_basic" },
            ["jwks_uri"] = $"https://{MicrosoftHost}/{tenant}/discovery/keys",
            ["response_modes_supported"] = new[] { "query", "fragment", "form_post" },
            ["subject_types_supported"] = new[] { "pairwise" },
            ["id_token_signing_alg_values_supported"] = new[] { "RS256" },
            ["response_types_supported"] = new[] { "code", "id_token", "code id_token", "token id_token", "token" },
            ["scopes_supported"] = new[] { "openid" },
            ["issuer"] = IssuerV1(tenant),
            ["authorization_endpoint"] = $"{origin}{PathPrefix}/{tenant}/oauth2/authorize",
            ["end_session_endpoint"] = $"{origin}{PathPrefix}/{tenant}/oauth2/logout",
            ["claims_supported"] = new[] { "sub", "iss", "aud", "exp", "iat", "nonce", "name", "tid", "ver", "upn", "unique_name", "oid" },
            ["tenant_region_scope"] = "EU",
            ["cloud_instance_name"] = "microsoftonline.com",
        };

        // The v2.0 keys carry the issuer of their tokens, as a template with the tenant id; the v1.0 keys
        // carry none. Microsoft.Identity.Web reads the one on the key it verified a token with.
        public Dictionary<string, object> KeysV2() => this.Keys($"https://{MicrosoftHost}/{{tenantid}}/v2.0");

        public Dictionary<string, object> KeysV1() => this.Keys(null);

        // The authorize endpoint hands out a code bound to what the client asked for.
        public string NewCode(FakeEntraAccount account, string clientId, string redirectUri, string codeChallenge, string nonce, string scope)
        {
            var code = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
            this.codes[code] = new AuthorizationCode(account, clientId, redirectUri, codeChallenge, nonce, scope, DateTimeOffset.UtcNow.Add(CodeLifetime));
            return code;
        }

        // The token endpoint redeems a code once, for the client it was given to, at the redirect URI
        // it was sent to, with the PKCE verifier of the challenge it was bound to.
        public AuthorizationCode Redeem(string code, string clientId, string redirectUri, string codeVerifier, out string error)
        {
            if (code == null || !this.codes.TryRemove(code, out var issued) || issued.Expires < DateTimeOffset.UtcNow)
            {
                error = "AADSTS70008: The provided authorization code is expired or already redeemed.";
                return null;
            }

            if (!string.Equals(issued.ClientId, clientId, StringComparison.OrdinalIgnoreCase))
            {
                error = "AADSTS70000: The code was issued to another client.";
                return null;
            }

            if (!string.Equals(issued.RedirectUri, redirectUri, StringComparison.Ordinal))
            {
                error = "AADSTS50011: The redirect URI does not match the one the code was issued for.";
                return null;
            }

            if (issued.CodeChallenge != null && (codeVerifier == null || Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier))) != issued.CodeChallenge))
            {
                error = "AADSTS501481: The code_verifier does not match the code_challenge.";
                return null;
            }

            error = null;
            return issued;
        }

        public string IdToken(Guid tenant, FakeEntraAccount account, string clientId, string nonce, TokenKnobs knobs = null)
        {
            knobs ??= new TokenKnobs();
            var claims = Subject(tenant, account, clientId, version: 2, knobs);
            if (nonce != null)
            {
                claims["nonce"] = nonce;
            }

            return this.Sign(IssuerV2(tenant), knobs.Audience ?? clientId, claims, knobs);
        }

        // An access token for the application's API, delegated by a person or owned by a program.
        public string AccessToken(Guid tenant, FakeEntraAccount account, string clientApplicationId, string[] scopes, TokenKnobs knobs = null)
        {
            knobs ??= new TokenKnobs();
            var claims = Subject(tenant, account, clientApplicationId, knobs.Version, knobs);

            if (account.IsProgram)
            {
                claims["roles"] = account.Roles ?? Array.Empty<string>();
                claims[knobs.Version == 1 ? "appidacr" : "azpacr"] = "1";
            }
            else
            {
                claims["scp"] = string.Join(' ', scopes ?? Array.Empty<string>());
                claims[knobs.Version == 1 ? "appidacr" : "azpacr"] = "0";
            }

            var issuer = knobs.Version == 1 ? IssuerV1(tenant) : IssuerV2(tenant);
            var audience = knobs.Audience ?? (knobs.Version == 1 ? "api://" + this.ClientId : this.ClientId);
            return this.Sign(issuer, audience, claims, knobs);
        }

        // The claims that say who the token stands for, in the shape of the token version.
        private static Dictionary<string, object> Subject(Guid tenant, FakeEntraAccount account, string clientApplicationId, int version, TokenKnobs knobs)
        {
            var tid = knobs.TenantId ?? tenant;
            var claims = new Dictionary<string, object>
            {
                ["tid"] = tid.ToString(),
                ["oid"] = account.ObjectId.ToString(),
                ["sub"] = account.IsProgram ? account.ObjectId.ToString() : PairwiseSubject(account.ObjectId, clientApplicationId),
                ["ver"] = version == 1 ? "1.0" : "2.0",
                ["idtyp"] = account.IsProgram ? "app" : "user",
                [version == 1 ? "appid" : "azp"] = clientApplicationId,
            };

            if (account.IsProgram)
            {
                return claims;
            }

            claims["name"] = account.DisplayName;
            claims["acct"] = account.IsGuest ? "1" : "0";
            if (version == 1)
            {
                claims["upn"] = account.UserName;
                claims["unique_name"] = account.UserName;
                claims["idp"] = IssuerV1(account.IsGuest ? Guid.Parse(account.HomeTenantId) : tid);
            }
            else
            {
                claims["preferred_username"] = account.UserName;
                if (account.IsGuest)
                {
                    claims["idp"] = IssuerV2(Guid.Parse(account.HomeTenantId));
                }
            }

            if (account.Email != null)
            {
                claims["email"] = account.Email;
            }

            return claims;
        }

        private string Sign(string issuer, string audience, Dictionary<string, object> claims, TokenKnobs knobs)
        {
            var now = DateTime.UtcNow;
            var lifetime = knobs.Lifetime ?? TokenLifetime;
            return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Claims = claims,
                IssuedAt = now,
                NotBefore = now.AddMinutes(-5),
                Expires = now.Add(lifetime),
                SigningCredentials = new SigningCredentials(knobs.UnknownKey ? this.unknownKey : this.key, SecurityAlgorithms.RsaSha256),
            });
        }

        private Dictionary<string, object> Keys(string issuer)
        {
            var parameters = this.key.Rsa.ExportParameters(false);
            var jwk = new Dictionary<string, object>
            {
                ["kty"] = "RSA",
                ["use"] = "sig",
                ["kid"] = this.key.KeyId,
                ["n"] = Base64UrlEncoder.Encode(parameters.Modulus),
                ["e"] = Base64UrlEncoder.Encode(parameters.Exponent),
                ["cloud_instance_name"] = "microsoftonline.com",
            };

            if (issuer != null)
            {
                jwk["issuer"] = issuer;
            }

            return new Dictionary<string, object> { ["keys"] = new[] { jwk } };
        }

        // Entra's subject is pairwise: the same person has another sub for every application.
        private static string PairwiseSubject(Guid objectId, string clientId) =>
            Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(objectId + "|" + clientId)));

        private static RsaSecurityKey NewKey() => new(RSA.Create(2048)) { KeyId = Guid.NewGuid().ToString("N") };

        public sealed record AuthorizationCode(FakeEntraAccount Account, string ClientId, string RedirectUri, string CodeChallenge, string Nonce, string Scope, DateTimeOffset Expires);

        // What a test may ask the fake to do differently, to get a token Microsoft would never issue
        // for the tenant: the token's version, a lifetime in the past, another tenant id in the claims
        // than in the issuer, another audience, or a signature with a key the documents do not list.
        public sealed class TokenKnobs
        {
            public int Version { get; set; } = 2;

            public TimeSpan? Lifetime { get; set; }

            public Guid? TenantId { get; set; }

            public string Audience { get; set; }

            public bool UnknownKey { get; set; }
        }
    }
}
