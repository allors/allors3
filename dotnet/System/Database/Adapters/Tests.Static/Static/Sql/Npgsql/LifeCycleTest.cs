// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Linq;
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

        // The adapter sends statements and calls no function of the schema: with the functions gone, transactions
        // create, read, prefetch, change and delete objects as before.
        [Fact]
        public void TransactionCallsNoFunction()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // The database checked its schema for its first transaction, and does not check it again.
                const string Functions = "SELECT COUNT(*) FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace WHERE n.nspname = 'allors'";
                Assert.NotEqual(0L, this.profile.ExecuteScalar(Functions));
                this.profile.ExecuteScalar(@"
DO $$
DECLARE routine regprocedure;
BEGIN
    FOR routine IN SELECT p.oid FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace WHERE n.nspname = 'allors' LOOP
        EXECUTE 'DROP FUNCTION ' || routine;
    END LOOP;
END
$$");
                Assert.Equal(0L, this.profile.ExecuteScalar(Functions));

                // Two objects with a role in each of the five ways in which the adapter stores a composite role, and
                // with unit roles: the first is read and changed role by role, the second is prefetched.
                var c1Ids = new long[2];
                var c2Ids = new long[4];
                using (var transaction = this.CreateTransaction())
                {
                    // An object by itself, and objects by their count.
                    var c1s = new[] { C1.Create(transaction), C1.Create(transaction) };
                    var c2s = (C2[])transaction.Create(m.C2, c2Ids.Length);

                    for (var i = 0; i < c1s.Length; i++)
                    {
                        var c1 = c1s[i];
                        var first = c2s[2 * i];
                        var second = c2s[(2 * i) + 1];

                        c1.C1AllorsString = "a";
                        c1.C1C2one2one = first;
                        c1.C1C2many2one = first;
                        c1.AddC1C2one2many(first);
                        c1.AddC1C2one2many(second);
                        c1.AddC1C2many2many(first);
                        c1.AddC1C2many2many(second);
                        c1.I12C2one2one = first;

                        c1Ids[i] = c1.Id;
                        c2Ids[2 * i] = first.Id;
                        c2Ids[(2 * i) + 1] = second.Id;
                    }

                    transaction.Commit();
                }

                using (var transaction = this.CreateTransaction())
                {
                    // An object by its id, and objects by their ids.
                    var c1 = (C1)transaction.Instantiate(c1Ids[0]);
                    var c2s = transaction.Instantiate(c2Ids).Cast<C2>().ToArray();

                    // The roles and the associations of the first object, each with a command of its own.
                    Assert.Equal("a", c1.C1AllorsString);
                    Assert.Equal(c2s[0], c1.C1C2one2one);
                    Assert.Equal(c2s[0], c1.C1C2many2one);
                    Assert.Equal(2, c1.C1C2one2manies.Count());
                    Assert.Equal(2, c1.C1C2many2manies.Count());
                    Assert.Equal(c2s[0], c1.I12C2one2one);
                    Assert.Equal(c1, c2s[0].C1WhereC1C2one2one);
                    Assert.Equal(c1, c2s[0].C1sWhereC1C2many2one.Single());
                    Assert.Equal(c1, c2s[0].C1WhereC1C2one2many);
                    Assert.Equal(c1, c2s[0].C1sWhereC1C2many2many.Single());
                    Assert.Equal(c1, c2s[0].I12WhereI12C2one2one);

                    // The roles and the associations of the second object, prefetched.
                    var prefetched = (C1)transaction.Instantiate(c1Ids[1]);
                    transaction.Prefetch(
                        new PrefetchPolicyBuilder()
                            .WithRule(m.C1.C1AllorsString)
                            .WithRule(m.C1.C1C2one2one)
                            .WithRule(m.C1.C1C2many2one)
                            .WithRule(m.C1.C1C2one2manies)
                            .WithRule(m.C1.C1C2many2manies)
                            .WithRule(m.C1.I12C2one2one)
                            .Build(),
                        prefetched);
                    transaction.Prefetch(
                        new PrefetchPolicyBuilder()
                            .WithRule(m.C2.C1WhereC1C2one2one)
                            .WithRule(m.C2.C1sWhereC1C2many2one)
                            .WithRule(m.C2.C1WhereC1C2one2many)
                            .WithRule(m.C2.C1sWhereC1C2many2many)
                            .WithRule(m.C2.I12WhereI12C2one2one)
                            .Build(),
                        c2s[2]);
                    Assert.Equal("a", prefetched.C1AllorsString);
                    Assert.Equal(c2s[2], prefetched.C1C2one2one);
                    Assert.Equal(2, prefetched.C1C2many2manies.Count());
                    Assert.Equal(prefetched, c2s[2].C1WhereC1C2one2many);
                    Assert.Equal(prefetched, c2s[2].I12WhereI12C2one2one);

                    // One unit role and two, and a to-many role that loses one of its two objects.
                    c1.C1AllorsString = "b";
                    prefetched.C1AllorsString = "c";
                    prefetched.C1AllorsInteger = 1;
                    c1.RemoveC1C2one2many(c2s[1]);
                    c1.RemoveC1C2many2many(c2s[1]);
                    transaction.Commit();

                    // After a commit the transaction asks for the version of an object again.
                    Assert.Equal("b", c1.C1AllorsString);
                    Assert.Equal(c2s[0], c1.C1C2one2manies.Single());
                    Assert.Equal(c2s[0], c1.C1C2many2manies.Single());
                    Assert.Equal(1, prefetched.C1AllorsInteger);

                    // Every role of the first object goes.
                    c1.RemoveC1C2one2one();
                    c1.RemoveC1C2many2one();
                    c1.RemoveC1C2one2manies();
                    c1.RemoveC1C2many2manies();
                    c1.RemoveI12C2one2one();
                    transaction.Commit();

                    Assert.Null(c1.C1C2one2one);
                    Assert.Null(c1.C1C2many2one);
                    Assert.Empty(c1.C1C2one2manies);
                    Assert.Empty(c1.C1C2many2manies);
                    Assert.Null(c1.I12C2one2one);

                    c1.Strategy.Delete();
                    c2s[0].Strategy.Delete();
                    transaction.Commit();

                    Assert.Null(transaction.Instantiate(c1Ids[0]));
                    Assert.Null(transaction.Instantiate(c2Ids[0]));
                    Assert.Equal(c2s[2], ((C1)transaction.Instantiate(c1Ids[1])).C1C2many2one);
                }
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
                        // The function keeps its name, but its result has a different column type.
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
