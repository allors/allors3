// <copyright file="NumbersRemoveTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using Xunit;

    public abstract class NumbersRemoveTests
    {
        public abstract INumbers Numbers { get; }

        [Fact]
        public void Null()
        {
            var num = this.Numbers;

            var x = num.From();
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Null(z);
        }

        [Fact]
        public void ValueBefore()
        {
            var num = this.Numbers;

            var x = num.From(0);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {0}, num.Enumerate(z));
        }

        [Fact]
        public void ValueAfter()
        {
            var num = this.Numbers;

            var x = num.From(2);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {2}, num.Enumerate(z));
        }

        [Fact]
        public void ValueSame()
        {
            var num = this.Numbers;

            var x = num.From(1);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Null(z);
        }

        [Fact]
        public void PairBefore()
        {
            var num = this.Numbers;

            var x = num.From(-1, 0);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {-1, 0}, num.Enumerate(z));
        }

        [Fact]
        public void PairAfter()
        {
            var num = this.Numbers;

            var x = num.From(2, 3);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {2, 3}, num.Enumerate(z));
        }

        [Fact]
        public void PairOverlapping()
        {
            var num = this.Numbers;

            var x = num.From(0, 2);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {0, 2}, num.Enumerate(z));
        }

        [Fact]
        public void PairSameBegin()
        {
            var num = this.Numbers;

            var x = num.From(0, 1);
            const int y = 0;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {1}, num.Enumerate(z));
        }

        [Fact]
        public void PairSameEnd()
        {
            var num = this.Numbers;

            var x = num.From(0, 1);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {0}, num.Enumerate(z));
        }

        [Fact]
        public void TripletSameMiddle()
        {
            var num = this.Numbers;

            var x = num.From(0, 1, 2);
            const int y = 1;
            var z = num.Remove(x, y);

            Assert.Equal(new long[] {0, 2}, num.Enumerate(z));
        }
    }
}
