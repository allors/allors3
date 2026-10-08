// <copyright file="PersistenceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
{
    using System;
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
    /// The persistence provider behind the cache: what a pull stores, what a connection with an
    /// empty cache restores instead of asking the server, and what it still asks the server
    /// because the persisted entry is not the one the pull advertises.
    /// </summary>
    public class PersistenceTests
    {
        private const string WorkspaceName = "Default";

        private readonly M m = new MetaBuilder().Build();

        private readonly IRanges<long> ranges = new DefaultStructRanges<long>();

        private readonly FakeTransport transport;

        private readonly MemoryPersistenceProvider provider = new MemoryPersistenceProvider();

        private readonly Pull[] pull;

        public PersistenceTests()
        {
            this.transport = new FakeTransport(this.m);

            var server = this.transport.Server;
            server.AddPermission(100, this.m.C1, this.m.C1.C1AllorsString, Operations.Read);
            server.AddPermission(101, this.m.C1, this.m.C1.C1AllorsString, Operations.Write);
            server.AddPermission(102, this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read);
            server.AddGrant(10, 100, 101);
            server.AddRevocation(20, 101);
            server.AddObject(1, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "one").WithRole(this.m.C1.C1C1Many2Manies, new long[] { 2 });
            server.AddObject(2, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "two").Revocations = new long[] { 20 };

            this.pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
        }

        [Fact]
        public async Task APullStoresWhatTheServerSentUnderTheKeyOfTheConnection()
        {
            var connection = this.CreateConnection();

            await connection.PullAsync(this.pull);

            var key = connection.Cache.Key;
            Assert.Equal(key, Assert.Single(this.provider.Keys));
            Assert.Equal(1, this.provider.StoreCount);

            var objects = this.provider.Objects(key);
            Assert.Equal(new long[] { 1, 2 }, objects.Keys.OrderBy(v => v));
            Assert.Equal(this.m.C1.Tag, objects[1].c);
            Assert.Equal(1, objects[1].v);
            Assert.Equal(new long[] { 10 }, objects[1].g);
            Assert.Contains(objects[1].ro, v => v.t == this.m.C1.C1AllorsString.RelationType.Tag && (string)v.v == "one");
            Assert.Equal(new long[] { 20 }, objects[2].r);

            Assert.Equal(new long[] { 10 }, this.provider.Grants(key).Keys);
            Assert.Equal(new long[] { 100, 101 }, this.provider.Grants(key)[10].p);
            Assert.Equal(new long[] { 20 }, this.provider.Revocations(key).Keys);
            Assert.Equal(new long[] { 100, 101 }, this.provider.Permissions(key).Keys.OrderBy(v => v));
        }

        [Fact]
        public async Task AConnectionWithAnEmptyCacheRestoresFromTheProviderInsteadOfAskingTheServer()
        {
            var first = this.CreateConnection();
            await first.PullAsync(this.pull);

            var second = this.CreateConnection();
            var result = await second.PullAsync(this.pull);

            Assert.Equal(new long[] { 1, 2 }, result.Pool);
            Assert.Single(this.transport.Server.SyncRequests);
            Assert.Single(this.transport.Server.AccessRequests);
            Assert.Single(this.transport.Server.PermissionRequests);
            Assert.Equal(1, this.provider.StoreCount);

            var record = second.GetRecord(1);
            Assert.Same(this.m.C1, record.Class);
            Assert.Equal(1, record.Version);
            Assert.Equal("one", record.GetRole(this.m.C1.C1AllorsString));
            Assert.Equal(this.ranges.Load(2), record.GetRole(this.m.C1.C1C1Many2Manies));
            Assert.Equal(this.ranges.Load(10), record.GrantIds);
            Assert.True(record.IsPermitted(100));
            Assert.True(record.IsPermitted(101));
            Assert.False(second.GetRecord(2).IsPermitted(101));
            Assert.Equal(100, second.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));

            // What memory holds is not asked of the provider again.
            var loads = this.provider.LoadCount;
            await second.PullAsync(this.pull);
            Assert.Equal(loads, this.provider.LoadCount);
        }

        [Fact]
        public async Task APersistedObjectIsRestoredOnlyAtTheVersionAndAccessThePullAdvertises()
        {
            var first = this.CreateConnection();
            await first.PullAsync(this.pull);

            var @object = this.transport.Server.Objects[1];
            @object.Version++;
            @object.WithRole(this.m.C1.C1AllorsString, "changed");

            var second = this.CreateConnection();
            await second.PullAsync(this.pull);

            Assert.Equal(2, this.transport.Server.SyncRequests.Count);
            Assert.Equal(new long[] { 1 }, this.transport.Server.SyncRequests[1].o);
            Assert.Equal("changed", second.GetRecord(1).GetRole(this.m.C1.C1AllorsString));
            Assert.Equal("two", second.GetRecord(2).GetRole(this.m.C1.C1AllorsString));
            Assert.Single(this.transport.Server.AccessRequests);

            // The provider holds the new version now.
            Assert.Equal(2, this.provider.Objects(second.Cache.Key)[1].v);

            var third = this.CreateConnection();
            await third.PullAsync(this.pull);
            Assert.Equal(2, this.transport.Server.SyncRequests.Count);
        }

        [Fact]
        public async Task APersistedGrantIsRestoredOnlyAtTheVersionThePullAdvertises()
        {
            var first = this.CreateConnection();
            await first.PullAsync(this.pull);

            var grant = this.transport.Server.Grants[10];
            grant.Version++;
            grant.Permissions = new long[] { 100, 102 };

            var second = this.CreateConnection();
            await second.PullAsync(this.pull);

            Assert.Single(this.transport.Server.SyncRequests);
            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
            Assert.Equal(new long[] { 10 }, this.transport.Server.AccessRequests[1].g);
            Assert.Equal(2, this.transport.Server.PermissionRequests.Count);
            Assert.Equal(new long[] { 102 }, this.transport.Server.PermissionRequests[1].p);

            Assert.True(second.GetRecord(1).IsPermitted(100));
            Assert.False(second.GetRecord(1).IsPermitted(101));
            Assert.True(second.GetRecord(1).IsPermitted(102));
            Assert.Equal(2, this.provider.Grants(second.Cache.Key)[10].v);
            Assert.Contains(102, this.provider.Permissions(second.Cache.Key).Keys);
        }

        [Fact]
        public async Task ClearAsyncForgetsTheCacheAndThePersistedView()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            var key = connection.Cache.Key;

            await connection.ClearAsync();

            Assert.Equal(1, this.provider.ClearCount);
            Assert.Empty(this.provider.Keys);
            Assert.Null(connection.GetRecord(1));
            Assert.Null(connection.Cache.Key);

            await connection.PullAsync(this.pull);
            Assert.Equal(2, this.transport.Server.SyncRequests.Count);
            Assert.Equal(key, connection.Cache.Key);
        }

        [Fact]
        public async Task AFaultForgetsThePersistedViewToo()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);

            this.transport.Server.UserId = 2;
            await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));

            Assert.Equal(1, this.provider.ClearCount);
            Assert.Empty(this.provider.Keys);
        }

        [Fact]
        public async Task AConnectionWithoutAProviderWorksAsBefore()
        {
            var connection = new DatabaseConnection(WorkspaceName, this.m, this.transport, this.ranges);

            await connection.PullAsync(this.pull);
            await connection.ClearAsync();

            Assert.Empty(this.provider.Keys);
            Assert.Null(connection.GetRecord(1));
        }

        private DatabaseConnection CreateConnection() => new DatabaseConnection(WorkspaceName, this.m, this.transport, this.ranges, new MemoryCache(WorkspaceName, this.m), this.provider);
    }
}
