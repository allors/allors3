// <copyright file="ConfigurationBuilderExtensionsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Allors.Database.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ServerReadsApplicationSettingsOnlyFromTheConfigRootAndEnvironment(bool hasConfigRoot)
        {
            var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
            var aspNetCoreHttpPorts = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
            IConfigurationRoot configuration = null;
            var stop = new InvalidOperationException("Stop after reading the server configuration.");

            try
            {
                this.Write("core/appsettings.json", @"{ ""ConfigurationBuilderExtensionsTests"": { ""Value"": ""domain"", ""Domain"": ""domain"" } }");
                Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, hasConfigRoot ? this.directory : null);
                Environment.SetEnvironmentVariable(Variable, "environment");
                Environment.SetEnvironmentVariable("HTTP_PORTS", null);
                Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", "18080");

                var exception = Assert.Throws<InvalidOperationException>(() => Allors.Server.Program.CreateHostBuilder(new[]
                    {
                        $"--{Key}=command-line",
                        "--ConfigurationBuilderExtensionsTests:CommandLine=command-line",
                    })
                    .UseEnvironment("ConfigurationTest")
                    .UseContentRoot(this.directory)
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        Assert.Equal("ConfigurationTest", context.HostingEnvironment.EnvironmentName);
                        Assert.Equal(this.directory, context.HostingEnvironment.ContentRootPath);
                        configuration = builder.Build();
                        // Inspect the actual server configuration without starting services or a database.
                        throw stop;
                    })
                    .Build());

                Assert.Same(stop, exception);
                Assert.Null(configuration["ConfigurationBuilderExtensionsTests:CommandLine"]);
                Assert.Equal("environment", configuration[Key]);
                Assert.Equal(hasConfigRoot ? "domain" : null, configuration["ConfigurationBuilderExtensionsTests:Domain"]);
                Assert.Equal("18080", configuration["HTTP_PORTS"]);
            }
            finally
            {
                (configuration as IDisposable)?.Dispose();
                Environment.SetEnvironmentVariable("HTTP_PORTS", httpPorts);
                Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", aspNetCoreHttpPorts);
            }
        }

        // The generic web host reads `urls` from the application configuration before it falls back to
        // the host options, so the URL of the command line must be in the application configuration to
        // keep its precedence over ASPNETCORE_URLS; no other command-line value is read.
        [Theory]
        [InlineData(null)]
        [InlineData("--urls")]
        [InlineData("--urls=")]
        public void ServerKeepsTheUrlsOfTheCommandLineOverTheEnvironment(string option)
        {
            const string environmentUrl = "http://127.0.0.1:18080";
            const string commandLineUrl = "http://127.0.0.1:18081";
            var variables = new[] { "URLS", "urls", "DOTNET_URLS", "ASPNETCORE_URLS" }.ToDictionary(v => v, Environment.GetEnvironmentVariable);
            IConfigurationRoot configuration = null;
            var stop = new InvalidOperationException("Stop after reading the server configuration.");

            try
            {
                Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);
                foreach (var name in variables.Keys)
                {
                    Environment.SetEnvironmentVariable(name, null);
                }

                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", environmentUrl);

                var arguments = new List<string> { "--ConfigurationBuilderExtensionsTests:CommandLine=command-line" };
                if (option == "--urls")
                {
                    arguments.Add(option);
                    arguments.Add(commandLineUrl);
                }
                else if (option != null)
                {
                    arguments.Add(option + commandLineUrl);
                }

                var exception = Assert.Throws<InvalidOperationException>(() => Allors.Server.Program.CreateHostBuilder(arguments.ToArray())
                    .UseEnvironment("ConfigurationTest")
                    .UseContentRoot(this.directory)
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        configuration = builder.Build();
                        // Inspect the actual server configuration without starting services or a database.
                        throw stop;
                    })
                    .Build());

                Assert.Same(stop, exception);
                Assert.Equal(option == null ? environmentUrl : commandLineUrl, configuration[WebHostDefaults.ServerUrlsKey]);
                Assert.Null(configuration["ConfigurationBuilderExtensionsTests:CommandLine"]);
            }
            finally
            {
                (configuration as IDisposable)?.Dispose();
                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Key, variable.Value);
                }
            }
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
