// <copyright file="PreparedExtentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Protocol.Json.SystemTextJson;
    using Database;
    using Domain;
    using Protocol.Json;
    using Services;
    using Xunit;

    public class PersistentPreparedExtentTests : DomainTest, IClassFixture<Fixture>
    {
        public PersistentPreparedExtentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void WithParameter()
        {
            var organisations = new Organisations(this.Transaction).Extent().ToArray();

            var extentService = this.Transaction.Database.Services.Get<IPreparedExtents>();
            var organizationByName = extentService.Get(PersistentPreparedExtents.ByName);

            var arguments = new Arguments(new Dictionary<string, object>
            {
                { "name", "Acme" },
            }, new UnitConvert());

            Extent<Organisation> organizations = organizationByName.Build(this.Transaction, arguments).ToArray();

            Assert.Single(organizations);

            var organization = organizations[0];

            Assert.Equal("Acme", organization.Name);
        }
    }
}
