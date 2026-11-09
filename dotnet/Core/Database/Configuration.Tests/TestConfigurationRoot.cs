// <copyright file="TestConfigurationRoot.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.IO;

    /// <summary>
    /// The configuration root of a test run. A program reads its settings from the root that
    /// <see cref="ConfigurationBuilderExtensions.ConfigRootVariable"/> names and stops without one. The
    /// test projects default to the <c>config/snapshot</c> template of the repository instead, so a test
    /// run without the variable uses the Snapshot adapter and needs no database server.
    /// </summary>
    public static class TestConfigurationRoot
    {
        /// <summary>
        /// The template that a test run uses when <see cref="ConfigurationBuilderExtensions.ConfigRootVariable"/>
        /// is not set.
        /// </summary>
        public const string DefaultTemplate = "snapshot";

        /// <summary>
        /// Returns the configuration root of this process. When
        /// <see cref="ConfigurationBuilderExtensions.ConfigRootVariable"/> is not set, points it at the
        /// <see cref="DefaultTemplate"/> of the repository that holds the running assembly and returns
        /// that folder.
        /// </summary>
        public static string Default()
        {
            var root = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);
            if (!string.IsNullOrWhiteSpace(root))
            {
                return root;
            }

            root = FindTemplate(DefaultTemplate, AppContext.BaseDirectory);
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, root);
            return root;
        }

        /// <summary>
        /// Finds the template <c>config/&lt;name&gt;</c>, recognised by its <c>core/appsettings.json</c>,
        /// in <paramref name="start"/> or a folder above it.
        /// </summary>
        public static string FindTemplate(string name, string start)
        {
            for (var directory = new DirectoryInfo(start); directory != null; directory = directory.Parent)
            {
                var template = Path.Combine(directory.FullName, "config", name);
                if (File.Exists(Path.Combine(template, "core", "appsettings.json")))
                {
                    return template;
                }
            }

            throw new InvalidOperationException($"{ConfigurationBuilderExtensions.ConfigRootVariable} is not set and no template config/{name} was found in '{start}' or a folder above it. Run the tests from a clone of the repository, or set {ConfigurationBuilderExtensions.ConfigRootVariable} to a configuration root.");
        }
    }
}
