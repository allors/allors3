// <copyright file="EffectScopeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Tests
{
    using Xunit;

    public class EffectScopeTests
    {
        [Fact]
        public void DisposingScopeStopsOwnedEffects()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(0);
            var runs = 0;

            var scope = factory.EffectScope();
            factory.Effect(() =>
            {
                _ = state.Value;
                runs++;
            });

            Assert.Equal(1, runs);

            state.Value = 1;

            Assert.Equal(2, runs);

            scope.Dispose();
            state.Value = 2;

            Assert.Equal(2, runs);
        }

        [Fact]
        public void DisposingParentScopeWhileChildScopeIsActiveRestoresOuterScope()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(0);
            var runs = 0;

            var outerScope = factory.EffectScope();
            var parentScope = factory.EffectScope();
            _ = factory.EffectScope(); // child of parentScope; stays the active scope

            parentScope.Dispose();

            factory.Effect(() =>
            {
                _ = state.Value;
                runs++;
            });

            Assert.Equal(1, runs);

            outerScope.Dispose();
            state.Value = 1;

            Assert.Equal(1, runs);
        }

        [Fact]
        public void DisposingNestedScopeKeepsParentEffectsActive()
        {
            var factory = new DefaultSignalFactory();
            var state = factory.State(0);
            var parentRuns = 0;
            var childRuns = 0;

            using var parentScope = factory.EffectScope();
            factory.Effect(() =>
            {
                _ = state.Value;
                parentRuns++;
            });

            var childScope = factory.EffectScope();
            factory.Effect(() =>
            {
                _ = state.Value;
                childRuns++;
            });

            Assert.Equal(1, parentRuns);
            Assert.Equal(1, childRuns);

            childScope.Dispose();
            state.Value = 1;

            Assert.Equal(2, parentRuns);
            Assert.Equal(1, childRuns);
        }
    }
}
