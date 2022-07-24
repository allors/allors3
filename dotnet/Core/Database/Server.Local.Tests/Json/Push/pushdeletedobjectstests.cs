// <copyright file="PushTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Threading;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Push;
    using Xunit;

    public class PushDeletedObjectsTests : ApiTest, IClassFixture<Fixture>
    {
        public PushDeletedObjectsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            this.SetUser("jane@example.com");

            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Commit();

            var organisationId = organisation.Id;
            var organisationVersion = organisation.Strategy.ObjectVersion;

            organisation.Delete();
            this.Transaction.Commit();

            var uri = new Uri(@"allors/push", UriKind.Relative);

            var pushRequest = new PushRequest
            {
                o = new[]
                {
                    new PushRequestObject
                    {
                        d = organisationId,
                        v = organisationVersion,
                        r = new[]
                        {
                            new PushRequestRole
                            {
                              t = this.M.Organisation.Name.RelationType.Tag,
                              u = "Acme"
                            },
                        },
                    },
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Single(pushResponse._m);
            Assert.Contains(organisationId, pushResponse._m);
        }
    }
}
