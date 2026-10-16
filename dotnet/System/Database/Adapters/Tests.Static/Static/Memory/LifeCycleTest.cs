// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Linq;
    using Adapters;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    public class LifeCycleTest : Adapters.LifeCycleTest, IDisposable
    {
        private readonly Profile profile = new Profile();

        protected override IProfile Profile => this.profile;

        [Fact]
        public override void DifferentTransactions()
        {
        }

        [Fact]
        public void TransactionHandlesShareUncommittedRolesQueriesAndInverseRelations()
        {
            this.profile.Init();
            var reader = this.Transaction;
            var source = reader.Create<C1>();
            source.C1AllorsString = "Committed";
            var target = reader.Create<C2>();
            reader.Commit();
            var version = source.Strategy.ObjectVersion;

            var writer = this.profile.Database.CreateTransaction();
            Assert.Same(reader, writer);
            var sourceInWriter = (C1)writer.Instantiate(source.Id);
            sourceInWriter.C1AllorsString = "Uncommitted";
            var added = writer.Create<C1>();
            added.C1C2many2one = (C2)writer.Instantiate(target.Id);

            Assert.Equal("Uncommitted", source.C1AllorsString);
            Assert.Equal(version, source.Strategy.ObjectVersion);
            Assert.Equal(2, reader.Extent<C1>().ToArray().Length);
            Assert.Same(added, Assert.Single(target.C1sWhereC1C2many2one));

            writer.Rollback();

            Assert.Equal("Committed", source.C1AllorsString);
            Assert.Equal(version, source.Strategy.ObjectVersion);
            Assert.Same(source, Assert.Single(reader.Extent<C1>().ToArray()));
            Assert.Empty(target.C1sWhereC1C2many2one);
        }

        public override void Dispose() => this.profile.Dispose();

        protected override void SwitchDatabase()
        {
        }

        protected override IDatabase CreatePopulation() => this.profile.CreateDatabase();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();
    }
}
