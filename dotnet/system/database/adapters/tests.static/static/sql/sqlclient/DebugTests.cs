// <copyright file="DebugTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.SqlClient
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Adapters;
    using Domain;
    using Caching;
    using Debug;
    using Xunit;

    public class DebugTests : IDisposable, IClassFixture<Fixture<DebugTests>>
    {
        private readonly Profile profile;

        private readonly DebugConnectionFactory connectionFactory;
        private readonly DefaultCacheFactory cacheFactory;

        protected IProfile Profile => this.profile;

        public DebugTests()
        {
            this.connectionFactory = new DebugConnectionFactory();
            this.cacheFactory = new DefaultCacheFactory();
            this.profile = new Profile(this.GetType().Name, this.connectionFactory, this.cacheFactory);
        }

        public void Dispose() => this.profile.Dispose();

        #region Population
#pragma warning disable IDE1006
        protected C1 c1A;
        protected C1 c1B;
        protected C1 c1C;
        protected C1 c1D;
        protected C2 c2A;
        protected C2 c2B;
        protected C2 c2C;
        protected C2 c2D;
        protected C3 c3A;
        protected C3 c3B;
        protected C3 c3C;
        protected C3 c3D;
        protected C4 c4A;
        protected C4 c4B;
        protected C4 c4C;
        protected C4 c4D;
#pragma warning restore IDE1006
        #endregion

        protected Database Database => (Database)this.Profile.Database;
        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        [Fact]
        public void TransactionCreation()
        {
            foreach (var init in this.Inits)
            {
                init();

                using (var transaction = this.Database.CreateTransaction())
                {
                    var connectionFactory = (DebugConnectionFactory)this.Database.ConnectionFactory;

                    var connection = connectionFactory.Connections.Last();

                    Assert.Empty(connection.Commands);
                }
            }
        }

        [Fact]
        public void Extent()
        {
            foreach (var init in this.Inits)
            {
                init();
                this.Populate();
                var m = this.Database.Context().M;

                var connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {

                    connection.Commands.Clear();

                    var extent = transaction.Extent<C1>();

                    foreach (C1 c1 in extent)
                    {
                        Assert.Equal(c1.Strategy.Class, m.C1);
                    }

                    Assert.Equal(2, connection.Commands.Count);
                    Assert.Equal(2, connection.Executions.Count());

                    transaction.Rollback();
                    connection.Commands.Clear();
                    this.InvalidateCache();
                }

                connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();

                    foreach (C1 c1 in extent)
                    {
                        Assert.Equal(c1.Strategy.Class, m.C1);
                    }

                    Assert.Equal(2, connection.Commands.Count);
                    Assert.Equal(2, connection.Executions.Count());
                }
            }
        }

        [Fact]
        public void GenericExtent()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;
                this.Populate();

                this.InvalidateCache();

                var connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    C1[] extent = transaction.Extent<C1>();

                    foreach (var c1 in extent)
                    {
                        Assert.Equal(c1.Strategy.Class, m.C1);
                    }

                    Assert.Equal(2, connection.Commands.Count);
                    Assert.Equal(2, connection.Executions.Count());
                }

                this.InvalidateCache();

                connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();

                    foreach (C1 c1 in extent)
                    {
                        Assert.Equal(c1.Strategy.Class, m.C1);
                    }

                    Assert.Equal(2, connection.Commands.Count);
                    Assert.Equal(2, connection.Executions.Count());
                }
            }
        }

        [Fact]
        public void ExtentRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;
                this.Populate();

                var connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);
                    }

                    Assert.Equal(3, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());
                }

                this.InvalidateCache();

                connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);
                    }

                    Assert.Equal(3, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());
                }
            }
        }

        [Fact]
        public void Prefetch()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;
                this.Populate();

                var c1Prefetcher = new PrefetchPolicyBuilder().WithRule(m.C1.C1AllorsString).Build();

                var connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();
                    transaction.Prefetch(c1Prefetcher, extent);

                    Assert.Equal(3, connection.Commands.Count);
                    Assert.Equal(3, connection.Executions.Count());

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);
                    }

                    Assert.Equal(3, connection.Commands.Count);
                    Assert.Equal(3, connection.Executions.Count());
                }

                this.InvalidateCache();

                connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();
                    transaction.Prefetch(c1Prefetcher, extent);

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);
                    }

                    Assert.Equal(3, connection.Commands.Count);
                    Assert.Equal(3, connection.Executions.Count());
                }
            }
        }

        [Fact]
        public void PrefetchOneClass()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;
                this.Populate();

                var c2Prefetcher = new PrefetchPolicyBuilder().WithRule(m.C1.C1AllorsString).Build();
                var c1Prefetcher =
                    new PrefetchPolicyBuilder().WithRule(m.C1.C1C2one2one, c2Prefetcher)
                        .WithRule(m.C1.C1AllorsString)
                        .Build();

                var connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();
                    transaction.Prefetch(c1Prefetcher, extent);

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);

                        var c2 = c1.C1C2one2one;
                        stringBuilder.Append(c2?.C2AllorsString);
                    }

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());
                }

                this.InvalidateCache();

                connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();
                    transaction.Prefetch(c1Prefetcher, extent);

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);

                        var c2 = c1.C1C2one2one;
                        stringBuilder.Append(c2?.C2AllorsString);
                    }

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());
                }
            }
        }

        [Fact]
        public void PrefetchManyInterface()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Database.Context().M;
                this.Populate();

                var i12Prefetcher = new PrefetchPolicyBuilder().WithRule(m.C1.I12AllorsString).Build();

                var c1Prefetcher =
                    new PrefetchPolicyBuilder().WithRule(m.C1.C1I12one2manies, i12Prefetcher)
                        .WithRule(m.C1.C1AllorsString)
                        .Build();

                var connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();
                    transaction.Prefetch(c1Prefetcher, extent);

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);

                        foreach (I12 i12 in c1.C1I12one2manies)
                        {
                            stringBuilder.Append(i12?.I12AllorsString);
                        }
                    }

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());

                    transaction.Prefetch(c1Prefetcher, extent);

                    stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);

                        foreach (I12 i12 in c1.C1I12one2manies)
                        {
                            stringBuilder.Append(i12?.I12AllorsString);
                        }
                    }

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());
                }

                this.InvalidateCache();

                connection = (DebugConnection)this.connectionFactory.Create(this.Database);
                using (var transaction = this.Database.CreateTransaction(connection))
                {
                    var extent = transaction.Extent<C1>();
                    transaction.Prefetch(c1Prefetcher, extent);

                    var stringBuilder = new StringBuilder();
                    foreach (C1 c1 in extent)
                    {
                        stringBuilder.Append(c1.C1AllorsString);
                        foreach (I12 i12 in c1.C1I12one2manies)
                        {
                            stringBuilder.Append(i12?.I12AllorsString);
                        }
                    }

                    Assert.Equal(5, connection.Commands.Count);
                    Assert.Equal(6, connection.Executions.Count());
                }
            }
        }

        protected void Populate()
        {
            var m = this.Database.Context().M;

            using (var transaction = this.Database.CreateTransaction())
            {
                var population = new TestPopulation(transaction);

                this.c1A = population.C1A;
                this.c1B = population.C1B;
                this.c1C = population.C1C;
                this.c1D = population.C1D;

                this.c2A = population.C2A;
                this.c2B = population.C2B;
                this.c2C = population.C2C;
                this.c2D = population.C2D;

                this.c3A = population.C3A;
                this.c3B = population.C3B;
                this.c3C = population.C3C;
                this.c3D = population.C3D;

                this.c4A = population.C4A;
                this.c4B = population.C4B;
                this.c4C = population.C4C;
                this.c4D = population.C4D;

                transaction.Commit();
            }
        }

        protected ITransaction CreateTransaction() => this.Profile.Database.CreateTransaction();

        private void InvalidateCache()
        {
            var method = this.Database.GetType().GetProperty("Cache", BindingFlags.Instance | BindingFlags.NonPublic);
            var cache = (ICache)method.GetValue(this.Database);
            cache.Invalidate();
        }
    }
}
