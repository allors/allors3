// <copyright file="MultiIndexBenchmarks.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Meta;
using C1 = Domain.C1;
using C2 = Domain.C2;

/// <summary>
/// Benchmarks for multi-index intersection queries.
/// Tests the performance improvement of using multiple indexes simultaneously
/// compared to single-index evaluation followed by predicate filtering.
///
/// Scenario: Query with multiple equality predicates where each has an index.
/// Before: Use first index, then filter all candidates through remaining predicates.
/// After: Intersect all index results first, then filter the smaller candidate set.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class MultiIndexBenchmarks
{
    private Database database = null!;
    private ITransaction transaction = null!;
    private MetaPopulation m = null!;

    // Test data characteristics
    private const int TotalObjects = 10000;
    private const int StringValues = 100;   // 100 distinct string values -> ~100 objects per value
    private const int IntValues = 50;       // 50 distinct int values -> ~200 objects per value

    // Query target values (should return ~2 objects when intersected)
    private string targetString = null!;
    private int targetInt;

    [Params(10000)]
    public int ObjectCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var metaPopulation = new MetaBuilder().Build();
        var services = new DefaultDomainDatabaseServices();
        this.database = new Database(services, new Configuration
        {
            ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
        });

        this.m = this.database.Context().M;

        // Create indexes on both string and integer properties
        this.database.IndexStore.CreateUnitRoleIndex(this.m.C1.C1AllorsString);
        this.database.IndexStore.CreateUnitRoleIndex(this.m.C1.C1AllorsInteger);

        using var setupTx = this.database.CreateTransaction();

        // Create objects with varied property values
        // Each string value appears in ~100 objects
        // Each int value appears in ~200 objects
        // Combined, each (string, int) pair appears in ~2 objects
        var random = new Random(42); // Fixed seed for reproducibility
        for (var i = 0; i < this.ObjectCount; i++)
        {
            var c1 = (C1)setupTx.Create<C1>();
            c1.C1AllorsString = $"Value_{i % StringValues}";
            c1.C1AllorsInteger = i % IntValues;
        }

        setupTx.Commit();

        // Select target values that should match few objects when combined
        this.targetString = "Value_25";  // ~100 objects
        this.targetInt = 25;             // ~200 objects
        // Combined: only objects where (i % 100 == 25) AND (i % 50 == 25)
        // This means i must be 25, 125, 225, ... (every 100) AND also 25, 75, 125, ... (every 50)
        // LCM(100, 50) = 100, so ~100 objects match

        // Create a fresh transaction for benchmarking
        this.transaction = this.database.CreateTransaction();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this.transaction?.Dispose();
    }

    /// <summary>
    /// Baseline: Query with single indexed predicate plus filter.
    /// Uses only string index, then filters by integer.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int SingleIndexQuery()
    {
        // Query using only the string predicate (index will be used)
        var extent = this.transaction.Extent<C1>();
        extent.Filter.AddEquals(this.m.C1.C1AllorsString, this.targetString);

        var count = 0;
        foreach (var c1 in extent)
        {
            // Manual filter by integer
            if (((C1)c1).C1AllorsInteger == this.targetInt)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Multi-index: Query with both indexed predicates.
    /// Uses both string AND integer indexes, intersects results.
    /// </summary>
    [Benchmark]
    public int MultiIndexQuery()
    {
        var extent = this.transaction.Extent<C1>();
        extent.Filter.AddEquals(this.m.C1.C1AllorsString, this.targetString);
        extent.Filter.AddEquals(this.m.C1.C1AllorsInteger, this.targetInt);

        var count = 0;
        foreach (var c1 in extent)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Full scan baseline: Query without any indexes.
    /// Scans all objects and applies both predicates.
    /// </summary>
    [Benchmark]
    public int FullScanQuery()
    {
        var extent = this.transaction.Extent<C1>();
        // Use predicates that don't have indexes (negations bypass index)
        // Actually, we'll use a different approach - query all and filter manually
        var count = 0;
        foreach (var c1 in this.transaction.Extent<C1>())
        {
            var obj = (C1)c1;
            if (obj.C1AllorsString == this.targetString && obj.C1AllorsInteger == this.targetInt)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Tests intersection selectivity with different overlap ratios.
    /// High selectivity = small result set after intersection.
    /// </summary>
    [Benchmark]
    public int HighSelectivityMultiIndex()
    {
        // Query for rare combination
        var extent = this.transaction.Extent<C1>();
        extent.Filter.AddEquals(this.m.C1.C1AllorsString, "Value_0");  // ~100 objects
        extent.Filter.AddEquals(this.m.C1.C1AllorsInteger, 0);          // ~200 objects
        // Combined: objects where i % 100 == 0 AND i % 50 == 0, i.e., i % 100 == 0
        // So actually 100 objects match (0, 100, 200, ...)

        var count = 0;
        foreach (var c1 in extent)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests three-way intersection (if supported).
    /// Adds a third indexed predicate.
    /// </summary>
    [Benchmark]
    public int ThreeIndexQuery()
    {
        var extent = this.transaction.Extent<C1>();
        extent.Filter.AddEquals(this.m.C1.C1AllorsString, this.targetString);
        extent.Filter.AddEquals(this.m.C1.C1AllorsInteger, this.targetInt);
        // Add existence check (third predicate, but not indexed by default)
        extent.Filter.AddExists(this.m.C1.C1AllorsString);

        var count = 0;
        foreach (var c1 in extent)
        {
            count++;
        }

        return count;
    }
}
