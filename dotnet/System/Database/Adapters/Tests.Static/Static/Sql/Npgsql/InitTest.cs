// <copyright file="InitTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Threading;
    using global::Npgsql;
    using Xunit;
    using C1 = Domain.C1;

    public class InitTest : Sql.InitTest, IClassFixture<Fixture<InitTest>>
    {
        private readonly Profile profile;

        public InitTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        // Init tells from the size of its file whether a table holds data. A partitioned table and a table with
        // a child keep rows in the files of other tables: Init empties them whatever their size.
        [Fact]
        public void APartitionedTableAndATableWithAChildAreEmptied()
        {
            var database = this.CreateDatabase();
            database.Init();

            try
            {
                // The relation table of C1.C1C2many2manies as a partitioned table, and a child of the table of C2.
                this.ExecuteScalar("DROP TABLE allors.c1c1c2many2many");
                this.ExecuteScalar("CREATE TABLE allors.c1c1c2many2many (a bigint, r bigint, CONSTRAINT pk_c1c1c2many2many PRIMARY KEY (a, r)) PARTITION BY HASH (a)");
                this.ExecuteScalar("CREATE TABLE allors.c1c1c2many2many_0 PARTITION OF allors.c1c1c2many2many FOR VALUES WITH (MODULUS 1, REMAINDER 0)");
                this.ExecuteScalar("INSERT INTO allors.c1c1c2many2many (a, r) VALUES (1, 2)");

                this.ExecuteScalar("CREATE TABLE allors.c2_child () INHERITS (allors.c2)");
                this.ExecuteScalar("INSERT INTO allors.c2_child (o) VALUES (1)");

                foreach (var table in new[] { "allors.c1c1c2many2many", "allors.c2" })
                {
                    Assert.Equal(0L, this.ExecuteScalar($"SELECT pg_relation_size('{table}')"));
                    Assert.Equal(1L, this.ExecuteScalar($"SELECT COUNT(*) FROM {table}"));
                }

                database.Init();

                Assert.Equal(0L, this.ExecuteScalar("SELECT COUNT(*) FROM allors.c1c1c2many2many"));
                Assert.Equal(0L, this.ExecuteScalar("SELECT COUNT(*) FROM allors.c2"));

                // Init has emptied the tables, not built them again.
                Assert.Equal("p", this.ExecuteScalar("SELECT relkind::text FROM pg_class WHERE oid = 'allors.c1c1c2many2many'::regclass"));
                Assert.Equal(1L, this.ExecuteScalar("SELECT COUNT(*) FROM pg_inherits WHERE inhparent = 'allors.c2'::regclass"));
            }
            finally
            {
                // The next Init builds the schema again.
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.c2_child");
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.c1c1c2many2many");
            }
        }

        // The objects table is emptied whatever its size: the numbering also starts again when the table holds no
        // page while its sequence has moved on, as after a delete of every object and a vacuum.
        [Fact]
        public void TheNumberingStartsAgainWhenTheObjectsTableHoldsNoPage()
        {
            var database = this.CreateDatabase();
            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                C1.Create(transaction);
                C1.Create(transaction);
                transaction.Commit();
            }

            // Emptied by hand, which leaves the sequence where it is.
            this.ExecuteScalar("TRUNCATE TABLE allors._o, allors.c1");
            Assert.Equal(0L, this.ExecuteScalar("SELECT pg_relation_size('allors._o')"));

            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                Assert.Equal(1, C1.Create(transaction).Id);
            }
        }

        // Init locks its tables before it looks which of them hold data. A transaction that has read a table
        // holds that table: Init waits for the transaction, and empties what the transaction then wrote.
        [Fact]
        public void InitWaitsForATransactionThatHasReadATable()
        {
            var database = (Database)this.CreateDatabase();
            database.Init();

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
                using (var connection = new NpgsqlConnection(database.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM allors.c2", connection, transaction))
                        {
                            Assert.Equal(0L, command.ExecuteScalar());
                        }

                        thread.Start();
                        started = true;
                        Assert.False(thread.Join(TimeSpan.FromSeconds(2)), "Init did not wait for the transaction that has read the table.");

                        using (var command = new NpgsqlCommand("INSERT INTO allors.c2 (o) VALUES (1)", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

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
            Assert.Equal(0L, this.ExecuteScalar("SELECT COUNT(*) FROM allors.c2"));
        }

        protected override IDatabase CreateDatabase() => this.profile.CreateDatabase();

        protected override object ExecuteScalar(string sql) => this.profile.ExecuteScalar(sql);
    }
}
