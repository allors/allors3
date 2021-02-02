// <copyright file="PartyTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PartyTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPartyWithOpenOrders_WhenDeriving_ThenOpenOrderAmountIsUpdated()
        {
            var store = this.Session.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var organisation = new OrganisationBuilder(this.Session).WithName("customer").Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Session).WithCustomer(organisation).Build();

            this.Session.Derive();

            var partyFinancial = organisation.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var postalAddress = new PostalAddressBuilder(this.Session)
                  .WithAddress1("Kleine Nieuwedijkstraat 2")
                  .WithPostalAddressBoundary(mechelen)
                  .Build();

            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            this.Session.Derive();

            var salesOrder1 = new SalesOrderBuilder(this.Session).WithBillToCustomer(organisation).WithAssignedShipToAddress(postalAddress).WithComment("salesorder1").Build();
            this.Session.Derive();

            var orderItem1 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(10)
                .Build();
            salesOrder1.AddSalesOrderItem(orderItem1);
            this.Session.Derive();

            var salesOrder2 = new SalesOrderBuilder(this.Session).WithBillToCustomer(organisation).WithAssignedShipToAddress(postalAddress).WithComment("salesorder2").Build();
            this.Session.Derive();

            var orderItem2 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(10)
                .Build();
            salesOrder2.AddSalesOrderItem(orderItem2);
            this.Session.Derive();

            var salesOrder3 = new SalesOrderBuilder(this.Session).WithBillToCustomer(organisation).WithAssignedShipToAddress(postalAddress).WithComment("salesorder3").Build();
            this.Session.Derive();

            var orderItem3 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(10)
                .Build();
            salesOrder3.AddSalesOrderItem(orderItem3);

            salesOrder3.Cancel();
            this.Session.Derive();

            Assert.Equal(200M, partyFinancial.OpenOrderAmount);
        }
    }

    public class PartyDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartyContactMechanismsDeriveBillingAddress()
        {
            var party = new PersonBuilder(this.Session).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Session.Derive(false);

            Assert.Equal(partyContactMechanism.ContactMechanism, party.BillingAddress);
        }

        [Fact]
        public void ChangedPartyContactMechanismContactPurposesDeriveBillingAddress()
        {
            var party = new PersonBuilder(this.Session).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Session.Derive(false);

            partyContactMechanism.AddContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress);
            this.Session.Derive(false);

            Assert.Equal(partyContactMechanism.ContactMechanism, party.BillingAddress);
        }

        [Fact]
        public void ChangedCustomerRelationshipEmployerDeriveCurrentPartyRelationships()
        {
            var customer = new PersonBuilder(this.Session).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithFromDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            this.Session.Derive(false);

            customerRelationship.InternalOrganisation = internalOrganisation;
            this.Session.Derive(false);

            Assert.Contains(customerRelationship, internalOrganisation.CurrentPartyRelationships);
        }

        [Fact]
        public void ChangedCustomerRelationshipFromDateDeriveCurrentPartyRelationships()
        {
            var customer = new PersonBuilder(this.Session).Build();
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Session.Derive(false);

            customerRelationship.FromDate = this.Session.Now();
            this.Session.Derive(false);

            Assert.Contains(customerRelationship, internalOrganisation.CurrentPartyRelationships);
        }

        [Fact]
        public void ChangedCustomerRelationshipThroughDateDeriveCurrentPartyRelationships()
        {
            var customer = new PersonBuilder(this.Session).Build();
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Session.Derive(false);

            Assert.Contains(customerRelationship, internalOrganisation.CurrentPartyRelationships);

            customerRelationship.ThroughDate = customerRelationship.FromDate;
            this.Session.Derive(false);

            Assert.DoesNotContain(customerRelationship, internalOrganisation.CurrentPartyRelationships);
        }

        [Fact]
        public void ChangedPartyContactMechanismsDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Session).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Session.Derive(false);

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedPartyContactMechanismFromDateDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Session).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .WithFromDate(this.Session.Now().AddDays(1))
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Session.Derive(false);

            Assert.DoesNotContain(partyContactMechanism, party.CurrentPartyContactMechanisms);

            partyContactMechanism.FromDate = this.Session.Now();
            this.Session.Derive(false);

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedPartyContactMechanismThroughDateDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Session).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Session.Derive(false);

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);

            partyContactMechanism.ThroughDate = partyContactMechanism.FromDate;
            this.Session.Derive(false);

            Assert.DoesNotContain(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedDerivationTriggerCreatePartyFinancialRelationship()
        {
            var party = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(party.ExistPartyFinancialRelationshipsWhereFinancialParty);
        }
    }
}
