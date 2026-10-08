// <copyright file="ConnectionCacheTests.cs" company="Allors bv">
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
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;

    /// <summary>
    /// The connection and its cache, over the fake transport: what a pull leaves in the cache,
    /// the connections of one user sharing a cache, and the checks a shared cache makes.
    /// </summary>
    public class ConnectionCacheTests
    {
        private const string WorkspaceName = "Default";

        private readonly M m = new MetaBuilder().Build();

        private readonly IRanges<long> ranges = new DefaultStructRanges<long>();

        private readonly FakeTransport transport;

        public ConnectionCacheTests()
        {
            this.transport = new FakeTransport(this.m);

            var server = this.transport.Server;
            server.AddPermission(100, this.m.C1, this.m.C1.C1AllorsString, Operations.Read);
            server.AddPermission(101, this.m.C1, this.m.C1.C1AllorsString, Operations.Write);
            server.AddGrant(10, 100, 101);
            server.AddObject(1, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "one");
            server.AddObject(2, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "two");
        }

        [Fact]
        public async Task APullLeavesRecordsGrantsAndPermissionsInTheCache()
        {
            var connection = this.CreateConnection();

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.Equal(new long[] { 1, 2 }, result.Pool);

            var record = connection.Cache.GetRecord(1);
            Assert.Same(record, connection.GetRecord(1));
            Assert.Same(this.m.C1, record.Class);
            Assert.Equal("one", record.GetRole(this.m.C1.C1AllorsString));
            Assert.Equal(this.ranges.Load(10), record.GrantIds);
            Assert.True(record.RevocationIds.IsEmpty);

            Assert.Equal(10, connection.Cache.GetGrant(10).Id);
            Assert.Equal(100, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.True(record.IsPermitted(100));
            Assert.True(record.IsPermitted(101));
            Assert.False(record.IsPermitted(102));
        }

        [Fact]
        public void AConnectionWithoutACacheHasAMemoryCacheOfItsOwn()
        {
            var connection = this.CreateConnection();
            var other = this.CreateConnection();

            Assert.IsType<MemoryCache>(connection.Cache);
            Assert.NotSame(connection.Cache, other.Cache);
            Assert.Equal(WorkspaceName, connection.Cache.WorkspaceName);
            Assert.Same(this.m, connection.Cache.MetaPopulation);
        }

        [Fact]
        public async Task TheConnectionsOfOneUserShareACache()
        {
            var cache = new MemoryCache(WorkspaceName, this.m);
            var first = this.CreateConnection(cache);
            var second = this.CreateConnection(cache);

            await first.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.Same(cache, second.Cache);
            Assert.Same(first.GetRecord(1), second.GetRecord(1));
            Assert.Single(this.transport.Server.SyncRequests);

            await second.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.Single(this.transport.Server.SyncRequests);
            Assert.Single(this.transport.Server.AccessRequests);
            Assert.Single(this.transport.Server.PermissionRequests);
        }

        [Fact]
        public async Task TheCacheTellsEveryConnectionWhenARecordIsReplaced()
        {
            var cache = new MemoryCache(WorkspaceName, this.m);
            var first = this.CreateConnection(cache);
            var second = this.CreateConnection(cache);

            var pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
            await first.PullAsync(pull);

            var cacheChanges = new List<RecordChangedEventArgs>();
            var firstChanges = new List<RecordChangedEventArgs>();
            var secondChanges = new List<RecordChangedEventArgs>();
            cache.RecordChanged += (sender, e) => cacheChanges.Add(e);
            first.RecordChanged += (sender, e) => firstChanges.Add(e);
            second.RecordChanged += (sender, e) => secondChanges.Add(e);

            var @object = this.transport.Server.Objects[1];
            @object.Version++;
            @object.WithRole(this.m.C1.C1AllorsString, "changed");

            await second.PullAsync(pull);

            var cacheChange = Assert.Single(cacheChanges);
            Assert.Equal(1, cacheChange.Id);
            Assert.Same(first.GetRecord(1), cacheChange.Record);
            Assert.Equal("changed", first.GetRecord(1).GetRole(this.m.C1.C1AllorsString));

            Assert.Empty(firstChanges);
            Assert.Single(secondChanges);
        }

        [Fact]
        public async Task AnOlderRecordFromOneConnectionDoesNotReplaceANewerOneFromAnother()
        {
            var cache = new MemoryCache(WorkspaceName, this.m);
            var first = this.CreateConnection(cache);
            var second = this.CreateConnection(cache);

            var pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
            await first.PullAsync(pull);

            var @object = this.transport.Server.Objects[1];
            @object.Version++;
            @object.WithRole(this.m.C1.C1AllorsString, "newer");
            await second.PullAsync(pull);
            var newer = cache.GetRecord(1);

            // A sync response built before the change arrives late: the cache keeps the newer record.
            var late = new Allors.Protocol.Json.Api.Sync.SyncResponse { o = new[] { new Allors.Protocol.Json.Api.Sync.SyncResponseObject { i = 1, v = newer.Version - 1, c = this.m.C1.Tag, g = new long[] { 10 }, r = Array.Empty<long>(), ro = Array.Empty<Allors.Protocol.Json.Api.Sync.SyncResponseRole>() } } };
            Assert.False(cache.SetRecord(new FakeRecord(this.m.C1, 1, late.o[0].v)));

            Assert.Same(newer, cache.GetRecord(1));
            Assert.Equal("newer", first.GetRecord(1).GetRole(this.m.C1.C1AllorsString));
        }

        [Fact]
        public void ACacheOfAnotherWorkspaceNameIsRefused()
        {
            var cache = new MemoryCache("Other", this.m);

            var exception = Assert.Throws<ArgumentException>(() => this.CreateConnection(cache));

            Assert.Contains("Other", exception.Message);
            Assert.Contains(WorkspaceName, exception.Message);
        }

        [Fact]
        public void ACacheOfAnotherMetaPopulationIsRefused()
        {
            var cache = new MemoryCache(WorkspaceName, new MetaBuilder().Build());

            var exception = Assert.Throws<ArgumentException>(() => this.CreateConnection(cache));

            Assert.Contains("meta population", exception.Message);
        }

        private DatabaseConnection CreateConnection(ICache cache = null) => new DatabaseConnection(WorkspaceName, this.m, this.transport, this.ranges, cache);
    }
}
