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
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var organisation = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(organisation).Build();

            this.Transaction.Derive();

            var partyFinancial = organisation.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var postalAddress = new PostalAddressBuilder(this.Transaction)
                  .WithAddress1("Kleine Nieuwedijkstraat 2")
                  .WithPostalAddressBoundary(mechelen)
                  .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();

            var salesOrder1 = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(organisation).WithAssignedShipToAddress(postalAddress).WithComment("salesorder1").Build();
            this.Transaction.Derive();

            var orderItem1 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(10)
                .Build();
            salesOrder1.AddSalesOrderItem(orderItem1);
            this.Transaction.Derive();

            var salesOrder2 = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(organisation).WithAssignedShipToAddress(postalAddress).WithComment("salesorder2").Build();
            this.Transaction.Derive();

            var orderItem2 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(10)
                .Build();
            salesOrder2.AddSalesOrderItem(orderItem2);
            this.Transaction.Derive();

            var salesOrder3 = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(organisation).WithAssignedShipToAddress(postalAddress).WithComment("salesorder3").Build();
            this.Transaction.Derive();

            var orderItem3 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(10)
                .Build();
            salesOrder3.AddSalesOrderItem(orderItem3);

            salesOrder3.Cancel();
            this.Transaction.Derive();

            Assert.Equal(200M, partyFinancial.OpenOrderAmount);
        }
    }

    public class PartyRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartyContactMechanismsDeriveBillingAddress()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Derive();

            Assert.Equal(partyContactMechanism.ContactMechanism, party.BillingAddress);
        }

        [Fact]
        public void ChangedPartyContactMechanismContactPurposesDeriveBillingAddress()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
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
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Derive();

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedPartyContactMechanismFromDateDeriveCurrentPartyContactMechanisms()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
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
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism);
            this.Derive();

            Assert.Contains(partyContactMechanism, party.CurrentPartyContactMechanisms);

            partyContactMechanism.ThroughDate = partyContactMechanism.FromDate;
            this.Derive();

            Assert.DoesNotContain(partyContactMechanism, party.CurrentPartyContactMechanisms);
        }

        [Fact]
        public void ChangedDerivationTriggerCreatePartyFinancialRelationship()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(party.ExistPartyFinancialRelationshipsWhereFinancialParty);
        }
    }
}
