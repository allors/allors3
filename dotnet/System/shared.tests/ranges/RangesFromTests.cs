// <copyright file="RangesFromTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System;
    using Xunit;

    public abstract class RangesFromTests
    {
        public abstract IRanges Ranges { get; }

        [Fact]
        public void FromDefault()
        {
            var num = this.Ranges;

            var x = num.New();

            Assert.Equal(Array.Empty<long>(), x);
        }

        [Fact]
        public void FromValue()
        {
            var num = this.Ranges;

            var x = num.New(0L);

            Assert.Equal(new[] { 0L }, x);
        }

        [Fact]
        public void FromPair()
        {
            var num = this.Ranges;

            var x = num.New(0L, 1L);

            Assert.Equal(new[] { 0L, 1L }, x);
        }
    }
}
