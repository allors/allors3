// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Linq;
    using Domain;
    using Xunit;
    using IDatabase = IDatabase;

    public abstract class CacheTest : IDisposable
    {
        public abstract void Dispose();

        [Fact(Skip = "Cache invalidation")]
        public void InitDifferentDatabase()
        {
            var database = this.CreateDatabase();
            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                var c1 = C1.Create(transaction);
                c1.C1AllorsString = "a";
                transaction.Commit();
            }

            using (var transaction = database.CreateTransaction())
            {
                var c1 = transaction.Extent<C1>().First;
                Assert.Equal("a", c1.C1AllorsString);
            }

            database.Init();

            var database2 = this.CreateDatabase();

            using (var transaction = database.CreateTransaction())
            {
                var c1 = C1.Create(transaction);
                c1.C1AllorsString = "b";
                transaction.Commit();
            }

            using (var transaction = database2.CreateTransaction())
            {
                var c1 = transaction.Extent<C1>().First;
                c1.C1AllorsString = "c";
            }

            using (var transaction = database.CreateTransaction())
            {
                var c1 = transaction.Extent<C1>().First;
                Assert.Equal("c", c1.C1AllorsString);
            }
        }

        [Fact]
        public void FlushCacheOnInit()
        {
            var database = this.CreateDatabase();
            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                var c1a = C1.Create(transaction);
                var c2a = C2.Create(transaction);
                c1a.C1C2one2one = c2a;
                transaction.Commit();

                // load cache
                c2a = c1a.C1C2one2one;
            }

            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                var c1a = C1.Create(transaction);
                var c1b = C1.Create(transaction);

                transaction.Commit();

                c1a = C1.Instantiate(transaction, c1a.Id);

                Assert.Null(c1a.C1C2one2one);
            }
        }

        [Fact]
        public void CacheUnitRole()
        {
            var database = this.CreateDatabase();
            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                var c1 = C1.Create(transaction);
                c1.C1AllorsString = "Test";

                transaction.Commit();
            }
        }

        [Fact]
        public void FailedCommit()
        {
            var database = this.CreateDatabase();
            database.Init();

            long c1Id = 0;
            long c2Id = 0;

            using (var transaction = database.CreateTransaction())
            {
                var c1 = C1.Create(transaction);
                var c2 = C2.Create(transaction);

                c1Id = c1.Id;
                c2Id = c2.Id;

                c1.C1C2one2one = c2;

                transaction.Commit();

                c1.C1AllorsString = "Transaction 1";

                using (var transaction2 = database.CreateTransaction())
                {
                    var transaction2C1 = (C1)transaction2.Instantiate(c1);
                    transaction2C1.C1AllorsString = "Transaction 2";

                    transaction2C1.C1C2one2one = null;

                    transaction2.Commit();

                    var transaction2C2 = (C2)transaction2.Instantiate(c2);
                    transaction2C2.Strategy.Delete();

                    transaction2.Commit();
                }

                var triggerCache = c1.C1C2one2one;

                var exceptionThrown = false;
                try
                {
                    transaction.Commit();
                }
                catch
                {
                    exceptionThrown = true;
                }

                Assert.True(exceptionThrown);
            }

            using (var transaction = database.CreateTransaction())
            {
                var c1 = (C1)transaction.Instantiate(c1Id);
                var c2 = transaction.Instantiate(c2Id);

                Assert.Null(c1.C1C2one2one);
            }
        }

        [Fact]
        public void PrefetchCompositeRole()
        {
            var database = this.CreateDatabase();
            database.Init();
            var m = database.Context().M;

            using (var transaction = database.CreateTransaction())
            {
                var c1a = C1.Create(transaction);
                var c1b = C1.Create(transaction);
                var c2a = C2.Create(transaction);
                var c2b = C2.Create(transaction);

                transaction.Commit();

                c1a.C1C2many2one = c2a;

                var extent = transaction.Extent<C1>();
                var array = extent.ToArray();

                var nestedPrefetchPolicyBuilder = new PrefetchPolicyBuilder();
                nestedPrefetchPolicyBuilder.WithRule(m.C2.C2C2one2manies);
                var nestedPrefetchPolicy = nestedPrefetchPolicyBuilder.Build();

                var prefetchPolicyBuilder = new PrefetchPolicyBuilder();
                prefetchPolicyBuilder.WithRule(m.C1.C1C2many2one, nestedPrefetchPolicy);
                var prefetchPolicy = prefetchPolicyBuilder.Build();
                transaction.Prefetch(prefetchPolicy, new[] { c1a, c1b });

                var result = c1a.C1C2many2one;

                transaction.Rollback();

                Assert.False(c1a.ExistC1C2many2one);
            }
        }

        [Fact]
        public void PrefetchCompositesRole()
        {
            var database = this.CreateDatabase();
            database.Init();
            var m = database.Context().M;

            using (var transaction = database.CreateTransaction())
            {
                var c1a = C1.Create(transaction);
                var c1b = C1.Create(transaction);
                var c2a = C2.Create(transaction);
                var c2b = C2.Create(transaction);
                var c2c = C2.Create(transaction);

                c1a.AddC1C2one2many(c2a);
                c1a.AddC1C2one2many(c2b);

                transaction.Commit();

                c1a.RemoveC1C1one2manies();
                c1a.AddC1C2one2many(c2c);

                var extent = transaction.Extent<C1>();
                var array = extent.ToArray();

                var nestedPrefetchPolicyBuilder = new PrefetchPolicyBuilder();
                nestedPrefetchPolicyBuilder.WithRule(m.C2.C2C2one2manies);
                var nestedPrefetchPolicy = nestedPrefetchPolicyBuilder.Build();

                var prefetchPolicyBuilder = new PrefetchPolicyBuilder();
                prefetchPolicyBuilder.WithRule(m.C1.C1C2one2manies, nestedPrefetchPolicy);
                var prefetchPolicy = prefetchPolicyBuilder.Build();
                transaction.Prefetch(prefetchPolicy, new[] { c1a, c1b });

                var result = c1a.C1C2one2manies;

                transaction.Rollback();

                Assert.Equal(2, c1a.C1C2one2manies.Count());
                Assert.Contains(c2a, c1a.C1C2one2manies.ToArray());
                Assert.Contains(c2b, c1a.C1C2one2manies.ToArray());
            }
        }

        protected abstract IDatabase CreateDatabase();
    }
}
