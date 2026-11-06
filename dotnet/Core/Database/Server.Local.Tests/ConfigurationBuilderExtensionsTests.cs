// <copyright file="ConfigurationBuilderExtensionsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.IO;
    using Allors.Database.Configuration;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class ConfigurationBuilderExtensionsTests : IDisposable
    {
        private const string Key = "ConfigurationBuilderExtensionsTests:Value";
        private const string Variable = "ConfigurationBuilderExtensionsTests__Value";

        private readonly string configRoot = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);
        private readonly string directory = Path.Combine(Path.GetTempPath(), "allors-config-" + Guid.NewGuid().ToString("N"));

        public ConfigurationBuilderExtensionsTests() => Directory.CreateDirectory(this.directory);

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.configRoot);
            Environment.SetEnvironmentVariable(Variable, null);
            Directory.Delete(this.directory, true);
        }

        [Fact]
        public void WithoutAConfigRootOnlyEnvironmentVariablesAreRead()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);
            Environment.SetEnvironmentVariable(Variable, "environment");

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core", "commands").Build();

            Assert.Equal("environment", configuration[Key]);
            Assert.DoesNotContain(configuration.Providers, v => v is FileConfigurationProvider);
        }

        [Fact]
        public void TheConfigRootHoldsTheSettingsOfADomainAndOfItsSubFolder()
        {
            this.Write("core/appsettings.json", @"{ ""ConfigurationBuilderExtensionsTests"": { ""Value"": ""domain"", ""Domain"": ""domain"" } }");
            this.Write("core/commands/appsettings.json", @"{ ""ConfigurationBuilderExtensionsTests"": { ""Value"": ""commands"" } }");
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.directory);

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core", "commands").Build();

            Assert.Equal("commands", configuration[Key]);
            Assert.Equal("domain", configuration["ConfigurationBuilderExtensionsTests:Domain"]);
        }

        [Fact]
        public void EnvironmentVariablesOverrideTheConfigRoot()
        {
            this.Write("core/appsettings.json", @"{ ""ConfigurationBuilderExtensionsTests"": { ""Value"": ""domain"" } }");
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.directory);
            Environment.SetEnvironmentVariable(Variable, "environment");

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core").Build();

            Assert.Equal("environment", configuration[Key]);
        }

        [Fact]
        public void RequireAcceptsSettingsThatHaveAValue()
        {
            this.Write("core/appsettings.json", @"{ ""ConfigurationBuilderExtensionsTests"": { ""Value"": ""domain"" } }");
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.directory);

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core").Build();

            configuration.Require(Key);
        }

        // A folder that the config root lacks is not an error by itself: only a setting without a value
        // is, and the message says where the value was looked for.
        [Fact]
        public void RequireNamesTheSettingsWithoutAValueAndTheFoldersOfTheConfigRoot()
        {
            this.Write("core/appsettings.json", "{ }");
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.directory);

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core", "commands").Build();
            var exception = Assert.Throws<InvalidOperationException>(() => configuration.Require(Key, "ConfigurationBuilderExtensionsTests:Other"));

            Assert.Contains("'ConfigurationBuilderExtensionsTests:Value'", exception.Message);
            Assert.Contains("'ConfigurationBuilderExtensionsTests:Other'", exception.Message);
            Assert.Contains($"'{Variable}'", exception.Message);
            Assert.Contains($"'{Path.Combine(this.directory, "core")}'", exception.Message);
            Assert.Contains($"'{Path.Combine(this.directory, "core", "commands")}' (this folder does not exist)", exception.Message);
        }

        [Fact]
        public void RequireNamesTheEnvironmentVariablesWithoutAConfigRoot()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core").Build();
            var exception = Assert.Throws<InvalidOperationException>(() => configuration.Require(Key));

            Assert.Contains($"'{Key}'", exception.Message);
            Assert.Contains($"'{Variable}'", exception.Message);
            Assert.Contains($"{ConfigurationBuilderExtensions.ConfigRootVariable} is not set", exception.Message);
        }

        private void Write(string path, string json)
        {
            var file = Path.Combine(this.directory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllText(file, json);
        }
    }
}
