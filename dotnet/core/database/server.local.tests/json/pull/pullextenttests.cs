// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Allors.Database.Configuration;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;
    using Extent = Allors.Database.Data.Extent;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Result = Allors.Database.Data.Result;

    public class PullExtentTests : ApiTest, IClassFixture<Fixture>
    {
        public PullExtentTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public async void ExtentRef()
        {
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    new Pull
                    {
                        er = PreparedExtents.OrganisationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.c["Organisations"];

            Assert.Single(organisations);
        }

        [Fact]
        public async void SelectRef()
        {
            // TODO: Not implemented
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    new Pull
                    {
                        er = PreparedExtents.OrganisationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.c["Organisations"];

            Assert.Single(organisations);
        }

        [Fact]
        public async void NamedResult()
        {
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.Data),
                Results = new[]
                {
                    new  Result { Name = "Datas" },
                }
            };

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
        public async void IncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Include = new []
                        {
                            new Node(this.M.C1.C1C2One2One)
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c1b.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
        }

        [Fact]
        public async void IncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new Equals(this.M.C2.Name) { Value = "c2B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Include = new []
                        {
                            new Node(this.M.C2.C1WhereC1C2One2One)
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c2b = new C2s(this.Transaction).Extent().First(v => "c2B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c2b.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
        }

        [Fact]
        public async void SelectRoleOne2OneIncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C1.C1C2One2One,
                            Include = new[]
                            {
                                new Node(this.M.C2.C2C2One2One)
                            }
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.C2C2One2One.Id);
        }

        [Fact]
        public async void SelectAssociationOne2OneIncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new Equals(this.M.C2.Name) { Value = "c2B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C2.C1WhereC1C2One2One,
                            Include = new[]
                            {
                                new Node(this.M.C1.C1WhereC1C1One2One)
                            }
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c2b = new C2s(this.Transaction).Extent().First(v => "c2B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.C1WhereC1C1One2One.Id);
        }
    }
}
