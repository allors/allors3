// <copyright file="AccessVersionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
{
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
    /// A pull advertises the version of every grant and revocation of the objects it answers;
    /// the connection requests the ones it holds at another version again, even when no object
    /// changed, so that a changed permission set reaches the client.
    /// </summary>
    public class AccessVersionTests
    {
        private const string WorkspaceName = "Default";

        private readonly M m = new MetaBuilder().Build();

        private readonly IRanges<long> ranges = new DefaultStructRanges<long>();

        private readonly FakeTransport transport;

        private readonly Pull[] pull;

        public AccessVersionTests()
        {
            this.transport = new FakeTransport(this.m);

            var server = this.transport.Server;
            server.AddPermission(100, this.m.C1, this.m.C1.C1AllorsString, Operations.Read);
            server.AddPermission(101, this.m.C1, this.m.C1.C1AllorsString, Operations.Write);
            server.AddPermission(102, this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read);
            server.AddGrant(10, 100, 101);
            server.AddRevocation(20, 101);
            server.AddObject(1, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "one");
            server.AddObject(2, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "two").Revocations = new long[] { 20 };

            this.pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
        }

        [Fact]
        public async Task AnUnchangedGrantIsNotRequestedAgain()
        {
            var connection = this.CreateConnection();

            await connection.PullAsync(this.pull);
            await connection.PullAsync(this.pull);

            Assert.Single(this.transport.Server.SyncRequests);
            Assert.Single(this.transport.Server.AccessRequests);
            Assert.Single(this.transport.Server.PermissionRequests);
        }

        [Fact]
        public async Task AGrantWhoseVersionChangedIsRequestedAgain()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            Assert.True(connection.GetRecord(1).IsPermitted(101));

            var grant = this.transport.Server.Grants[10];
            grant.Version++;
            grant.Permissions = new long[] { 100 };

            await connection.PullAsync(this.pull);

            Assert.Single(this.transport.Server.SyncRequests);
            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
            Assert.Equal(new long[] { 10 }, this.transport.Server.AccessRequests[1].g);
            Assert.True(connection.GetRecord(1).IsPermitted(100));
            Assert.False(connection.GetRecord(1).IsPermitted(101));

            await connection.PullAsync(this.pull);
            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
        }

        [Fact]
        public async Task ARevocationWhoseVersionChangedIsRequestedAgain()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            Assert.True(connection.GetRecord(2).IsPermitted(100));
            Assert.False(connection.GetRecord(2).IsPermitted(101));

            var revocation = this.transport.Server.Revocations[20];
            revocation.Version++;
            revocation.Permissions = new long[] { 100, 101 };

            await connection.PullAsync(this.pull);

            Assert.Single(this.transport.Server.SyncRequests);
            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
            Assert.Equal(new long[] { 20 }, this.transport.Server.AccessRequests[1].r);
            Assert.False(connection.GetRecord(2).IsPermitted(100));
            Assert.True(connection.GetRecord(1).IsPermitted(100));
        }

        [Fact]
        public async Task ThePermissionsAChangedGrantNamesForTheFirstTimeAreRequested()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            Assert.Equal(0, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read));

            var grant = this.transport.Server.Grants[10];
            grant.Version++;
            grant.Permissions = new long[] { 100, 101, 102 };

            await connection.PullAsync(this.pull);

            Assert.Equal(2, this.transport.Server.PermissionRequests.Count);
            Assert.Equal(new long[] { 102 }, this.transport.Server.PermissionRequests[1].p);
            Assert.Equal(102, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsInteger, Operations.Read));
            Assert.True(connection.GetRecord(1).IsPermitted(102));
        }

        [Fact]
        public async Task AChangedGrantIsRequestedOnceForAllTheObjectsThatNameIt()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);

            var grant = this.transport.Server.Grants[10];
            grant.Version++;

            await connection.PullAsync(this.pull);

            var request = this.transport.Server.AccessRequests.Last();
            Assert.Equal(new long[] { 10 }, request.g);
            Assert.True(request.r == null || request.r.Length == 0);
        }

        [Fact]
        public async Task AnOlderGrantResponseDoesNotRestoreARemovedPermission()
        {
            var grant = this.transport.Server.Grants[10];
            grant.Version = 2;
            grant.Permissions = new long[] { 100 };
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            Assert.False(connection.GetRecord(1).IsPermitted(101));

            grant.Version = 1;
            grant.Permissions = new long[] { 100, 101 };
            await connection.PullAsync(this.pull);

            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
            Assert.True(connection.GetRecord(1).IsPermitted(100));
            Assert.False(connection.GetRecord(1).IsPermitted(101));
        }

        [Fact]
        public async Task AnOlderRevocationResponseDoesNotRestoreADeniedPermission()
        {
            var revocation = this.transport.Server.Revocations[20];
            revocation.Version = 2;
            revocation.Permissions = new long[] { 100, 101 };
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            Assert.False(connection.GetRecord(2).IsPermitted(100));

            revocation.Version = 1;
            revocation.Permissions = new long[] { 101 };
            await connection.PullAsync(this.pull);

            Assert.Equal(2, this.transport.Server.AccessRequests.Count);
            Assert.False(connection.GetRecord(2).IsPermitted(100));
            Assert.False(connection.GetRecord(2).IsPermitted(101));
        }

        private DatabaseConnection CreateConnection() => new DatabaseConnection(WorkspaceName, this.m, this.transport, this.ranges);
    }
}
