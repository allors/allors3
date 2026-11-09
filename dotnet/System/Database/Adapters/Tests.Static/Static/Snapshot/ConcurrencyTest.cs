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
