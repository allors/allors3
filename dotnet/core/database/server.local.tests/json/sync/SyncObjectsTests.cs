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

            var api = new Api(this.Transaction, "Default");
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

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            Assert.Single(syncResponse.o);
            var syncObject = syncResponse.o[0];

            Assert.Equal(person.Id, syncObject.i);
            Assert.Equal(this.M.Person.Tag, syncObject.t);
            Assert.Equal(person.Strategy.ObjectVersion, syncObject.v);
        }


        [Fact]
        public void WithoutAccessControl()
        {
            new PersonBuilder(this.Transaction).WithUserName("noacl").WithFirstName("No").WithLastName("acl").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            this.SetUser("noacl");

            var people = new People(this.Transaction).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                o = new[] { person.Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            Assert.Single(syncResponse.o);
            var syncObject = syncResponse.o[0];

            Assert.Empty(syncObject.a);
            Assert.Null(syncObject.d);
        }
    }
}
