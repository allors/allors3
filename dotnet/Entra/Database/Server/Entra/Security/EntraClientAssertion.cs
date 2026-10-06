// <copyright file="EntraClientAssertion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Abstractions;
    using Microsoft.Identity.Web;
    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft.IdentityModel.Tokens;

    // Supplies certificate authentication to the native code exchange. The OpenID Connect handler
    // still redeems the code with PKCE and validates the token response, including its nonce.
    internal static class EntraClientAssertion
    {
        internal static async Task AddAsync(AuthorizationCodeReceivedContext context)
        {
            var request = context.TokenEndpointRequest;
            if (request == null || !string.IsNullOrEmpty(request.ClientSecret) || !string.IsNullOrEmpty(request.ClientAssertion))
            {
                return;
            }

            var services = context.HttpContext.RequestServices;
            var options = services.GetRequiredService<IOptionsMonitor<MicrosoftIdentityOptions>>().Get(context.Scheme.Name);
            var credentials = options.ClientCredentials?.Where(v => v.CredentialType == CredentialType.Certificate && !v.Skip)
                              ?? Array.Empty<CredentialDescription>();
            CredentialDescription credential;
            try
            {
                credential = await services.GetRequiredService<ICredentialsLoader>().LoadFirstValidCredentialsAsync(credentials);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                throw new InvalidOperationException(
                    "Could not load an Entra:ClientCredentials certificate. Check its source, access permissions and password, and include its RSA private key.", exception);
            }

            var certificate = credential?.Certificate;
            if (certificate == null || !certificate.HasPrivateKey)
            {
                throw new InvalidOperationException(
                    "Entra:ClientCredentials has no usable certificate with a private key. Configure an accessible RSA certificate, or remove ClientCredentials and set Entra:ClientSecret.");
            }

            var now = context.Options.TimeProvider.GetUtcNow();
            if (now.UtcDateTime < certificate.NotBefore.ToUniversalTime() || now.UtcDateTime >= certificate.NotAfter.ToUniversalTime())
            {
                throw new InvalidOperationException("The Entra:ClientCredentials certificate is not currently valid. Configure a certificate whose validity period includes the current time.");
            }

            var endpoint = request.TokenEndpoint;
            if (string.IsNullOrEmpty(endpoint))
            {
                var configuration = context.Options.Configuration ?? await context.Options.ConfigurationManager.GetConfigurationAsync(context.HttpContext.RequestAborted);
                endpoint = configuration.TokenEndpoint;
                request.TokenEndpoint = endpoint;
            }

            var headers = new Dictionary<string, object>
            {
                ["x5t#S256"] = Base64UrlEncoder.Encode(certificate.GetCertHash(HashAlgorithmName.SHA256)),
            };
            if (options.SendX5C)
            {
                headers["x5c"] = new[] { Convert.ToBase64String(certificate.RawData) };
            }

            try
            {
                request.ClientAssertion = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
                {
                    Issuer = request.ClientId,
                    Audience = endpoint,
                    Claims = new Dictionary<string, object>
                    {
                        [JwtRegisteredClaimNames.Sub] = request.ClientId,
                        [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
                    },
                    IssuedAt = now.UtcDateTime,
                    NotBefore = now.UtcDateTime,
                    Expires = now.AddMinutes(5).UtcDateTime,
                    SigningCredentials = new SigningCredentials(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSsaPssSha256),
                    AdditionalHeaderClaims = headers,
                    IncludeKeyIdInHeader = false,
                });
            }
            catch (Exception exception) when (exception is CryptographicException or SecurityTokenException or ArgumentException or NotSupportedException)
            {
                throw new InvalidOperationException(
                    "Could not sign the Entra client assertion. Configure an Entra:ClientCredentials RSA certificate whose private key this process can access and use for PS256 signing.", exception);
            }

            request.ClientSecret = null;
            request.ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        }
    }
}
