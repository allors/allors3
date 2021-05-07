// <copyright file="LongArrayNumbersTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using System;
    using Xunit;

    public abstract class NumbersFromTests
    {
        public abstract INumbers Numbers { get; }

        [Fact]
        public void FromDefault()
        {
            var num = this.Numbers;

            var x = num.From();

            Assert.Equal(Array.Empty<long>(), num.Enumerate(x));
        }

        [Fact]
        public void FromValue()
        {
            var num = this.Numbers;

            var x = num.From(0L);

            Assert.Equal(new[] { 0L }, num.Enumerate(x));
        }

        [Fact]
        public void FromPair()
        {
            var num = this.Numbers;

            var x = num.From(0L, 1L);

            Assert.Equal(new[] { 0L, 1L }, num.Enumerate(x));
        }
    }
}
