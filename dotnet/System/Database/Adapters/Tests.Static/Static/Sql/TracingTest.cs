// <copyright file="CacheTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Linq;
    using Meta;
    using Tracing;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    public abstract class TracingTest : IDisposable
    {
        private TestPopulation population;

        private void Init()
        {
            var database = (Database)this.CreateDatabase();
            database.Init();
            using var transaction = database.CreateTransaction();
            this.population = new TestPopulation(transaction);
            transaction.Commit();

            this.M = (MetaPopulation)database.MetaPopulation;
        }

        public MetaPopulation M { get; private set; }

        public abstract void Dispose();

        [Fact]
        public void Initial()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            Assert.Empty(sink.TreeByTransaction);
        }

        [Fact]
        public void Instantiate()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            var c1 = (C1)transaction.Instantiate(this.population.C1A);

            var transactionSink = sink.TreeByTransaction[transaction];

            Assert.Equal(1, transactionSink.Nodes.Count);
            Assert.Equal(typeof(SqlInstantiateObjectEvent), transactionSink.Nodes[0].Event.GetType());
            Assert.Empty(transactionSink.Nodes[0].Nodes);
        }

        [Fact]
        public void Prefetch()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            var c1b = (C1)transaction.Instantiate(this.population.C1B);

            var transactionSink = sink.TreeByTransaction[transaction];
            transactionSink.Clear();

            var prefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(this.M.C1.C1C2one2one)
                .Build();

            transaction.Prefetch(prefetchPolicy, c1b);

            var events = transactionSink.Nodes;
            Assert.Equal(2, events.Count);
            Assert.Equal(typeof(SqlPrefetchCompositeRoleObjectTableEvent), events[0].Event.GetType());
            Assert.Empty(events[0].Nodes);
            Assert.Equal(typeof(SqlInstantiateReferencesEvent), events[1].Event.GetType());
            Assert.Empty(events[1].Nodes);

            transactionSink.Clear();

            var c2b = c1b.C1C2one2one;

            Assert.Empty(transactionSink.Nodes);
        }

        [Fact]
        public void PrefetchUsesModifiedCompositesRole()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            long c1Id;
            long c2bId;
            using (var setup = database.CreateTransaction())
            {
                var setupC1 = C1.Create(setup);
                var setupC2a = C2.Create(setup);
                var setupC2b = C2.Create(setup);
                var setupC2x = C2.Create(setup);
                setupC1.AddC1C2one2many(setupC2a);    // committed: c1 -> [c2a]
                setupC2b.AddC2C2one2many(setupC2x);   // committed nested: c2b -> [c2x]
                setup.Commit();
                c1Id = setupC1.Id;
                c2bId = setupC2b.Id;
            }

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            var c1 = (C1)transaction.Instantiate(c1Id);
            var c2b = (C2)transaction.Instantiate(c2bId);

            // Modify (uncommitted): c1 -> [c2a, c2b].
            c1.AddC1C2one2many(c2b);

            var nestedPolicy = new PrefetchPolicyBuilder().WithRule(this.M.C2.C2C2one2manies).Build();
            var prefetchPolicy = new PrefetchPolicyBuilder().WithRule(this.M.C1.C1C2one2manies, nestedPolicy).Build();
            transaction.Prefetch(prefetchPolicy, c1);

            var transactionSink = sink.TreeByTransaction[transaction];
            transactionSink.Clear();

            // c2b was added to c1's composites role in-memory; the prefetch must include it in the
            // transitive prefetch, so reading its nested role issues no database command.
            _ = c2b.C2C2one2manies.ToArray();

            Assert.Empty(transactionSink.Nodes);
        }

        protected abstract IDatabase CreateDatabase();
    }
}
