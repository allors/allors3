// <copyright file="ComputedSignalTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ComputedSignalTests
    {
        [Fact]
        public void DisposedComputedThrowsOnRead()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(1);
            var computed = factory.Computed(() => state.Value * 2);

            Assert.Equal(2, computed.Value);

            computed.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = computed.Value);
        }

        [Fact]
        public void ComputedWithComparerCutsPropagationWhenValuesAreEqual()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(0);
            var computed = factory.Computed(
                () =>
                {
                    _ = state.Value;
                    return new[] { 1, 2, 3 };
                },
                new IntArrayComparer());

            var runs = 0;
            factory.Effect(() =>
            {
                _ = computed.Value;
                runs++;
            });

            Assert.Equal(1, runs);

            // The computed recomputes to a fresh but equal array; the comparer must
            // keep the change from propagating to the effect.
            state.Value = 1;

            Assert.Equal(1, runs);
        }

        private sealed class IntArrayComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y) => x != null && y != null && x.SequenceEqual(y);

            public int GetHashCode(int[] obj) => 0;
        }
    }
}
