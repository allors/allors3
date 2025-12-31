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

[MemoryDiagnoser]
[ShortRunJob]
public class ConcurrencyBenchmarks
{
    private Database database = null!;
    private MetaPopulation m = null!;

    [Params(2, 4, 8)]
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

        // Pre-populate with some objects
        using var setupTx = this.database.CreateTransaction();
        for (var i = 0; i < 100; i++)
        {
            var c1 = (C1)setupTx.Create<C1>();
            c1.C1AllorsString = $"Initial{i}";
        }
        setupTx.Commit();
    }

    [Benchmark(Baseline = true)]
    public void ParallelCommits()
    {
        // Measures lock contention during parallel commits
        Parallel.For(0, this.ThreadCount, _ =>
        {
            using var tx = this.database.CreateTransaction();
            var c1 = (C1)tx.Create<C1>();
            c1.C1AllorsString = $"Created{Environment.CurrentManagedThreadId}";
            tx.Commit();
        });
    }

    [Benchmark]
    public void ParallelReads()
    {
        // Measures read concurrency (should be lock-free ideally)
        Parallel.For(0, this.ThreadCount, _ =>
        {
            using var tx = this.database.CreateTransaction();
            var count = tx.Extent<C1>().Count();
            _ = count; // Prevent optimization
        });
    }

    [Benchmark]
    public void MixedReadWrite()
    {
        // Half readers, half writers
        Parallel.For(0, this.ThreadCount, i =>
        {
            using var tx = this.database.CreateTransaction();
            if (i % 2 == 0)
            {
                // Reader
                _ = tx.Extent<C1>().Count();
            }
            else
            {
                // Writer
                var c1 = (C1)tx.Create<C1>();
                c1.C1AllorsString = $"Mixed{i}";
                tx.Commit();
            }
        });
    }
}
