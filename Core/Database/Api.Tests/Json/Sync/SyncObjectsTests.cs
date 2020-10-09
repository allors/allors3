// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using Allors;
    using Allors.Api.Json;
    using Allors.Domain;
    using Allors.Protocol.Database.Sync;
    using Xunit;

    public class SyncObjectTests : ApiTest, IClassFixture<Fixture>
    {
        public SyncObjectTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeletedObject()
        {
            this.SetUser("jane@example.com");

            var organisation = new OrganisationBuilder(this.Session).WithName("Acme").Build();
            this.Session.Derive();
            this.Session.Commit();

            organisation.Strategy.Delete();
            this.Session.Derive();
            this.Session.Commit();

            var syncRequest = new SyncRequest
            {
                Objects = new[] { organisation.Id.ToString() },
            };

            var api = new Api(this.Session, "Default");
            var syncResponse = api.Sync(syncRequest);

            Assert.Empty(syncResponse.Objects);
        }

        [Fact]
        public void ExistingObject()
        {
            this.SetUser("jane@example.com");

            var people = new People(this.Session).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                Objects = new[] { person.Id.ToString() },
            };

            var api = new Api(this.Session, "Default");
            var syncResponse = api.Sync(syncRequest);

            Assert.Single(syncResponse.Objects);
            var syncObject = syncResponse.Objects[0];

            Assert.Equal(person.Id.ToString(), syncObject.I);
            Assert.Equal($"{M.Person.Class.IdAsString}", syncObject.T);
            Assert.Equal(person.Strategy.ObjectVersion.ToString(), syncObject.V);
        }


        [Fact]
        public void WithoutAccessControl()
        {
            new AutomatedAgentBuilder(this.Session).WithUserName("noacl").Build();
            this.Session.Derive();
            this.Session.Commit();

            this.SetUser("noacl");

            var people = new People(this.Session).Extent();
            var person = people[0];

            var syncRequest = new SyncRequest
            {
                Objects = new[] { person.Id.ToString() },
            };

            var api = new Api(this.Session, "Default");
            var syncResponse = api.Sync(syncRequest);
            
            Assert.Single(syncResponse.Objects);
            var syncObject = syncResponse.Objects[0];

            Assert.Null(syncObject.A);
            Assert.Null(syncObject.D);
        }
    }
}
