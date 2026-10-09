// <copyright file="EnvelopeTests.cs" company="Allors bv">
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
    /// The envelope at the client: the connection sends its workspace name and meta fingerprint
    /// with every request, learns the database and the user from the first response, faults
    /// when either changes, and refuses a response for another workspace or another meta.
    /// </summary>
    public class EnvelopeTests
    {
        private const string WorkspaceName = "Default";

        private readonly M m = new MetaBuilder().Build();

        private readonly IRanges<long> ranges = new DefaultStructRanges<long>();

        private readonly FakeTransport transport;

        private readonly Pull[] pull;

        public EnvelopeTests()
        {
            this.transport = new FakeTransport(this.m);

            var server = this.transport.Server;
            server.AddPermission(100, this.m.C1, this.m.C1.C1AllorsString, Operations.Read);
            server.AddGrant(10, 100);
            server.AddObject(1, this.m.C1, 10).WithRole(this.m.C1.C1AllorsString, "one");

            this.pull = new[] { new Pull { Extent = new Filter(this.m.C1) } };
        }

        [Fact]
        public void TheFingerprintIsTheHashOfTheSortedTagsOfTheMetaPopulation()
        {
            var connection = this.CreateConnection();

            IMetaPopulation meta = this.m;
            var tags = meta.Composites.Select(v => v.Tag)
                .Concat(meta.RelationTypes.Select(v => v.Tag))
                .Concat(meta.MethodTypes.Select(v => v.Tag));

            Assert.Equal(MetaFingerprint.Compute(tags), connection.MetaFingerprint);
            Assert.Equal(connection.MetaFingerprint, this.m.Fingerprint());
            Assert.Equal(connection.MetaFingerprint, new MetaBuilder().Build().Fingerprint());
        }

        [Fact]
        public async Task EveryRequestCarriesTheWorkspaceNameAndTheFingerprint()
        {
            var connection = this.CreateConnection();

            await connection.PullAsync(this.pull);
            await connection.PushAsync(null, null);
            await connection.InvokeAsync(Array.Empty<Invocation>());

            var server = this.transport.Server;
            var requests = new Allors.Protocol.Json.Api.Request[]
            {
                Assert.Single(server.PullRequests),
                Assert.Single(server.SyncRequests),
                Assert.Single(server.AccessRequests),
                Assert.Single(server.PermissionRequests),
                Assert.Single(server.PushRequests),
                Assert.Single(server.InvokeRequests),
            };

            foreach (var request in requests)
            {
                Assert.Equal(WorkspaceName, request._w);
                Assert.Equal(connection.MetaFingerprint, request._f);
            }
        }

        [Fact]
        public async Task TheConnectionLearnsTheDatabaseAndTheUserFromTheFirstResponse()
        {
            var connection = this.CreateConnection();

            Assert.Null(connection.DatabaseId);
            Assert.Null(connection.UserId);

            await connection.PullAsync(this.pull);

            Assert.Equal("fake", connection.DatabaseId);
            Assert.Equal(1, connection.UserId);
        }

        [Fact]
        public async Task AChangedUserFaultsTheConnection()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);
            var record = connection.GetRecord(1);
            Assert.NotNull(record);
            Assert.True(record.IsPermitted(100));

            this.transport.Server.UserId = 2;

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));
            Assert.Contains("user 1", exception.Message);
            Assert.Contains("user 2", exception.Message);

            Assert.Null(connection.GetRecord(1));
            Assert.Equal(0, connection.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));
            Assert.False(record.IsPermitted(100));

            // The connection stays faulted, whatever the server answers next.
            this.transport.Server.UserId = 1;
            var again = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));
            Assert.Contains("user 2", again.Message);
            await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PushAsync(null, null));
            await Assert.ThrowsAsync<InvalidOperationException>(() => connection.InvokeAsync(Array.Empty<Invocation>()));
            Assert.Equal(2, this.transport.Server.PullRequests.Count);
            Assert.Empty(this.transport.Server.PushRequests);
            Assert.Empty(this.transport.Server.InvokeRequests);
        }

        [Fact]
        public async Task AChangedDatabaseFaultsTheConnection()
        {
            var connection = this.CreateConnection();
            await connection.PullAsync(this.pull);

            this.transport.Server.DatabaseId = "other";

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));
            Assert.Contains("'fake'", exception.Message);
            Assert.Contains("'other'", exception.Message);
            Assert.Null(connection.GetRecord(1));
        }

        [Fact]
        public async Task AResponseForAnotherWorkspaceNameIsRefusedBeforeAnythingIsStored()
        {
            this.transport.Server.WorkspaceName = "Other";
            var connection = this.CreateConnection();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));

            Assert.Contains("'Other'", exception.Message);
            Assert.Contains("'Default'", exception.Message);
            Assert.Null(connection.GetRecord(1));
            Assert.Null(connection.DatabaseId);
            Assert.Null(connection.UserId);
            Assert.Empty(this.transport.Server.SyncRequests);

            // Not a fault: the same connection serves once the server serves its workspace.
            this.transport.Server.WorkspaceName = WorkspaceName;
            await connection.PullAsync(this.pull);
            Assert.NotNull(connection.GetRecord(1));
        }

        [Fact]
        public async Task AResponseWithAnotherFingerprintIsRefusedBeforeAnythingIsStored()
        {
            this.transport.Server.MetaFingerprint = "0000000000000000";
            var connection = this.CreateConnection();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));

            Assert.Contains("0000000000000000", exception.Message);
            Assert.Contains(connection.MetaFingerprint, exception.Message);
            Assert.Null(connection.GetRecord(1));
            Assert.Empty(this.transport.Server.SyncRequests);
        }

        [Fact]
        public async Task AResponseWithoutTheEnvelopeIsRefused()
        {
            this.transport.Server.DatabaseId = null;
            var connection = this.CreateConnection();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(this.pull));

            Assert.Contains("database", exception.Message);
            Assert.Null(connection.GetRecord(1));
        }

        [Fact]
        public async Task AFaultedConnectionDoesNotClearAnotherUsersConnection()
        {
            var first = this.CreateConnection();
            await first.PullAsync(this.pull);

            this.transport.Server.UserId = 2;
            this.transport.Server.Objects[1].WithRole(this.m.C1.C1AllorsString, "second user's view");
            var second = this.CreateConnection();
            await second.PullAsync(this.pull);
            var secondRecord = second.GetRecord(1);

            Assert.Equal(1, first.UserId);
            Assert.Equal(2, second.UserId);
            Assert.Equal("one", first.GetRecord(1).GetRole(this.m.C1.C1AllorsString));
            Assert.Equal("second user's view", secondRecord.GetRole(this.m.C1.C1AllorsString));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => first.PullAsync(this.pull));

            Assert.Contains("user 1", exception.Message);
            Assert.Contains("user 2", exception.Message);
            Assert.Null(first.GetRecord(1));
            Assert.Same(secondRecord, second.GetRecord(1));
            Assert.True(secondRecord.IsPermitted(100));
            Assert.Equal(100, second.GetPermission(this.m.C1, this.m.C1.C1AllorsString, Operations.Read));

            await second.PullAsync(this.pull);
            Assert.Same(secondRecord, second.GetRecord(1));
            Assert.Equal(2, this.transport.Server.SyncRequests.Count);
        }

        private DatabaseConnection CreateConnection() => new DatabaseConnection(WorkspaceName, this.m, this.transport, this.ranges);
    }
}
