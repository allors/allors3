// <copyright file="RangesExceptTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges.Long;

using Xunit;
using Range = Allors.Shared.Ranges.ValueRange<long>;

public class RangeExceptTests
{
    [Fact]
    public void NullWithNull()
    {
        var x = Range.Load();
        var y = Range.Load();
        var z = x.Except(y);

        Assert.True(z.IsEmpty);
    }

    [Fact]
    public void ValueWithNull()
    {
        var x = Range.Load(1);
        var y = Range.Load();
        var z = x.Except(y);

        Assert.Equal(new long[] { 1 }, z);
    }

    [Fact]
    public void PairWithNull()
    {
        var x = Range.Load(1, 2);
        var y = Range.Load();
        var z = x.Except(y);

        Assert.Equal(new long[] { 1, 2 }, z);
    }

    [Fact]
    public void ValueDuplicates()
    {
        var x = Range.Load(1);
        var y = Range.Load(1);
        var z = x.Except(y);

        Assert.True(z.IsEmpty);
    }

    [Fact]
    public void PairDuplicates()
    {
        var x = Range.Load(1, 2);
        var y = Range.Load(1, 2);
        var z = x.Except(y);

        Assert.True(z.IsEmpty);
    }

    [Fact]
    public void BeforePairWithPair()
    {
        var x = Range.Load(1, 2);
        var y = Range.Load(3, 4);
        var z = x.Except(y);

        Assert.Equal(new long[] { 1, 2 }, z);
    }

    [Fact]
    public void AfterPairWithPair()
    {
        var x = Range.Load(5, 6);
        var y = Range.Load(3, 4);
        var z = x.Except(y);

        Assert.Equal(new long[] { 5, 6 }, z);
    }


    [Fact]
    public void OverlappingPairWithPair()
    {
        var x = Range.Load(2, 5);
        var y = Range.Load(3, 4);
        var z = x.Except(y);

        Assert.Equal(new long[] { 2, 5 }, z);
    }

    [Fact]
    public void BeforeIntertwinedPairWithPair()
    {
        var x = Range.Load(2, 4);
        var y = Range.Load(3, 5);
        var z = x.Except(y);

        Assert.Equal(new long[] { 2, 4 }, z);
    }

    [Fact]
    public void AfterIntertwinedPairWithPair()
    {
        var x = Range.Load(3, 5);
        var y = Range.Load(2, 4);
        var z = x.Except(y);

        Assert.Equal(new long[] { 3, 5 }, z);
    }

    [Fact]
    public void TripletValueSameBegin()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(2);
        var z = x.Except(y);

        Assert.Equal(new long[] { 3, 4 }, z);
    }

    [Fact]
    public void TripletValueSameMiddle()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(3);
        var z = x.Except(y);

        Assert.Equal(new long[] { 2, 4 }, z);
    }

    [Fact]
    public void TripletValueSameEnd()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(3);
        var z = x.Except(y);

        Assert.Equal(new long[] { 2, 4 }, z);
    }

    [Fact]
    public void TripletPairSameBegin()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(2, 3);
        var z = x.Except(y);

        Assert.Equal(new long[] { 4 }, z);
    }

    [Fact]
    public void TripletPairSamePartialBegin()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(1, 2);
        var z = x.Except(y);

        Assert.Equal(new long[] { 3, 4 }, z);
    }

    [Fact]
    public void TripletPairSameEnd()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(4);
        var z = x.Except(y);

        Assert.Equal(new long[] { 2, 3 }, z);
    }

    [Fact]
    public void TripletPairSamePartialEnd()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(4, 5);
        var z = x.Except(y);

        Assert.Equal(new long[] { 2, 3 }, z);
    }

    [Fact]
    public void TripletPairSameBeginEnd()
    {
        var x = Range.Load(2, 3, 4);
        var y = Range.Load(2, 4);
        var z = x.Except(y);

        Assert.Equal(new long[] { 3 }, z);
    }
}
