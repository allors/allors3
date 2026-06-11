// <copyright file="EffectTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Tests
{
    using System.Collections.Generic;
    using Xunit;

    public class EffectTests
    {
        [Fact]
        public void EffectObservesConsistentDiamond()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(1);
            var doubled = factory.Computed(() => state.Value * 2);
            var incremented = factory.Computed(() => state.Value + 1);

            var observations = new List<(int Doubled, int Incremented)>();
            factory.Effect(() => observations.Add((doubled.Value, incremented.Value)));

            state.Value = 2;

            // One consistent run per change: never a torn (stale, fresh) pair,
            // never a redundant second run for the same change.
            Assert.Equal(new[] { (2, 2), (4, 3) }, observations);
        }
    }
}
