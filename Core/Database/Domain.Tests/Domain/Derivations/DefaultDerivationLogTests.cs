// <copyright file="DefaultDerivationLogTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors;
    using Allors.Domain;
    using Xunit;

    public class DefaultDerivationLogTests : DomainTest, IClassFixture<Fixture>
    {
        public DefaultDerivationLogTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeletedUserinterfaceable()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();

            var validation = this.Session.Derive(false);
            Assert.Single(validation.Errors);

            var error = validation.Errors[0];
            Assert.Equal("Organisation.Name is required", error.Message);
        }
    }
}
