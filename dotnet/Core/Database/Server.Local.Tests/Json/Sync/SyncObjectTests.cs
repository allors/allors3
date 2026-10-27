// <copyright file="SyncTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
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
            Assert.Empty(syncResponse.g);
            Assert.Empty(syncResponse.r);
            Assert.Empty(syncResponse.p);
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
            Assert.Empty(syncResponse.g);
            Assert.Empty(syncResponse.r);
            Assert.Empty(syncResponse.p);
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

            Assert.Equal(acl.Grants.Select(v => v.Id).OrderBy(v => v), response.g.Select(v => v.i));
            Assert.Equal(acl.Revocations.Select(v => v.Id).OrderBy(v => v), response.r.Select(v => v.i));
            Assert.All(response.g, entry =>
            {
                var grant = Assert.Single(acl.Grants, v => v.Id == entry.i);
                Assert.Equal(grant.Version, entry.v);
                Assert.Equal(grant.PermissionRange.Save(), entry.p);
            });
            Assert.All(response.r, entry =>
            {
                var denied = Assert.Single(acl.Revocations, v => v.Id == entry.i);
                Assert.Equal(denied.Version, entry.v);
                Assert.Equal(denied.PermissionRange.Save(), entry.p);
            });

            var knownRequest = new SyncRequest
            {
                o = new[] { data.Id },
                g = response.g.SelectMany(v => new[] { new[] { v.i, v.v }, new[] { v.i, v.v + 1 } })
                    .Append(new[] { long.MaxValue, 2L }).ToArray(),
                r = response.r.SelectMany(v => new[] { new[] { v.i, v.v + 1 }, new[] { v.i, v.v } }).ToArray(),
            };
            var knownResponse = api.Sync(knownRequest);
            Assert.Equal(response.g.Select(v => (v.i, v.v)), knownResponse.g.Select(v => (v.i, v.v)));
            Assert.Equal(response.r.Select(v => (v.i, v.v)), knownResponse.r.Select(v => (v.i, v.v)));
            Assert.All(knownResponse.g, v => Assert.Null(v.p));
            Assert.All(knownResponse.r, v => Assert.Null(v.p));
            Assert.Empty(knownResponse.p);
            Assert.Empty(Assert.Single(knownResponse.o).ro);

            knownRequest.r = null;
            var missingResponse = api.Sync(knownRequest);
            Assert.All(missingResponse.g, v => Assert.Null(v.p));
            Assert.All(missingResponse.r, v => Assert.NotEmpty(v.p));
            Assert.Equal(readPermissions.Select(v => v.Id).OrderBy(v => v), missingResponse.p.Select(v => v.i));

            var grantEntry = response.g.First(v => v.p.Length > 0);
            var grantPermissions = grantEntry.p.ToArray();
            grantEntry.p[0] = -1;
            Assert.Equal(grantPermissions, Assert.Single(acl.Grants, v => v.Id == grantEntry.i).PermissionRange.ToArray());
            var revocationEntry = Assert.Single(response.r);
            var revokedPermissions = revocationEntry.p.ToArray();
            revocationEntry.p[0] = -1;
            Assert.Equal(revokedPermissions, Assert.Single(acl.Revocations).PermissionRange.ToArray());
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
            Assert.Equal(acl.Grants.Length, response.g.Length);
            Assert.All(response.g, v => Assert.Empty(v.p));
            Assert.Empty(response.r);
            Assert.Empty(response.p);

            var knownResponse = api.Sync(new SyncRequest
            {
                o = new[] { data.Id },
                g = response.g.SelectMany(v => new[] { new[] { v.i, v.v }, new[] { v.i, v.v } }).ToArray(),
            });
            Assert.Equal(response.g.Select(v => (v.i, v.v)), knownResponse.g.Select(v => (v.i, v.v)));
            Assert.All(knownResponse.g, v => Assert.Null(v.p));
            Assert.Empty(knownResponse.p);
        }

        [Fact]
        public void ChangedAccessVersionsReturnNewContents()
        {
            var user = Assert.IsAssignableFrom<User>(this.SetUser("noacl"));
            var permissions = new Permissions(this.Transaction);
            var read = permissions.Get(this.M.Data, this.M.Data.String, Operations.Read);
            var write = permissions.Get(this.M.Data, this.M.Data.String, Operations.Write);
            var role = new RoleBuilder(this.Transaction).WithName("Sync permissions").WithPermission(read).WithPermission(write).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(write).Build();
            var data = new DataBuilder(this.Transaction).WithString("Visible value").WithSecurityToken(token).WithRevocation(revocation).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var original = api.Sync(new SyncRequest { o = new[] { data.Id } });
            var originalGrant = Assert.Single(original.g);
            var originalRevocation = Assert.Single(original.r);
            Assert.Equal(new[] { read.Id, write.Id }.OrderBy(v => v), originalGrant.p);
            Assert.Equal(new[] { write.Id }, originalRevocation.p);
            Assert.Equal("Visible value", Assert.Single(original.o).GetRole(this.M.Data.String).v);

            role.RemovePermission(write);
            revocation.RemoveDeniedPermission(write);
            revocation.AddDeniedPermission(read);
            this.Transaction.Derive();
            this.Transaction.Commit();

            api = new Api(this.Transaction, "Default", CancellationToken.None);
            var response = api.Sync(new SyncRequest
            {
                o = new[] { data.Id },
                g = new[] { new[] { originalGrant.i, originalGrant.v } },
                r = new[] { new[] { originalRevocation.i, originalRevocation.v } },
            });
            var changedGrant = Assert.Single(response.g);
            var changedRevocation = Assert.Single(response.r);
            Assert.Equal(grant.Id, changedGrant.i);
            Assert.Equal(grant.Strategy.ObjectVersion, changedGrant.v);
            Assert.NotEqual(originalGrant.v, changedGrant.v);
            Assert.Equal(new[] { read.Id }, changedGrant.p);
            Assert.Equal(revocation.Id, changedRevocation.i);
            Assert.Equal(revocation.Strategy.ObjectVersion, changedRevocation.v);
            Assert.NotEqual(originalRevocation.v, changedRevocation.v);
            Assert.Equal(new[] { read.Id }, changedRevocation.p);
            Assert.Equal(read.Id, Assert.Single(response.p).i);
            var syncObject = Assert.Single(response.o);
            Assert.Equal(Assert.Single(original.o).v, syncObject.v);
            Assert.Empty(syncObject.ro);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InvalidKnownAccessVersions(bool revocations)
        {
            this.SetUser("jane@example.com");
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var invalid = new[]
            {
                new long[][] { null },
                new[] { Array.Empty<long>() },
                new[] { new[] { 1L } },
                new[] { new[] { 1L, 2L, 3L } },
                new[] { new[] { 0L, 2L } },
                new[] { new[] { -1L, 2L } },
                new[] { new[] { 1L, -1L } },
            };

            foreach (var entries in invalid)
            {
                var request = new SyncRequest { o = Array.Empty<long>() };
                if (revocations)
                {
                    request.r = entries;
                }
                else
                {
                    request.g = entries;
                }

                var error = Assert.Throws<ArgumentException>(() => api.Sync(request));
                Assert.Equal(revocations ? "r" : "g", error.ParamName);
            }
        }
    }
}
