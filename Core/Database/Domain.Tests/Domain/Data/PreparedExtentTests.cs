// <copyright file="PreparedExtentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database;
    using Allors.Database.Domain;
    using Configuration;
    using Xunit;

    public class PreparedExtentTests : DomainTest, IClassFixture<Fixture>
    {
        public PreparedExtentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void WithParameter()
        {
            var organisations = new Organisations(this.Session).Extent().ToArray();

            var extentService = this.Session.Database.Context().PreparedExtents;
            var organizationByName = extentService.Get(PreparedExtents.OrganisationByName);

            var arguments = new Dictionary<string, string>
            {
                { "name", "Acme" },
            };

            Extent<Organisation> organizations = organizationByName.Build(this.Session, arguments).ToArray();

            Assert.Single(organizations);

            var organization = organizations[0];

            Assert.Equal("Acme", organization.Name);
        }

    }
}
