// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Linq;
    using System.Threading;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Security;
    using Allors.Database.Services;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Xunit;

    public class SyncRolesTests : ApiTest, IClassFixture<Fixture>
    {
        public SyncRolesTests(Fixture fixture) : base(fixture) { }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PullSyncAndAccessRetainSnapshotAfterAnotherTransactionCommits(bool reuseApi)
        {
            var user = this.SetUser("jane@example.com");
            var permissions = new Permissions(this.Transaction);
            var readProperty = permissions.Get(this.M.C1, this.M.C1.C1AllorsString, Operations.Read);
            var readAnother = permissions.Get(this.M.C1, this.M.C1.C1AllorsStringEquals, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Snapshot reader").WithPermission(readProperty).WithPermission(readAnother).Build();
            var grant = new GrantBuilder(this.Transaction).WithRole(role).WithSubject((User)user).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(readAnother).Build();
            var source = new C1Builder(this.Transaction)
                .WithC1AllorsString("Before")
                .WithC1AllorsStringEquals("Before denied")
                .WithSecurityToken(token)
                .WithRevocation(revocation)
                .Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            using var reader = this.Transaction.Database.CreateTransaction();
            reader.Services.Get<IUserService>().User = (IUser)reader.Instantiate(user.Id);
            var readerApi = new Api(reader, "Default", CancellationToken.None);
            var pullRequest = new PullRequest
            {
                l = new[] { new Allors.Protocol.Json.Data.Pull { o = source.Id } },
            };
            var syncRequest = new SyncRequest { o = new[] { source.Id } };
            var accessRequest = new AccessRequest { g = new[] { grant.Id }, r = new[] { revocation.Id } };
            var before = readerApi.Pull(pullRequest);
            Assert.False(before.HasErrors);
            var beforeObject = Assert.Single(before.p);
            var beforeGrant = Assert.Single(before.g);
            var beforeRevocation = Assert.Single(before.r);

            // Change data and security after the reader's first query, then warm the shared
            // security caches with the newer committed versions before the reader continues.
            using var writer = this.Transaction.Database.CreateTransaction();
            writer.Services.Get<IUserService>().User = (IUser)writer.Instantiate(user.Id);
            var changedSource = (C1)writer.Instantiate(source.Id);
            changedSource.C1AllorsString = "After denied";
            changedSource.C1AllorsStringEquals = "After";
            ((Role)writer.Instantiate(role.Id)).RemovePermission((Permission)writer.Instantiate(readProperty.Id));
            ((Revocation)writer.Instantiate(revocation.Id)).DeniedPermissions = new[] { (Permission)writer.Instantiate(readProperty.Id) };
            writer.Derive();
            writer.Commit();

            var writerApi = new Api(writer, "Default", CancellationToken.None);
            var after = writerApi.Pull(pullRequest);
            var afterObject = Assert.Single(after.p);
            var afterGrant = Assert.Single(after.g);
            var afterRevocation = Assert.Single(after.r);
            Assert.NotEqual(beforeObject.v, afterObject.v);
            Assert.NotEqual(beforeGrant[1], afterGrant[1]);
            Assert.NotEqual(beforeRevocation[1], afterRevocation[1]);
            var current = Assert.Single(writerApi.Sync(syncRequest).o);
            Assert.Null(current.GetRole(this.M.C1.C1AllorsString));
            Assert.Equal("After", current.GetRole(this.M.C1.C1AllorsStringEquals).v);

            var continuation = reuseApi ? readerApi : new Api(reader, "Default", CancellationToken.None);
            var retained = Assert.Single(continuation.Sync(syncRequest).o);
            Assert.Equal(beforeObject.v, retained.v);
            Assert.Equal("Before", retained.GetRole(this.M.C1.C1AllorsString).v);
            Assert.Null(retained.GetRole(this.M.C1.C1AllorsStringEquals));
            var access = continuation.Access(accessRequest);
            var retainedGrant = Assert.Single(access.g);
            var retainedRevocation = Assert.Single(access.r);
            Assert.Equal(beforeGrant[1], retainedGrant.v);
            Assert.Equal(beforeRevocation[1], retainedRevocation.v);
            Assert.Contains(readProperty.Id, continuation.Ranges.Load(retainedGrant.p));
            Assert.Contains(readAnother.Id, continuation.Ranges.Load(retainedGrant.p));
            Assert.Equal(new[] { readAnother.Id }, continuation.Ranges.Load(retainedRevocation.p).ToArray());

            // Returning older snapshot entries to the shared cache must not make a new read
            // consume them when its committed versions differ.
            using var latest = this.Transaction.Database.CreateTransaction();
            latest.Services.Get<IUserService>().User = (IUser)latest.Instantiate(user.Id);
            var latestApi = new Api(latest, "Default", CancellationToken.None);
            var latestObject = Assert.Single(latestApi.Sync(syncRequest).o);
            Assert.Equal(afterObject.v, latestObject.v);
            Assert.Null(latestObject.GetRole(this.M.C1.C1AllorsString));
            Assert.Equal("After", latestObject.GetRole(this.M.C1.C1AllorsStringEquals).v);
            var latestAccess = latestApi.Access(accessRequest);
            Assert.Equal(afterGrant[1], Assert.Single(latestAccess.g).v);
            Assert.Equal(afterRevocation[1], Assert.Single(latestAccess.r).v);
        }

        [Fact]
        public void SyncRetainsRelationTargetsWhileFilteringUnreadableRoles()
        {
            this.SetUser("jane@example.com");
            var target = new TrimToBuilder(this.Transaction).WithName("Unreadable target").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Readable source").Build();
            source.Many2One = target;
            source.AddMany2Many(target);
            var permissions = new Permissions(this.Transaction);
            var namePermission = permissions.Get(this.M.TrimTo, this.M.TrimTo.Name, Operations.Read);
            target.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(namePermission).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new SyncRequest { o = new[] { source.Id, target.Id } };
            var response = new Api(this.Transaction, "Default", CancellationToken.None).Sync(request);

            var sourceRecord = response.o.Single(v => v.i == source.Id);
            Assert.Equal(target.Id, sourceRecord.GetRole(this.M.TrimFrom.Many2One).o);
            Assert.Equal(new[] { target.Id }, sourceRecord.GetRole(this.M.TrimFrom.Many2Manies).c);
            var targetRecord = response.o.Single(v => v.i == target.Id);
            Assert.Null(targetRecord.GetRole(this.M.TrimTo.Name));

            var relationPermission = permissions.Get(this.M.TrimFrom, this.M.TrimFrom.Many2One, Operations.Read);
            source.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(relationPermission).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();

            response = new Api(this.Transaction, "Default", CancellationToken.None).Sync(request);

            sourceRecord = response.o.Single(v => v.i == source.Id);
            Assert.Null(sourceRecord.GetRole(this.M.TrimFrom.Many2One));
            Assert.Equal(new[] { target.Id }, sourceRecord.GetRole(this.M.TrimFrom.Many2Manies).c);
            Assert.Null(response.o.Single(v => v.i == target.Id).GetRole(this.M.TrimTo.Name));
        }

        [Fact]
        public void Workspace()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction)
                .WithWorkspaceXString("x1:x")
                .WithWorkspaceYString("x1:y")
                .WithWorkspaceXYString("x1:xy")
                .WithWorkspaceNonString("x1:none")
                .Build();

            this.Transaction.Commit();

            var syncRequest = new SyncRequest
            {
                o = new[] { x1.Id },
            };
            var api = new Api(this.Transaction, "X", CancellationToken.None);
            var syncResponse = api.Sync(syncRequest);

            Assert.Single(syncResponse.o);

            var wx1 = syncResponse.o[0];

            Assert.Equal(2, wx1.ro.Length);

            var wx1WorkspaceXString = wx1.GetRole(m.WorkspaceXObject1.WorkspaceXString);
            var wx1WorkspaceXYString = wx1.GetRole(m.WorkspaceXObject1.WorkspaceXYString);

            Assert.Equal("x1:x", wx1WorkspaceXString.v);
            Assert.Equal("x1:xy", wx1WorkspaceXYString.v);
        }


        [Fact]
        public void WorkspaceNone()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction)
                .WithWorkspaceXString("x1:x")
                .WithWorkspaceYString("x1:y")
                .WithWorkspaceXYString("x1:xy")
                .WithWorkspaceNonString("x1:none")
                .Build();

            this.Transaction.Commit();

            var syncRequest = new SyncRequest
            {
                o = new[] { x1.Id},
            };
            var api = new Api(this.Transaction, "Y", CancellationToken.None);
            var syncResponse = api.Sync(syncRequest);

            Assert.Empty(syncResponse.o);
        }
    }
}
