// <copyright file="RepeatedSetupTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;
    using C1 = Allors.Database.Domain.C1;
    using ObjectFactory = Allors.Database.ObjectFactory;

    public class RepeatedSetupTests : IClassFixture<Fixture>
    {
        private readonly Fixture fixture;

        public RepeatedSetupTests(Fixture fixture) => this.fixture = fixture;

        // The out-of-process server runs this exact sequence for every Test/Setup, reusing a single
        // IDatabase. A second cycle used to fail on Npgsql with "Grant.Subjects, Grant.SubjectGroups at
        // least one!": Init recreates the schema and restarts object-id allocation, but data-scoped
        // database services kept pre-wipe id mappings, so the security Setup merger resolved a Grant to a
        // dead id and never re-linked its subjects. Both cycles must succeed on every adapter.
        [Fact]
        public void RepeatedSetupSucceeds()
        {
            var connectionString = this.fixture.EnsureDatabase(this.GetType().Name);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAllorsConfiguration("core", "commands");
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
            });
            var configuration = configurationBuilder.Build();

            var database = new DatabaseBuilder(
                new DefaultDatabaseServices(this.fixture.Engine),
                configuration,
                new ObjectFactory(this.fixture.MetaPopulation, typeof(C1))).Build();

            var m = this.fixture.MetaPopulation;

            for (var cycle = 0; cycle < 2; cycle++)
            {
                database.Init();

                new Setup(database, new Config()).Apply();

                using var transaction = database.CreateTransaction();
                transaction.Derive();
                transaction.Commit();

                var administrator = new PersonBuilder(transaction).WithUserName("administrator").Build();
                new UserGroups(transaction).Administrators.AddMember(administrator);
                transaction.Services.Get<IUserService>().User = administrator;

                new TestPopulation(transaction).Apply();
                transaction.Derive();
                transaction.Commit();
            }
        }

        [Theory]
        [InlineData("enroll")]
        [InlineData("renew")]
        public void PopulationCommandRequiresOfflineBeforeOpeningTheDatabase(string action)
        {
            var command = new Commands.Population(NullLogger<Commands.Population>.Instance) { Action = action };

            var exception = Assert.Throws<InvalidOperationException>(() => command.OnExecute(null));

            Assert.Contains("--offline", exception.Message);
        }

        [Fact]
        public void PopulationCommandRejectsAnUnknownActionBeforeOpeningTheDatabase()
        {
            var command = new Commands.Population(NullLogger<Commands.Population>.Instance) { Action = "complete", Offline = true };

            var exception = Assert.Throws<ArgumentException>(() => command.OnExecute(null));

            Assert.Contains("status, enroll, or renew", exception.Message);
        }

        [Fact]
        public void PopulationCommandStatusDoesNotEnrollAnUninitializedSchema()
        {
            var database = this.PopulationDatabase();

            Assert.Equal(Commands.ExitCode.Success, PopulationCommand(database, "status").OnExecute(null));

            Assert.Null(database.PopulationLifecycle.Read());
        }

        [Fact]
        public void PopulationCommandEnrollDoesNotCompleteInterruptedMaintenance()
        {
            var database = this.PopulationDatabase();
            database.Init();
            var before = database.PopulationLifecycle.Read();

            Assert.Throws<InvalidOperationException>(() => PopulationCommand(database, "enroll", true).OnExecute(null));

            var after = database.PopulationLifecycle.Read();
            Assert.Equal(before.Incarnation, after.Incarnation);
            Assert.Equal(Allors.Database.Adapters.Sql.PopulationState.Maintenance, after.State);
        }

        [Fact]
        public void PopulationCommandRenewRetainsDataAndPublishesANewIncarnation()
        {
            var database = this.PopulationDatabase();
            database.Init();
            new Setup(database, new Config()).Apply();
            long objectId;
            long objectVersion;
            using (var transaction = database.CreateTransaction())
            {
                var data = new DataBuilder(transaction).WithString("Retained during maintenance").Build();
                transaction.Derive();
                transaction.Commit();
                objectId = data.Id;
                objectVersion = data.Strategy.ObjectVersion;
            }

            var before = database.PopulationLifecycle.Read();

            Assert.Equal(Commands.ExitCode.Success, PopulationCommand(database, "renew", true).OnExecute(null));

            var after = database.PopulationLifecycle.RequireReady();
            Assert.NotEqual(before.Incarnation, after.Incarnation);
            using var verification = database.CreateTransaction();
            var retained = (Data)verification.Instantiate(objectId);
            Assert.Equal("Retained during maintenance", retained.String);
            Assert.Equal(objectVersion, retained.Strategy.ObjectVersion);
        }

        [Fact]
        public void PopulationLoadCommandPublishesReadyAfterSuccessfulImport()
        {
            var database = this.PopulationDatabase();
            database.Init();
            new Setup(database, new Config()).Apply();
            var before = database.PopulationLifecycle.Read();
            var file = Path.GetTempFileName();
            try
            {
                using (var writer = XmlWriter.Create(file))
                {
                    database.Save(writer);
                }

                var command = new Commands.Load(NullLogger<Commands.Load>.Instance) { Parent = CommandProgram(database), FileName = file };

                Assert.Equal(Commands.ExitCode.Success, command.OnExecute(null));

                var after = database.PopulationLifecycle.RequireReady();
                Assert.NotEqual(before.Incarnation, after.Incarnation);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public void PopulationLoadCommandFailureLeavesMaintenance()
        {
            var database = this.PopulationDatabase();
            database.Init();
            new Setup(database, new Config()).Apply();
            var before = database.PopulationLifecycle.Renew();
            var file = Path.GetTempFileName();
            try
            {
                File.WriteAllText(file, "<allors><population version=\"1\"><objects><database><ot");
                var command = new Commands.Load(NullLogger<Commands.Load>.Instance) { Parent = CommandProgram(database), FileName = file };

                Assert.Throws<XmlException>(() => command.OnExecute(null));

                var after = database.PopulationLifecycle.Read();
                Assert.NotEqual(before.Incarnation, after.Incarnation);
                Assert.Equal(Allors.Database.Adapters.Sql.PopulationState.Maintenance, after.State);
            }
            finally
            {
                File.Delete(file);
            }
        }

        private Allors.Database.Adapters.Sql.Database PopulationDatabase()
        {
            var configuration = new Allors.Database.Adapters.Sql.Configuration
            {
                ConnectionString = this.fixture.EnsureDatabase(this.GetType().Name),
                ObjectFactory = new ObjectFactory(this.fixture.MetaPopulation, typeof(C1)),
                SchemaName = "population_" + Guid.NewGuid().ToString("N"),
            };
            var services = new DefaultDatabaseServices(this.fixture.Engine);
            return this.fixture.Adapter.ToLowerInvariant() switch
            {
                "npgsql" => new Allors.Database.Adapters.Sql.Npgsql.Database(services, configuration),
                "sqlclient" => new Allors.Database.Adapters.Sql.SqlClient.Database(services, configuration),
                _ => throw new InvalidOperationException("Population command tests require the npgsql or sqlclient adapter."),
            };
        }

        private static Commands.Population PopulationCommand(Allors.Database.Adapters.Sql.Database database, string action, bool offline = false)
            => new Commands.Population(NullLogger<Commands.Population>.Instance) { Parent = CommandProgram(database), Action = action, Offline = offline };

        private static Commands.Program CommandProgram(Allors.Database.Adapters.Sql.Database database)
        {
            // Use the real command and a fixture-isolated database without adding a production test hook.
            var program = new Commands.Program();
            typeof(Commands.Program).GetField("database", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(program, database);
            return program;
        }
    }
}
