// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using Adapters;
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

        // The adapter prepares the commands whose statement the mapping fixes: after a transaction has read an
        // object, its connection holds prepared statements. The update of the unit roles that changed together on
        // one object is not prepared: its statement differs with every combination of roles, and a prepared
        // statement stays on its connection.
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
                    Assert.Equal(0L, Count(transaction, "SELECT COUNT(*) FROM pg_prepared_statements WHERE statement LIKE 'UPDATE%c1allorsstring%' AND statement LIKE 'UPDATE%c1allorsinteger%'"));
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
