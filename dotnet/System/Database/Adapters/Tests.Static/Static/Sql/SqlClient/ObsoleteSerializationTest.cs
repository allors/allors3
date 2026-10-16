// <copyright file="SerializationTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Adapters;
    using Domain;
    using Microsoft.Data.SqlClient;
    using Xunit;

    public class ObsoleteSerializationTest : Adapters.ObsoleteSerializationTest, IClassFixture<Fixture<ObsoleteSerializationTest>>
    {
        private readonly Profile profile;

        public ObsoleteSerializationTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreatePopulation() => this.profile.CreateMemoryDatabase();

        [Fact]
        public void LoadUsesSnapshotForBulkCopyAndClassInsertion()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                using var connection = new SqlConnection(database.ConnectionString);
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE";
                    command.ExecuteNonQuery();
                }

                var sessionId = GetSessionId(connection);
                Assert.Equal(4, GetIsolationLevel(connection, sessionId));
                using var observer = new SqlConnection(database.ConnectionString);
                observer.Open();
                var callbackCount = 0;
                var load = new Load(database, connection, (_, args) =>
                {
                    Assert.Equal(3, args.ObjectId);
                    callbackCount++;
                    // The callback runs while the bulk reader is active, so inspect its
                    // session through a separate connection instead of issuing a nested command.
                    Assert.Equal(5, GetIsolationLevel(observer, sessionId));
                }, null);

                using var reader = XmlReader.Create(new StringReader(CreateObjectXml(database, 2, true)));
                load.Execute(reader);

                Assert.Equal(1, callbackCount);
                Assert.Equal(5, GetIsolationLevel(connection, sessionId));
                using var transaction = database.CreateTransaction();
                Assert.IsType<C1>(transaction.Instantiate(1));
                Assert.IsType<C1>(transaction.Instantiate(2));
                Assert.Null(transaction.Instantiate(3));
                Assert.Equal(2, transaction.Extent<C1>().Count);
            }
        }

        [Fact]
        public void DirectLoadKeepsCompletedBulkBatchWhenObjectCallbackFails()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                using var connection = new SqlConnection(database.ConnectionString);
                connection.Open();
                var failure = new InvalidOperationException("Expected object-load callback failure.");
                var callbackCount = 0;
                var load = new Load(database, connection, (_, args) =>
                {
                    Assert.Equal(5002, args.ObjectId);
                    callbackCount++;
                    throw failure;
                }, null);

                using var reader = XmlReader.Create(new StringReader(CreateObjectXml(database, 5001, false)));
                Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => load.Execute(reader)));
                Assert.Equal(1, callbackCount);

                // The first 5,000-row bulk batch commits before the next batch's callback
                // fails. Class-table insertion starts only after the entire bulk copy succeeds.
                using var observer = new SqlConnection(database.ConnectionString);
                observer.Open();
                Assert.Equal(5000, CountRows(observer, database.Mapping.TableNameForObjects));
                var m = database.Context().M;
                Assert.Equal(0, CountRows(observer, database.Mapping.TableNameForObjectByClass[m.C1]));
            }
        }

        [Fact]
        public void DatabaseLoadClearsCompletedBulkBatchWhenObjectCallbackFails()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                var failure = new InvalidOperationException("Expected object-load callback failure.");
                var callbackCount = 0;
                database.ObjectNotLoaded += (_, args) =>
                {
                    Assert.Equal(5002, args.ObjectId);
                    callbackCount++;
                    throw failure;
                };

                using var reader = XmlReader.Create(new StringReader(CreateObjectXml(database, 5001, false)));
                Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => database.Load(reader)));
                Assert.Equal(1, callbackCount);

                using var transaction = database.CreateTransaction();
                Assert.Empty(transaction.Extent<C1>());
                Assert.Null(transaction.Instantiate(1));
                Assert.Null(transaction.Instantiate(5000));
                using var observer = new SqlConnection(database.ConnectionString);
                observer.Open();
                Assert.Equal(0, CountRows(observer, database.Mapping.TableNameForObjects));
            }
        }

        [Theory]
        [InlineData("<allors />")]
        [InlineData("<allors><objects /></allors>")]
        public void DirectLoadRejectsMissingPopulation(string xml)
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                using var connection = new SqlConnection(database.ConnectionString);
                connection.Open();
                using var reader = XmlReader.Create(new StringReader(xml));

                var exception = Assert.Throws<ArgumentException>(() => new Load(database, connection, null, null).Execute(reader));
                Assert.Contains("population", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FailedRelationSqlCannotCompletePopulationMaintenance(bool composite)
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                using var maintenance = database.PopulationLifecycle.BeginMaintenance();
                var m = database.Context().M;
                var relationType = composite ? m.C1.C1C1one2one.RelationType : m.C1.C1AllorsString.RelationType;
                var procedure = composite
                    ? database.Mapping.ProcedureNameForSetRoleByRelationType[relationType]
                    : database.Mapping.ProcedureNameForSetUnitRoleByRelationTypeByClass[m.C1][relationType];
                using var connection = new SqlConnection(database.ConnectionString);
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"DROP PROCEDURE {procedure}";
                    command.ExecuteNonQuery();
                }

                var relationElement = composite ? "rtc" : "rtu";
                var value = composite ? "2" : "QSBTdHJpbmc=";
                var xml = $"<allors><population version=\"1\"><objects><database><ot i=\"{m.C1.Id:N}\">1:0,2:0</ot></database></objects><relations><database><{relationElement} i=\"{relationType.Id:N}\"><r a=\"1\">{value}</r></{relationElement}></database></relations></population></allors>";
                using var reader = XmlReader.Create(new StringReader(xml));

                new Load(database, connection, null, null).Execute(reader);
                Assert.Throws<InvalidOperationException>(() => maintenance.Complete());
                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
            }
        }

        [Fact]
        public void PopulationImportRetainsIdsAndVersionsWithoutImportingIncarnation()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                var original = this.Transaction.Create<C1>();
                original.C1AllorsString = "Saved contents";
                this.Transaction.Commit();
                var originalId = original.Id;
                var originalVersion = original.Strategy.ObjectVersion;
                var originalIncarnation = database.PopulationLifecycle.Read().Incarnation;
                var text = new StringWriter();
                using (var writer = XmlWriter.Create(text))
                {
                    database.Save(writer);
                }

                var xml = text.ToString();
                Assert.DoesNotContain(originalIncarnation.ToString("D"), xml, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(originalIncarnation.ToString("N"), xml, StringComparison.OrdinalIgnoreCase);
                this.Transaction.Rollback();
                using (var maintenance = database.PopulationLifecycle.BeginMaintenance())
                {
                    var incarnation = database.PopulationLifecycle.Read().Incarnation;
                    using var reader = XmlReader.Create(new StringReader(xml));
                    database.Load(reader);
                    Assert.Equal(incarnation, database.PopulationLifecycle.Read().Incarnation);
                    Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
                    maintenance.Complete();
                }

                Assert.NotEqual(originalIncarnation, database.PopulationLifecycle.RequireReady().Incarnation);
                using var transaction = database.CreateTransaction();
                var restored = Assert.IsType<C1>(transaction.Instantiate(originalId));
                Assert.Equal(originalVersion, restored.Strategy.ObjectVersion);
                Assert.Equal("Saved contents", restored.C1AllorsString);
            }
        }

        [Fact]
        public void FailedPopulationImportCannotCompleteAfterCleanupInit()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                var failure = new InvalidOperationException("Expected lifecycle callback failure.");
                database.ObjectNotLoaded += (_, _) => throw failure;
                Guid failedIncarnation;
                using (var maintenance = database.PopulationLifecycle.BeginMaintenance())
                {
                    failedIncarnation = database.PopulationLifecycle.Read().Incarnation;
                    using var reader = XmlReader.Create(new StringReader(CreateObjectXml(database, 2, false)));
                    Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => database.Load(reader)));
                    Assert.Equal(failedIncarnation, database.PopulationLifecycle.Read().Incarnation);
                    Assert.Throws<InvalidOperationException>(() => maintenance.Complete());
                }

                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
                database.PopulationLifecycle.Renew();
                Assert.NotEqual(failedIncarnation, database.PopulationLifecycle.RequireReady().Incarnation);
                using var transaction = database.CreateTransaction();
                Assert.Empty(transaction.Extent<C1>());
            }
        }

        [Fact]
        public void SuccessfulDirectPopulationImportRemainsInMaintenance()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                database.PopulationLifecycle.Renew();
                var previous = database.PopulationLifecycle.RequireReady().Incarnation;
                using var connection = new SqlConnection(database.ConnectionString);
                connection.Open();
                using var reader = XmlReader.Create(new StringReader(CreateObjectXml(database, 2, false)));
                new Load(database, connection, (_, _) => { }, null).Execute(reader);

                Assert.NotEqual(previous, database.PopulationLifecycle.Read().Incarnation);
                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
                Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
                using var transaction = database.CreateTransaction();
                Assert.Equal(2, transaction.Extent<C1>().Count);
            }
        }

        [Fact]
        public void EmptyPopulationWithMalformedTrailingXmlCannotCompleteMaintenance()
        {
            foreach (var init in this.Inits)
            {
                init();
                var database = (Database)this.Population;
                using var maintenance = database.PopulationLifecycle.BeginMaintenance();
                using var connection = new SqlConnection(database.ConnectionString);
                connection.Open();
                using var reader = XmlReader.Create(new StringReader("<allors><population version=\"1\" /></allors><broken"));

                Assert.Throws<XmlException>(() => new Load(database, connection, null, null).Execute(reader));
                Assert.Throws<InvalidOperationException>(() => maintenance.Complete());
                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
            }
        }

        private static string CreateObjectXml(Database database, int count, bool unknownFirst)
        {
            var m = database.Context().M;
            var known = $"<ot i=\"{m.C1.Id:N}\">{string.Join(",", Enumerable.Range(1, count).Select(v => $"{v}:0"))}</ot>";
            var unknown = $"<ot i=\"71000000000000000000000000000000\">{count + 1}:0</ot>";
            return $"<allors><population version=\"1\"><objects><database>{(unknownFirst ? unknown + known : known + unknown)}</database></objects></population></allors>";
        }

        private static int GetSessionId(SqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT @@SPID";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static int GetIsolationLevel(SqlConnection connection, int sessionId)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT transaction_isolation_level FROM sys.dm_exec_sessions WHERE session_id = @sessionId";
            command.Parameters.Add("@sessionId", SqlDbType.Int).Value = sessionId;
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static int CountRows(SqlConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
