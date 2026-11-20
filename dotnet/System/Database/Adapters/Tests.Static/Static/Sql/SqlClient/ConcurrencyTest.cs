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
                    ICommand CreateCommand(string statement)
                    {
                        var command = transaction.Connection.CreateCommand();
                        command.CommandText = statement;
                        return command;
                    }

                    // The transaction takes no statement, however it is executed, and no commit, and says why.
                    void AssertRefuses(string message, Exception failure)
                    {
                        using var command = CreateCommand("SELECT 1");

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
                        using var command = CreateCommand("DECLARE @v int = 1 / 0");
                        var failure = Assert.Throws<SqlException>(() => execute(command));
                        AssertRefuses(StatementFailed + failure.Message, failure);
                        transaction.Rollback();
                    }

                    // The division fails for the second row, after the first one has been read.
                    SqlException late;
                    using (var command = CreateCommand("SELECT 1 / v FROM (VALUES (1), (0)) AS t (v)"))
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.True(reader.Read());
                        late = Assert.Throws<SqlException>(() => reader.Read());
                    }
                    AssertRefuses(StatementFailed + late.Message, late);
                    transaction.Rollback();

                    // SQL Server can return this decimal, but SqlClient cannot fit it into a System.Decimal.
                    const string WideDecimal = "SELECT CAST(1e30 AS decimal(38, 0))";
                    using (var command = CreateCommand(WideDecimal))
                    {
                        var failure = Assert.Throws<OverflowException>(() => command.ExecuteScalar());
                        AssertRefuses(StatementFailed + failure.Message, failure);
                    }
                    transaction.Rollback();

                    // The other reads of a column value fail on a column of another type, and IsDBNull on a column
                    // that is not there.
                    const string Text = "SELECT N'x'";
                    const string Number = "SELECT 1";
                    var reads = new (string Statement, Type Failure, Func<IReader, object> GetValue)[]
                    {
                        (WideDecimal, typeof(OverflowException), r => r.GetValue(0)),
                        (WideDecimal, typeof(OverflowException), r => r.GetDecimal(0)),
                        (WideDecimal, typeof(OverflowException), r => r[0]),
                        (Text, typeof(InvalidCastException), r => r.GetBoolean(0)),
                        (Text, typeof(InvalidCastException), r => r.GetDateTime(0)),
                        (Text, typeof(InvalidCastException), r => r.GetDouble(0)),
                        (Text, typeof(InvalidCastException), r => r.GetInt32(0)),
                        (Text, typeof(InvalidCastException), r => r.GetInt64(0)),
                        (Number, typeof(InvalidCastException), r => r.GetString(0)),
                        (Number, typeof(InvalidCastException), r => r.GetGuid(0)),
                        (Number, typeof(IndexOutOfRangeException), r => r.IsDBNull(1)),
                    };

                    foreach (var (statement, expected, getValue) in reads)
                    {
                        Exception failure;
                        using (var command = CreateCommand(statement))
                        using (var reader = command.ExecuteReader())
                        {
                            Assert.True(reader.Read());
                            failure = Assert.Throws(expected, () => getValue(reader));
                        }
                        AssertRefuses(StatementFailed + failure.Message, failure);
                        transaction.Rollback();
                    }

                    // The statement ends the transaction of the server and does not fail.
                    using (var command = CreateCommand("ROLLBACK TRANSACTION"))
                    {
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

        [Fact]
        public void FailuresFromAnEarlierCommandOrReaderLeaveTheCurrentTransactionUsable()
        {
            foreach (var init in this.Inits)
            {
                init();
                using var transaction = (Transaction)this.Population.CreateTransaction();

                void AssertFreshWorkCommits(Action useOldHandle)
                {
                    var created = C1.Create(transaction);
                    Assert.Throws<InvalidOperationException>(useOldHandle);
                    transaction.Commit();
                    this.Transaction.Rollback();
                    Assert.NotNull(this.Transaction.Instantiate(created.Id));
                }

                using var oldCommand = transaction.Connection.CreateCommand();
                oldCommand.CommandText = "SELECT 1";
                transaction.Commit();
                AssertFreshWorkCommits(() => oldCommand.ExecuteScalar());

                using var command = transaction.Connection.CreateCommand();
                command.CommandText = "SELECT 1";
                var oldReader = command.ExecuteReader();
                oldReader.Dispose();
                transaction.Rollback();
                AssertFreshWorkCommits(() => oldReader.Read());
            }
        }

        protected override IDatabase CreateDatabase(int commandTimeout) => this.profile.CreateDatabase(commandTimeout);
    }
}
