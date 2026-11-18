// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using Adapters;
    using global::Npgsql;
    using Meta;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    public class LifeCycleTest : Adapters.LifeCycleTest, IClassFixture<Fixture<LifeCycleTest>>
    {
        private readonly Profile profile;

        public LifeCycleTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override void SwitchDatabase() => this.profile.SwitchDatabase();

        protected override IDatabase CreatePopulation() => this.profile.CreateDatabase();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();

        // The adapter prepares its commands: after a transaction has read an object, its connection holds
        // prepared statements, and after it has flushed two unit roles of the object, the update of those two
        // roles is among them.
        [Fact]
        public void TransactionPreparesItsCommands()
        {
            foreach (var init in this.Inits)
            {
                init();

                var id = C1.Create(this.Transaction).Id;
                this.Transaction.Commit();

                using (var transaction = (Transaction)this.CreateTransaction())
                {
                    var c1 = (C1)transaction.Instantiate(id);
                    Assert.Null(c1.C1AllorsString);

                    Assert.NotEqual(0L, Count(transaction, "SELECT COUNT(*) FROM pg_prepared_statements"));

                    c1.C1AllorsString = "a";
                    c1.C1AllorsInteger = 1;
                    transaction.State.Flush();

                    Assert.Equal(1L, Count(transaction, "SELECT COUNT(*) FROM allors.c1 WHERE c1allorsstring = 'a' AND c1allorsinteger = 1"));
                    Assert.NotEqual(0L, Count(transaction, "SELECT COUNT(*) FROM pg_prepared_statements WHERE statement LIKE 'UPDATE%c1allorsstring%' AND statement LIKE 'UPDATE%c1allorsinteger%'"));
                }
            }
        }

        // The adapter sends statements only. Init creates the tables and their indexes: no function.
        [Fact]
        public void InitCreatesNoFunction()
        {
            foreach (var init in this.Inits)
            {
                init();

                Assert.Equal(0L, this.profile.ExecuteScalar("SELECT COUNT(*) FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace WHERE n.nspname = 'allors'"));
            }
        }

        // A prefetch leaves out an association that its transaction already knows: once a transaction has
        // prefetched the associations of an object, the same prefetch executes no command.
        [Fact]
        public void PrefetchDoesNotFetchAKnownAssociationAgain()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = C1.Create(this.Transaction);
                var c2 = C2.Create(this.Transaction);
                c1.C1C2one2one = c2;
                c1.I12C2one2one = c2;
                c1.C1C2many2one = c2;
                c1.AddC1C2many2many(c2);
                var id = c2.Id;
                this.Transaction.Commit();

                // An association in each of the four ways in which the SQL adapters store one: to-one and to-many,
                // in a column of a class table and in a relation table.
                var prefetchPolicy = new PrefetchPolicyBuilder()
                    .WithRule(m.C2.C1WhereC1C2one2one)
                    .WithRule(m.C2.I12WhereI12C2one2one)
                    .WithRule(m.C2.C1sWhereC1C2many2one)
                    .WithRule(m.C2.C1sWhereC1C2many2many)
                    .Build();

                using (var transaction = (Transaction)this.CreateTransaction())
                {
                    var role = transaction.Instantiate(id);
                    transaction.Prefetch(prefetchPolicy, role);

                    // PostgreSQL counts the executions of a prepared statement as the plans that it chose for it.
                    const string Executions = "SELECT COALESCE(SUM(generic_plans + custom_plans), 0)::bigint FROM pg_prepared_statements WHERE statement NOT LIKE '%pg_prepared_statements%'";
                    var executions = Count(transaction, Executions);
                    Assert.NotEqual(0L, executions);

                    transaction.Prefetch(prefetchPolicy, role);

                    Assert.Equal(executions, Count(transaction, Executions));
                }
            }
        }

        // An Init that recreates the schema closes the pooled connections: a statement that was prepared for the
        // earlier schema fails once the type of its result has changed. An Init of a valid schema, which only
        // empties the tables, keeps the connections and what they have prepared.
        [Fact]
        public void InitRecreatesPreparedCommands()
        {
            var connectionString = new NpgsqlConnectionStringBuilder(Provisioning.ConnectionString(nameof(LifeCycleTest)))
            {
                ApplicationName = nameof(this.InitRecreatesPreparedCommands),
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = 1,
                MaxAutoPrepare = 0,
            }.ConnectionString;

            Database original = null;
            try
            {
                foreach (var value in new object[] { "before", 42 })
                {
                    var m = new MetaBuilder().Build();
                    if (value is int)
                    {
                        // The statement keeps its text, but its result has a different column type.
                        m.C1.C1AllorsString.ObjectType = m.C1.C1AllorsInteger.ObjectType;
                    }

                    var database = new Database(new DefaultDomainDatabaseServices(), new Sql.Configuration
                    {
                        ObjectFactory = new ObjectFactory(m, typeof(C1)),
                        ConnectionString = connectionString,
                    });
                    original ??= database;
                    database.Init();

                    long id;
                    using (var transaction = database.CreateTransaction())
                    {
                        var c1 = C1.Create(transaction);
                        id = c1.Id;
                        c1.Strategy.SetUnitRole(m.C1.C1AllorsString, value);
                        transaction.Commit();
                    }

                    long backend;
                    long prepared;
                    using (var transaction = (Transaction)database.CreateTransaction())
                    {
                        var c1 = transaction.Instantiate(id);
                        Assert.Equal(value, c1.Strategy.GetUnitRole(m.C1.C1AllorsString));
                        backend = Count(transaction, "SELECT pg_backend_pid()::bigint");
                        prepared = Count(transaction, "SELECT COUNT(*) FROM pg_prepared_statements");
                        Assert.NotEqual(0L, prepared);
                    }

                    // An unchanged schema only loses its objects, so its prepared commands can stay.
                    database.Init();
                    using (var transaction = (Transaction)database.CreateTransaction())
                    {
                        Assert.Equal(backend, Count(transaction, "SELECT pg_backend_pid()::bigint"));
                        Assert.Equal(prepared, Count(transaction, "SELECT COUNT(*) FROM pg_prepared_statements"));
                    }
                }
            }
            finally
            {
                try
                {
                    original?.Init();
                }
                finally
                {
                    using var connection = new NpgsqlConnection(connectionString);
                    NpgsqlConnection.ClearPool(connection);
                }
            }
        }

        // Runs on the connection of the transaction: a prepared statement belongs to its connection.
        private static long Count(Transaction transaction, string sql)
        {
            using (var command = transaction.Connection.CreateCommand())
            {
                command.CommandText = sql;
                return (long)command.ExecuteScalar();
            }
        }
    }
}
