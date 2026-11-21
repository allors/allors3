// <copyright file="UpgradeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Xunit;

    // Each command runs on a Program of its own, as it does in a process of its own. Without a configuration
    // root the tool reads its settings from the environment; the Snapshot adapter needs no database server.
    public sealed class UpgradeTests : IDisposable
    {
        private const string AdapterVariable = "adapter";
        private const string PopulationFileVariable = "populationFile";

        private readonly string configRoot = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);
        private readonly string adapter = Environment.GetEnvironmentVariable(AdapterVariable);
        private readonly string populationFile = Environment.GetEnvironmentVariable(PopulationFileVariable);
        private readonly string currentDirectory = Environment.CurrentDirectory;
        private readonly string directory = Path.Combine(Path.GetTempPath(), "allors-sample-commands-tests", Path.GetRandomFileName());

        public UpgradeTests()
        {
            Directory.CreateDirectory(this.directory);
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);
            Environment.SetEnvironmentVariable(AdapterVariable, "snapshot");
            Environment.SetEnvironmentVariable(PopulationFileVariable, null);
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
            }
        }

        [Fact]
        public void UpgradeLoadsAPopulationThatSaveWroteAndCreatesThePermissions()
        {
            var earlier = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = earlier }.OnExecute(null));
            using (var transaction = earlier.Database.CreateTransaction())
            {
                var jane = new PersonBuilder(transaction).WithFirstName("Jane").WithUserName("jane@example.com").Build();
                new WorkItemBuilder(transaction).WithTitle("Write the rule").WithAssignee(jane).Build();
                transaction.Derive();
                transaction.Commit();
            }

            Assert.Equal(ExitCode.Success, new Save { Parent = earlier }.OnExecute(null));

            var later = new Program();
            Assert.Equal(ExitCode.Success, new Commands.Upgrade { Parent = later }.OnExecute(null));

            using (var transaction = later.Database.CreateTransaction())
            {
                var workItem = Assert.Single(new WorkItems(transaction).Extent().ToArray());
                Assert.Equal("Write the rule", workItem.Title);
                Assert.Equal("Jane", workItem.Assignee.FirstName);
                Assert.Equal(1, workItem.Assignee.OpenWorkItemCount);
                Assert.NotEmpty(new ReadPermissions(transaction).Extent().ToArray());
            }

            Assert.NotNull(later.Database.Services.Get<ISecurity>());
        }

        [Fact]
        public void UpgradeStopsAtAnObjectOfAClassThatThisVersionDoesNotHave()
        {
            var earlier = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = earlier }.OnExecute(null));
            Assert.Equal(ExitCode.Success, new Save { Parent = earlier }.OnExecute(null));

            // An object of a class that an earlier version had and this version does not: its id is in no model.
            var population = XDocument.Load("population.xml");
            population.Descendants("objects").Single().Element("database")
                .Add(new XElement("ot", new XAttribute("i", "46198ae0-5bba-44db-8c16-7d629951b3f4"), "1000000:2"));
            population.Save("population.xml");

            var later = new Program();
            Assert.Equal(ExitCode.Error, new Commands.Upgrade { Parent = later }.OnExecute(null));
        }
    }
}
