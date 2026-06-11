// <copyright file="SignalTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class SignalTests : Test
    {
        protected SignalTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async Task SessionsHaveIsolatedSignalFactories()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            // Each session owns its reactive graph and effect scheduler; sharing one
            // factory across sessions would share the (single-threaded) scheduler.
            Assert.NotNull(session1.SignalFactory);
            Assert.NotSame(session1.SignalFactory, session2.SignalFactory);
        }

        [Fact]
        public async Task EffectRerunsWhenObservedRoleChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var c1 = session.Create<C1>();
            var signals = session.SignalFactory;

            var runs = 0;
            using var effect = signals.Effect(() =>
            {
                _ = c1.C1AllorsString.Value;
                runs++;
            });

            Assert.Equal(1, runs);

            c1.C1AllorsString.Set("X");

            Assert.Equal(2, runs);
        }

        [Fact]
        public async Task EffectThatWritesARoleSettles()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var c1 = session.Create<C1>();
            var signals = session.SignalFactory;

            // The guard turns a non-settling effect into a test failure instead of a hang:
            // a write inside an effect must not subscribe the effect to session-wide state.
            var runs = 0;
            using var effect = signals.Effect(() =>
            {
                _ = c1.C1AllorsString.Value;
                runs++;
                if (runs <= 5)
                {
                    c1.C1AllorsInteger.Set(42);
                }
            });

            Assert.Equal(1, runs);
        }

        [Fact]
        public async Task PullFlushesEffectsOncePerPull()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) };

            var result1 = await session1.PullAsync(pull);
            var c1s1 = result1.GetCollection<C1>();
            var c1a = c1s1.First(v => "c1A".Equals(v.Name.Value));
            var c1b = c1s1.First(v => "c1B".Equals(v.Name.Value));

            var result2 = await session2.PullAsync(pull);
            var c1s2 = result2.GetCollection<C1>();
            c1s2.First(v => "c1A".Equals(v.Name.Value)).C1AllorsString.Set("batchedA");
            c1s2.First(v => "c1B".Equals(v.Name.Value)).C1AllorsString.Set("batchedB");
            await session2.PushAsync();

            var observations = new List<(string A, string B)>();
            using var effect = session1.SignalFactory.Effect(() =>
                observations.Add((c1a.C1AllorsString.Value, c1b.C1AllorsString.Value)));

            await session1.PullAsync(pull);

            // A pull merges records one by one; effects must observe only the fully
            // merged state — one consistent run per pull, never a torn pair.
            Assert.Equal(2, observations.Count);
            Assert.Equal(("batchedA", "batchedB"), observations[1]);
        }
    }
}
