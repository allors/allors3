// <copyright file="ExtentOperationBenchmarks.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Meta;
using C1 = Domain.C1;
using C2 = Domain.C2;

[MemoryDiagnoser]
[ShortRunJob]
public class ExtentOperationBenchmarks
{
    private Database database = null!;
    private ITransaction transaction = null!;
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

        // Create C1 objects
        for (var i = 0; i < this.ObjectCount; i++)
        {
            var c1 = (C1)setupTx.Create<C1>();
            c1.C1AllorsString = $"C1Name{i}";
            c1.C1AllorsInteger = i;
        }

        // Create C2 objects
        for (var i = 0; i < this.ObjectCount; i++)
        {
            var c2 = (C2)setupTx.Create<C2>();
            c2.C2AllorsString = $"C2Name{i}";
        }

        setupTx.Commit();
        this.transaction = this.database.CreateTransaction();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this.transaction?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public object[] ExtentUnion()
    {
        // This tests the O(n²) List.Contains issue
        var extent1 = this.transaction.Extent(this.m.C1);
        extent1.Filter.AddLessThan(this.m.C1.C1AllorsInteger, this.ObjectCount / 2);

        var extent2 = this.transaction.Extent(this.m.C1);
        extent2.Filter.AddGreaterThan(this.m.C1.C1AllorsInteger, this.ObjectCount / 4);

        var union = this.transaction.Union(extent1, extent2);
        return union.ToArray();
    }

    [Benchmark]
    public object[] ExtentIntersect()
    {
        var extent1 = this.transaction.Extent(this.m.C1);
        extent1.Filter.AddLessThan(this.m.C1.C1AllorsInteger, this.ObjectCount * 3 / 4);

        var extent2 = this.transaction.Extent(this.m.C1);
        extent2.Filter.AddGreaterThan(this.m.C1.C1AllorsInteger, this.ObjectCount / 4);

        var intersect = this.transaction.Intersect(extent1, extent2);
        return intersect.ToArray();
    }

    [Benchmark]
    public object[] ExtentExcept()
    {
        var extent1 = this.transaction.Extent(this.m.C1);

        var extent2 = this.transaction.Extent(this.m.C1);
        extent2.Filter.AddGreaterThan(this.m.C1.C1AllorsInteger, this.ObjectCount / 2);

        var except = this.transaction.Except(extent1, extent2);
        return except.ToArray();
    }
}
