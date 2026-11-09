// <copyright file="CloneTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using Xunit;
    using C1 = Domain.C1;

    public class CloneTest : IDisposable
    {
        private readonly Profile profile = new Profile();

        public void Dispose() => this.profile.Dispose();

        [Fact]
        public void ACloneHasTheCommittedStateAndContinuesTheIds()
        {
            foreach (var init in this.profile.Inits)
            {
                init();
                var m = this.profile.Transaction.Database.Context().M;
                var source = (Database)this.profile.Database;
                var transaction = this.profile.Transaction;

                var committed = C1.Create(transaction);
                committed.C1AllorsString = "committed";
                var deleted = C1.Create(transaction);
                transaction.Commit();

                deleted.Strategy.Delete();
                transaction.Commit();

                var pending = C1.Create(transaction);
                committed.C1AllorsString = "pending";

                var clone = source.CreateClone(new DefaultDomainDatabaseServices());

                using (var cloneTransaction = clone.CreateTransaction())
                {
                    Assert.Equal("committed", ((C1)cloneTransaction.Instantiate(committed.Id)).C1AllorsString);
                    Assert.Null(cloneTransaction.Instantiate(deleted.Id));
                    Assert.Null(cloneTransaction.Instantiate(pending.Id));
                    Assert.Single(cloneTransaction.Extent(m.C1));

                    var createdInClone = C1.Create(cloneTransaction);
                    Assert.True(createdInClone.Id > pending.Id);
                    cloneTransaction.Commit();
                }
            }
        }

        [Fact]
        public void ACloneAndItsSourceDiverge()
        {
            foreach (var init in this.profile.Inits)
            {
                init();
                var m = this.profile.Transaction.Database.Context().M;
                var source = (Database)this.profile.Database;
                var transaction = this.profile.Transaction;

                var shared = C1.Create(transaction);
                shared.C1AllorsString = "shared";
                transaction.Commit();

                var clone = source.CreateClone(new DefaultDomainDatabaseServices());

                shared.C1AllorsString = "source";
                C1.Create(transaction).C1AllorsString = "source only";
                transaction.Commit();

                using (var cloneTransaction = clone.CreateTransaction())
                {
                    var sharedInClone = (C1)cloneTransaction.Instantiate(shared.Id);
                    Assert.Equal("shared", sharedInClone.C1AllorsString);
                    Assert.Single(cloneTransaction.Extent(m.C1));

                    sharedInClone.C1AllorsString = "clone";
                    C1.Create(cloneTransaction).C1AllorsString = "clone only";
                    cloneTransaction.Commit();

                    Assert.Equal(2, cloneTransaction.Extent(m.C1).Count);
                }

                transaction.Rollback();

                Assert.Equal("source", shared.C1AllorsString);
                Assert.Equal(2, transaction.Extent(m.C1).Count);
            }
        }
    }
}
