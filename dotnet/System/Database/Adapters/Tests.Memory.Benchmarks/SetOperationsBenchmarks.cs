// <copyright file="SetOperationsBenchmarks.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Benchmarks comparing sorted merge operations vs HashSet operations.
/// Tests show the performance characteristics of each approach.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class SetOperationsBenchmarks
{
    private long[] sortedLeft = null!;
    private long[] sortedRight = null!;
    private HashSet<long> hashSetLeft = null!;
    private HashSet<long> hashSetRight = null!;

    [Params(100, 1000, 10000)]
    public int Size { get; set; }

    [Params(0.1, 0.5)] // 10% and 50% overlap
    public double Overlap { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Create sorted arrays with specified overlap
        var overlapCount = (int)(this.Size * this.Overlap);
        var uniqueLeft = this.Size - overlapCount;
        var uniqueRight = this.Size - overlapCount;

        // Left: [0..uniqueLeft) union [uniqueLeft..uniqueLeft+overlapCount)
        // Right: [uniqueLeft..uniqueLeft+overlapCount) union [uniqueLeft+overlapCount..uniqueLeft+overlapCount+uniqueRight)
        this.sortedLeft = new long[this.Size];
        this.sortedRight = new long[this.Size];

        // Fill left array: unique elements then overlapping
        for (var i = 0; i < uniqueLeft; i++)
        {
            this.sortedLeft[i] = i;
        }

        for (var i = 0; i < overlapCount; i++)
        {
            this.sortedLeft[uniqueLeft + i] = uniqueLeft + i;
        }

        // Fill right array: overlapping elements then unique
        for (var i = 0; i < overlapCount; i++)
        {
            this.sortedRight[i] = uniqueLeft + i;
        }

        for (var i = 0; i < uniqueRight; i++)
        {
            this.sortedRight[overlapCount + i] = uniqueLeft + overlapCount + i;
        }

        // Arrays are already sorted by construction
        Array.Sort(this.sortedLeft);
        Array.Sort(this.sortedRight);

        // Create HashSets for comparison
        this.hashSetLeft = new HashSet<long>(this.sortedLeft);
        this.hashSetRight = new HashSet<long>(this.sortedRight);
    }

    [Benchmark(Baseline = true)]
    public long[] HashSet_Intersect()
    {
        var result = new HashSet<long>(this.hashSetLeft);
        result.IntersectWith(this.hashSetRight);
        return result.ToArray();
    }

    [Benchmark]
    public long[] SortedMerge_Intersect()
    {
        return SetOperations.IntersectSorted(this.sortedLeft, this.sortedRight);
    }

    [Benchmark]
    public long[] HashSet_Union()
    {
        var result = new HashSet<long>(this.hashSetLeft);
        result.UnionWith(this.hashSetRight);
        return result.ToArray();
    }

    [Benchmark]
    public long[] SortedMerge_Union()
    {
        return SetOperations.UnionSorted(this.sortedLeft, this.sortedRight);
    }

    [Benchmark]
    public long[] HashSet_Except()
    {
        var result = new HashSet<long>(this.hashSetLeft);
        result.ExceptWith(this.hashSetRight);
        return result.ToArray();
    }

    [Benchmark]
    public long[] SortedMerge_Except()
    {
        return SetOperations.ExceptSorted(this.sortedLeft, this.sortedRight);
    }
}
