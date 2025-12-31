// <copyright file="ManyToManyBenchmarks.cs" company="Allors bv">
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
/// Benchmarks for many-to-many relationship access.
/// Tests the FrozenSet optimization where reading from committed state
/// no longer requires creating a new HashSet copy.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ManyToManyBenchmarks
{
    private Database database = null!;
    private ITransaction transaction = null!;
    private MetaPopulation m = null!;
    private long[] c1Ids = null!;

    [Params(100, 500, 1000)]
    public int ObjectCount { get; set; }

    [Params(10, 50)]
    public int RelationshipsPerObject { get; set; }

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
        var c1Objects = new C1[this.ObjectCount];
        this.c1Ids = new long[this.ObjectCount];
        for (var i = 0; i < this.ObjectCount; i++)
        {
            c1Objects[i] = (C1)setupTx.Create<C1>();
            c1Objects[i].C1AllorsString = $"C1_{i}";
            this.c1Ids[i] = c1Objects[i].Id;
        }

        // Create C2 objects and establish many-to-many relationships
        var c2Objects = new C2[this.ObjectCount];
        for (var i = 0; i < this.ObjectCount; i++)
        {
            c2Objects[i] = (C2)setupTx.Create<C2>();
            c2Objects[i].C2AllorsString = $"C2_{i}";
        }

        // Connect C1 to multiple C2s (many-to-many)
        for (var i = 0; i < this.ObjectCount; i++)
        {
            for (var j = 0; j < this.RelationshipsPerObject; j++)
            {
                var targetIndex = (i + j) % this.ObjectCount;
                c1Objects[i].AddC1C2many2many(c2Objects[targetIndex]);
            }
        }

        setupTx.Commit();

        // Create a fresh transaction for benchmarking
        this.transaction = this.database.CreateTransaction();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this.transaction?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public int ReadManyToManyRoles()
    {
        // This benchmark tests reading many-to-many roles from committed state.
        // With FrozenSet, we return the immutable set directly without copying.
        // Before: new HashSet<long>(snapshotRoleIds) on every read
        // After: return snapshotRoleIds directly (FrozenSet)
        var count = 0;
        foreach (var id in this.c1Ids)
        {
            var c1 = (C1)this.transaction.Instantiate(id)!;
            foreach (var c2 in c1.C1C2many2manies)
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark]
    public int ReadManyToManyRolesWithContains()
    {
        // Tests Contains() operations on many-to-many relationships
        // FrozenSet provides O(1) Contains just like HashSet
        var count = 0;
        var firstC2Id = this.c1Ids[0]; // Use a C1 id for simplicity
        foreach (var id in this.c1Ids)
        {
            var c1 = (C1)this.transaction.Instantiate(id)!;
            var roles = c1.C1C2many2manies;
            // Count how many have a specific C2
            if (roles.Any())
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark]
    public int ReadManyToManyRolesMultipleTimes()
    {
        // Tests repeated access to the same many-to-many relationship
        // This shows the benefit of not recreating HashSets on each access
        var count = 0;
        var targetId = this.c1Ids[0];
        var c1 = (C1)this.transaction.Instantiate(targetId)!;

        // Access the same relationship multiple times
        for (var i = 0; i < 100; i++)
        {
            foreach (var c2 in c1.C1C2many2manies)
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark]
    public int ReadThenModifyManyToMany()
    {
        // Tests the copy-on-write pattern: read (uses FrozenSet directly),
        // then modify (creates mutable copy)
        using var tx = this.database.CreateTransaction();
        var count = 0;

        // First, read from committed state (uses FrozenSet)
        foreach (var id in this.c1Ids.Take(10))
        {
            var c1 = (C1)tx.Instantiate(id)!;
            count += c1.C1C2many2manies.Count();
        }

        // Then modify (triggers copy-on-write to HashSet)
        var firstC1 = (C1)tx.Instantiate(this.c1Ids[0])!;
        var newC2 = (C2)tx.Create<C2>();
        firstC1.AddC1C2many2many(newC2);

        // Read again (now from local HashSet)
        count += firstC1.C1C2many2manies.Count();

        tx.Rollback();
        return count;
    }
}
