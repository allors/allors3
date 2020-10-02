// <copyright file="PushTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using Allors.Api.Json;
    using Allors.Domain;
    using Allors.Protocol.Remote.Push;
    using Xunit;

    public class PushTests : ApiTest, IClassFixture<Fixture>
    {
        public PushTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WorkspaceNewObject()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                NewObjects = new[] { new PushRequestNewObject { T = M.Build.Class.IdAsString, NI = "-1" }, },
            };

            var api = new Api(this.Session, "Default");
            var pushResponse = api.Push(pushRequest);
            
            this.Session.Rollback();

            var build = (Build)this.Session.Instantiate(pushResponse.NewObjects[0].I);

            Assert.Equal(new Guid("DCE649A4-7CF6-48FA-93E4-CDE222DA2A94"), build.Guid);
            Assert.Equal("Exist", build.String);
        }

        [Fact]
        public void DeletedObject()
        {
            this.SetUser("jane@example.com");

            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Commit();

            var organisationId = organisation.Id.ToString();
            var organisationVersion = organisation.Strategy.ObjectVersion.ToString();

            organisation.Delete();
            this.Session.Commit();

            var uri = new Uri(@"allors/push", UriKind.Relative);

            var pushRequest = new PushRequest
            {
                Objects = new[]
                {
                    new PushRequestObject
                    {
                        I = organisationId,
                        V = organisationVersion,
                        Roles = new[]
                        {
                            new PushRequestRole
                            {
                              T = M.Organisation.Name.RelationType.IdAsString,
                              S = "Acme"
                            },
                        },
                    },
                },
            };

            var api = new Api(this.Session, "Default");
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Single(pushResponse.MissingErrors);
            Assert.Contains(organisationId, pushResponse.MissingErrors);
        }
    }
}
