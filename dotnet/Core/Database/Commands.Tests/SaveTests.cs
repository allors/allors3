// <copyright file="SaveTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Allors.Database.Configuration;
    using Xunit;

    public sealed class SaveTests : IDisposable
    {
        private const string AdapterVariable = "adapter";
        private const string PopulationFileVariable = "populationFile";

        private readonly string configRoot = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);
        private readonly string adapter = Environment.GetEnvironmentVariable(AdapterVariable);
        private readonly string populationFile = Environment.GetEnvironmentVariable(PopulationFileVariable);
        private readonly string currentDirectory = Environment.CurrentDirectory;
        private readonly string directory = Path.Combine(Path.GetTempPath(), "allors-commands-tests", Path.GetRandomFileName());

        public SaveTests()
        {
            Directory.CreateDirectory(this.directory);

            // The command reads its settings from the environment alone and saves a Snapshot database, which needs
            // no database server. A file without a directory is a file of the working directory.
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);
            Environment.SetEnvironmentVariable(AdapterVariable, "snapshot");
            Environment.CurrentDirectory = this.directory;
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = this.currentDirectory;
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.configRoot);
            Environment.SetEnvironmentVariable(AdapterVariable, this.adapter);
            Environment.SetEnvironmentVariable(PopulationFileVariable, this.populationFile);

            try
            {
                Directory.Delete(this.directory, recursive: true);
            }
            catch (IOException)
            {
                // best-effort cleanup of the temp tree
            }
        }

        [Theory]
        [InlineData(null, null, "population.xml")]
        [InlineData(null, "setting.xml", "setting.xml")]
        [InlineData("option.xml", null, "option.xml")]
        [InlineData("option.xml", "setting.xml", "option.xml")]
        public void SaveWritesTheFileOfTheOptionThenOfTheSettingThenPopulationXml(string option, string setting, string expected)
        {
            Environment.SetEnvironmentVariable(PopulationFileVariable, setting);

            var exitCode = Program.Main(option != null ? new[] { "save", "-f", option } : new[] { "save" });

            Assert.Equal(ExitCode.Success, exitCode);
            Assert.Equal(new[] { expected }, Directory.GetFiles(this.directory).Select(Path.GetFileName));
        }
    }
}
