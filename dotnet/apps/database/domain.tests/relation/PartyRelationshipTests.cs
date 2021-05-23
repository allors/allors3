// <copyright file="PartyRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PartyRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyRelationshipTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrganisationContactRelationship_WhenRelationshipPeriodIsNotValid_ThenContactIsNotInCustomerContactUserGroup()
        {
            var party = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            this.Transaction.Derive();

            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithCustomer(party)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .Build();

            this.Transaction.Derive();

            Assert.Contains(customerRelationship, party.CurrentPartyRelationships);
            Assert.Empty(party.InactivePartyRelationships);

            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithSupplier(party)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .Build();

            this.Transaction.Derive();

            Assert.Contains(customerRelationship, party.CurrentPartyRelationships);
            Assert.Contains(supplierRelationship, party.CurrentPartyRelationships);
            Assert.Empty(party.InactivePartyRelationships);

            customerRelationship.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.Contains(supplierRelationship, party.CurrentPartyRelationships);
            Assert.Contains(customerRelationship, party.InactivePartyRelationships);
        }
    }
}
