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
        public void ComputedMemoizesBetweenChanges()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(1);
            var computations = 0;
            var computed = factory.Computed(() =>
            {
                computations++;
                return state.Value * 2;
            });

            Assert.Equal(2, computed.Value);
            Assert.Equal(2, computed.Value);
            Assert.Equal(1, computations);

            state.Value = 2;

            Assert.Equal(4, computed.Value);
            Assert.Equal(4, computed.Value);
            Assert.Equal(2, computations);
        }

        [Fact]
        public void UnchangedComputedValueDoesNotPropagate()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(-1);
            var clamped = factory.Computed(() => Math.Max(state.Value, 0));
            var downstream = 0;
            var doubled = factory.Computed(() =>
            {
                downstream++;
                return clamped.Value * 2;
            });

            Assert.Equal(0, doubled.Value);
            Assert.Equal(1, downstream);

            // clamped recomputes to an equal value; doubled must not recompute.
            state.Value = -2;

            Assert.Equal(0, doubled.Value);
            Assert.Equal(1, downstream);
        }

        [Fact]
        public void ComputedDropsAbandonedDependencies()
        {
            var factory = new DefaultSignalFactory();
            var useA = factory.State(true);
            var a = factory.State("a");
            var b = factory.State("b");
            var computations = 0;
            var computed = factory.Computed(() =>
            {
                computations++;
                return useA.Value ? a.Value : b.Value;
            });

            Assert.Equal("a", computed.Value);
            Assert.Equal(1, computations);

            useA.Value = false;

            Assert.Equal("b", computed.Value);
            Assert.Equal(2, computations);

            // The a branch is no longer read; changing it must not recompute.
            a.Value = "a2";

            Assert.Equal("b", computed.Value);
            Assert.Equal(2, computations);
        }

        [Fact]
        public void CircularDependencyThrows()
        {
            var factory = new DefaultSignalFactory();
            IComputedSignal<int> computed = null;
            computed = factory.Computed(() => computed.Value + 1);

            Assert.Throws<InvalidOperationException>(() => _ = computed.Value);
        }

        [Fact]
        public void ChangePropagatesThroughComputedChain()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(1);
            var doubled = factory.Computed(() => state.Value * 2);
            var doubledPlusOne = factory.Computed(() => doubled.Value + 1);

            var observations = new List<int>();
            factory.Effect(() => observations.Add(doubledPlusOne.Value));

            state.Value = 2;
            state.Value = 3;

            Assert.Equal(new[] { 3, 5, 7 }, observations);
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
