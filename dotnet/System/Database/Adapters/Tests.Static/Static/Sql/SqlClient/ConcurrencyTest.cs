// <copyright file="ConcurrencyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Adapters;
    using Microsoft.Data.SqlClient;
    using Xunit;
    using C1 = Domain.C1;

    public class ConcurrencyTest : Sql.ConcurrencyTest, IClassFixture<Fixture<ConcurrencyTest>>
    {
        private readonly Profile profile;

        public ConcurrencyTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        // The ways in which a transaction fails on SQL Server, each on the connection of one transaction: a
        // statement that fails when it is executed, a statement that fails while its rows are read, and a
        // transaction that the server ends without a statement that fails. After each of them the transaction
        // refuses what follows, and the refusal says what happened.
        [Fact]
        public void ARefusalNamesWhatFailed()
        {
            const string StatementFailed = "A statement of this transaction failed, so the transaction takes no further statement and no commit. Roll back and redo the work. The statement failed with: ";
            const string TransactionEnded = "SQL Server has ended the database transaction, so this transaction takes no further statement and no commit. Roll back and redo the work.";

            foreach (var init in this.Inits)
            {
                init();

                var c1 = C1.Create(this.Transaction);
                c1.C1AllorsString = "v0";
                this.Transaction.Commit();

                using (var transaction = (Transaction)this.Population.CreateTransaction())
                {
                    SqlException Fail(string statement, Action<ICommand> execute)
                    {
                        using var command = transaction.Connection.CreateCommand();
                        command.CommandText = statement;
                        return Assert.Throws<SqlException>(() => execute(command));
                    }

                    // The transaction takes no statement, however it is executed, and no commit, and says why.
                    void AssertRefuses(string message, Exception failure)
                    {
                        using var command = transaction.Connection.CreateCommand();
                        command.CommandText = "SELECT 1";

                        foreach (var refused in new Action[] { () => command.ExecuteScalar(), command.ExecuteNonQuery, () => command.ExecuteReader().Dispose(), transaction.Commit })
                        {
                            var exception = Assert.Throws<TransactionFailedException>(refused);
                            Assert.Equal(message, exception.Message);
                            Assert.Same(failure, exception.InnerException);
                        }
                    }

                    var c1InTransaction = (C1)transaction.Instantiate(c1);
                    Assert.Equal("v0", c1InTransaction.C1AllorsString);

                    // SQL Server ends only the statement that divides by zero, however it is executed.
                    foreach (var execute in new Action<ICommand>[] { v => v.ExecuteScalar(), v => v.ExecuteNonQuery(), v => v.ExecuteReader().Dispose() })
                    {
                        var failure = Fail("DECLARE @v int = 1 / 0", execute);
                        AssertRefuses(StatementFailed + failure.Message, failure);
                        transaction.Rollback();
                    }

                    // The division fails for the second row, after the first one has been read.
                    var late = Fail("SELECT 1 / v FROM (VALUES (1), (0)) AS t (v)", v =>
                    {
                        using var reader = v.ExecuteReader();
                        Assert.True(reader.Read());
                        reader.Read();
                    });
                    AssertRefuses(StatementFailed + late.Message, late);
                    transaction.Rollback();

                    // The statement ends the transaction of the server and does not fail.
                    using (var command = transaction.Connection.CreateCommand())
                    {
                        command.CommandText = "ROLLBACK TRANSACTION";
                        command.ExecuteNonQuery();
                    }

                    AssertRefuses(TransactionEnded, null);
                    transaction.Rollback();

                    // After a rollback the transaction stores its work.
                    c1InTransaction.C1AllorsString = "v1";
                    transaction.Commit();
                }

                this.Transaction.Rollback();

                Assert.Equal("v1", c1.C1AllorsString);
            }
        }

        protected override IDatabase CreateDatabase(int commandTimeout) => this.profile.CreateDatabase(commandTimeout);
    }
}
