// <copyright file="UpgradeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Xunit;
    using MetaPopulation = Allors.Database.Meta.MetaPopulation;

    // What a file holds that this version cannot load, and that the upgrade did not exclude.
    public enum Omission
    {
        ObjectOfAClassThatThisVersionDoesNotHave,
        RelationOfARelationTypeThatThisVersionDoesNotHave,
        CompositeRoleWithAnObjectThatTheRoleDoesNotTake,
        UnitRoleWithTheIdOfAnObjectOfAnExcludedClass,
    }

    // Each command runs on a Program of its own, as it does in a process of its own. Without a configuration
    // root the tool reads its settings from the environment; the Snapshot adapter needs no database server.
    public sealed class UpgradeTests : IDisposable
    {
        private const string AdapterVariable = "adapter";
        private const string PopulationFileVariable = "populationFile";

        // The id of a class that an earlier version had and this version removed on purpose: the tests that
        // upgrade a file with objects of this class exclude it.
        private static readonly Guid RemovedClassId = new Guid("46198ae0-5bba-44db-8c16-7d629951b3f4");

        // The ids of a class and of a relation type that no version had.
        private static readonly Guid UnknownClassId = new Guid("e886375a-6ed5-4e7a-b8a3-f0400419bb1e");
        private static readonly Guid UnknownRelationTypeId = new Guid("e60d6389-1111-4a54-a770-0e17ce993e02");

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
        public void UpgradeLeavesTheObjectsOfAnExcludedClassOutOfTheRolesOfTheObjectsThatItLoads()
        {
            var earlier = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = earlier }.OnExecute(null));

            long janeId, joeId, keptLoginId, removedLoginId, ownerGrantId, reviewersId, workItemId;
            using (var transaction = earlier.Database.CreateTransaction())
            {
                var jane = new PersonBuilder(transaction).WithFirstName("Jane").WithUserName("jane@example.com").Build();
                var joe = new PersonBuilder(transaction).WithFirstName("Joe").WithUserName("joe@example.com").Build();
                var keptLogin = new LoginBuilder(transaction).WithProvider("example").WithKey("jane").Build();
                var removedLogin = new LoginBuilder(transaction).WithProvider("example").WithKey("jane-earlier").Build();
                jane.AddLogin(keptLogin);
                jane.AddLogin(removedLogin);
                var reviewers = new UserGroupBuilder(transaction).WithName("Reviewers").WithMember(jane).WithMember(joe).Build();
                var workItem = new WorkItemBuilder(transaction).WithTitle("Review the rule").WithAssignee(joe).Build();
                transaction.Derive();
                transaction.Commit();

                (janeId, joeId, keptLoginId, removedLoginId, ownerGrantId, reviewersId, workItemId) =
                    (jane.Id, joe.Id, keptLogin.Id, removedLogin.Id, jane.OwnerGrant.Id, reviewers.Id, workItem.Id);
            }

            Assert.Equal(ExitCode.Success, new Save { Parent = earlier }.OnExecute(null));

            // This version removed the class of Joe, of one of Jane's logins and of her owner grant. Roles of each
            // multiplicity refer to them: the assignee of the work item (many-to-one), Jane's owner grant (one-to-one),
            // her logins (one-to-many) and the members of the user group (many-to-many).
            var population = XDocument.Load("population.xml");
            MoveToClass(population, RemovedClassId, joeId, removedLoginId, ownerGrantId);
            population.Save("population.xml");

            var later = new Program();
            Assert.Equal(ExitCode.Success, new Commands.Upgrade { Parent = later, ExcludedObjectTypes = { RemovedClassId } }.OnExecute(null));

            using (var transaction = later.Database.CreateTransaction())
            {
                var jane = (Person)transaction.Instantiate(janeId);
                Assert.Null(((WorkItem)transaction.Instantiate(workItemId)).Assignee);
                Assert.Null(jane.OwnerGrant);
                Assert.Equal(new[] { keptLoginId }, jane.Logins.Select(v => v.Id));
                Assert.Equal(new[] { janeId }, ((UserGroup)transaction.Instantiate(reviewersId)).Members.Select(v => v.Id));
            }
        }

        [Theory]
        [InlineData(Omission.ObjectOfAClassThatThisVersionDoesNotHave)]
        [InlineData(Omission.RelationOfARelationTypeThatThisVersionDoesNotHave)]
        [InlineData(Omission.CompositeRoleWithAnObjectThatTheRoleDoesNotTake)]
        [InlineData(Omission.UnitRoleWithTheIdOfAnObjectOfAnExcludedClass)]
        public void UpgradeStopsAtAnOmissionThatIsNotExcludedAndLeavesTheDatabaseAsInitLeavesIt(Omission omission)
        {
            var later = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = later }.OnExecute(null));
            using (var transaction = later.Database.CreateTransaction())
            {
                new WorkItemBuilder(transaction).WithTitle("Original target item").Build();
                transaction.Derive();
                transaction.Commit();
            }

            var earlier = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = earlier }.OnExecute(null));

            long janeId, joeId, workItemId;
            using (var transaction = earlier.Database.CreateTransaction())
            {
                var jane = new PersonBuilder(transaction).WithFirstName("Jane").WithUserName("jane@example.com").Build();
                var joe = new PersonBuilder(transaction).WithFirstName("Joe").WithUserName("joe@example.com").Build();
                var workItem = new WorkItemBuilder(transaction).WithTitle("Imported work item").WithAssignee(jane).Build();
                transaction.Derive();
                transaction.Commit();

                (janeId, joeId, workItemId) = (jane.Id, joe.Id, workItem.Id);
            }

            Assert.Equal(ExitCode.Success, new Save { Parent = earlier }.OnExecute(null));

            // The class of Joe was removed on purpose, and the omission was not.
            var m = earlier.Database.Services.Get<MetaPopulation>();
            var population = XDocument.Load("population.xml");
            MoveToClass(population, RemovedClassId, joeId);
            switch (omission)
            {
                case Omission.ObjectOfAClassThatThisVersionDoesNotHave:
                    Objects(population).Add(new XElement("ot", new XAttribute("i", UnknownClassId), "1000000:2"));
                    break;

                case Omission.RelationOfARelationTypeThatThisVersionDoesNotHave:
                    Relations(population).Add(new XElement("rtu", new XAttribute("i", UnknownRelationTypeId), new XElement("r", new XAttribute("a", janeId), "A value")));
                    break;

                case Omission.CompositeRoleWithAnObjectThatTheRoleDoesNotTake:
                    Relation(population, m.WorkItem.Assignee.RelationType.Id, workItemId).Value = workItemId.ToString(CultureInfo.InvariantCulture);
                    break;

                case Omission.UnitRoleWithTheIdOfAnObjectOfAnExcludedClass:
                    Relation(population, m.WorkItem.Title.RelationType.Id, workItemId).AddAfterSelf(new XElement("r", new XAttribute("a", janeId), joeId));
                    break;
            }

            population.Save("population.xml");

            Assert.Equal(ExitCode.Error, new Commands.Upgrade { Parent = later, ExcludedObjectTypes = { RemovedClassId } }.OnExecute(null));

            AssertAsInitLeavesIt(later.Database);
        }

        private static XElement Objects(XDocument population) => population.Descendants("objects").Single().Element("database");

        private static XElement Relations(XDocument population) => population.Descendants("relations").Single().Element("database");

        private static XElement Relation(XDocument population, Guid relationTypeId, long associationId) =>
            Relations(population).Elements().Single(v => (Guid)v.Attribute("i") == relationTypeId)
                .Elements("r").Single(v => (long)v.Attribute("a") == associationId);

        // Moves objects in the file to another class, as if they had belonged to a class that this version removed.
        private static void MoveToClass(XDocument population, Guid classId, params long[] objectIds)
        {
            var moved = new List<string>();
            foreach (var objectType in Objects(population).Elements("ot").ToArray())
            {
                var objects = objectType.Value.Split(',');
                var kept = objects.Where(v => !objectIds.Contains(long.Parse(v.Split(':')[0], CultureInfo.InvariantCulture))).ToArray();
                moved.AddRange(objects.Except(kept));

                if (kept.Length > 0)
                {
                    objectType.Value = string.Join(",", kept);
                }
                else
                {
                    objectType.Remove();
                }
            }

            Assert.Equal(objectIds.Length, moved.Count);
            Objects(population).Add(new XElement("ot", new XAttribute("i", classId), string.Join(",", moved)));
        }

        // As Init leaves a database: without objects, and with the first id for a new object.
        private static void AssertAsInitLeavesIt(IDatabase database)
        {
            var empty = new Program().Database;
            empty.Init();

            long firstId;
            using (var transaction = empty.CreateTransaction())
            {
                firstId = new WorkItemBuilder(transaction).Build().Id;
            }

            using (var transaction = database.CreateTransaction())
            {
                foreach (var @class in database.MetaPopulation.DatabaseClasses)
                {
                    Assert.Empty(transaction.Extent(@class));
                }

                Assert.Equal(firstId, new WorkItemBuilder(transaction).Build().Id);
            }
        }
    }
}
