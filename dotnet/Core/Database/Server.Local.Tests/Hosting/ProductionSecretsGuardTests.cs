// <copyright file="ProductionSecretsGuardTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Server;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class ProductionSecretsGuardTests
    {
        [Fact]
        public void PlaceholderKeyOutsideDevelopmentThrows()
        {
            var configuration = Configuration("0123456789ABCDEF0123456789ABCDEF");

            var exception = Assert.Throws<InvalidOperationException>(() => ProductionSecretsGuard.Validate(configuration, isDevelopment: false));
            Assert.Contains("JwtToken:Key", exception.Message);
        }

        [Fact]
        public void MissingKeyOutsideDevelopmentThrows()
        {
            var configuration = Configuration(null);

            Assert.Throws<InvalidOperationException>(() => ProductionSecretsGuard.Validate(configuration, isDevelopment: false));
        }

        [Fact]
        public void ShortKeyOutsideDevelopmentThrows()
        {
            var configuration = Configuration("tooshort");

            Assert.Throws<InvalidOperationException>(() => ProductionSecretsGuard.Validate(configuration, isDevelopment: false));
        }

        [Fact]
        public void PlaceholderKeyInDevelopmentIsAllowed()
        {
            var configuration = Configuration("0123456789ABCDEF0123456789ABCDEF");

            ProductionSecretsGuard.Validate(configuration, isDevelopment: true);
        }

        [Fact]
        public void StrongKeyOutsideDevelopmentIsAllowed()
        {
            var configuration = Configuration("a-sufficiently-long-and-unique-signing-key-value");

            ProductionSecretsGuard.Validate(configuration, isDevelopment: false);
        }

        private static IConfiguration Configuration(string jwtKey)
        {
            var values = new Dictionary<string, string>();
            if (jwtKey != null)
            {
                values["JwtToken:Key"] = jwtKey;
            }

            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }
    }
}
