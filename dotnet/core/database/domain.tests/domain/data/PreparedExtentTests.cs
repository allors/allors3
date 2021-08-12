// <copyright file="PreparedExtentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Configuration;
    using Services;
    using Xunit;

    public class PreparedExtentTests : DomainTest, IClassFixture<Fixture>
    {
        public PreparedExtentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void WithParameter()
        {
            var organisations = new Organisations(this.Transaction).Extent().ToArray();

            var extentService = this.Transaction.Database.Services.Get<IPreparedExtents>();
            var organizationByName = extentService.Get(PreparedExtents.OrganisationByName);

            var arguments = new Arguments(new Dictionary<string, object> { { "name", "Acme" }, });

            Extent<Organisation> organizations = organizationByName.Build(this.Transaction, arguments).ToArray();

            Assert.Single(organizations);

            var organization = organizations[0];

            Assert.Equal("Acme", organization.Name);
        }

    }
}
