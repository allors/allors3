// <copyright file="InitTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Threading;
    using Microsoft.Data.SqlClient;
    using Xunit;
    using C1 = Domain.C1;

    public class InitTest : Sql.InitTest, IClassFixture<Fixture<InitTest>>
    {
        private readonly Profile profile;

        public InitTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        // Init switches snapshot isolation on in the database of its connection, whatever the name of that
        // database: a name that needs brackets, and a name that holds a quote or a bracket itself.
        [Theory]
        [InlineData("InitTest-Name")]
        [InlineData("InitTest's")]
        [InlineData("InitTest]")]
        public void InitAllowsSnapshotIsolationWhateverTheNameOfTheDatabase(string name)
        {
            // A new database, where snapshot isolation is off.
            Provisioning.DropCreate(name);
            var database = new Profile(name).CreateDatabase();

            database.Init();

            // A transaction of the adapter is a snapshot transaction.
            using (var transaction = database.CreateTransaction())
            {
                C1.Create(transaction);
                transaction.Commit();
            }
        }

        // With read committed snapshot on, a statement that looks at a table neither sees the row that another
        // transaction has not committed nor waits for it. Init looks under a lock on the table: it waits for that
        // transaction, and empties the table once the row is committed.
        [Fact]
        public void InitWaitsForARowThatIsNotCommittedWhenReadCommittedSnapshotIsOn()
        {
            // A database of its own: the option is switched on while nothing else is connected.
            const string Name = "InitTestReadCommittedSnapshot";
            Provisioning.DropCreate(Name);
            var snapshotProfile = new Profile(Name);
            snapshotProfile.ExecuteScalar("ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE");

            var database = (Database)snapshotProfile.CreateDatabase();
            database.Init();
            var c2 = this.Table(this.M.C2);

            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    database.Init();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            var started = false;
            try
            {
                using (var connection = new SqlConnection(database.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new SqlCommand($"INSERT INTO {c2} (o) VALUES (1)", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        thread.Start();
                        started = true;
                        Assert.False(thread.Join(TimeSpan.FromSeconds(2)), "Init did not wait for the transaction that holds a row in the table.");

                        transaction.Commit();
                    }
                }
            }
            finally
            {
                // The transaction has ended, also when an assertion failed: nothing holds Init any more.
                if (started)
                {
                    thread.Join();
                }
            }

            Assert.Null(exception);
            Assert.Equal(0, snapshotProfile.ExecuteScalar($"SELECT COUNT(*) FROM {c2}"));
        }

        protected override IDatabase CreateDatabase() => this.profile.CreateDatabase();

        protected override object ExecuteScalar(string sql) => this.profile.ExecuteScalar(sql);
    }
}
