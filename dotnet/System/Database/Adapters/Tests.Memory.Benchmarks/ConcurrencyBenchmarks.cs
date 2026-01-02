// <copyright file="ConcurrencyBenchmarks.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Meta;
using C1 = Domain.C1;
using C2 = Domain.C2;

/// <summary>
/// Benchmarks for concurrency and lock striping performance.
/// Tests parallel commits with disjoint vs overlapping object sets.
///
/// Lock striping allows transactions affecting different "stripes" (based on object ID)
/// to commit in parallel, reducing contention compared to a single global lock.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ConcurrencyBenchmarks
{
    private Database database = null!;
    private MetaPopulation m = null!;
    private long[] existingObjectIds = null!;

    [Params(4, 8, 16)]
    public int ThreadCount { get; set; }

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

        // Pre-populate with objects for modification tests
        using var setupTx = this.database.CreateTransaction();
        this.existingObjectIds = new long[1000];
        for (var i = 0; i < 1000; i++)
        {
            var c1 = (C1)setupTx.Create<C1>();
            c1.C1AllorsString = $"Initial{i}";
            c1.C1AllorsInteger = i;
            this.existingObjectIds[i] = c1.Id;
        }
        setupTx.Commit();
    }

    /// <summary>
    /// Baseline: Parallel commits creating new objects.
    /// New objects get sequential IDs, which should distribute across stripes.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void ParallelNewObjectCommits()
    {
        Parallel.For(0, this.ThreadCount, _ =>
        {
            using var tx = this.database.CreateTransaction();
            var c1 = (C1)tx.Create<C1>();
            c1.C1AllorsString = $"Created{Environment.CurrentManagedThreadId}";
            tx.Commit();
        });
    }

    /// <summary>
    /// Parallel commits modifying disjoint object sets.
    /// Each thread modifies objects in a different ID range.
    /// Lock striping should allow these to run in parallel.
    /// </summary>
    [Benchmark]
    public void ParallelDisjointModifications()
    {
        var objectsPerThread = this.existingObjectIds.Length / this.ThreadCount;

        Parallel.For(0, this.ThreadCount, threadIndex =>
        {
            using var tx = this.database.CreateTransaction();

            // Each thread modifies a disjoint range of objects
            var startIndex = threadIndex * objectsPerThread;
            var endIndex = Math.Min(startIndex + objectsPerThread, this.existingObjectIds.Length);

            for (var i = startIndex; i < endIndex; i++)
            {
                var c1 = (C1)tx.Instantiate(this.existingObjectIds[i])!;
                c1.C1AllorsInteger = c1.C1AllorsInteger + 1;
            }

            tx.Commit();
        });
    }

    /// <summary>
    /// Parallel commits creating objects with relationships.
    /// Tests lock acquisition for related objects.
    /// </summary>
    [Benchmark]
    public void ParallelCommitsWithRelationships()
    {
        Parallel.For(0, this.ThreadCount, threadIndex =>
        {
            using var tx = this.database.CreateTransaction();

            // Create parent and children
            var parent = (C1)tx.Create<C1>();
            parent.C1AllorsString = $"Parent{threadIndex}";

            for (var i = 0; i < 5; i++)
            {
                var child = (C2)tx.Create<C2>();
                child.C2AllorsString = $"Child{threadIndex}_{i}";
                parent.AddC1C2one2many(child);
            }

            tx.Commit();
        });
    }

    /// <summary>
    /// Parallel read-only transactions.
    /// Reads should be lock-free with ConcurrentDictionary.
    /// </summary>
    [Benchmark]
    public void ParallelReads()
    {
        Parallel.For(0, this.ThreadCount, _ =>
        {
            using var tx = this.database.CreateTransaction();
            var count = 0;
            foreach (var obj in tx.Extent<C1>())
            {
                count++;
            }
            _ = count; // Prevent optimization
        });
    }

    /// <summary>
    /// Mixed read-write workload.
    /// Tests interaction between readers and writers.
    /// </summary>
    [Benchmark]
    public void MixedReadWrite()
    {
        Parallel.For(0, this.ThreadCount, i =>
        {
            using var tx = this.database.CreateTransaction();
            if (i % 2 == 0)
            {
                // Reader - iterate through objects
                var count = 0;
                foreach (var obj in tx.Extent<C1>())
                {
                    var c1 = (C1)obj;
                    _ = c1.C1AllorsString;
                    count++;
                }
                _ = count;
            }
            else
            {
                // Writer - create and commit
                var c1 = (C1)tx.Create<C1>();
                c1.C1AllorsString = $"Mixed{i}";
                tx.Commit();
            }
        });
    }

    /// <summary>
    /// High-contention scenario: multiple threads trying to modify the same objects.
    /// This tests optimistic concurrency conflict handling.
    /// </summary>
    [Benchmark]
    public void HighContentionModifications()
    {
        var targetObjectId = this.existingObjectIds[0];
        var successCount = 0;
        var conflictCount = 0;

        Parallel.For(0, this.ThreadCount, _ =>
        {
            using var tx = this.database.CreateTransaction();
            try
            {
                var c1 = (C1)tx.Instantiate(targetObjectId)!;
                c1.C1AllorsInteger = c1.C1AllorsInteger + 1;
                tx.Commit();
                Interlocked.Increment(ref successCount);
            }
            catch (ConcurrencyException)
            {
                // Expected when multiple threads modify the same object
                Interlocked.Increment(ref conflictCount);
            }
        });

        // At least one should succeed
        _ = successCount + conflictCount;
    }

    /// <summary>
    /// Throughput test: many small transactions.
    /// Measures how well lock striping handles high transaction rates.
    /// </summary>
    [Benchmark]
    public void HighThroughputSmallTransactions()
    {
        const int transactionsPerThread = 100;

        Parallel.For(0, this.ThreadCount, threadIndex =>
        {
            for (var t = 0; t < transactionsPerThread; t++)
            {
                using var tx = this.database.CreateTransaction();
                var c1 = (C1)tx.Create<C1>();
                c1.C1AllorsString = $"T{threadIndex}_{t}";
                tx.Commit();
            }
        });
    }
}
