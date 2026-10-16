// <copyright file="SyncTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using System.Threading;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Security;
    using Allors.Database.Services;
    using Allors.Protocol.Json.Api.Security;
    using Xunit;

    public class SecurityAccessControlTests : ApiTest, IClassFixture<Fixture>
    {
        public SecurityAccessControlTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var workspaceName = "X";
            var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
            var grant = new Grants(this.Transaction).Administrator;
            var securityToken = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();

            this.Transaction.Derive();

            this.SetUser("jane@example.com");

            var accessRequest = new AccessRequest
            {
                g = new[] { grant.Id },
            };

            var api = new Api(this.Transaction, workspaceName, CancellationToken.None);
            var securityResponse = api.Access(accessRequest);

            Assert.Single(securityResponse.g);

            var securityResponseAccessControl = securityResponse.g.First();

            Assert.Equal(grant.Id, securityResponseAccessControl.i);
            Assert.Equal(grant.Strategy.ObjectVersion, securityResponseAccessControl.v);

            var permissions = securityResponseAccessControl.p
                .Select(v => this.Transaction.Instantiate(v))
                .Cast<Permission>()
                .Where(v => v != null)
                .ToArray();

            foreach (var permission in permissions)
            {
                Assert.Contains(permission, grant.EffectivePermissions);
                Assert.Contains(permission.Class, metaCache.GetWorkspaceClasses(workspaceName));
            }

            foreach (var effectivePermission in grant.EffectivePermissions.Where(v => v.InWorkspace(workspaceName)))
            {
                Assert.Contains(effectivePermission, permissions);
            }
        }

        // Transaction-local security changes must not escape into an independent reader.
        // Each fresh Security instance is a control for the reader's committed state.
        [Fact]
        public void WorkspaceGrantPermissionsFollowTransactionAfterRollback()
        {
            var permission = (Permission)this.Transaction.Instantiate(this.M.Organisation.ReadPermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var writePermission = (Permission)this.Transaction.Instantiate(this.M.Organisation.WritePermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var person = new PersonBuilder(this.Transaction).WithFirstName("Grant reader").Build();
            var role = new RoleBuilder(this.Transaction).WithName("Grant cache characterization").WithPermission(writePermission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(person).WithRole(role).Build();
            Assert.False(this.Transaction.Derive(false).HasErrors);
            this.Transaction.Commit();
            var committedVersion = grant.Strategy.ObjectVersion;
            Assert.DoesNotContain(permission, grant.EffectivePermissions);

            role.AddPermission(permission);
            Assert.False(this.Transaction.Derive(false).HasErrors);
            Assert.Contains(permission, grant.EffectivePermissions);
            Assert.Equal(committedVersion, grant.Strategy.ObjectVersion);

            var dirtyBody = Assert.Single(this.Security.GetVersionedGrants(this.Transaction, person, new[] { grant }, "Default"));
            Assert.Equal(committedVersion, dirtyBody.Version);
            Assert.Contains(permission.Id, dirtyBody.PermissionSet);
            this.Transaction.Rollback();

            using var reader = this.Transaction.Database.CreateTransaction();
            Assert.NotSame(this.Transaction, reader);
            var committedGrant = (Grant)reader.Instantiate(grant.Id);
            var committedPerson = (Person)reader.Instantiate(person.Id);
            Assert.Equal(committedVersion, committedGrant.Strategy.ObjectVersion);
            Assert.DoesNotContain(permission.Id, committedGrant.EffectivePermissions.Select(v => v.Id));

            var sharedBody = Assert.Single(this.Security.GetVersionedGrants(reader, committedPerson, new[] { committedGrant }, "Default"));
            var uncachedBody = Assert.Single(this.CreateUncachedSecurity().GetVersionedGrants(reader, committedPerson, new[] { committedGrant }, "Default"));
            Assert.Equal(committedVersion, uncachedBody.Version);
            Assert.DoesNotContain(permission.Id, uncachedBody.PermissionSet);
            Assert.Equal(committedVersion, sharedBody.Version);
            Assert.DoesNotContain(permission.Id, sharedBody.PermissionSet);
            Assert.Equal(uncachedBody.PermissionSet, sharedBody.PermissionSet);
        }

        [Fact]
        public void WorkspaceGrantUsersFollowTransactionAfterRollback()
        {
            var permission = (Permission)this.Transaction.Instantiate(this.M.Organisation.ReadPermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var person = new PersonBuilder(this.Transaction).WithFirstName("Committed subject").Build();
            var otherPerson = new PersonBuilder(this.Transaction).WithFirstName("Uncommitted subject").Build();
            var role = new RoleBuilder(this.Transaction).WithName("Grant user cache characterization").WithPermission(permission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(person).WithRole(role).Build();
            Assert.False(this.Transaction.Derive(false).HasErrors);
            this.Transaction.Commit();
            var committedVersion = grant.Strategy.ObjectVersion;
            Assert.DoesNotContain(otherPerson, grant.EffectiveUsers);

            grant.AddSubject(otherPerson);
            Assert.False(this.Transaction.Derive(false).HasErrors);
            Assert.Contains(otherPerson, grant.EffectiveUsers);
            Assert.Equal(committedVersion, grant.Strategy.ObjectVersion);

            var dirtyBody = Assert.Single(this.Security.GetVersionedGrants(this.Transaction, otherPerson, new[] { grant }, "Default"));
            Assert.Equal(committedVersion, dirtyBody.Version);
            Assert.Contains(otherPerson.Id, dirtyBody.UserSet);
            this.Transaction.Rollback();

            using var reader = this.Transaction.Database.CreateTransaction();
            Assert.NotSame(this.Transaction, reader);
            var committedGrant = (Grant)reader.Instantiate(grant.Id);
            var committedOtherPerson = (Person)reader.Instantiate(otherPerson.Id);
            Assert.Equal(committedVersion, committedGrant.Strategy.ObjectVersion);
            Assert.DoesNotContain(otherPerson.Id, committedGrant.EffectiveUsers.Select(v => v.Id));

            var sharedBodies = this.Security.GetVersionedGrants(reader, committedOtherPerson, new[] { committedGrant }, "Default");
            Assert.Empty(this.CreateUncachedSecurity().GetVersionedGrants(reader, committedOtherPerson, new[] { committedGrant }, "Default"));
            Assert.Empty(sharedBodies);
        }

        [Fact]
        public void WorkspaceRevocationPermissionsFollowTransactionAfterRollback()
        {
            var permission = (Permission)this.Transaction.Instantiate(this.M.Organisation.ReadPermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var writePermission = (Permission)this.Transaction.Instantiate(this.M.Organisation.WritePermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var person = new PersonBuilder(this.Transaction).WithFirstName("Revocation reader").Build();
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(writePermission).Build();
            Assert.False(this.Transaction.Derive(false).HasErrors);
            this.Transaction.Commit();
            var committedVersion = revocation.Strategy.ObjectVersion;
            Assert.DoesNotContain(permission, revocation.DeniedPermissions);

            revocation.AddDeniedPermission(permission);
            Assert.False(this.Transaction.Derive(false).HasErrors);
            Assert.Contains(permission, revocation.DeniedPermissions);
            Assert.Equal(committedVersion, revocation.Strategy.ObjectVersion);

            var dirtyBody = Assert.Single(this.Security.GetVersionedRevocations(this.Transaction, person, new[] { revocation }, "Default"));
            Assert.Equal(committedVersion, dirtyBody.Version);
            Assert.Contains(permission.Id, dirtyBody.PermissionSet);
            this.Transaction.Rollback();

            using var reader = this.Transaction.Database.CreateTransaction();
            Assert.NotSame(this.Transaction, reader);
            var committedRevocation = (Revocation)reader.Instantiate(revocation.Id);
            var committedPerson = (Person)reader.Instantiate(person.Id);
            Assert.Equal(committedVersion, committedRevocation.Strategy.ObjectVersion);
            Assert.DoesNotContain(permission.Id, committedRevocation.DeniedPermissions.Select(v => v.Id));

            var sharedBody = Assert.Single(this.Security.GetVersionedRevocations(reader, committedPerson, new[] { committedRevocation }, "Default"));
            var uncachedBody = Assert.Single(this.CreateUncachedSecurity().GetVersionedRevocations(reader, committedPerson, new[] { committedRevocation }, "Default"));
            Assert.Equal(committedVersion, uncachedBody.Version);
            Assert.DoesNotContain(permission.Id, uncachedBody.PermissionSet);
            Assert.Equal(committedVersion, sharedBody.Version);
            Assert.DoesNotContain(permission.Id, sharedBody.PermissionSet);
            Assert.Equal(uncachedBody.PermissionSet, sharedBody.PermissionSet);
        }

        [Fact]
        public void WorkspaceSecurityTokenGrantsFollowTransactionAfterRollback()
        {
            var permission = (Permission)this.Transaction.Instantiate(this.M.Organisation.ReadPermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var writePermission = (Permission)this.Transaction.Instantiate(this.M.Organisation.WritePermissionIdByRelationTypeId[this.M.Organisation.Name.RelationType.Id]);
            var person = new PersonBuilder(this.Transaction).WithFirstName("Token reader").Build();
            var readRole = new RoleBuilder(this.Transaction).WithName("Token read cache characterization").WithPermission(permission).Build();
            var writeRole = new RoleBuilder(this.Transaction).WithName("Token write cache characterization").WithPermission(writePermission).Build();
            var readGrant = new GrantBuilder(this.Transaction).WithSubject(person).WithRole(readRole).Build();
            var writeGrant = new GrantBuilder(this.Transaction).WithSubject(person).WithRole(writeRole).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(writeGrant).Build();
            Assert.False(this.Transaction.Derive(false).HasErrors);
            this.Transaction.Commit();
            var committedVersion = token.Strategy.ObjectVersion;
            var committedStamp = token.SecurityStamp;
            Assert.DoesNotContain(readGrant, token.Grants);

            token.AddGrant(readGrant);
            Assert.False(this.Transaction.Derive(false).HasErrors);
            Assert.NotEqual(committedStamp, token.SecurityStamp);
            Assert.Equal(committedVersion, token.Strategy.ObjectVersion);

            var dirtyBodies = this.Security.GetVersionedGrants(this.Transaction, person, new[] { token }, "Default");
            Assert.Contains(readGrant.Id, dirtyBodies.Select(v => v.Id));
            this.Transaction.Rollback();

            using var reader = this.Transaction.Database.CreateTransaction();
            Assert.NotSame(this.Transaction, reader);
            var committedToken = (SecurityToken)reader.Instantiate(token.Id);
            var committedPerson = (Person)reader.Instantiate(person.Id);
            Assert.Equal(committedVersion, committedToken.Strategy.ObjectVersion);
            Assert.Equal(committedStamp, committedToken.SecurityStamp);
            Assert.Equal(writeGrant.Id, Assert.Single(committedToken.Grants).Id);

            var sharedBodies = this.Security.GetVersionedGrants(reader, committedPerson, new[] { committedToken }, "Default");
            var uncachedBodies = this.CreateUncachedSecurity().GetVersionedGrants(reader, committedPerson, new[] { committedToken }, "Default");
            Assert.Equal(writeGrant.Id, Assert.Single(uncachedBodies).Id);
            Assert.Equal(writeGrant.Id, Assert.Single(sharedBodies).Id);
        }

        private ISecurity CreateUncachedSecurity() => new Allors.Database.Configuration.Security((Allors.Database.Configuration.DatabaseServices)this.Transaction.Database.Services);

        // TODO: non existing workspace should throw error
        //[Fact]
        //public void NoneWorkspace()
        //{
        //    var workspaceName = "Y";
        //    var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
        //    var grant = new Grants(this.Transaction).Administrator;

        //    this.SetUser("jane@example.com");

        //    var accessRequest = new AccessRequest
        //    {
        //        g = new[] { grant.Id },
        //    };

        //    var api = new Api(this.Transaction, workspaceName, CancellationToken.None);
        //    var accessResponse = api.Access(accessRequest);

        //    Assert.Single(accessResponse.g);

        //    var accessResponseGrant = accessResponse.g.First();

        //    Assert.Equal(grant.Id, accessResponseGrant.i);
        //    Assert.Equal(grant.Strategy.ObjectVersion, accessResponseGrant.v);
        //    Assert.Null(accessResponseGrant.p);
        //}
    }
}
