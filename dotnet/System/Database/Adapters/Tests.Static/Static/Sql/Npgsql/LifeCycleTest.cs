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
