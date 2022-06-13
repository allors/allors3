// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Threading;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Sync;
    using Xunit;

    public class SyncRolesTests : ApiTest, IClassFixture<Fixture>
    {
        public SyncRolesTests(Fixture fixture) : base(fixture) { }

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
