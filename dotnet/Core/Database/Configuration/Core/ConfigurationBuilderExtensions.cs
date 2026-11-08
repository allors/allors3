// <copyright file="ConfigurationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// The environment variable that names the configuration root: the folder that holds one folder
        /// of settings for every domain.
        /// </summary>
        public const string ConfigRootVariable = "ALLORS_CONFIG_ROOT";

        /// <summary>
        /// Adds the settings of a domain. When <see cref="ConfigRootVariable"/> is set, they are read from
        /// the folder of the domain below that root and then from <paramref name="subFolder"/> below that
        /// folder, each as <see cref="AddCrossPlatform"/> reads a folder. When it is not set, no file is
        /// read. Environment variables override every file. A folder or a file that does not exist is
        /// skipped: <see cref="Require"/> reports the settings that are left without a value.
        /// </summary>
        public static IConfigurationBuilder AddAllorsConfiguration(this IConfigurationBuilder configurationBuilder, string domain, string subFolder = null, string environmentName = null)
        {
            var configRoot = Environment.GetEnvironmentVariable(ConfigRootVariable);
            var folders = Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(configRoot))
            {
                configRoot = null;
            }
            else
            {
                configRoot = Path.GetFullPath(configRoot);

                var domainFolder = Path.Combine(configRoot, domain);
                folders = string.IsNullOrWhiteSpace(subFolder)
                    ? new[] { domainFolder }
                    : new[] { domainFolder, Path.Combine(domainFolder, subFolder) };

                foreach (var folder in folders)
                {
                    configurationBuilder.AddCrossPlatform(folder, environmentName);
                }
            }

            // Without a configuration root this is the only source of settings.
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.Add(new Origin(configRoot, domain, folders));

            return configurationBuilder;
        }

        /// <summary>
        /// Throws when one of the settings has no value. The message names those settings and the
        /// environment variables that set them and, for a configuration that
        /// <see cref="AddAllorsConfiguration"/> built, where the values were looked for.
        /// </summary>
        public static void Require(this IConfiguration configuration, params string[] keys)
        {
            var missing = keys.Where(v => string.IsNullOrWhiteSpace(configuration[v])).ToArray();
            if (missing.Length == 0)
            {
                return;
            }

            var settings = string.Join(", ", missing.Select(v => $"'{v}'"));
            var variables = string.Join(", ", missing.Select(v => $"'{v.Replace(":", "__")}'"));
            var message = $"The configuration has no value for {settings}.";

            var origin = (configuration as IConfigurationRoot)?.Providers.OfType<Origin>().LastOrDefault();
            if (origin != null && origin.ConfigRoot == null)
            {
                message += $" {ConfigRootVariable} is not set, so only environment variables are read: set {variables} in the environment, or point {ConfigRootVariable} at a folder that holds '{origin.Domain}/appsettings.json'.";
            }
            else if (origin != null)
            {
                var folders = string.Join(" or ", origin.Folders.Select(v => Directory.Exists(v) ? $"'{v}'" : $"'{v}' (this folder does not exist)"));
                message += $" {ConfigRootVariable} is '{origin.ConfigRoot}': add the value to an appsettings.json in {folders}, or set {variables} in the environment.";
            }

            throw new InvalidOperationException(message);
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

            if (!skipDefault)
            {
                configurationBuilder.AddJsonFile(Path.Combine(path, "appsettings.json"), true);
                configurationBuilder.AddJsonFile(Path.Combine(path, "appSettings.json"), true);
            }

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                configurationBuilder.AddJsonFile(Path.Combine(path, $"appsettings.{environmentName}.json"), true);
                configurationBuilder.AddJsonFile(Path.Combine(path, $"appSettings.{environmentName}.json"), true);
            }

            configurationBuilder.AddJsonFile(Path.Combine(path, $"appsettings.{platform}.json"), true);
            configurationBuilder.AddJsonFile(Path.Combine(path, $"appSettings.{platform}.json"), true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                configurationBuilder.AddJsonFile(Path.Combine(path, $"appsettings.{environmentName}.{platform}.json"), true);
                configurationBuilder.AddJsonFile(Path.Combine(path, $"appSettings.{environmentName}.{platform}.json"), true);
            }

            // Environment variables must win over every platform- and environment-specific JSON file.
            configurationBuilder.AddEnvironmentVariables();
        }

        /// <summary>
        /// Records where <see cref="AddAllorsConfiguration"/> read the settings from, for the message of
        /// <see cref="Require"/>. It provides no settings.
        /// </summary>
        private sealed class Origin : ConfigurationProvider, IConfigurationSource
        {
            public Origin(string configRoot, string domain, string[] folders)
            {
                this.ConfigRoot = configRoot;
                this.Domain = domain;
                this.Folders = folders;
            }

            public string ConfigRoot { get; }

            public string Domain { get; }

            public string[] Folders { get; }

            public IConfigurationProvider Build(IConfigurationBuilder builder) => this;
        }
    }
}
