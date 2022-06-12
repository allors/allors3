// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class PullSecurityTests : ApiTest, IClassFixture<Fixture>
    {
        public PullSecurityTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

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
        public void WithDeniedPermissionsFromDatabaseAndOtherWorkspace()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var pull = new Pull { Extent = new Extent(m.Denied) };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pullResponseObject = pullResponse.p[0];

            var databaseWrite = new Permissions(this.Transaction).Extent().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.DatabaseProperty));
            var defaultWorkspaceWrite = new Permissions(this.Transaction).Extent().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.DefaultWorkspaceProperty) && v.Operation == Operations.Write);
            var workspaceXWrite = new Permissions(this.Transaction).Extent().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.WorkspaceXProperty) && v.Operation == Operations.Write);

            // TODO: Koen
            //Assert.Single(pullResponseObject.d);

            //Assert.Contains(defaultWorkspaceWrite.Id, pullResponseObject.d);
            //Assert.DoesNotContain(databaseWrite.Id, pullResponseObject.d);
            //Assert.DoesNotContain(workspaceXWrite.Id, pullResponseObject.d);
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
    }
}
