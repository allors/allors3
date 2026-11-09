// <copyright file="TestConfigurationRootTests.cs" company="Allors bv">
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

    public class TestConfigurationRootTests : IDisposable
    {
        private readonly string configRoot = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);

        public void Dispose() => Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.configRoot);

        [Fact]
        public void WithoutAConfigRootTheTestsRunOnTheSnapshotTemplate()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);

            var root = TestConfigurationRoot.Default();

            Assert.Equal(root, Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable));
            Assert.Equal(TestConfigurationRoot.DefaultTemplate, Path.GetFileName(root));
            Assert.Equal("config", Path.GetFileName(Path.GetDirectoryName(root)));

            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core").Build();
            Assert.Equal("snapshot", configuration["adapter"]);
        }

        [Fact]
        public void AConfigRootThatIsSetIsKept()
        {
            var directory = Path.Combine(Path.GetTempPath(), "allors-config-" + Guid.NewGuid().ToString("N"));
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, directory);

            Assert.Equal(directory, TestConfigurationRoot.Default());
            Assert.Equal(directory, Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable));
        }

        [Fact]
        public void WithoutATemplateTheMessageNamesTheVariableAndTheStartFolder()
        {
            var name = "no-such-template-" + Guid.NewGuid().ToString("N");
            var start = AppContext.BaseDirectory;

            var exception = Assert.Throws<InvalidOperationException>(() => TestConfigurationRoot.FindTemplate(name, start));

            Assert.Contains(ConfigurationBuilderExtensions.ConfigRootVariable, exception.Message, StringComparison.Ordinal);
            Assert.Contains("config/" + name, exception.Message, StringComparison.Ordinal);
            Assert.Contains(start, exception.Message, StringComparison.Ordinal);
        }
    }
}
