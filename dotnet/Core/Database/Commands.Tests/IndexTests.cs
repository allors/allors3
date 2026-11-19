// <copyright file="IndexTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Tests
{
    using System;
    using Allors.Database.Configuration;
    using Xunit;

    public sealed class IndexTests : IDisposable
    {
        private const string AdapterVariable = "adapter";

        private readonly string configRoot = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);
        private readonly string adapter = Environment.GetEnvironmentVariable(AdapterVariable);

        public IndexTests()
        {
            // The command reads its settings from the environment alone and takes a Snapshot database, which needs
            // no database server.
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);
            Environment.SetEnvironmentVariable(AdapterVariable, "snapshot");
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.configRoot);
            Environment.SetEnvironmentVariable(AdapterVariable, this.adapter);
        }

        [Fact]
        public void IndexSucceedsForAnAdapterThatKeepsNoIndexes()
        {
            var exitCode = Program.Main(new[] { "index" });

            Assert.Equal(ExitCode.Success, exitCode);
        }
    }
}
