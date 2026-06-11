// <copyright file="SignalTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class SignalTests : Test
    {
        protected SignalTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async Task EffectRerunsWhenObservedRoleChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var c1 = session.Create<C1>();
            var signals = this.Workspace.Configuration.SignalFactory;

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
            var signals = this.Workspace.Configuration.SignalFactory;

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
    }
}
