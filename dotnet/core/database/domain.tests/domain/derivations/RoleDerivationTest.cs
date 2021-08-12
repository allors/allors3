// <copyright file="DerivationNodesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class RoleDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public RoleDerivationTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void RemoveRole()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithName("Acme")
                .Build();

            var jane = new PersonBuilder(this.Transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .Build();

            this.Transaction.Derive();

            organisation.Owner = jane;

            this.Transaction.Derive();

            Assert.True(jane.Owning);

            organisation.RemoveOwner();

            this.Transaction.Derive();

            Assert.False(jane.Owning);
        }
    }
}
