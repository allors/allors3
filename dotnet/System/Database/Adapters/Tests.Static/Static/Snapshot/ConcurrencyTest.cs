// <copyright file="ConcurrencyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.IO;
    using System.Xml;
    using Adapters;
    using Xunit;
    using C1 = Domain.C1;

    public class ConcurrencyTest : Adapters.ConcurrencyTest
    {
        private readonly Profile profile = new Profile();

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        [Fact]
        public void InitOrLoadWhileATransactionHasPendingChangesFailsItsCommit()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var transaction = this.Transaction;
                C1.Create(transaction).C1AllorsString = "pending";

                this.Population.Init();

                var exception = Assert.Throws<InvalidOperationException>(() => transaction.Commit());
                Assert.Contains("initialised, loaded or restored", exception.Message, StringComparison.Ordinal);

                // After a rollback the transaction works on the fresh population.
                transaction.Rollback();
                C1.Create(transaction).C1AllorsString = "fresh";
                transaction.Commit();

                C1.Create(transaction).C1AllorsString = "pending";

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    this.profile.CreateDatabase().Save(writer);
                }

                using (var reader = XmlReader.Create(new StringReader(stringWriter.ToString())))
                {
                    this.Population.Load(reader);
                }

                exception = Assert.Throws<InvalidOperationException>(() => transaction.Commit());
                Assert.Contains("initialised, loaded or restored", exception.Message, StringComparison.Ordinal);

                transaction.Rollback();

                using (var other = this.Population.CreateTransaction())
                {
                    Assert.Equal(0, other.Extent(m.C1).Count);
                }
            }
        }

        [Fact]
        public void TheLaterCommitOfTheSameObjectWins()
        {
            foreach (var init in this.Inits)
            {
                init();

                var c1 = C1.Create(this.Transaction);
                c1.C1AllorsString = "v0";
                this.Transaction.Commit();
                var version0 = c1.Strategy.ObjectVersion;

                using (var first = this.Population.CreateTransaction())
                using (var second = this.Population.CreateTransaction())
                {
                    var c1First = (C1)first.Instantiate(c1);
                    var c1Second = (C1)second.Instantiate(c1);

                    c1First.C1AllorsString = "first";
                    c1Second.C1AllorsString = "second";

                    first.Commit();
                    Assert.Equal(version0 + 1, c1First.Strategy.ObjectVersion);

                    second.Commit();
                    Assert.Equal("second", c1Second.C1AllorsString);
                    Assert.Equal(version0 + 2, c1Second.Strategy.ObjectVersion);
                }

                this.Transaction.Rollback();

                Assert.Equal("second", c1.C1AllorsString);
                Assert.Equal(version0 + 2, c1.Strategy.ObjectVersion);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConcurrentOneToOneAssignmentsRemainBidirectional(bool sameRole)
        {
            foreach (var init in this.Inits)
            {
                init();
                var association = C1.Create(this.Transaction);
                var firstRole = C1.Create(this.Transaction);
                var secondRole = C1.Create(this.Transaction);
                this.Transaction.Commit();
                var version = association.Strategy.ObjectVersion;

                using (var first = this.Population.CreateTransaction())
                using (var second = this.Population.CreateTransaction())
                {
                    ((C1)first.Instantiate(association)).C1C1one2one = (C1)first.Instantiate(firstRole);
                    if (sameRole)
                    {
                        ((C1)second.Instantiate(secondRole)).C1C1one2one = (C1)second.Instantiate(firstRole);
                    }
                    else
                    {
                        ((C1)second.Instantiate(association)).C1C1one2one = (C1)second.Instantiate(secondRole);
                    }

                    first.Commit();
                    second.Commit();
                }

                this.Transaction.Rollback();
                if (sameRole)
                {
                    Assert.Null(association.C1C1one2one);
                    Assert.Equal(firstRole, secondRole.C1C1one2one);
                    Assert.Equal(secondRole, firstRole.C1WhereC1C1one2one);
                }
                else
                {
                    Assert.Equal(secondRole, association.C1C1one2one);
                    Assert.Null(firstRole.C1WhereC1C1one2one);
                    Assert.Equal(association, secondRole.C1WhereC1C1one2one);
                }

                Assert.Equal(version + 2, association.Strategy.ObjectVersion);
            }
        }

        [Fact]
        public void ConcurrentManyToOneAssignmentsKeepBothAssociations()
        {
            foreach (var init in this.Inits)
            {
                init();
                var firstAssociation = C1.Create(this.Transaction);
                var secondAssociation = C1.Create(this.Transaction);
                var role = C1.Create(this.Transaction);
                this.Transaction.Commit();

                using (var first = this.Population.CreateTransaction())
                using (var second = this.Population.CreateTransaction())
                {
                    ((C1)first.Instantiate(firstAssociation)).C1C1many2one = (C1)first.Instantiate(role);
                    ((C1)second.Instantiate(secondAssociation)).C1C1many2one = (C1)second.Instantiate(role);
                    first.Commit();
                    second.Commit();
                }

                this.Transaction.Rollback();
                Assert.Equal(role, firstAssociation.C1C1many2one);
                Assert.Equal(role, secondAssociation.C1C1many2one);
                Assert.Contains(firstAssociation, role.C1sWhereC1C1many2one);
                Assert.Contains(secondAssociation, role.C1sWhereC1C1many2one);
            }
        }

        [Fact]
        public void ConcurrentAssociationChangePreservesCommittedRoles()
        {
            foreach (var init in this.Inits)
            {
                init();
                var association = C1.Create(this.Transaction);
                var role = C1.Create(this.Transaction);
                role.C1AllorsString = "before";
                this.Transaction.Commit();
                var version = role.Strategy.ObjectVersion;

                using (var first = this.Population.CreateTransaction())
                using (var second = this.Population.CreateTransaction())
                {
                    ((C1)first.Instantiate(role)).C1AllorsString = "after";
                    ((C1)second.Instantiate(association)).C1C1many2one = (C1)second.Instantiate(role);
                    first.Commit();
                    second.Commit();
                }

                this.Transaction.Rollback();
                Assert.Equal("after", role.C1AllorsString);
                Assert.Equal(version + 1, role.Strategy.ObjectVersion);
                Assert.Equal(association, Assert.Single(role.C1sWhereC1C1many2one));
            }
        }

        [Fact]
        public void DeletionRemovesRelationsCommittedAfterTheTransactionStarted()
        {
            foreach (var init in this.Inits)
            {
                init();
                var association = C1.Create(this.Transaction);
                var role = C1.Create(this.Transaction);
                this.Transaction.Commit();

                using (var deleter = this.Population.CreateTransaction())
                using (var changer = this.Population.CreateTransaction())
                {
                    ((C1)changer.Instantiate(association)).AddC1C1many2many((C1)changer.Instantiate(role));
                    changer.Commit();
                    deleter.Instantiate(role).Strategy.Delete();
                    deleter.Commit();
                }

                this.Transaction.Rollback();
                Assert.Null(this.Transaction.Instantiate(role.Id));
                Assert.False(association.ExistC1C1many2manies);
                Assert.Empty(association.C1C1many2manies);
            }
        }

        [Fact]
        public void ACommitDoesNotRestoreAnObjectAnotherTransactionDeleted()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var deleted = C1.Create(this.Transaction);
                var other = C1.Create(this.Transaction);
                this.Transaction.Commit();

                using (var changer = this.Population.CreateTransaction())
                {
                    var deletedInChanger = (C1)changer.Instantiate(deleted);
                    deletedInChanger.C1AllorsString = "changed";
                    ((C1)changer.Instantiate(other)).C1C1one2one = deletedInChanger;

                    using (var deleter = this.Population.CreateTransaction())
                    {
                        deleter.Instantiate(deleted).Strategy.Delete();
                        deleter.Commit();
                    }

                    changer.Commit();
                }

                this.Transaction.Rollback();

                Assert.Null(this.Transaction.Instantiate(deleted.Id));
                Assert.Single(this.Transaction.Extent(m.C1));
                Assert.False(other.ExistC1C1one2one);
                Assert.Null(other.C1C1one2one);
            }
        }
    }
}
