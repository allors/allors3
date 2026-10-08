// <copyright file="MemoryCacheTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Ranges;
    using Allors.Workspace.Connection;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;
    using Grant = Allors.Workspace.Connection.Grant;
    using Permission = Allors.Workspace.Connection.Permission;
    using Revocation = Allors.Workspace.Connection.Revocation;

    /// <summary>
    /// The memory cache on its own: what it keeps, the version guard on set, the record-changed
    /// event, the hooks, the key it is bound to, and that it holds under concurrent use.
    /// </summary>
    public class MemoryCacheTests
    {
        private readonly M m = new MetaBuilder().Build();

        private readonly IRanges<long> ranges = new DefaultStructRanges<long>();

        [Fact]
        public void ARecordIsHeldById()
        {
            var cache = new MemoryCache("Default", this.m);
            var record = new FakeRecord(this.m.C1, 1, 1);

            Assert.True(cache.SetRecord(record));

            Assert.Same(record, cache.GetRecord(1));
            Assert.Null(cache.GetRecord(2));
        }

        [Fact]
        public void AnOlderVersionIsRefused()
        {
            var cache = new MemoryCache("Default", this.m);
            var newer = new FakeRecord(this.m.C1, 1, 2);
            var older = new FakeRecord(this.m.C1, 1, 1);

            cache.SetRecord(newer);

            Assert.False(cache.SetRecord(older));
            Assert.Same(newer, cache.GetRecord(1));
        }

        [Fact]
        public void TheSameOrANewerVersionReplaces()
        {
            var cache = new MemoryCache("Default", this.m);
            var first = new FakeRecord(this.m.C1, 1, 1);
            var same = new FakeRecord(this.m.C1, 1, 1);
            var newer = new FakeRecord(this.m.C1, 1, 2);

            cache.SetRecord(first);

            Assert.True(cache.SetRecord(same));
            Assert.Same(same, cache.GetRecord(1));

            Assert.True(cache.SetRecord(newer));
            Assert.Same(newer, cache.GetRecord(1));
        }

        [Fact]
        public void RecordChangedIsRaisedWhenARecordIsReplaced()
        {
            var cache = new MemoryCache("Default", this.m);
            var changed = new List<RecordChangedEventArgs>();
            cache.RecordChanged += (sender, e) => changed.Add(e);

            var first = new FakeRecord(this.m.C1, 1, 1);
            cache.SetRecord(first);
            Assert.Empty(changed);

            var newer = new FakeRecord(this.m.C1, 1, 2);
            cache.SetRecord(newer);
            var e = Assert.Single(changed);
            Assert.Equal(1, e.Id);
            Assert.Same(newer, e.Record);

            cache.SetRecord(first);
            Assert.Single(changed);
        }

        [Fact]
        public void RemoveRecordForgetsTheRecord()
        {
            var cache = new MemoryCache("Default", this.m);
            cache.SetRecord(new FakeRecord(this.m.C1, 1, 1));

            cache.RemoveRecord(1);

            Assert.Null(cache.GetRecord(1));
            cache.RemoveRecord(1);
        }

        [Fact]
        public void ClearForgetsEverythingAndTheKey()
        {
            var cache = new MemoryCache("Default", this.m);
            cache.SetRecord(new FakeRecord(this.m.C1, 1, 1));
            cache.SetGrant(new Grant(10, 1, this.ranges.Load(100)));
            cache.SetRevocation(new Revocation(20, 1, this.ranges.Load(101)));
            cache.SetPermission(new Permission(100, this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            cache.Bind(new CacheKey("db", 5, "Default", "fingerprint"));

            cache.Clear();

            Assert.Null(cache.GetRecord(1));
            Assert.Null(cache.GetGrant(10));
            Assert.Null(cache.GetRevocation(20));
            Assert.False(cache.HasPermission(100));
            Assert.Equal(0, cache.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.Null(cache.Key);
        }

        [Fact]
        public void GrantsAndRevocationsKeepTheNewestVersion()
        {
            var cache = new MemoryCache("Default", this.m);

            var newerGrant = new Grant(10, 2, this.ranges.Load(100));
            cache.SetGrant(newerGrant);
            cache.SetGrant(new Grant(10, 1, this.ranges.Load(101)));
            Assert.Same(newerGrant, cache.GetGrant(10));

            var newerRevocation = new Revocation(20, 2, this.ranges.Load(100));
            cache.SetRevocation(newerRevocation);
            cache.SetRevocation(new Revocation(20, 1, this.ranges.Load(101)));
            Assert.Same(newerRevocation, cache.GetRevocation(20));

            var newestGrant = new Grant(10, 3, this.ranges.Load(102));
            cache.SetGrant(newestGrant);
            Assert.Same(newestGrant, cache.GetGrant(10));
        }

        [Fact]
        public void APermissionIsFoundByClassOperandTypeAndOperation()
        {
            var cache = new MemoryCache("Default", this.m);

            cache.SetPermission(new Permission(100, this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            cache.SetPermission(new Permission(101, this.m.C1, this.m.C1.C1AllorsString, Operations.Write));
            cache.SetPermission(new Permission(102, this.m.C1, this.m.C1.ClassMethod, Operations.Execute));

            Assert.True(cache.HasPermission(100));
            Assert.False(cache.HasPermission(103));
            Assert.Equal(100, cache.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.Equal(101, cache.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Write));
            Assert.Equal(102, cache.GetPermission(this.m.C1, this.m.C1.ClassMethod, Operations.Execute));
            Assert.Equal(0, cache.GetPermission(this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read));
            Assert.Equal(0, cache.GetPermission(this.m.C2, this.m.C1.C1AllorsString, Operations.Read));
        }

        [Fact]
        public void TheCacheKnowsTheWorkspaceNameAndTheMetaPopulationItServes()
        {
            var cache = new MemoryCache("Default", this.m);

            Assert.Equal("Default", cache.WorkspaceName);
            Assert.Same(this.m, cache.MetaPopulation);
            Assert.Null(cache.Key);
        }

        [Fact]
        public void BindTakesOneKeyAndRefusesAnother()
        {
            var cache = new MemoryCache("Default", this.m);
            var key = new CacheKey("db", 5, "Default", "fingerprint");

            cache.Bind(key);
            cache.Bind(new CacheKey("db", 5, "Default", "fingerprint"));
            Assert.Equal(key, cache.Key);

            var exception = Assert.Throws<InvalidOperationException>(() => cache.Bind(new CacheKey("db", 7, "Default", "fingerprint")));
            Assert.Contains("5", exception.Message);
            Assert.Contains("7", exception.Message);
            Assert.Equal(key, cache.Key);
        }

        [Fact]
        public void ConcurrentSetsKeepTheNewestVersionOfEveryRecord()
        {
            var cache = new MemoryCache("Default", this.m);
            const int objects = 16;
            const int versions = 200;

            Parallel.For(0, Environment.ProcessorCount * 4, worker =>
            {
                var random = new Random(worker);
                for (var i = 0; i < versions; i++)
                {
                    var id = 1 + random.Next(objects);
                    var version = 1 + random.Next(versions);
                    cache.SetRecord(new FakeRecord(this.m.C1, id, version));
                    cache.SetGrant(new Grant(id, version, this.ranges.Load(version)));
                    _ = cache.GetRecord(id);
                    _ = cache.GetGrant(id);
                }
            });

            var seen = new Dictionary<long, long>();
            for (long id = 1; id <= objects; id++)
            {
                var record = cache.GetRecord(id);
                if (record != null)
                {
                    seen[id] = record.Version;
                    Assert.Equal(record.Version, cache.GetGrant(id).Version);
                }
            }

            Assert.NotEmpty(seen);
            Assert.All(seen.Values, version => Assert.InRange(version, 1, versions));

            // Every record is the newest version that was set for its id: a second pass of
            // older versions changes nothing.
            foreach (var kvp in seen)
            {
                Assert.False(cache.SetRecord(new FakeRecord(this.m.C1, kvp.Key, kvp.Value - 1)));
                Assert.Equal(kvp.Value, cache.GetRecord(kvp.Key).Version);
            }
        }
    }
}
