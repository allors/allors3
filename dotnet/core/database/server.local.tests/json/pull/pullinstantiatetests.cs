// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Linq;
    using System.Threading;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;

    public class PullInstantiateTests : ApiTest, IClassFixture<Fixture>
    {
        public PullInstantiateTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public async void NamedResult()
        {
            var user = this.SetUser("jane@example.com");

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull { Object = c1b, Results = new[] { new Result { Name = "Data" }, } };

            var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedObject = pullResponse.o["Data"];

            Assert.Equal(c1b.Id, namedObject);
        }

        [Fact]
        public async void IncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
            {
                Object = c1b,
                Results = new[] { new Result { Include = new[] { new Node(this.M.C1.C1C2One2One) } }, }
            };

            var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            Assert.Contains(pool, v => v.i == c1b.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
        }

        [Fact]
        public async void IncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            var c2b = new C2s(this.Transaction).Extent().First(v => "c2B".Equals(v.Name));

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
            {
                Object = c2b,
                Results = new[] { new Result { Include = new[] { new Node(this.M.C2.C1WhereC1C2One2One) } }, }
            };

            var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            Assert.Contains(pool, v => v.i == c2b.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
        }

        [Fact]
        public async void SelectRoleOne2OneIncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
            {
                Object = c1b,
                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C1.C1C2One2One,
                            Include = new[] { new Node(this.M.C2.C2C2One2One) }
                        }
                    },
                }
            };

            var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.C2C2One2One.Id);
        }

        [Fact]
        public async void SelectAssociationOne2OneIncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            var c2b = new C2s(this.Transaction).Extent().First(v => "c2B".Equals(v.Name));

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
            {
                Object = c2b,
                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C2.C1WhereC1C2One2One,
                            Include = new[] { new Node(this.M.C1.C1WhereC1C1One2One) }
                        }
                    },
                }
            };

            var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.C1WhereC1C1One2One.Id);
        }


        [Fact]
        public async void OfType()
        {
            var user = this.SetUser("jane@example.com");

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
            {
                Object = c1b,
                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C1.C1I12Many2Manies,
                            OfType = this.M.C1
                        }
                    }
                }
            };

            var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Single(pool);
            Assert.Contains(pool, v => v.i == c1b.Id);
        }
    }
}
