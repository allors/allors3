// <copyright file="AddAllorsConfigurationTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.IO;
    using Allors.Database.Configuration;
    using Xunit;

    public class AddAllorsConfigurationTests
    {
        [Fact]
        public void ResolveDomainPathThrowsWhenConfigRootIsNull()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => ConfigurationBuilderExtensions.ResolveDomainPath(null, "core"));

            Assert.Contains(ConfigurationBuilderExtensions.ConfigRootVariable, exception.Message);
        }

        [Fact]
        public void ResolveDomainPathThrowsWhenConfigRootIsWhitespace()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => ConfigurationBuilderExtensions.ResolveDomainPath("   ", "core"));

            Assert.Contains(ConfigurationBuilderExtensions.ConfigRootVariable, exception.Message);
        }

        [Fact]
        public void ResolveDomainPathThrowsWhenAppsettingsMissing()
        {
            var configRoot = NewTempDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(configRoot, "core"));

                var exception = Assert.Throws<InvalidOperationException>(
                    () => ConfigurationBuilderExtensions.ResolveDomainPath(configRoot, "core"));

                Assert.Contains("appsettings.json", exception.Message);
                Assert.Contains(configRoot, exception.Message);
            }
            finally
            {
                Directory.Delete(configRoot, true);
            }
        }

        [Fact]
        public void ResolveDomainPathReturnsDomainDirectoryWhenValid()
        {
            var configRoot = NewTempDirectory();
            var domainPath = Path.Combine(configRoot, "core");
            try
            {
                Directory.CreateDirectory(domainPath);
                File.WriteAllText(Path.Combine(domainPath, "appsettings.json"), "{ \"adapter\": \"npgsql\" }");

                var resolved = ConfigurationBuilderExtensions.ResolveDomainPath(configRoot, "core");

                Assert.Equal(domainPath, resolved);
            }
            finally
            {
                Directory.Delete(configRoot, true);
            }
        }

        private static string NewTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "allors-config-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
