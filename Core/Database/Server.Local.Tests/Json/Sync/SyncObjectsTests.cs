// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Database.Protocol.Json;
    using Xunit;

    public class SyncObjectTests : ApiTest, IClassFixture<Fixture>
    {
        public SyncObjectTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeletedObject()
        {
            _ = this.SetUser("jane@example.com");

            var organisation = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
            _ = this.Transaction.Derive();
            this.Transaction.Commit();

            organisation.Strategy.Delete();
            _ = this.Transaction.Derive();
            this.Transaction.Commit();

            var syncRequest = new SyncRequest
            {
                Objects = new[] { organisation.Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            Assert.Empty(syncResponse.Objects);
        }

        [Fact]
        public void ExistingObject()
        {
            _ = this.SetUser("jane@example.com");

            var people = new People(this.Transaction).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                Objects = new[] { person.Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            _ = Assert.Single(syncResponse.Objects);
            var syncObject = syncResponse.Objects[0];

            Assert.Equal(person.Id, syncObject.Id);
            Assert.Equal(this.M.Person.Tag, syncObject.ObjectType);
            Assert.Equal(person.Strategy.ObjectVersion, syncObject.Version);
        }


        [Fact]
        public void WithoutAccessControl()
        {
            _ = new PersonBuilder(this.Transaction).WithUserName("noacl").WithFirstName("No").WithLastName("acl").Build();
            _ = this.Transaction.Derive();
            this.Transaction.Commit();

            _ = this.SetUser("noacl");

            var people = new People(this.Transaction).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                Objects = new[] { person.Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            _ = Assert.Single(syncResponse.Objects);
            var syncObject = syncResponse.Objects[0];

            Assert.Empty(syncObject.AccessControls);
            Assert.Null(syncObject.DeniedPermissions);
        }
    }
}
