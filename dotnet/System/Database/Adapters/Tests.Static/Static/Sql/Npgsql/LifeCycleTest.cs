// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Data;
    using System;
    using Adapters;
    using Domain;
    using global::Npgsql;
    using Xunit;

    public class LifeCycleTest : Adapters.LifeCycleTest, IClassFixture<Fixture<LifeCycleTest>>
    {
        private readonly Profile profile;

        public LifeCycleTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        [Fact]
        public void ProtectedWriteCapturesChangesAcrossCheckpoints() =>
            AssertProtectedWriteCapturesChangesAcrossCheckpoints((Database)this.profile.CreateDatabase());

        [Fact]
        public void ProtectedWriteAdmissionAndDisposal() =>
            AssertProtectedWriteAdmissionAndDisposal((Database)this.profile.CreateDatabase());


        [Fact]
        public void ProtectedWriteIncludesSubmittedNoOpWithoutInventingChanges()
        {
            var database = (Database)this.profile.CreateDatabase();
            var baseline = PrepareProtectedReadPopulation(database);
            using var write = database.OpenWrite(database.PopulationLifecycle.RequireReady().Incarnation);
            write.AllowWrites();
            write.IncludeSubmitted(write.Transaction.Instantiate(baseline.SourceId).Strategy);
            write.Transaction.Checkpoint();
            write.Commit();
            var affected = Assert.Single(write.AffectedObjects);
            Assert.Equal(baseline.SourceId, affected.Id);
            Assert.Equal(AffectedObjectKind.Submitted, affected.Kind);
            Assert.Equal(database.Context().M.C1, affected.Class);
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("maintenance")]
        [InlineData("malformed")]
        public void ProtectedWriteRejectsUnavailablePopulationBeforeInitializingServices(string state)
        {
            var database = (Database)this.profile.CreateDatabase();
            PrepareProtectedReadPopulation(database);
            var population = database.PopulationLifecycle.RequireReady();
            using (var connection = new NpgsqlConnection(database.ConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = state switch
                {
                    "missing" => "DROP TABLE allors._population",
                    "maintenance" => "UPDATE allors._population SET state = 'Maintenance'",
                    "malformed" => "DELETE FROM allors._population",
                    _ => throw new ArgumentOutOfRangeException(nameof(state)),
                };
                command.ExecuteNonQuery();
            }

            var services = new ProtectedReadTestDatabaseServices(_ => throw new InvalidOperationException("Services must not initialize."));
            var writer = CreateProtectedReadDatabase(database, services);
            Assert.Throws<InvalidOperationException>(() => writer.OpenWrite(population.Incarnation));
            Assert.Equal(0, services.Initializations);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ProtectedWriteCommitFailureDistinguishesKnownAndUnknown(bool knownCommit)
        {
            var template = (Database)this.profile.CreateDatabase();
            PrepareProtectedReadPopulation(template);
            var database = new CommitFailureDatabase(template, knownCommit);
            using var write = database.OpenWrite(template.PopulationLifecycle.RequireReady().Incarnation);
            write.AllowWrites();
            Assert.Throws<InvalidOperationException>(() => write.Commit());
            Assert.Equal(knownCommit ? WriteCommitState.Committed : WriteCommitState.Unknown, write.CommitState);
            Assert.Throws<InvalidOperationException>(() => write.Commit());
            Assert.Equal(1, database.CommitAttempts);
        }

        [Fact]
        public void ProtectedWriteCacheCleanupFailureKeepsCommit()
        {
            var template = (Database)this.profile.CreateDatabase();
            PrepareProtectedReadPopulation(template);
            var expected = new InvalidOperationException("Cache cleanup failed after commit.");
            var cache = new Moq.Mock<Sql.Caching.ICache>();
            cache.Setup(v => v.OnCommit(Moq.It.IsAny<System.Collections.Generic.IList<long>>(), Moq.It.IsAny<System.Collections.Generic.IList<long>>())).Throws(expected);
            var factory = new Moq.Mock<Sql.Caching.ICacheFactory>();
            factory.Setup(v => v.CreateCache()).Returns(cache.Object);
            var database = new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = template.ObjectFactory,
                ConnectionString = template.ConnectionString,
                CacheFactory = factory.Object,
            });
            using var write = database.OpenWrite(template.PopulationLifecycle.RequireReady().Incarnation);
            write.AllowWrites();
            var value = write.Transaction.Create<C1>();
            var id = value.Id;
            value.C1AllorsString = "Committed before cache cleanup";
            Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => write.Commit()));
            Assert.Equal(WriteCommitState.Committed, write.CommitState);
            using var check = template.CreateTransaction();
            Assert.Equal("Committed before cache cleanup", ((C1)check.Instantiate(id)).C1AllorsString);
        }

        private sealed class CommitFailureDatabase : Database
        {
            private readonly bool knownCommit;

            internal CommitFailureDatabase(Database template, bool knownCommit)
                : base(new DefaultDomainDatabaseServices(), new Sql.Configuration
                {
                    ObjectFactory = template.ObjectFactory,
                    ConnectionString = template.ConnectionString,
                }) => this.knownCommit = knownCommit;

            internal int CommitAttempts { get; private set; }

            public override WriteScope OpenWrite(Guid expectedIncarnation) =>
                this.CreateWriteScope(new Moq.Mock<IConnection>().Object,
                    new PopulationMetadata(expectedIncarnation, PopulationState.Ready), expectedIncarnation,
                    committed =>
                    {
                        ++this.CommitAttempts;
                        if (this.knownCommit)
                        {
                            committed();
                        }

                        throw new InvalidOperationException("Connection commit or cleanup failed.");
                    });
        }

        [Theory]
        [InlineData("commit")]
        [InlineData("rollback")]
        [InlineData("connection-commit")]
        [InlineData("connection-rollback")]
        [InlineData("connection-command")]
        public void ProtectedWriteRejectsDirectLifecycleOperations(string operation) =>
            AssertProtectedWriteRejectsDirectLifecycleOperations((Database)this.profile.CreateDatabase(), operation);

        [Theory]
        [InlineData("create")]
        [InlineData("commit")]
        [InlineData("checkpoint")]
        public void ProtectedWriteInitializationCannotMutate(string operation)
        {
            var database = (Database)this.profile.CreateDatabase();
            AssertProtectedWriteInitializationCannotMutate(database, services => CreateProtectedReadDatabase(database, services), operation);
        }

        [Fact]
        public void ProtectedWriteCleanupFailureKeepsCommit()
        {
            var database = (Database)this.profile.CreateDatabase();
            AssertProtectedWriteCleanupFailureKeepsCommit(database, services => CreateProtectedReadDatabase(database, services));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(IsolationLevel.Snapshot, true)]
        [InlineData(IsolationLevel.RepeatableRead, true)]
        [InlineData(IsolationLevel.ReadCommitted, false)]
        public void ReadScopeAcrossCommit(IsolationLevel? isolationLevel, bool retainsSnapshot)
        {
            var writerDatabase = (Database)this.profile.CreateDatabase();
            var readerDatabase = new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = writerDatabase.ObjectFactory,
                ConnectionString = writerDatabase.ConnectionString,
                IsolationLevel = isolationLevel,
            });

            AssertReadVisibilityAcrossCommit(readerDatabase, writerDatabase, retainsSnapshot);
        }


        [Theory]
        [InlineData(null)]
        [InlineData(IsolationLevel.Snapshot)]
        [InlineData(IsolationLevel.RepeatableRead)]
        public void ProtectedReadRetainsSnapshotBeforeFirstDomainRead(IsolationLevel? isolationLevel)
        {
            var writer = (Database)this.profile.CreateDatabase();
            var reader = CreateProtectedReadDatabase(writer, isolationLevel: isolationLevel);
            AssertProtectedReadRetainsSnapshot(writer, reader);
        }

        [Theory]
        [InlineData("create")]
        [InlineData("create-many")]
        [InlineData("set-unit")]
        [InlineData("remove-unit")]
        [InlineData("set-composite")]
        [InlineData("remove-composite")]
        [InlineData("add-composites")]
        [InlineData("remove-composites")]
        [InlineData("set-composites")]
        [InlineData("clear-composites")]
        [InlineData("delete")]
        [InlineData("commit")]
        [InlineData("rollback")]
        [InlineData("dispose-transaction")]
        [InlineData("connection-command")]
        [InlineData("connection-commit")]
        [InlineData("connection-rollback")]
        [InlineData("require-flush")]
        [InlineData("new-state")]
        [InlineData("services-init")]
        [InlineData("services-dispose")]
        public void ProtectedReadRejectsMutationsAndKeepsTaint(string operation)
        {
            var writer = (Database)this.profile.CreateDatabase();
            var reader = CreateProtectedReadDatabase(writer);
            AssertProtectedReadRejectsMutation(writer, reader, operation);
        }

        [Fact]
        public void ProtectedReadDisposalPreventsCachedAndNewReads()
        {
            var database = (Database)this.profile.CreateDatabase();
            AssertProtectedReadRejectsAccessAfterDisposal(database);
        }

        [Fact]
        public void ProtectedReadEstablishesSnapshotBeforeServiceInitialization()
        {
            var writer = (Database)this.profile.CreateDatabase();
            AssertProtectedReadInitializesServicesAfterSnapshot(writer, services => CreateProtectedReadDatabase(writer, services));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ProtectedReadServiceMutationCannotEscapeAndCleansUp(bool catchViolation)
        {
            var writer = (Database)this.profile.CreateDatabase();
            AssertProtectedReadInitializationFailureCleansUp(writer, services => CreateProtectedReadDatabase(writer, services), catchViolation);
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("maintenance")]
        [InlineData("malformed")]
        public void ProtectedReadRejectsUnavailablePopulationBeforeInitializingServices(string state)
        {
            var database = (Database)this.profile.CreateDatabase();
            PrepareProtectedReadPopulation(database);
            if (state == "missing")
            {
                ExecutePopulationSql(database, "DROP TABLE allors._population");
            }
            else if (state == "maintenance")
            {
                using var maintenance = database.PopulationLifecycle.BeginMaintenance();
            }
            else
            {
                ExecutePopulationSql(database, "DELETE FROM allors._population");
            }

            var services = new ProtectedReadTestDatabaseServices(_ => throw new InvalidOperationException("Services must not initialize."));
            var reader = CreateProtectedReadDatabase(database, services);
            Assert.ThrowsAny<InvalidOperationException>(() => reader.OpenRead());
            Assert.Equal(0, services.Initializations);
        }

        [Fact]
        public void ProtectedReadRejectsCustomConnectionFactory()
        {
            var database = (Database)this.profile.CreateDatabase();
            PrepareProtectedReadPopulation(database);
            var factory = new Moq.Mock<Sql.IConnectionFactory>();
            database.ConnectionFactory = factory.Object;
            Assert.ThrowsAny<InvalidOperationException>(() => database.OpenRead());
            factory.Verify(v => v.Create(), Moq.Times.Never());
        }

        private static Database CreateProtectedReadDatabase(
            Database template,
            IDatabaseServices services = null,
            IsolationLevel? isolationLevel = null,
            string applicationName = null) =>
            new Database(services ?? new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = template.ObjectFactory,
                ConnectionString = applicationName == null ? template.ConnectionString :
                    new NpgsqlConnectionStringBuilder(template.ConnectionString) { ApplicationName = applicationName }.ConnectionString,
                SchemaName = template.SchemaName,
                IsolationLevel = isolationLevel,
            });

        [Fact]
        public void ProtectedReadServiceDisposalFailureStillClosesNativeTransaction()
        {
            var writer = (Database)this.profile.CreateDatabase();
            var applicationName = "allors-read-cleanup-" + Guid.NewGuid().ToString("N");
            AssertProtectedReadCleanupFailureEndsScope(
                writer,
                services => CreateProtectedReadDatabase(writer, services, applicationName: applicationName),
                () => AssertNoActiveProtectedReadTransaction(writer, applicationName));
        }

        [Fact]
        public void ProtectedReadCaughtServiceCleanupMutationStillFailsAndClosesNativeTransaction()
        {
            var writer = (Database)this.profile.CreateDatabase();
            var applicationName = "allors-read-cleanup-" + Guid.NewGuid().ToString("N");
            AssertProtectedReadCaughtCleanupMutationEndsScope(
                writer,
                services => CreateProtectedReadDatabase(writer, services, applicationName: applicationName),
                () => AssertNoActiveProtectedReadTransaction(writer, applicationName));
        }

        [Fact]
        public void ProtectedReadKeepsBinaryValuesIndependentFromLegacyCacheAndReturnedArrays() =>
            AssertProtectedReadKeepsBinaryValuesIndependent((Database)this.profile.CreateDatabase());

        [Fact]
        public void ProtectedReadDisposalPreventsRetainedExtentAndEnumeratorAccess() =>
            AssertProtectedReadRejectsRetainedExtentAccess((Database)this.profile.CreateDatabase());

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ProtectedReadRejectsReferencesCreatedByForeignState(bool newObject) =>
            AssertProtectedReadRejectsForeignStateReferences((Database)this.profile.CreateDatabase(), newObject);

        [Theory]
        [InlineData("union")]
        [InlineData("intersect")]
        [InlineData("except")]
        public void ProtectedReadRejectsExtentOperandsFromAmbientTransaction(string operation) =>
            AssertProtectedReadRejectsForeignExtentOperands((Database)this.profile.CreateDatabase(), operation);

        private static void AssertNoActiveProtectedReadTransaction(Database database, string applicationName)
        {

            using var connection = new NpgsqlConnection(database.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT count(*) FROM pg_stat_activity WHERE application_name = @application AND xact_start IS NOT NULL";
            command.Parameters.AddWithValue("application", applicationName);
            Assert.Equal(0L, Convert.ToInt64(command.ExecuteScalar()));

        }

        [Theory]
        [InlineData(IsolationLevel.Unspecified)]
        [InlineData(IsolationLevel.Chaos)]
        [InlineData(IsolationLevel.ReadUncommitted)]
        [InlineData(IsolationLevel.ReadCommitted)]
        [InlineData(IsolationLevel.Serializable)]
        [InlineData((IsolationLevel)int.MaxValue)]
        public void ProtectedReadRejectsUnverifiedIsolationBeforeInitializingServices(IsolationLevel isolationLevel)
        {
            var database = (Database)this.profile.CreateDatabase();
            PrepareProtectedReadPopulation(database);
            var services = new ProtectedReadTestDatabaseServices(_ => throw new InvalidOperationException("Services must not initialize."));
            var reader = CreateProtectedReadDatabase(database, services, isolationLevel);
            var exception = Assert.ThrowsAny<InvalidOperationException>(() => reader.OpenRead());
            Assert.Contains("isolation", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, services.Initializations);
        }

        public override void Dispose() => this.profile.Dispose();

        protected override void SwitchDatabase() => this.profile.SwitchDatabase();

        protected override IDatabase CreatePopulation() => this.profile.CreateDatabase();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();

        [Fact]
        public void PopulationLifecycleEnrollsWithoutChangingCommittedObjects()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            long id;
            long version;
            using (var transaction = database.CreateTransaction())
            {
                var value = transaction.Create<C1>();
                value.C1AllorsString = "Preserved by enrollment";
                transaction.Commit();
                id = value.Id;
                version = value.Strategy.ObjectVersion;
            }

            ExecutePopulationSql(database, "DROP TABLE allors._population");
            Assert.Null(database.PopulationLifecycle.Read());
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
            Assert.Null(database.PopulationLifecycle.Read());
            database.PopulationLifecycle.Enroll();
            var enrolled = database.PopulationLifecycle.RequireReady();
            Assert.NotEqual(Guid.Empty, enrolled.Incarnation);
            Assert.Equal(PopulationState.Ready, enrolled.State);
            database.PopulationLifecycle.Enroll();
            Assert.Equal(enrolled.Incarnation, database.PopulationLifecycle.RequireReady().Incarnation);

            var reopened = (Database)this.profile.CreateDatabase();
            Assert.Equal(enrolled.Incarnation, reopened.PopulationLifecycle.RequireReady().Incarnation);
            using var verification = reopened.CreateTransaction();
            var preserved = Assert.IsType<C1>(verification.Instantiate(id));
            Assert.Equal(version, preserved.Strategy.ObjectVersion);
            Assert.Equal("Preserved by enrollment", preserved.C1AllorsString);
        }

        [Fact]
        public void PopulationLifecycleInitRequiresExplicitCompletionAfterSetup()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var first = database.PopulationLifecycle.Read();
            Assert.Equal(PopulationState.Maintenance, first.State);
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Enroll());
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());

            using (var maintenance = database.PopulationLifecycle.BeginMaintenance())
            {
                var started = database.PopulationLifecycle.Read();
                Assert.NotEqual(first.Incarnation, started.Incarnation);
                database.Init();
                Assert.Equal(started.Incarnation, database.PopulationLifecycle.Read().Incarnation);
                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
                using (var setup = database.CreateTransaction())
                {
                    setup.Create<C1>().C1AllorsString = "Setup committed";
                    setup.Commit();
                }

                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
                maintenance.Complete();
            }

            var ready = database.PopulationLifecycle.RequireReady();
            database.Init();
            Assert.NotEqual(ready.Incarnation, database.PopulationLifecycle.Read().Incarnation);
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
        }

        [Fact]
        public void PopulationLifecycleAbandonedAndSupersededOwnersCannotPublish()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            using (database.PopulationLifecycle.BeginMaintenance())
            {
                database.Init();
            }

            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
            using (var owner = database.PopulationLifecycle.BeginMaintenance())
            {
                var another = (Database)this.profile.CreateDatabase();
                another.PopulationLifecycle.Renew();
                Assert.Throws<InvalidOperationException>(() => owner.Complete());
            }

            Assert.Equal(PopulationState.Ready, database.PopulationLifecycle.RequireReady().State);
        }

        [Fact]
        public void PopulationLifecycleSeparatesSchemasAndSurvivesDomainRebuild()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            database.PopulationLifecycle.Renew();
            var first = database.PopulationLifecycle.RequireReady();
            var separate = new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = database.ObjectFactory,
                ConnectionString = database.ConnectionString,
                SchemaName = "lifecycle_other",
            });
            using (var owner = separate.PopulationLifecycle.BeginMaintenance())
            {
                separate.Init();
                owner.Complete();
            }

            Assert.NotEqual(first.Incarnation, separate.PopulationLifecycle.RequireReady().Incarnation);
            ExecutePopulationSql(database, "DROP TABLE allors._o");
            using (var owner = database.PopulationLifecycle.BeginMaintenance())
            {
                var rebuilding = database.PopulationLifecycle.Read().Incarnation;
                database.Init();
                Assert.Equal(rebuilding, database.PopulationLifecycle.Read().Incarnation);
                owner.Complete();
            }

            Assert.NotEqual(first.Incarnation, database.PopulationLifecycle.RequireReady().Incarnation);
        }

        [Fact]
        public void PopulationLifecycleRejectsMalformedMetadata()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            ExecutePopulationSql(database, "DELETE FROM allors._population");
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ProtectedWritePopulationFenceBlocksMaintenanceUntilEnd(bool commit)
        {
            var database = (Database)this.profile.CreateDatabase();
            var baseline = PrepareProtectedReadPopulation(database);
            var population = database.PopulationLifecycle.RequireReady();
            using var write = database.OpenWrite(population.Incarnation);
            write.AllowWrites();
            ((C1)write.Transaction.Instantiate(baseline.SourceId)).C1AllorsString = "Admitted write";
            using var connection = new NpgsqlConnection(database.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            var next = Guid.NewGuid();
            command.CommandText = "SET lock_timeout = '200ms'; UPDATE allors._population SET incarnation = '" + next + "'";
            var blocked = Assert.Throws<PostgresException>(() => command.ExecuteNonQuery());
            Assert.Equal("55P03", blocked.SqlState);
            if (commit)
            {
                write.Commit();
            }
            else
            {
                write.Dispose();
            }

            Assert.Equal(1, command.ExecuteNonQuery());
            Assert.Throws<InvalidOperationException>(() => database.OpenWrite(population.Incarnation));
            using var check = database.CreateTransaction();
            Assert.Equal(commit ? "Admitted write" : "Committed baseline", ((C1)check.Instantiate(baseline.SourceId)).C1AllorsString);
        }

        private static void ExecutePopulationSql(Database database, string sql)
        {
            using var connection = new NpgsqlConnection(database.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        [Fact]
        public void PopulationLifecycleRejectsActiveFailedAndFinishedOwners()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            using (var owner = database.PopulationLifecycle.BeginMaintenance())
            {
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.BeginMaintenance());
                using var active = database.PopulationLifecycle.BeginOperation();
                Assert.Throws<InvalidOperationException>(() => owner.Complete());
            }

            using (var owner = database.PopulationLifecycle.BeginMaintenance())
            {
                using (database.PopulationLifecycle.BeginOperation())
                {
                }

                Assert.Throws<InvalidOperationException>(() => owner.Complete());
            }

            using (var owner = database.PopulationLifecycle.BeginMaintenance())
            {
                owner.Complete();
                Assert.Throws<InvalidOperationException>(() => owner.Complete());
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.BeginOperation());
            }

            var disposed = database.PopulationLifecycle.BeginMaintenance();
            disposed.Dispose();
            Assert.Throws<InvalidOperationException>(() => disposed.Complete());
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
        }

        [Fact]
        public void PopulationLifecycleCompletionValidatesCurrentSchema()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            Assert.True(database.IsValid);
            using (var owner = database.PopulationLifecycle.BeginMaintenance())
            {
                ExecutePopulationSql(database, "DROP TABLE allors._o");
                Assert.Throws<InvalidOperationException>(() => owner.Complete());
            }

            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
            database.Init();
        }

        [Fact]
        public void PopulationLifecycleCannotCompleteMultipleMetadataRows()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            try
            {
                using var owner = database.PopulationLifecycle.BeginMaintenance();
                var incarnation = database.PopulationLifecycle.Read().Incarnation;
                ExecutePopulationSql(database, $@"DROP TABLE allors._population;
CREATE TABLE allors._population (singleton SMALLINT PRIMARY KEY, incarnation UUID NOT NULL, state VARCHAR(11) NOT NULL);
INSERT INTO allors._population VALUES (1, '{incarnation:D}', 'Maintenance'), (2, '{Guid.NewGuid():D}', 'Ready');");
                Assert.Throws<InvalidOperationException>(() => owner.Complete());
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
            }
            finally
            {
                ExecutePopulationSql(database, "DROP TABLE allors._population");
            }
        }

        [Fact]
        public void PopulationLifecycleMalformedMetadataSchemaHasActionableDiagnostic()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            try
            {
                ExecutePopulationSql(database, "DROP TABLE allors._population; CREATE TABLE allors._population (singleton SMALLINT)");
                var exception = Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
                Assert.Contains("malformed", exception.Message);
                Assert.Contains("repair", exception.Message);
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.BeginMaintenance());
            }
            finally
            {
                ExecutePopulationSql(database, "DROP TABLE allors._population");
            }
        }

        [Fact]
        public void PopulationLifecycleRejectsFractionalSingletonMetadata()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            try
            {
                ExecutePopulationSql(database, $@"DROP TABLE allors._population;
CREATE TABLE allors._population (singleton NUMERIC PRIMARY KEY, incarnation UUID NOT NULL, state VARCHAR(11) NOT NULL);
INSERT INTO allors._population VALUES (1.2, '{Guid.NewGuid():D}', 'Ready');");
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
            }
            finally
            {
                ExecutePopulationSql(database, "DROP TABLE allors._population");
            }
        }
    }
}
