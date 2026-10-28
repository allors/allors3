// <copyright file="SyncTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Api.Pull;
    using PreparedExtents = Allors.Database.Configuration.PreparedExtents;
    using Allors.Database.Services;
    using Allors.Database.Adapters.Sql.Tracing;
    using SqlDatabase = Allors.Database.Adapters.Sql.Database;
    using Allors.Services;
    using Microsoft.AspNetCore.Mvc;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Xunit;

    public class SyncObjectTests : ApiTest, IClassFixture<Fixture>
    {
        public SyncObjectTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void FinalSelectionReexecutesPullArgumentsAndDependencies()
        {
            this.SetUser("jane@example.com");
            var oldData = new DataBuilder(this.Transaction).WithString("Old dependency").Build();
            var leaving = new OrganisationBuilder(this.Transaction).WithName("Final selection").WithOneData(oldData).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new PullRequest
            {
                l = new[]
                {
                    new Pull
                    {
                        er = PreparedExtents.OrganisationByName,
                        a = new Dictionary<string, object> { ["name"] = "Final selection" },
                    },
                    new Pull
                    {
                        o = leaving.Id,
                        r = new[] { new Allors.Protocol.Json.Data.Result { n = "Current", s = new Allors.Protocol.Json.Data.Select { r = this.M.Organisation.OneData.RelationType.Tag } } },
                    },
                },
                d = new[] { new PullDependency { o = this.M.Organisation.Tag, r = this.M.Organisation.OneData.RelationType.Tag } },
            };
            var first = new Api(this.Transaction, "Default", CancellationToken.None).Pull(request);
            Assert.Equal(new[] { leaving.Id }, first.c["Organisations"]);
            Assert.Equal(oldData.Id, first.o["Current"]);

            leaving.Name = "No longer selected";
            var data = new DataBuilder(this.Transaction).WithString("New dependency").Build();
            leaving.OneData = data;
            var entering = new OrganisationBuilder(this.Transaction).WithName("Final selection").WithOneData(data).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var response = new Api(this.Transaction, "Default", CancellationToken.None).Sync(new SyncRequest { pull = request });
            Assert.Equal(new[] { entering.Id }, response.result.c["Organisations"]);
            Assert.Equal(data.Id, response.result.o["Current"]);
            Assert.Equal(new[] { entering.Id, data.Id }.OrderBy(v => v), response.o.Select(v => v.i).OrderBy(v => v));
            Assert.Equal("New dependency", Assert.Single(response.o, v => v.i == data.Id).GetRole(this.M.Data.String).v);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExactCacheHintsRequireDataAndSecurityVersions(bool securityOnly)
        {
            this.SetUser("jane@example.com");
            var data = new DataBuilder(this.Transaction).WithString("Before").Build();
            var permission = new Permissions(this.Transaction).Get(this.M.Data, this.M.Data.String, Operations.Read);
            var revocation = new RevocationBuilder(this.Transaction)
                .WithDeniedPermission(new Permissions(this.Transaction).Get(this.M.Data, this.M.Data.Checkbox, Operations.Read)).Build();
            data.AddRevocation(revocation);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new PullRequest { l = new[] { new Pull { o = data.Id } } };
            var first = new Api(this.Transaction, "Default", CancellationToken.None).Pull(pull);
            var request = new SyncRequest { pull = pull, o = first.p, g = first.g, r = first.r };
            var reused = new Api(this.Transaction, "Default", CancellationToken.None).Sync(request);
            Assert.Null(Assert.Single(reused.o).ro);
            Assert.All(reused.g, v => Assert.Null(v.p));
            Assert.Empty(reused.p);

            if (securityOnly)
            {
                revocation.AddDeniedPermission(permission);
            }
            else
            {
                data.String = "After";
            }

            this.Transaction.Derive();
            this.Transaction.Commit();

            var response = new Api(this.Transaction, "Default", CancellationToken.None).Sync(request);
            var result = Assert.Single(response.o);
            Assert.NotNull(result.ro);
            if (securityOnly)
            {
                Assert.Equal(first.p[0].v, result.v);
                Assert.Null(result.GetRole(this.M.Data.String));
                Assert.Contains(response.r, v => v.i == revocation.Id && v.p.Contains(permission.Id));
                Assert.Contains(response.p, v => v.i == permission.Id);
            }
            else
            {
                Assert.NotEqual(first.p[0].v, result.v);
                Assert.Equal("After", result.GetRole(this.M.Data.String).v);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SharedSecurityIncludesCompleteDeduplicatedPermissions(bool warmCache)
        {
            var user = Assert.IsAssignableFrom<User>(this.SetUser("noacl"));
            var permission = new Permissions(this.Transaction).Get(this.M.Data, this.M.Data.String, Operations.Read);
            var create = (CreatePermission)this.Transaction.Instantiate(this.M.Data.CreatePermissionId);
            var role = new RoleBuilder(this.Transaction).WithName("Shared Sync grant").WithPermission(permission).WithPermission(create).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var first = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var second = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var data = new DataBuilder(this.Transaction).WithString("First").WithSecurityToken(first).WithSecurityToken(second).Build();
            var other = new DataBuilder(this.Transaction).WithString("Second").WithSecurityToken(first).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            if (warmCache)
            {
                this.Security.GetVersionedGrants(this.Transaction, user, new[] { first }, "Default");
            }

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pull = new PullRequest { l = new[] { data.Id, other.Id }.Select(id => new Pull { o = id }).ToArray() };
            var selected = api.Pull(pull);
            Assert.All(selected.p, v => Assert.Equal(new[] { grant.Id }, v.g));
            var response = api.Sync(new SyncRequest
            {
                pull = pull,
            });
            var permissionIds = response.g.SelectMany(v => api.Ranges.Load(v.p)).Concat(response.r.SelectMany(v => api.Ranges.Load(v.p))).ToHashSet();
            Assert.NotEmpty(permissionIds);
            Assert.Equal(permissionIds.OrderBy(v => v), response.p.Select(v => v.i).OrderBy(v => v));
            Assert.Equal(response.g.Length, response.g.Select(v => v.i).Distinct().Count());
            Assert.Equal(response.r.Length, response.r.Select(v => v.i).Distinct().Count());
            Assert.Equal(response.p.Length, response.p.Select(v => v.i).Distinct().Count());
            var definition = Assert.Single(response.p, v => v.i == create.Id);
            Assert.Equal((long)Operations.Create, definition.o);
            Assert.Null(definition.t);
        }

        [Fact]
        public void ConflictingSecurityHintVersionsAreRejected()
        {
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var request = new SyncRequest
            {
                pull = new PullRequest(),
                g = new[] { new long[] { 1, 1 }, new long[] { 1, 2 } },
            };
            Assert.Throws<ArgumentException>(() => api.Sync(request));

            var controller = new SyncController(new StubTransactionService { Transaction = this.Transaction }, new StubWorkspaceService { Name = "Default" }, new PolicyService());
            var result = Assert.IsType<BadRequestObjectResult>(controller.Post(request, CancellationToken.None).Result);
            Assert.Contains("one entry per ID", Assert.IsType<SyncResponse>(result.Value)._e);
        }

        [Fact]
        public void ServerErrorsAreNotReportedAsMalformedCacheHints()
        {
            var controller = new SyncController(new FailingTransactionService(), new StubWorkspaceService { Name = "Default" }, new PolicyService());
            Assert.Throws<ArgumentException>(() => controller.Post(new SyncRequest { pull = new PullRequest() }, CancellationToken.None));
        }

        [Fact]
        public void SelectionContentsAndSecurityUseTheSameSnapshot()
        {
            var user = this.SetUser("jane@example.com");
            var data = new DataBuilder(this.Transaction).WithString("Before").Build();
            var permissions = new Permissions(this.Transaction);
            var readString = permissions.Get(this.M.Data, this.M.Data.String, Operations.Read);
            var readCheckbox = permissions.Get(this.M.Data, this.M.Data.Checkbox, Operations.Read);
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(readCheckbox).Build();
            data.AddRevocation(revocation);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var dataVersion = data.Strategy.ObjectVersion;
            var revocationVersion = revocation.Strategy.ObjectVersion;
            var database = (SqlDatabase)this.Transaction.Database;
            var sink = new Sink();
            database.Sink = sink;
            this.Transaction.Rollback();
            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var changed = false;
            var pull = new PullRequest { l = new[] { new Pull { o = data.Id } } };
            sink.PostOnAfter = @event =>
            {
                if (changed || !ReferenceEquals(@event.Transaction, this.Transaction) ||
                    @event is not SqlInstantiateReferencesEvent instantiate || !instantiate.ObjectIds.Contains(data.Id))
                {
                    return;
                }

                changed = true;
                using var writer = database.CreateTransaction();
                writer.Services.Get<IUserService>().User = (Allors.Database.Security.IUser)writer.Instantiate(user.Id);
                ((Data)writer.Instantiate(data.Id)).String = "After";
                ((Revocation)writer.Instantiate(revocation.Id)).AddDeniedPermission((Permission)writer.Instantiate(readString.Id));
                writer.Derive();
                writer.Commit();
                new Api(writer, "Default", CancellationToken.None).Pull(pull);
            };

            var response = new Api(this.Transaction, "Default", CancellationToken.None).Sync(new SyncRequest { pull = pull });
            Assert.True(changed);
            var result = Assert.Single(response.o);
            Assert.Equal(dataVersion, result.v);
            Assert.Equal("Before", result.GetRole(this.M.Data.String).v);
            var security = Assert.Single(response.r, v => v.i == revocation.Id);
            Assert.Equal(revocationVersion, security.v);
            Assert.Contains(readCheckbox.Id, security.p);
            Assert.DoesNotContain(readString.Id, security.p);
        }

        [Fact]
        public void DeletedObject()
        {
            this.SetUser("jane@example.com");

            var organisation = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new PullRequest { l = new[] { new Pull { o = organisation.Id } } };
            Assert.Single(new Api(this.Transaction, "Default", CancellationToken.None).Pull(request).p);
            organisation.Strategy.Delete();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var syncRequest = new SyncRequest
            {
                pull = request,
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var syncResponse = api.Sync(syncRequest);

            Assert.Empty(syncResponse.o);
            Assert.Empty(syncResponse.result.o);
            Assert.Empty(syncResponse.result.c);
        }

        [Fact]
        public void ExistingObject()
        {
            this.SetUser("jane@example.com");

            var people = new People(this.Transaction).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                pull = new Allors.Protocol.Json.Api.Pull.PullRequest { l = new[] { new Allors.Protocol.Json.Data.Pull { o = person.Id } } },
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
                pull = new Allors.Protocol.Json.Api.Pull.PullRequest { l = new[] { new Allors.Protocol.Json.Data.Pull { o = person.Id } } },
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

            var response = api.Sync(new SyncRequest { pull = new Allors.Protocol.Json.Api.Pull.PullRequest { l = new[] { new Allors.Protocol.Json.Data.Pull { o = data.Id } } } });
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

            var response = api.Sync(new SyncRequest { pull = new Allors.Protocol.Json.Api.Pull.PullRequest { l = new[] { new Allors.Protocol.Json.Data.Pull { o = data.Id } } } });
            var syncObject = Assert.Single(response.o);

            Assert.Equal(data.Id, syncObject.i);
            Assert.Equal(this.M.Data.Tag, syncObject.c);
            Assert.Equal(data.Strategy.ObjectVersion, syncObject.v);
            Assert.Empty(syncObject.ro);
            Assert.All(response.g, v => Assert.Empty(v.p));
        }

        private sealed class StubTransactionService : ITransactionService
        {
            public Allors.Database.ITransaction Transaction { get; set; }
        }

        private sealed class StubWorkspaceService : IWorkspaceService
        {
            public string Name { get; set; }
        }

        private sealed class FailingTransactionService : ITransactionService
        {
            public Allors.Database.ITransaction Transaction => throw new ArgumentException("Transaction unavailable");
        }
    }
}
