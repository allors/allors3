// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data;
    using Adapters;
    using Domain;
    using Microsoft.Data.SqlClient;
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
            using (var connection = new SqlConnection(database.ConnectionString))
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
            using var connection = new SqlConnection(database.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            var next = Guid.NewGuid();
            command.CommandText = "SET LOCK_TIMEOUT 200; UPDATE allors._population SET incarnation = '" + next + "'";
            var blocked = Assert.Throws<SqlException>(() => command.ExecuteNonQuery());
            Assert.Equal(1222, blocked.Number);
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
        [InlineData(IsolationLevel.Unspecified)]
        [InlineData(IsolationLevel.Chaos)]
        [InlineData(IsolationLevel.ReadUncommitted)]
        [InlineData(IsolationLevel.ReadCommitted)]
        [InlineData(IsolationLevel.RepeatableRead)]
        [InlineData(IsolationLevel.Serializable)]
        [InlineData((IsolationLevel)int.MaxValue)]
        public void RejectsNonSnapshotIsolationLevel(IsolationLevel isolationLevel)
        {
            var database = (Database)this.profile.CreateDatabase();
            var exception = Assert.Throws<ArgumentException>(() => new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = database.ObjectFactory,
                ConnectionString = database.ConnectionString,
                IsolationLevel = isolationLevel,
            }));

            Assert.Equal("configuration", exception.ParamName);
            Assert.Contains("IsolationLevel", exception.Message);
            Assert.Contains("Snapshot", exception.Message);
            Assert.Contains("unset", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(IsolationLevel.Snapshot)]
        public void TransactionUsesSnapshotAfterCommitAndRollback(IsolationLevel? isolationLevel)
        {
            var configuredDatabase = (Database)this.profile.CreateDatabase();
            var database = new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = configuredDatabase.ObjectFactory,
                ConnectionString = configuredDatabase.ConnectionString,
                IsolationLevel = isolationLevel,
            });

            var connection = database.ConnectionFactory.Create();
            try
            {
                AssertSnapshotIsolation();
                connection.Commit();
                AssertSnapshotIsolation();
                connection.Rollback();
                AssertSnapshotIsolation();
            }
            finally
            {
                connection.Rollback();
            }

            void AssertSnapshotIsolation()
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT transaction_isolation_level FROM sys.dm_exec_sessions WHERE session_id = @@SPID";

                // SQL Server reports Snapshot as 5, independently of System.Data.IsolationLevel's enum value.
                Assert.Equal(5, Convert.ToInt32(command.ExecuteScalar()));
            }
        }

        [Fact]
        public void PopulationEnrollmentPreservesDataAndIsSharedAcrossInstances()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            long id;
            long version;
            using (var transaction = database.CreateTransaction())
            {
                var value = transaction.Create<C1>();
                value.C1AllorsString = "Existing population";
                transaction.Commit();
                id = value.Id;
                version = value.Strategy.ObjectVersion;
            }

            ExecuteMetadataSql(database, $"DROP TABLE [{database.SchemaName}].[_population]");
            Assert.Null(database.PopulationLifecycle.Read());
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());

            database.PopulationLifecycle.Enroll();
            var first = database.PopulationLifecycle.RequireReady();
            Assert.NotEqual(Guid.Empty, first.Incarnation);
            database.PopulationLifecycle.Enroll();
            Assert.Equal(first.Incarnation, database.PopulationLifecycle.RequireReady().Incarnation);

            var reopened = (Database)this.profile.CreateDatabase();
            Assert.Equal(first.Incarnation, reopened.PopulationLifecycle.RequireReady().Incarnation);
            using var reader = reopened.CreateTransaction();
            var loaded = Assert.IsType<C1>(reader.Instantiate(id));
            Assert.Equal("Existing population", loaded.C1AllorsString);
            Assert.Equal(version, loaded.Strategy.ObjectVersion);
        }

        [Fact]
        public void PopulationInitRenewsIdentityAndRequiresExplicitCompletion()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var original = database.PopulationLifecycle.Read();
            Assert.Equal(PopulationState.Maintenance, original.State);
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Enroll());

            using (var maintenance = database.PopulationLifecycle.BeginMaintenance())
            {
                var started = database.PopulationLifecycle.Read().Incarnation;
                database.Init();
                Assert.Equal(started, database.PopulationLifecycle.Read().Incarnation);
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
                maintenance.Complete();
            }

            var ready = database.PopulationLifecycle.RequireReady();
            Assert.NotEqual(original.Incarnation, ready.Incarnation);
            database.Init();
            Assert.NotEqual(ready.Incarnation, database.PopulationLifecycle.Read().Incarnation);
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
        }

        [Fact]
        public void PopulationRenewPreservesDataAndRotatesIdentity()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            using (var transaction = database.CreateTransaction())
            {
                transaction.Create<C1>().C1AllorsString = "Restored contents";
                transaction.Commit();
            }

            var prior = database.PopulationLifecycle.Read().Incarnation;
            database.PopulationLifecycle.Renew();
            var renewed = database.PopulationLifecycle.RequireReady();
            Assert.NotEqual(prior, renewed.Incarnation);
            using var reader = database.CreateTransaction();
            Assert.Equal("Restored contents", Assert.Single(reader.Extent<C1>()).C1AllorsString);
        }

        [Fact]
        public void PopulationSchemasHaveIndependentIdentities()
        {
            var first = (Database)this.profile.CreateDatabase();
            first.Init();
            first.PopulationLifecycle.Renew();
            var second = new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
            {
                ObjectFactory = first.ObjectFactory,
                ConnectionString = first.ConnectionString,
                SchemaName = "population_lifecycle_second",
            });
            second.Init();
            second.PopulationLifecycle.Renew();

            Assert.NotEqual(first.PopulationLifecycle.RequireReady().Incarnation, second.PopulationLifecycle.RequireReady().Incarnation);
            var firstIncarnation = first.PopulationLifecycle.RequireReady().Incarnation;
            second.Init();
            Assert.Equal(firstIncarnation, first.PopulationLifecycle.RequireReady().Incarnation);
        }

        [Fact]
        public void PopulationMetadataRejectsEmptyAndMalformedRows()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            ExecuteMetadataSql(database, $"DELETE FROM [{database.SchemaName}].[_population]");
            var empty = Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
            Assert.Contains("population", empty.Message, StringComparison.OrdinalIgnoreCase);

            ExecuteMetadataSql(database, $"INSERT INTO [{database.SchemaName}].[_population] ([singleton], [incarnation], [state]) VALUES (1, '00000000-0000-0000-0000-000000000000', 'Maintenance')");
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
        }

        [Fact]
        public void PopulationMaintenanceCannotCompleteWithAdditionalMetadataRows()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var table = $"[{database.SchemaName}].[_population]";
            try
            {
                ExecuteMetadataSql(database, $"DROP TABLE {table}; CREATE TABLE {table} ([singleton] tinyint, [incarnation] uniqueidentifier, [state] nvarchar(16)); INSERT INTO {table} VALUES (1, NEWID(), 'Ready'), (2, NEWID(), 'Ready')");
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
                using var maintenance = database.PopulationLifecycle.BeginMaintenance();
                Assert.Throws<InvalidOperationException>(() => maintenance.Complete());
            }
            finally
            {
                ExecuteMetadataSql(database, $"DROP TABLE {table}");
                database.Init();
            }
        }

        [Fact]
        public void PopulationMetadataReportsMalformedLayoutAndRefusesCompletion()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var table = $"[{database.SchemaName}].[_population]";
            try
            {
                using var maintenance = database.PopulationLifecycle.BeginMaintenance();
                ExecuteMetadataSql(database, $"DROP TABLE {table}; CREATE TABLE {table} ([singleton] int, [incarnation] uniqueidentifier)");
                ExecuteMetadataSql(database, $"INSERT INTO {table} VALUES (1, NEWID())");
                var exception = Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
                Assert.Contains("population", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("offline", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Throws<InvalidOperationException>(() => maintenance.Complete());
            }
            finally
            {
                ExecuteMetadataSql(database, $"DROP TABLE {table}");
                database.Init();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PopulationAbandonedOwnerBlocksRenewalUntilPendingPhasesEnd(bool implicitOwner)
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var owner = implicitOwner ? (IDisposable)database.PopulationLifecycle.BeginOperation() : database.PopulationLifecycle.BeginMaintenance();
            var phase = database.PopulationLifecycle.BeginOperation();
            try
            {
                owner.Dispose();
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Renew());
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.BeginMaintenance());
                Assert.Throws<InvalidOperationException>(() => phase.Complete());
            }
            finally
            {
                phase.Dispose();
                owner.Dispose();
            }

            database.PopulationLifecycle.Renew();
            Assert.Equal(PopulationState.Ready, database.PopulationLifecycle.RequireReady().State);
        }

        [Fact]
        public void PopulationMetadataTransitionsUseSnapshotIsolation()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var trigger = $"[{database.SchemaName}].[population_requires_snapshot]";
            ExecuteMetadataSql(database, $@"CREATE TRIGGER {trigger} ON [{database.SchemaName}].[_population] AFTER INSERT, UPDATE AS
BEGIN
    IF (SELECT transaction_isolation_level FROM sys.dm_exec_sessions WHERE session_id = @@SPID) <> 5
        THROW 51000, 'Population metadata transitions require Snapshot isolation.', 1;
END");
            try
            {
                database.PopulationLifecycle.Renew();
                Assert.Equal(PopulationState.Ready, database.PopulationLifecycle.RequireReady().State);
            }
            finally
            {
                ExecuteMetadataSql(database, $"DROP TRIGGER {trigger}");
            }
        }

        [Fact]
        public void PopulationMetadataRejectsFractionalSingleton()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            var table = $"[{database.SchemaName}].[_population]";
            try
            {
                ExecuteMetadataSql(database, $"DROP TABLE {table}; CREATE TABLE {table} ([singleton] decimal(2,1), [incarnation] uniqueidentifier, [state] nvarchar(16))");
                ExecuteMetadataSql(database, $"INSERT INTO {table} VALUES (1.2, NEWID(), 'Ready')");
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.Read());
            }
            finally
            {
                ExecuteMetadataSql(database, $"DROP TABLE {table}");
                database.Init();
            }
        }

        private static void ExecuteMetadataSql(Database database, string sql)
        {
            using var connection = new SqlConnection(database.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }


        [Theory]
        [InlineData(null)]
        [InlineData(IsolationLevel.Snapshot)]
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
                ExecuteMetadataSql(database, "DROP TABLE allors._population");
            }
            else if (state == "maintenance")
            {
                using var maintenance = database.PopulationLifecycle.BeginMaintenance();
            }
            else
            {
                ExecuteMetadataSql(database, "DELETE FROM allors._population");
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
                    new SqlConnectionStringBuilder(template.ConnectionString) { ApplicationName = applicationName }.ConnectionString,
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

            using var connection = new SqlConnection(database.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COALESCE(SUM(open_transaction_count), 0) FROM sys.dm_exec_sessions WHERE program_name = @application";
            command.Parameters.Add("@application", SqlDbType.NVarChar, 128).Value = applicationName;
            Assert.Equal(0L, Convert.ToInt64(command.ExecuteScalar()));

        }

        public override void Dispose() => this.profile.Dispose();

        protected override void SwitchDatabase() => this.profile.SwitchDatabase();

        protected override IDatabase CreatePopulation() => this.profile.CreateDatabase();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();
    }
}
