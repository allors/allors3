// <copyright file="JwtBearerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using Allors.Security;
    using Allors.Server;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Xunit;

    public class JwtBearerTests
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("issuer", null, "issuer")]
        [InlineData(null, "audience", "audience")]
        [InlineData("issuer", "audience", "audience")]
        public void TheServerAcceptsItsOwnTokens(string issuer, string audience, string expectedAudience)
        {
            var configuration = Configuration("012345678901234567890123456789é", issuer, audience);
            var token = new IdentityUser("Jane").CreateToken(configuration);

            _ = new JwtSecurityTokenHandler().ValidateToken(token, BearerValidation(configuration), out var validated);

            var jwt = Assert.IsType<JwtSecurityToken>(validated);
            Assert.Equal(issuer, jwt.Claims.SingleOrDefault(v => v.Type == JwtRegisteredClaimNames.Iss)?.Value);
            Assert.Equal(expectedAudience, jwt.Audiences.SingleOrDefault());
        }

        [Theory]
        [InlineData("issuer", "audience", "other", "audience", typeof(SecurityTokenInvalidIssuerException))]
        [InlineData("issuer", "audience", "issuer", "other", typeof(SecurityTokenInvalidAudienceException))]
        [InlineData("issuer", "https://example.com/api", "issuer", "https://example.com/api/", typeof(SecurityTokenInvalidAudienceException))]
        [InlineData("issuer", null, "issuer", "other", typeof(SecurityTokenInvalidAudienceException))]
        [InlineData(null, "audience", null, "other", typeof(SecurityTokenInvalidAudienceException))]
        public void TheServerRejectsTokensIssuedForAnotherIssuerOrAudience(string issuer, string audience, string otherIssuer, string otherAudience, Type error)
        {
            const string key = "01234567890123456789012345678901";
            var validation = BearerValidation(Configuration(key, issuer, audience));
            var token = new IdentityUser("Jane").CreateToken(Configuration(key, otherIssuer, otherAudience));

            Assert.Throws(error, () => new JwtSecurityTokenHandler().ValidateToken(token, validation, out _));
        }

        private static IConfiguration Configuration(string key, string issuer, string audience) =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = key,
                ["JwtToken:Issuer"] = issuer,
                ["JwtToken:Audience"] = audience,
            }).Build();

        private static TokenValidationParameters BearerValidation(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            new Startup(configuration).ConfigureServices(services);
            using var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
        }
    }
}
