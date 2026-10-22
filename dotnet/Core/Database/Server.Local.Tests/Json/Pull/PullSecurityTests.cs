// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using Allors;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;
    using IRoleType = Allors.Database.Meta.IRoleType;

    public class PullSecurityTests : ApiTest, IClassFixture<Fixture>
    {
        public PullSecurityTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RetainsObjectWhenAllRolesAreUnreadable(bool fromExtent)
        {
            this.SetUser("jane@example.com");
            var data = this.CreateUnreadableData();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var acl = api.AccessControl[data];
            Assert.Contains(this.M.Data.String, api.RoleTypesByClass[this.M.Data]);
            Assert.All(api.RoleTypesByClass[this.M.Data], roleType => Assert.False(acl.CanRead(roleType)));
            Assert.NotEmpty(acl.Grants);
            Assert.NotEmpty(acl.Revocations);
            Assert.Equal("Unreadable value", data.String);

            var pull = fromExtent
                ? new Pull { Extent = new Extent(this.M.Data) }
                : new Pull { Object = data };
            var response = api.Pull(new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) } });

            Assert.False(response.HasErrors);
            if (fromExtent)
            {
                Assert.True(response.c.TryGetValue("Datas", out var ids));
                Assert.Equal(data.Id, Assert.Single(ids));
            }
            else
            {
                Assert.True(response.o.TryGetValue("Data", out var id));
                Assert.Equal(data.Id, id);
            }

            var returned = Assert.Single(response.p);
            Assert.Equal(data.Id, returned.i);
            Assert.Equal(data.Strategy.ObjectVersion, returned.v);
            Assert.Equal(api.Ranges.Import(acl.Grants.Select(v => v.Id)).Save(), returned.g);
            Assert.Equal(api.Ranges.Import(acl.Revocations.Select(v => v.Id)).Save(), returned.r);
            Assert.All(acl.Grants, grant =>
                Assert.Equal(grant.Version, Assert.Single(response.g, v => v[0] == grant.Id)[1]));
            Assert.All(acl.Revocations, revocation =>
                Assert.Equal(revocation.Version, Assert.Single(response.r, v => v[0] == revocation.Id)[1]));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IncludesUnreadableTargetsOfReadableRelations(bool toMany)
        {
            this.SetUser("jane@example.com");
            var unreadable = this.CreateUnreadableData();
            var readable = new DataBuilder(this.Transaction).WithString("Readable value").Build();
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithName("Source")
                .WithOneData(unreadable)
                .WithManyData(unreadable)
                .WithManyData(readable)
                .Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            IRoleType roleType = toMany ? this.M.Organisation.ManyDatas : this.M.Organisation.OneData;
            Assert.True(api.AccessControl[organisation].CanRead(roleType));
            Assert.Contains(this.M.Data.String, api.RoleTypesByClass[this.M.Data]);
            Assert.All(api.RoleTypesByClass[this.M.Data], v => Assert.False(api.AccessControl[unreadable].CanRead(v)));
            Assert.True(api.AccessControl[readable].CanRead(this.M.Data.String));

            var pull = new Pull
            {
                Object = organisation,
                Results = new[] { new Result { Include = new[] { new Node(roleType) } } },
            };
            var response = api.Pull(new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) } });

            Assert.False(response.HasErrors);
            Assert.Equal(organisation.Id, response.o["Organisation"]);
            var expected = toMany
                ? new[] { organisation.Id, unreadable.Id, readable.Id }
                : new[] { organisation.Id, unreadable.Id };
            Assert.Equal(expected.OrderBy(v => v), response.p.Select(v => v.i).OrderBy(v => v));
        }

        private Data CreateUnreadableData()
        {
            var data = new DataBuilder(this.Transaction).WithString("Unreadable value").Build();
            var deniedPermissions = new Permissions(this.Transaction).Extent()
                .Where(v => Equals(v.Class, this.M.Data) && v.Operation == Operations.Read && v.InWorkspace("Default"))
                .ToArray();
            Assert.NotEmpty(deniedPermissions);
            var revocation = new RevocationBuilder(this.Transaction).Build();
            revocation.DeniedPermissions = deniedPermissions;
            data.AddRevocation(revocation);
            return data;
        }

        [Fact]
        public void SameWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Extent(m.WorkspaceXObject1) };
                var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

                var api = new Api(this.Transaction, "X", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                var wx1s = pullResponse.c["WorkspaceXObject1s"];

                Assert.Single(wx1s);

                var wx1 = wx1s.First();

                Assert.Equal(x1.Id, wx1);
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l = new[]
                                {
                    new Allors.Protocol.Json.Data.Pull
                    {
                        o = x1.Id,
                    },
                },
                };

                var api = new Api(this.Transaction, "X", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                var wx1 = pullResponse.o["WorkspaceXObject1"];

                Assert.Equal(x1.Id, wx1);
            }
        }

        [Fact]
        public void DifferentWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Extent(m.WorkspaceXObject1) };

                var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) } };

                var api = new Api(this.Transaction, "Y", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.False(pullResponse.c.ContainsKey("WorkspaceXObject1s"));
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l = new[]
                    {
                        new Allors.Protocol.Json.Data.Pull
                        {
                            o = x1.Id,
                        },
                    },
                };

                var api = new Api(this.Transaction, "Y", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                Assert.Empty(pullResponse.o);
            }
        }

        [Fact]
        public void NoneWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Extent(m.WorkspaceXObject1) };
                var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

                var api = new Api(this.Transaction, "None", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.False(pullResponse.c.ContainsKey("WorkspaceXObject1s"));
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l = new[]
                    {
                        new Allors.Protocol.Json.Data.Pull
                        {
                            o = x1.Id,
                        },
                    },
                };

                var api = new Api(this.Transaction, "None", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.Empty(pullResponse.o);
            }
        }

        [Fact]
        public void WithDeniedPermissions()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();
            var permissions = new Permissions(this.Transaction).Extent();
            var permission = permissions.First(v => Equals(v.Class, this.M.Data) && v.InWorkspace("Default"));
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build();
            data.AddRevocation(revocation);

            this.Transaction.Commit();

            var pull = new Pull { Extent = new Extent(m.Data) };
            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
            Assert.Equal(acl.Revocations.Select(v => v.Id), @object.r);
        }

        [Fact]
        public async void WithoutDeniedPermissions()
        {
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var uri = new Uri(@"allors/pull", UriKind.Relative);

            var pull = new Pull { Extent = new Extent(this.M.Data) };

            var pullRequest = new PullRequest
            {
                l = new[]
                      {
                          pull.ToJson(this.UnitConvert)
                      },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
        }

        [Fact]
        public void WithoutDeniedPermissionsAsAgent()
        {
            var user = this.SetUser("agent");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull { Extent = new Extent(this.M.Data) };

            var pullRequest = new PullRequest
            {
                l = new[]
                      {
                          pull.ToJson(this.UnitConvert)
                      },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.NotEmpty(@object.g);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
        }
    }
}
