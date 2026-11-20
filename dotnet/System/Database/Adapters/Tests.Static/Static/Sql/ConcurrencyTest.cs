// <copyright file="ConcurrencyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Data.Common;
    using Xunit;
    using C1 = Domain.C1;

    public abstract class ConcurrencyTest : Adapters.ConcurrencyTest
    {
        // A statement that waits for the row of another transaction fails at the command timeout. SQL Server then
        // ends that statement only and PostgreSQL aborts the transaction. On both, the transaction stores nothing
        // more until it is rolled back.
        [Fact]
        public void AfterAStatementThatTimesOutATransactionTakesOnlyARollback()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var held = C1.Create(this.Transaction);
                held.C1AllorsString = "v0";
                var other = C1.Create(this.Transaction);
                other.C1AllorsString = "v0";
                this.Transaction.Commit();

                // A database object of its own, so that only the statements of this transaction have a command
                // timeout of one second.
                var database = this.CreateDatabase(1);

                using (var holder = this.Population.CreateTransaction())
                using (var transaction = database.CreateTransaction())
                {
                    var heldInTransaction = (C1)transaction.Instantiate(held);
                    var otherInTransaction = (C1)transaction.Instantiate(other);
                    Assert.Equal("v0", heldInTransaction.C1AllorsString);
                    Assert.Equal("v0", otherInTransaction.C1AllorsString);

                    // The holder writes the object and does not commit: it holds the row. An extent sends the
                    // changes of its transaction to the database.
                    ((C1)holder.Instantiate(held)).C1AllorsString = "holder";
                    Assert.Equal(2, holder.Extent(m.C1).Count);

                    heldInTransaction.C1AllorsString = "v1";
                    Assert.ThrowsAny<DbException>(() => _ = transaction.Extent(m.C1).Count);

                    holder.Rollback();

                    // The row is free again. The change that the statement was to store is lost, so a commit of
                    // a change to another object fails, and no other transaction sees it.
                    Assert.ThrowsAny<DbException>(() =>
                    {
                        otherInTransaction.C1AllorsString = "v1";
                        transaction.Commit();
                    });

                    this.Transaction.Rollback();
                    Assert.Equal("v0", held.C1AllorsString);
                    Assert.Equal("v0", other.C1AllorsString);

                    // After a rollback the transaction can redo its work.
                    transaction.Rollback();
                    heldInTransaction.C1AllorsString = "v1";
                    otherInTransaction.C1AllorsString = "v1";
                    transaction.Commit();
                }

                this.Transaction.Rollback();

                Assert.Equal("v1", held.C1AllorsString);
                Assert.Equal("v1", other.C1AllorsString);
            }
        }

        // A database object whose transactions give each statement the command timeout, in seconds.
        protected abstract IDatabase CreateDatabase(int commandTimeout);
    }
}
