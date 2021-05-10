// <copyright file="NumbersUnionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using Xunit;

    public abstract class NumbersUnionTests
    {
        public abstract INumbers Numbers { get; }

        [Fact]
        public void NullWithNull()
        {
            var num = this.Numbers;

            var x = num.From();
            var y = num.From();
            var z = num.Union(x, y);

            Assert.Null(z);
        }

        [Fact]
        public void ValueWithNull()
        {
            var num = this.Numbers;

            var x = num.From(0);
            var y = num.From();
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 0 }, num.Enumerate(z));
        }

        [Fact]
        public void PairWithNull()
        {
            var num = this.Numbers;

            var x = num.From(0, 1);
            var y = num.From();
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 0, 1 }, num.Enumerate(z));
        }

        [Fact]
        public void ValueDuplicates()
        {
            var num = this.Numbers;

            var x = num.From(0);
            var y = num.From(0);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 0 }, num.Enumerate(z));
        }

        [Fact]
        public void PairDuplicates()
        {
            var num = this.Numbers;

            var x = num.From(0, 1);
            var y = num.From(0, 1);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 0, 1 }, num.Enumerate(z));
        }


        [Fact]
        public void BeforePairWithPair()
        {
            var num = this.Numbers;

            var x = num.From(0, 1);
            var y = num.From(2, 3);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 0, 1, 2, 3 }, num.Enumerate(z));
        }

        [Fact]
        public void AfterPairWithPair()
        {
            var num = this.Numbers;

            var x = num.From(4, 5);
            var y = num.From(2, 3);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 2, 3, 4, 5 }, num.Enumerate(z));
        }


        [Fact]
        public void OverlappingPairWithPair()
        {
            var num = this.Numbers;

            var x = num.From(1, 4);
            var y = num.From(2, 3);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 1, 2, 3, 4 }, num.Enumerate(z));
        }

        [Fact]
        public void BeforeIntertwinedPairWithPair()
        {
            var num = this.Numbers;

            var x = num.From(1, 3);
            var y = num.From(2, 4);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 1, 2, 3, 4 }, num.Enumerate(z));
        }

        [Fact]
        public void AfterIntertwinedPairWithPair()
        {
            var num = this.Numbers;

            var x = num.From(2, 4);
            var y = num.From(1, 3);
            var z = num.Union(x, y);

            Assert.Equal(new long[] { 1, 2, 3, 4 }, num.Enumerate(z));
        }
    }
}
