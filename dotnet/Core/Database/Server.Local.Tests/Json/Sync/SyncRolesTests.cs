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
            Assert.NotEmpty(syncResponse.p);
            Assert.All(syncResponse.p, definition =>
            {
                var permission = Assert.IsAssignableFrom<Permission>(this.Transaction.Instantiate(definition.i));
                Assert.True(permission.InWorkspace("X"));
            });
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
            Assert.Empty(syncResponse.g);
            Assert.Empty(syncResponse.r);
            Assert.Empty(syncResponse.p);
        }

        [Fact]
        public void ReadableRelationsKeepTargetsWithoutReadableRoles()
        {
            this.SetUser("jane@example.com");

            var unreadable = new DataBuilder(this.Transaction).WithString("Hidden value").Build();
            var readable = new DataBuilder(this.Transaction).WithString("Visible value").Build();
            var readPermissions = new Permissions(this.Transaction).Extent()
                .Where(v => Equals(v.Class, this.M.Data) && v.Operation == Operations.Read && v.InWorkspace("Default"))
                .ToArray();
            Assert.NotEmpty(readPermissions);

            var revocation = new RevocationBuilder(this.Transaction).Build();
            foreach (var permission in readPermissions)
            {
                revocation.AddDeniedPermission(permission);
            }

            unreadable.AddRevocation(revocation);

            var organisation = new OrganisationBuilder(this.Transaction)
                .WithName("Hidden organisation name")
                .WithOneData(unreadable)
                .WithManyData(unreadable)
                .WithManyData(readable)
                .Build();
            var namePermission = new Permissions(this.Transaction).Get(this.M.Organisation, this.M.Organisation.Name, Operations.Read);
            organisation.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(namePermission).Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var targetAcl = api.AccessControl[unreadable];
            Assert.Contains(this.M.Data.String, api.RoleTypesByClass[this.M.Data]);
            Assert.All(api.RoleTypesByClass[this.M.Data], roleType => Assert.False(targetAcl.CanRead(roleType)));
            Assert.True(api.AccessControl[readable].CanRead(this.M.Data.String));
            Assert.False(api.AccessControl[organisation].CanRead(this.M.Organisation.Name));
            Assert.True(api.AccessControl[organisation].CanRead(this.M.Organisation.OneData));
            Assert.True(api.AccessControl[organisation].CanRead(this.M.Organisation.ManyDatas));
            Assert.Equal("Hidden value", unreadable.String);
            Assert.Equal("Hidden organisation name", organisation.Name);

            var response = api.Sync(new SyncRequest { o = new[] { organisation.Id, unreadable.Id, readable.Id } });
            Assert.Equal(3, response.o.Length);

            var syncOrganisation = Assert.Single(response.o, v => v.i == organisation.Id);
            Assert.Null(syncOrganisation.GetRole(this.M.Organisation.Name));
            Assert.Equal(unreadable.Id, syncOrganisation.GetRole(this.M.Organisation.OneData).o);
            Assert.Equal(new[] { unreadable.Id, readable.Id }.OrderBy(v => v), syncOrganisation.GetRole(this.M.Organisation.ManyDatas).c);

            var syncUnreadable = Assert.Single(response.o, v => v.i == unreadable.Id);
            Assert.Empty(syncUnreadable.ro);
            var syncReadable = Assert.Single(response.o, v => v.i == readable.Id);
            Assert.Equal("Visible value", syncReadable.GetRole(this.M.Data.String).v);

            var acls = new[] { api.AccessControl[organisation], targetAcl, api.AccessControl[readable] };
            Assert.Equal(acls.SelectMany(v => v.Grants).Select(v => v.Id).Distinct().OrderBy(v => v), response.g.Select(v => v.i));
            Assert.Equal(acls.SelectMany(v => v.Revocations).Select(v => v.Id).Distinct().OrderBy(v => v), response.r.Select(v => v.i));
            var expectedPermissionIds = acls.SelectMany(v => v.Grants).SelectMany(v => v.PermissionSet)
                .Concat(acls.SelectMany(v => v.Revocations).SelectMany(v => v.PermissionSet))
                .Distinct().OrderBy(v => v);
            Assert.Equal(expectedPermissionIds, response.p.Select(v => v.i));
            Assert.Contains(response.p, v => v.i == namePermission.Id && v.c == this.M.Organisation.Tag &&
                v.t == this.M.Organisation.Name.RelationType.Tag && v.o == (long)Operations.Read);
        }
    }
}
