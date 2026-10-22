// <copyright file="SyncTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using System.Threading;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Sync;
    using Xunit;

    public class SyncObjectTests : ApiTest, IClassFixture<Fixture>
    {
        public SyncObjectTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeletedObject()
        {
            this.SetUser("jane@example.com");

            var organisation = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            organisation.Strategy.Delete();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var syncRequest = new SyncRequest
            {
                o = new[] { organisation.Id },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var syncResponse = api.Sync(syncRequest);

            Assert.Empty(syncResponse.o);
        }

        [Fact]
        public void ExistingObject()
        {
            this.SetUser("jane@example.com");

            var people = new People(this.Transaction).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                o = new[] { person.Id },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var syncResponse = api.Sync(syncRequest);

            Assert.Single(syncResponse.o);
            var syncObject = syncResponse.o[0];

            Assert.Equal(person.Id, syncObject.i);
            Assert.Equal(this.M.Person.Tag, syncObject.c);
            Assert.Equal(person.Strategy.ObjectVersion, syncObject.v);
        }

        [Fact]
        public void WithoutAccessControl()
        {
            new People(this.Transaction).Extent().First(v => "noacl".Equals(v.UserName));

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.SetUser("noacl");

            var people = new People(this.Transaction).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                o = new[] { person.Id },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var syncResponse = api.Sync(syncRequest);

            Assert.Single(syncResponse.o);
            var syncObject = syncResponse.o[0];

            Assert.Null(syncObject.g);
            Assert.Null(syncObject.r);
        }

        [Fact]
        public void ObjectWithoutReadableRolesKeepsIdentityAndSecurity()
        {
            this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("Hidden value").Build();
            var readPermissions = new Permissions(this.Transaction).Extent()
                .Where(v => Equals(v.Class, this.M.Data) && v.Operation == Operations.Read && v.InWorkspace("Default"))
                .ToArray();
            Assert.NotEmpty(readPermissions);

            var revocation = new RevocationBuilder(this.Transaction).Build();
            foreach (var permission in readPermissions)
            {
                revocation.AddDeniedPermission(permission);
            }

            data.AddRevocation(revocation);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var acl = api.AccessControl[data];
            Assert.Equal("Hidden value", data.String);
            Assert.Contains(this.M.Data.String, api.RoleTypesByClass[this.M.Data]);
            Assert.All(api.RoleTypesByClass[this.M.Data], roleType => Assert.False(acl.CanRead(roleType)));
            Assert.NotEmpty(acl.Grants);
            Assert.NotEmpty(acl.Revocations);

            var response = api.Sync(new SyncRequest { o = new[] { data.Id } });
            var syncObject = Assert.Single(response.o);

            Assert.Equal(data.Id, syncObject.i);
            Assert.Equal(this.M.Data.Tag, syncObject.c);
            Assert.Equal(data.Strategy.ObjectVersion, syncObject.v);
            Assert.Equal(api.Ranges.Import(acl.Grants.Select(v => v.Id)).Save(), syncObject.g);
            Assert.Equal(api.Ranges.Import(acl.Revocations.Select(v => v.Id)).Save(), syncObject.r);
            Assert.Empty(syncObject.ro);
        }

        [Theory]
        [InlineData("noacl")]
        [InlineData("noperm")]
        public void ObjectWithoutReadPermissionsKeepsIdentity(string userName)
        {
            var user = Assert.IsAssignableFrom<User>(this.SetUser(userName));

            var securityToken = new SecurityTokenBuilder(this.Transaction).Build();
            if (userName == "noperm")
            {
                var role = new RoleBuilder(this.Transaction).WithName("No permissions").Build();
                var grant = new GrantBuilder(this.Transaction).WithRole(role).WithSubject(user).Build();
                securityToken.AddGrant(grant);
            }

            var data = new DataBuilder(this.Transaction).WithString("Hidden value").WithSecurityToken(securityToken).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var acl = api.AccessControl[data];
            Assert.Equal("Hidden value", data.String);
            Assert.Contains(this.M.Data.String, api.RoleTypesByClass[this.M.Data]);
            Assert.All(api.RoleTypesByClass[this.M.Data], roleType => Assert.False(acl.CanRead(roleType)));

            Assert.Equal(userName == "noperm", acl.Grants.Length > 0);
            Assert.All(acl.Grants, grant => Assert.Empty(grant.PermissionSet));

            var response = api.Sync(new SyncRequest { o = new[] { data.Id } });
            var syncObject = Assert.Single(response.o);

            Assert.Equal(data.Id, syncObject.i);
            Assert.Equal(this.M.Data.Tag, syncObject.c);
            Assert.Equal(data.Strategy.ObjectVersion, syncObject.v);
            Assert.Empty(syncObject.ro);
        }
    }
}
