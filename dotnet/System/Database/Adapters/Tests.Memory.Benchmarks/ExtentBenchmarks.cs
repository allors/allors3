// <copyright file="ExtentBenchmarks.cs" company="Allors bv">
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
public class ExtentBenchmarks
{
    private Database database = null!;
    private ITransaction transaction = null!;
    private MetaPopulation m = null!;

    [Params(100, 1000, 10000)]
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

        this.transaction = this.database.CreateTransaction();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this.transaction?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public object[] ExtentAll()
    {
        return this.transaction.Extent<C1>().ToArray();
    }

    [Benchmark]
    public object[] ExtentFiltered()
    {
        var extent = this.transaction.Extent(this.m.C1);
        extent.Filter.AddEquals(this.m.C1.C1AllorsInteger, this.ObjectCount / 2);
        return extent.ToArray();
    }

    [Benchmark]
    public object[] ExtentFilteredString()
    {
        var extent = this.transaction.Extent(this.m.C1);
        extent.Filter.AddEquals(this.m.C1.C1AllorsString, $"Name{this.ObjectCount / 2}");
        return extent.ToArray();
    }
}
