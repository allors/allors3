// <copyright file="PartyTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PartyRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartyContactMechanismsDeriveBillingAddress()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(party)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            this.Derive();

            Assert.Equal(partyContactMechanism.ContactMechanism, party.BillingAddress);
        }

        [Fact]
        public void ChangedPartyContactMechanismContactPurposesDeriveBillingAddress()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(party)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            this.Derive();

            partyContactMechanism.AddContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress);
            this.Derive();

            Assert.Equal(partyContactMechanism.ContactMechanism, party.BillingAddress);
        }

        [Fact]
        public void ChangedCustomerRelationshipEmployerDeriveCurrentPartyRelationships()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            customerRelationship.InternalOrganisation = internalOrganisation;
            this.Derive();

            Assert.Contains(customerRelationship, internalOrganisation.CurrentPartyRelationships);
        }

        [Fact]
        public void ChangedCustomerRelationshipFromDateDeriveCurrentPartyRelationships()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Derive();

            customerRelationship.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(customerRelationship, internalOrganisation.CurrentPartyRelationships);
        }

        [Fact]
        public void ChangedCustomerRelationshipThroughDateDeriveCurrentPartyRelationships()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Derive();

            Assert.Contains(customerRelationship, internalOrganisation.CurrentPartyRelationships);

            customerRelationship.ThroughDate = customerRelationship.FromDate;
            this.Derive();

            Assert.DoesNotContain(customerRelationship, internalOrganisation.CurrentPartyRelationships);
        }

        [Fact]
        public void ChangedPartyContactMechanismsDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(party)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            this.Derive();

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedPartyContactMechanismFromDateDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(party)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();
            this.Derive();

            Assert.DoesNotContain(partyContactMechanism, party.CurrentPartyContactMechanisms);

            partyContactMechanism.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedPartyContactMechanismThroughDateDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(party)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            this.Derive();

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);

            partyContactMechanism.ThroughDate = partyContactMechanism.FromDate;
            this.Derive();

            Assert.DoesNotContain(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }
    }
}
