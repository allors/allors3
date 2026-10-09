// <copyright file="ConnectionRecordTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Ranges;
    using Allors.Workspace.Connection;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;

    /// <summary>
    /// The records and permissions a connection receives over the fake transport: their
    /// values, versions, replacement events and independence from other connections.
    /// </summary>
    public class ConnectionRecordTests
    {
        private const string WorkspaceName = "Default";

        private readonly M m = new MetaBuilder().Build();

        private readonly DefaultStructRanges<long> ranges = new DefaultStructRanges<long>();

        private readonly FakeTransport transport;

        public ConnectionRecordTests()
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
        public async Task APullMakesRecordsAndPermissionsAvailable()
        {
            var connection = this.CreateConnection();

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.Equal(new long[] { 1, 2 }, result.Pool);

            var record = connection.GetRecord(1);
            Assert.Same(this.m.C1, record.Class);
            Assert.Equal("one", record.GetRole(this.m.C1.C1AllorsString));
            Assert.Equal(this.ranges.Load(10), record.GrantIds);
            Assert.True(record.RevocationIds.IsEmpty);

            Assert.Equal(100, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.True(record.IsPermitted(100));
            Assert.True(record.IsPermitted(101));
            Assert.False(record.IsPermitted(102));
        }

        [Fact]
        public void AConnectionStartsWithoutRecordsOrPermissions()
        {
            var connection = this.CreateConnection();

            Assert.Null(connection.GetRecord(1));
            Assert.Equal(0, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
        }

        [Fact]
        public async Task APermissionIsFoundByClassOperandTypeAndOperation()
        {
            var server = this.transport.Server;
            server.AddPermission(102, this.m.C1, this.m.C1.ClassMethod, Operations.Execute);
            server.AddGrant(10, 100, 101, 102);
            var connection = this.CreateConnection();

            await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.Equal(100, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.Equal(101, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Write));
            Assert.Equal(102, connection.GetPermission(this.m.C1, this.m.C1.ClassMethod, Operations.Execute));
            Assert.Equal(0, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read));
            Assert.Equal(0, connection.GetPermission(this.m.C2, this.m.C1.C1AllorsString, Operations.Read));
            Assert.True(connection.GetRecord(1).IsPermitted(102));
        }

        [Fact]
        public async Task TheConnectionsOfOneUserReceiveTheirOwnRecordsAndPermissions()
        {
            var first = this.CreateConnection();
            var second = this.CreateConnection();

            await first.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.Null(second.GetRecord(1));
            Assert.Equal(0, second.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.Single(this.transport.Server.SyncRequests);

            await second.PullAsync(new[] { new Pull { Extent = new Filter(this.m.C1) } });

            Assert.NotSame(first.GetRecord(1), second.GetRecord(1));
            Assert.Equal("one", second.GetRecord(1).GetRole(this.m.C1.C1AllorsString));
            Assert.True(second.GetRecord(1).IsPermitted(100));
            Assert.Equal(100, second.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.Equal(2, this.transport.Server.SyncRequests.Count);
            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
            Assert.Equal(2, this.transport.Server.PermissionRequests.Count);
        }

        [Fact]
        public async Task ARecordChangeBelongsToTheConnectionThatRefreshedItAndIncludesItsPermissions()
        {
            var first = this.CreateConnection();
            var second = this.CreateConnection();

            var firstChanges = new List<RecordChangedEventArgs>();
            var secondChanges = new List<RecordChangedEventArgs>();
            first.RecordChanged += (sender, e) => firstChanges.Add(e);
            second.RecordChanged += (sender, e) => secondChanges.Add(e);

            var pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
            await first.PullAsync(pull);
            await second.PullAsync(pull);
            Assert.Empty(firstChanges);
            Assert.Empty(secondChanges);

            var server = this.transport.Server;
            server.AddPermission(102, this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read);
            server.AddGrant(11, 100, 102);
            var @object = server.Objects[1];
            @object.Version++;
            @object.Grants = new long[] { 11 };
            @object.WithRole(this.m.C1.C1AllorsString, "changed");

            second.RecordChanged += (sender, e) =>
            {
                Assert.Equal(102, second.GetPermission(this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read));
                Assert.True(e.Record.IsPermitted(102));
                Assert.False(e.Record.IsPermitted(101));
            };

            await second.PullAsync(pull);

            var change = Assert.Single(secondChanges);
            Assert.Equal(1, change.Id);
            Assert.Same(second.GetRecord(1), change.Record);
            Assert.Equal("changed", change.Record.GetRole(this.m.C1.C1AllorsString));
            Assert.Equal("one", first.GetRecord(1).GetRole(this.m.C1.C1AllorsString));
            Assert.True(first.GetRecord(1).IsPermitted(101));
            Assert.False(first.GetRecord(1).IsPermitted(102));
            Assert.Equal(0, first.GetPermission(this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read));
            Assert.Empty(firstChanges);
        }

        [Fact]
        public async Task AnOlderResponseDoesNotReplaceANewerRecord()
        {
            var connection = this.CreateConnection();
            var @object = this.transport.Server.Objects[1];
            @object.Version = 2;
            @object.WithRole(this.m.C1.C1AllorsString, "newer");

            var pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
            await connection.PullAsync(pull);
            var newer = connection.GetRecord(1);
            var changes = new List<RecordChangedEventArgs>();
            connection.RecordChanged += (sender, e) => changes.Add(e);

            // The transport answers with an older snapshot on the next pull.
            @object.Version = 1;
            @object.WithRole(this.m.C1.C1AllorsString, "older");
            await connection.PullAsync(pull);

            Assert.Same(newer, connection.GetRecord(1));
            Assert.Equal(2, connection.GetRecord(1).Version);
            Assert.Equal("newer", connection.GetRecord(1).GetRole(this.m.C1.C1AllorsString));
            Assert.Equal(new long[] { 1 }, this.transport.Server.SyncRequests[1].o);
            Assert.Empty(changes);
        }

        [Fact]
        public async Task ChangedGrantIdsReplaceARecordAtTheSameObjectVersion()
        {
            var connection = this.CreateConnection();
            var pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
            await connection.PullAsync(pull);
            var previous = connection.GetRecord(1);

            this.transport.Server.AddGrant(11, 100);
            this.transport.Server.Objects[1].Grants = new long[] { 11 };

            await connection.PullAsync(pull);

            var record = connection.GetRecord(1);
            Assert.NotSame(previous, record);
            Assert.Equal(previous.Version, record.Version);
            Assert.Equal(this.ranges.Load(11), record.GrantIds);
            Assert.True(record.IsPermitted(100));
            Assert.False(record.IsPermitted(101));
            Assert.Equal(new long[] { 1 }, this.transport.Server.SyncRequests[1].o);
        }

        private DatabaseConnection CreateConnection() => new DatabaseConnection(WorkspaceName, this.m, this.transport, this.ranges);
    }
}
