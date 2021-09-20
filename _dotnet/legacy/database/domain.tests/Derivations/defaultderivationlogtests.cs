// <copyright file="DefaultDerivationLogTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors;
    using Allors.Database.Domain;

    using Xunit;

    public class DefaultDerivationLogTests : DomainTest, IClassFixture<Fixture>
    {
        public DefaultDerivationLogTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SingleErrorMessage()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();

            var validation = this.Transaction.Derive(false);
            Assert.Single(validation.Errors);

            var error = validation.Errors[0];
            Assert.Equal("Organisation.Name is required", error.Message);
        }
    }
}
