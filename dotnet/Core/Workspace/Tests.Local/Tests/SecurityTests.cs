// <copyright file="MethodTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Database.Domain;
    using Allors.Database.Security;
    using Allors.Database.Services;
    using Allors.Workspace.Data;
    using Xunit;

    public class SecurityTests : Workspace.SecurityTests, IClassFixture<Fixture>
    {
        public SecurityTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        public override IProfile Profile { get; }

        [Fact]
        public async Task RefreshesKnownGrantWhenAnObjectIsSynchronized()
        {
            var profile = (Profile)this.Profile;
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.C1) });
            var c1 = result.GetCollection<Allors.Workspace.Domain.C1>().First();
            Assert.True(c1.CanReadC1AllorsString);

            using (var transaction = profile.Database.CreateTransaction())
            {
                transaction.Services.Get<IUserService>().User = (IUser)transaction.Instantiate(profile.DatabaseConnection.UserId);
                var databaseC1 = (C1)transaction.Instantiate(c1.Id);
                var permissionId = databaseC1.Strategy.Class.ReadPermissionIdByRelationTypeId[databaseC1.Meta.C1AllorsString.RelationType.Id];
                var permission = (Permission)transaction.Instantiate(permissionId);
                var grants = transaction.Services.Get<IWorkspaceAclsService>().Create("Default")[databaseC1].Grants.Select(grant => (Grant)transaction.Instantiate(grant.Id)).ToArray();
                Assert.Contains(grants, grant => grant.EffectivePermissions.Contains(permission));
                foreach (var grant in grants)
                {
                    grant.Role.RemovePermission(permission);
                }

                databaseC1.C1AllorsString = "Changed after the permission was removed";
                Assert.False(transaction.Derive().HasErrors);
                transaction.Commit();
            }

            result = await session.PullAsync(new Pull { Object = c1 });
            Assert.False(result.HasErrors);
            Assert.False(c1.CanReadC1AllorsString);
        }

        [Fact]
        public async Task RefreshesKnownRevocationWhenAnObjectIsSynchronized()
        {
            var profile = (Profile)this.Profile;
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.Denied) });
            var denied = Assert.Single(result.GetCollection<Allors.Workspace.Domain.Denied>());
            Assert.True(denied.CanReadDefaultWorkspaceProperty);
            Assert.False(denied.CanWriteDefaultWorkspaceProperty);

            foreach (var denyRead in new[] { true, false })
            {
                using (var transaction = profile.Database.CreateTransaction())
                {
                    transaction.Services.Get<IUserService>().User = (IUser)transaction.Instantiate(profile.DatabaseConnection.UserId);
                    var databaseDenied = (Denied)transaction.Instantiate(denied.Id);
                    var permissionId = databaseDenied.Strategy.Class.ReadPermissionIdByRelationTypeId[databaseDenied.Meta.DefaultWorkspaceProperty.RelationType.Id];
                    var permission = (Permission)transaction.Instantiate(permissionId);
                    var revocation = Assert.Single(databaseDenied.Revocations);
                    if (denyRead)
                    {
                        revocation.AddDeniedPermission(permission);
                    }
                    else
                    {
                        revocation.RemoveDeniedPermission(permission);
                    }

                    databaseDenied.DefaultWorkspaceProperty = denyRead ? "Read denied" : "Read restored";
                    Assert.False(transaction.Derive().HasErrors);
                    transaction.Commit();
                }

                result = await session.PullAsync(new Pull { Object = denied });
                Assert.False(result.HasErrors);
                Assert.Equal(!denyRead, denied.CanReadDefaultWorkspaceProperty);
                Assert.False(denied.CanWriteDefaultWorkspaceProperty);
            }
        }

        [Fact]
        public async Task RejectedExistingObjectPushReturnsAccessErrorAndRollsBack()
        {
            var profile = (Profile)this.Profile;
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.C1) });
            var c1 = result.GetCollection<Allors.Workspace.Domain.C1>().First();
            var originalInteger = c1.C1AllorsInteger;
            var originalString = c1.C1AllorsString;
            Assert.True(c1.CanWriteC1AllorsString);
            c1.C1AllorsInteger = 2345;
            c1.C1AllorsString = "Rejected edit";

            using (var transaction = profile.Database.CreateTransaction())
            {
                transaction.Services.Get<IUserService>().User = (IUser)transaction.Instantiate(profile.DatabaseConnection.UserId);
                var databaseC1 = (C1)transaction.Instantiate(c1.Id);
                var permissionId = databaseC1.Strategy.Class.WritePermissionIdByRelationTypeId[databaseC1.Meta.C1AllorsString.RelationType.Id];
                var permission = (Permission)transaction.Instantiate(permissionId);
                var grants = transaction.Services.Get<IWorkspaceAclsService>().Create("Default")[databaseC1].Grants.Select(grant => (Grant)transaction.Instantiate(grant.Id)).ToArray();
                Assert.Contains(grants, grant => grant.EffectivePermissions.Contains(permission));
                foreach (var grant in grants)
                {
                    grant.Role.RemovePermission(permission);
                }

                Assert.False(transaction.Derive().HasErrors);
                transaction.Commit();
            }

            var pushResult = await session.PushAsync();
            Assert.True(pushResult.HasErrors);
            Assert.Same(c1, Assert.Single(pushResult.AccessErrors));
            Assert.Empty(pushResult.VersionErrors);
            Assert.Equal(2345, c1.C1AllorsInteger);
            Assert.Equal("Rejected edit", c1.C1AllorsString);

            using var verification = profile.Database.CreateTransaction();
            var storedC1 = (C1)verification.Instantiate(c1.Id);
            Assert.Equal(originalInteger, storedC1.C1AllorsInteger);
            Assert.Equal(originalString, storedC1.C1AllorsString);
        }
    }
}
