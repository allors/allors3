// <copyright file="JwtTokens.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Database.Configuration;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    public sealed class JwtTokens
    {
        private readonly SigningCredentials credentials;
        private readonly string issuer;
        private readonly string audience;
        private readonly TimeSpan expiration;

        public JwtTokens(IConfiguration configuration)
        {
            configuration.Require("JwtToken:Key");
            var keyBytes = Encoding.UTF8.GetBytes(configuration["JwtToken:Key"]);
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException("'JwtToken:Key' must contain at least 32 UTF-8 bytes for HS256. Set 'JwtToken__Key' in the environment or update the configuration root.");
            }

            this.credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var issuer = configuration["JwtToken:Issuer"];
            var audience = configuration["JwtToken:Audience"];
            this.issuer = string.IsNullOrWhiteSpace(issuer) ? null : issuer;
            this.audience = string.IsNullOrWhiteSpace(audience) ? this.issuer : audience;
            this.expiration = TimeSpan.TryParse(configuration["JwtToken:Expiration"], out var lifetime) ? lifetime : TimeSpan.FromDays(30);
        }

        public string CreateToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(this.issuer, this.audience, claims,
                expires: DateTime.Now.Add(this.expiration), signingCredentials: this.credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        internal TokenValidationParameters CreateValidationParameters() => new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = this.credentials.Key,
            ValidateIssuer = this.issuer != null,
            ValidIssuer = this.issuer,
            ValidateAudience = this.audience != null,
            ValidAudience = this.audience,
            IgnoreTrailingSlashWhenValidatingAudience = false,
        };
    }
}
