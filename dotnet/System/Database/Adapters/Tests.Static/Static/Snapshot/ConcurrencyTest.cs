// <copyright file="ConcurrencyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Data.Common;
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
        public void AWriteConflictNamesTheObjectAndTheCause()
        {
            foreach (var init in this.Inits)
            {
                init();

                var changed = C1.Create(this.Transaction);
                var deleted = C1.Create(this.Transaction);
                this.Transaction.Commit();
                var version = changed.Strategy.ObjectVersion;

                using (var transaction = this.Population.CreateTransaction())
                {
                    ((C1)transaction.Instantiate(changed)).C1AllorsString = "mine";

                    using (var other = this.Population.CreateTransaction())
                    {
                        ((C1)other.Instantiate(changed)).C1AllorsString = "theirs";
                        other.Commit();
                    }

                    var exception = Assert.Throws<ConcurrencyException>(() => transaction.Commit());
                    Assert.IsAssignableFrom<DbException>(exception);
                    Assert.Equal($"Object {changed.Id} of class C1 was changed by another transaction after this transaction read it (version {version} then, {version + 1} now). Roll back and redo the work.", exception.Message);

                    transaction.Rollback();
                    ((C1)transaction.Instantiate(deleted)).C1AllorsString = "mine";

                    using (var other = this.Population.CreateTransaction())
                    {
                        other.Instantiate(deleted).Strategy.Delete();
                        other.Commit();
                    }

                    exception = Assert.Throws<ConcurrencyException>(() => transaction.Commit());
                    Assert.Equal($"Object {deleted.Id} of class C1 was deleted by another transaction after this transaction read it. Roll back and redo the work.", exception.Message);
                }
            }
        }

        [Fact]
        public void ConcurrentOneToOneAssignmentsOfTheSameRoleRemainBidirectional()
        {
            foreach (var init in this.Inits)
            {
                init();
                var association = C1.Create(this.Transaction);
                var firstRole = C1.Create(this.Transaction);
                var secondRole = C1.Create(this.Transaction);
                this.Transaction.Commit();
                var version = association.Strategy.ObjectVersion;

                // Each transaction writes its own association; the role's inverse is reconciled, not written.
                using (var first = this.Population.CreateTransaction())
                using (var second = this.Population.CreateTransaction())
                {
                    ((C1)first.Instantiate(association)).C1C1one2one = (C1)first.Instantiate(firstRole);
                    ((C1)second.Instantiate(secondRole)).C1C1one2one = (C1)second.Instantiate(firstRole);

                    first.Commit();
                    second.Commit();
                }

                this.Transaction.Rollback();
                Assert.Null(association.C1C1one2one);
                Assert.Equal(firstRole, secondRole.C1C1one2one);
                Assert.Equal(secondRole, firstRole.C1WhereC1C1one2one);
                Assert.Equal(version + 2, association.Strategy.ObjectVersion);
            }
        }

        [Fact]
        public void ConcurrentOneToOneAssignmentsOnTheSameAssociationFailTheSecondWriter()
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
                    ((C1)second.Instantiate(association)).C1C1one2one = (C1)second.Instantiate(secondRole);

                    first.Commit();
                    Assert.Throws<ConcurrencyException>(() => second.Commit());
                }

                this.Transaction.Rollback();
                Assert.Equal(firstRole, association.C1C1one2one);
                Assert.Equal(association, firstRole.C1WhereC1C1one2one);
                Assert.Null(secondRole.C1WhereC1C1one2one);
                Assert.Equal(version + 1, association.Strategy.ObjectVersion);
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
    }
}
