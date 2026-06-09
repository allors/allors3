// <copyright file="ConfigurationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Name of the environment variable that must point at the Allors configuration root
        /// (e.g. <c>/opt/allors</c> in production, or <c>&lt;repo&gt;/config/npgsql</c> in development).
        /// </summary>
        public const string ConfigRootVariable = "ALLORS_CONFIG_ROOT";

        /// <summary>
        /// Layers the Allors configuration for a domain (<c>core</c>, <c>base</c>, <c>apps</c>) on top of
        /// the configuration builder. The configuration root is read from the
        /// <see cref="ConfigRootVariable"/> environment variable; it is required and missing/invalid values
        /// fail fast. Environment variables are added last so they always override the JSON files.
        /// </summary>
        public static IConfigurationBuilder AddAllorsConfiguration(this IConfigurationBuilder configurationBuilder, string domain, string subFolder = null, string environmentName = null)
        {
            var configRoot = Environment.GetEnvironmentVariable(ConfigRootVariable);
            var domainPath = ResolveDomainPath(configRoot, domain);

            configurationBuilder.AddCrossPlatform(domainPath, environmentName);

            if (!string.IsNullOrWhiteSpace(subFolder))
            {
                configurationBuilder.AddCrossPlatform(Path.Combine(domainPath, subFolder), environmentName);
            }

            configurationBuilder.AddEnvironmentVariables();

            return configurationBuilder;
        }

        /// <summary>
        /// Validates the configuration root and returns the directory for the given domain.
        /// Throws an actionable <see cref="InvalidOperationException"/> when the root is not set or when
        /// the expected <c>appsettings.json</c> is missing.
        /// </summary>
        public static string ResolveDomainPath(string configRoot, string domain)
        {
            if (string.IsNullOrWhiteSpace(configRoot))
            {
                throw new InvalidOperationException(
                    $"{ConfigRootVariable} is not set. Point it at a configuration directory, for example:{Environment.NewLine}" +
                    $"  dev (Postgres):  export {ConfigRootVariable}=\"<repo>/config/npgsql\"{Environment.NewLine}" +
                    $"  prod:            export {ConfigRootVariable}=/opt/allors{Environment.NewLine}" +
                    $"Expected to find <{ConfigRootVariable}>/{domain}/appsettings.json.");
            }

            var domainPath = Path.Combine(configRoot, domain);
            var primary = Path.Combine(domainPath, "appsettings.json");
            if (!File.Exists(primary))
            {
                throw new InvalidOperationException(
                    $"{ConfigRootVariable} is '{configRoot}', but '{primary}' was not found. " +
                    $"Install a provider template, for example: cp -r <repo>/config/npgsql/{domain} \"{domainPath}\"");
            }

            return domainPath;
        }

        public static void AddCrossPlatform(this IConfigurationBuilder configurationBuilder, string path, string environmentName = null, bool skipDefault = false)
        {
            var platform = "other";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = "linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "osx";
            }

            // Lowercase "appsettings.json" so the same files resolve on case-sensitive filesystems (Linux).
            if (!skipDefault)
            {
                configurationBuilder.AddJsonFile(Path.Combine(path, "appsettings.json"), true);
            }

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                configurationBuilder.AddJsonFile(Path.Combine(path, $"appsettings.{environmentName}.json"), true);
            }

            configurationBuilder.AddJsonFile(Path.Combine(path, $"appsettings.{platform}.json"), true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                configurationBuilder.AddJsonFile(Path.Combine(path, $"appsettings.{environmentName}.{platform}.json"), true);
            }
        }
    }
}
