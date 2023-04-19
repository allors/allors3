// <copyright file="RangesFromTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges.Long;

using System;
using System.Linq;
using Xunit;
using Range = Allors.Shared.Ranges.ValueRange<long>;

public class RangeLoadTests
{
    [Fact]
    public void LoadDefault()
    {
        var x = Range.Load();

        Assert.Equal(Array.Empty<long>(), x);
    }

    [Fact]
    public void LoadValue()
    {
        var x = Range.Load(1L);

        Assert.Equal(new[] { 1L }, x);
    }

    [Fact]
    public void LoadPair()
    {
        var x = Range.Load(1L, 2L);

        Assert.Equal(new[] { 1L, 2L }, x);
    }

    [Fact]
    public void LoadDistinctIterator()
    {
        var distinctIterator = Array.Empty<long>().Distinct();

        var x = Range.Load(distinctIterator);

        Assert.True(x.IsEmpty);
        Assert.Null(x.Save());
    }
}
