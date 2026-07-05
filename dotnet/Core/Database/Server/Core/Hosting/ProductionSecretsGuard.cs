// <copyright file="ProductionSecretsGuard.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using Microsoft.Extensions.Configuration;

    public static class ProductionSecretsGuard
    {
        // The signing key checked into the config templates. Never acceptable outside Development.
        private const string PlaceholderJwtKey = "0123456789ABCDEF0123456789ABCDEF";

        private const int MinimumJwtKeyLength = 32;

        public static void Validate(IConfiguration configuration, bool isDevelopment)
        {
            if (isDevelopment)
            {
                return;
            }

            var jwtKey = configuration["JwtToken:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "JwtToken:Key is not configured. Set a unique signing key of at least 32 characters in the environment or appsettings for this deployment (it must not be the checked-in template key).");
            }

            if (jwtKey == PlaceholderJwtKey)
            {
                throw new InvalidOperationException(
                    "JwtToken:Key is still the checked-in template value. Set a unique signing key of at least 32 characters for this deployment.");
            }

            if (jwtKey.Length < MinimumJwtKeyLength)
            {
                throw new InvalidOperationException(
                    $"JwtToken:Key is too short ({jwtKey.Length} characters). Use a unique signing key of at least {MinimumJwtKeyLength} characters.");
            }
        }
    }
}
