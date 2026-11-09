// <copyright file="RestorePointTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using Xunit;
    using C1 = Domain.C1;

    public class RestorePointTest : IDisposable
    {
        private readonly Profile profile = new Profile();

        public void Dispose() => this.profile.Dispose();

        [Fact]
        public void RestoreReturnsToTheCommittedStateWithTheSameIds()
        {
            foreach (var init in this.profile.Inits)
            {
                init();
                var m = this.profile.Transaction.Database.Context().M;
                var database = (Database)this.profile.Database;
                var transaction = this.profile.Transaction;

                var kept = C1.Create(transaction);
                kept.C1AllorsString = "kept";
                transaction.Commit();

                // Pending work is not part of a restore point, but the id it took is.
                var pending = C1.Create(transaction);
                var restorePoint = database.TakeRestorePoint();
                transaction.Rollback();

                var later = C1.Create(transaction);
                kept.C1AllorsString = "changed";
                transaction.Commit();

                database.Restore(restorePoint);
                transaction.Rollback();

                Assert.Equal("kept", kept.C1AllorsString);
                Assert.Null(transaction.Instantiate(later.Id));
                Assert.Null(transaction.Instantiate(pending.Id));
                Assert.Single(transaction.Extent(m.C1));

                var first = C1.Create(transaction);
                transaction.Commit();

                Assert.Equal(pending.Id + 1, first.Id);

                database.Restore(restorePoint);
                transaction.Rollback();

                var second = C1.Create(transaction);
                transaction.Commit();

                Assert.Equal(first.Id, second.Id);
                Assert.Equal("kept", kept.C1AllorsString);
                Assert.Equal(2, transaction.Extent(m.C1).Count);
            }
        }

        [Fact]
        public void RestoreStartsANewEpoch()
        {
            foreach (var init in this.profile.Inits)
            {
                init();
                var database = (Database)this.profile.Database;
                var transaction = this.profile.Transaction;

                var restorePoint = database.TakeRestorePoint();

                C1.Create(transaction).C1AllorsString = "pending";
                database.Restore(restorePoint);

                var exception = Assert.Throws<InvalidOperationException>(() => transaction.Commit());
                Assert.Contains("initialised, loaded or restored", exception.Message, StringComparison.Ordinal);

                transaction.Rollback();
                C1.Create(transaction).C1AllorsString = "fresh";
                transaction.Commit();
            }
        }

        [Fact]
        public void RestorePointOfAnotherDatabaseIsRefused()
        {
            foreach (var init in this.profile.Inits)
            {
                init();
                var database = (Database)this.profile.Database;
                var other = (Database)this.profile.CreateDatabase();

                var exception = Assert.Throws<ArgumentException>(() => database.Restore(other.TakeRestorePoint()));
                Assert.Contains("another database", exception.Message, StringComparison.Ordinal);
            }
        }
    }
}
