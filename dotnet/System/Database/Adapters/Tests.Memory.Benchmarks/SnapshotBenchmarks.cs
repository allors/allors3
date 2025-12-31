// <copyright file="SnapshotBenchmarks.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Meta;
using C1 = Domain.C1;

[MemoryDiagnoser]
[ShortRunJob]
public class SnapshotBenchmarks
{
    private Database database = null!;
    private MetaPopulation m = null!;

    [Params(100, 500, 1000)]
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

        using var setupTx = this.database.CreateTransaction();
        for (var i = 0; i < this.ObjectCount; i++)
        {
            var c1 = (C1)setupTx.Create<C1>();
            c1.C1AllorsString = $"Name{i}";
            c1.C1AllorsInteger = i;
        }
        setupTx.Commit();
    }

    [Benchmark(Baseline = true)]
    public int ReadOnlyTransaction()
    {
        // Measures snapshot cloning overhead for read-only access
        using var tx = this.database.CreateTransaction();
        var count = 0;
        foreach (var obj in tx.Extent<C1>())
        {
            var c1 = (C1)obj;
            if (c1.C1AllorsString != null)
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark]
    public int ModifyAndRollback()
    {
        // Measures rollback overhead
        using var tx = this.database.CreateTransaction();
        var count = 0;
        foreach (var obj in tx.Extent<C1>())
        {
            var c1 = (C1)obj;
            c1.C1AllorsString = "Modified";
            count++;
        }
        tx.Rollback();
        return count;
    }

    [Benchmark]
    public int ModifyAndCommit()
    {
        // Measures commit overhead
        using var tx = this.database.CreateTransaction();
        var first = (C1?)tx.Extent<C1>().FirstOrDefault();
        if (first != null)
        {
            first.C1AllorsString = $"Modified{Random.Shared.Next()}";
        }
        tx.Commit();
        return 1;
    }
}
