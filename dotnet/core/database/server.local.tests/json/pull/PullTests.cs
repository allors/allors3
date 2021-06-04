// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Database.Protocol.Json;
    using Xunit;
    using Extent = Allors.Database.Data.Extent;
    using Pull = Allors.Database.Data.Pull;
    using Result = Allors.Database.Data.Result;

    public class PullTests : ApiTest, IClassFixture<Fixture>
    {
        public PullTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WithDeniedPermissions()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();
            var permissions = new Permissions(this.Transaction).Extent();
            var permission = permissions.First(v => Equals(v.Class, this.M.Data));
            data.AddDeniedPermission(permission);

            this.Transaction.Commit();

            var pull = new Pull { Extent = new Extent(m.Data) };
            var pullRequest = new PullRequest
            {
                List = new[]
                {
                    pull.ToJson()
                },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.Collections["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.Pool;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControlLists(user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.Id);
            Assert.Equal(data.Strategy.ObjectVersion, @object.Version);
            Assert.Equal(acl.AccessControls.Select(v=>v.Strategy.ObjectId), @object.AccessControls);
            Assert.Equal(acl.DeniedPermissionIds, @object.DeniedPermissions);
        }

        [Fact]
        public async void WithExtentRef()
        {
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                List = new[]
                  {
                      new Allors.Protocol.Json.Data.Pull
                          {
                              ExtentRef = PreparedExtents.OrganisationByName,
                              Arguments = new Dictionary<string, object> { ["name"] = "Acme" },
                          },
                  },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.Collections["Organisations"];

            Assert.Single(organisations);
        }

        [Fact]
        public async void WithSelectRef()
        {
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                List = new[]
                  {
                      new Allors.Protocol.Json.Data.Pull
                          {
                              ExtentRef = PreparedExtents.OrganisationByName,
                              Arguments = new Dictionary<string, object> { ["name"] = "Acme" },
                          },
                  },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.Collections["Organisations"];

            Assert.Single(organisations);
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
                List = new[]
                      {
                          pull.ToJson()
                      },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.Collections["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.Pool;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControlLists(user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.Id);
            Assert.Equal(data.Strategy.ObjectVersion, @object.Version);
            Assert.Equal(acl.AccessControls.Select(v=>v.Strategy.ObjectId), @object.AccessControls);
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
                List = new[]
                {
                    pull.ToJson()
                },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.Collections["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.Pool;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControlLists(user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.Id);
            Assert.Equal(data.Strategy.ObjectVersion, @object.Version);
            Assert.Equal(acl.AccessControls.Select(v=>v.Strategy.ObjectId), @object.AccessControls);
        }
    }
}
