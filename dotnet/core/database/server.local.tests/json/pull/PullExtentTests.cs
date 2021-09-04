// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Configuration;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;

    public class PullExtentTests : ApiTest, IClassFixture<Fixture>
    {
        public PullExtentTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public async void WithExtentRef()
        {
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    new Allors.Protocol.Json.Data.Pull
                    {
                        er = PreparedExtents.OrganisationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.c["Organisations"];

            Assert.Single(organisations);
        }

        [Fact]
        public async void WithSelectRef()
        {
            // TODO: Not implemented
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    new Allors.Protocol.Json.Data.Pull
                    {
                        er = PreparedExtents.OrganisationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.c["Organisations"];

            Assert.Single(organisations);
        }

        [Fact]
        public async void WithResult()
        {
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
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

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControlLists(user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.AccessControls.Select(v => v.Strategy.ObjectId), @object.a);
        }
    }
}
