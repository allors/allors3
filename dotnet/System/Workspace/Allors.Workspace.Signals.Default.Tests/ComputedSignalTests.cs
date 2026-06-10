// <copyright file="ComputedSignalTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Tests
{
    using System;
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
    }
}
