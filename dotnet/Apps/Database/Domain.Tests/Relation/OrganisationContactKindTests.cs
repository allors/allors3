// <copyright file="OrganisationContactKindTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrganisationContactKindTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationContactKindTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrganisationContactKind_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OrganisationContactKindBuilder(this.Transaction);
            var contactKind = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("contactkind");
            contactKind = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
