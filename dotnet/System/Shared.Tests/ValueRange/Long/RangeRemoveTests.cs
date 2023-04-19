// <copyright file="RangesRemoveTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges.Long;

using Xunit;
using Range = Allors.Shared.Ranges.ValueRange<long>;

public abstract class RangeRemoveTests
{
    [Fact]
    public void Null()
    {
        var x = Range.Load();
        const int y = 2;
        var z = x.Remove(y);

        Assert.Empty(z);
    }

    [Fact]
    public void ValueBefore()
    {
        var x = Range.Load(1);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 1 }, z);
    }

    [Fact]
    public void ValueAfter()
    {
        var x = Range.Load(3);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 3 }, z);
    }

    [Fact]
    public void ValueSame()
    {
        var x = Range.Load(2);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Empty(z);
    }

    [Fact]
    public void PairBefore()
    {
        var x = Range.Load(-2, 1);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { -2, 1 }, z);
    }

    [Fact]
    public void PairAfter()
    {
        var x = Range.Load(3, 4);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 3, 4 }, z);
    }

    [Fact]
    public void PairOverlapping()
    {
        var x = Range.Load(1, 3);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 1, 3 }, z);
    }

    [Fact]
    public void PairSameBegin()
    {
        var x = Range.Load(1, 2);
        const int y = 1;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 2 }, z);
    }

    [Fact]
    public void PairSameEnd()
    {
        var x = Range.Load(1, 2);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 1 }, z);
    }

    [Fact]
    public void TripletSameMiddle()
    {
        var x = Range.Load(1, 2, 3);
        const int y = 2;
        var z = x.Remove(y);

        Assert.Equal(new long[] { 1, 3 }, z);
    }
}
