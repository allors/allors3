// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using System.Threading;
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
        }
    }
}
