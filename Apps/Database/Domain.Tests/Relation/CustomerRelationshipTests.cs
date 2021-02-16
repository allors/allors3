// <copyright file="CustomerRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class CustomerRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerRelationshipTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCustomerRelationship_WhenDerivingWithout_ThenAmountDueIsZero()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();

            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var partyFinancial = customer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));
            Assert.Equal(0, partyFinancial.AmountDue);
        }

        [Fact]
        public void GivenCustomerRelationship_WhenDerivingWithout_ThenAmountOverDueIsZero()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();

            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var partyFinancial = customer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));

            Assert.Equal(0, partyFinancial.AmountOverDue);
        }

        [Fact]
        public void GivenCustomerRelationshipToCome_WhenDeriving_ThenInternalOrganisationCustomersDosNotContainCustomer()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)

                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            this.Transaction.Derive();

            Assert.DoesNotContain(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void GivenCustomerRelationshipThatHasEnded_WhenDeriving_ThenInternalOrganisationCustomersDosNotContainCustomer()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)

                .WithFromDate(this.Transaction.Now().AddDays(-10))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            this.Transaction.Derive();

            Assert.DoesNotContain(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void GivenCustomerRelationshipBuilder_WhenBuild_ThenSubAccountNumerIsValidElevenTestNumber()
        {
            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.SubAccountCounter.Value = 1000;

            this.Transaction.Commit();

            var customer1 = new PersonBuilder(this.Transaction).WithLastName("customer1").Build();
            var customerRelationship1 = new CustomerRelationshipBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithCustomer(customer1)
                .Build();

            this.Transaction.Derive();

            var partyFinancial1 = customer1.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship1.InternalOrganisation));

            Assert.Equal(1007, partyFinancial1.SubAccountNumber);

            var customer2 = new PersonBuilder(this.Transaction).WithLastName("customer2").Build();
            var customerRelationship2 = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer2).Build();

            this.Transaction.Derive();

            var partyFinancial2 = customer2.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship2.InternalOrganisation));
            Assert.Equal(1015, partyFinancial2.SubAccountNumber);

            var customer3 = new PersonBuilder(this.Transaction).WithLastName("customer3").Build();
            var customerRelationship3 = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer3).Build();

            this.Transaction.Derive();

            var partyFinancial3 = customer3.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship3.InternalOrganisation));
            Assert.Equal(1023, partyFinancial3.SubAccountNumber);
        }

        [Fact]
        public void GivenCustomerRelationship_WhenDeriving_ThenSubAccountNumberMustBeUniqueWithinSingleton()
        {
            var customer2 = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var euro = belgium.Currency;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var address1 = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var internalOrganisation2 = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("internalOrganisation2")
                .WithSubAccountCounter(new CounterBuilder(this.Transaction).WithUniqueId(Guid.NewGuid()).WithValue(0).Build())
                .Build();

            var bank = new BankBuilder(this.Transaction).WithCountry(belgium).WithName("ING België").WithBic("BBRUBEBB").Build();

            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("BE23 3300 6167 6391")
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("BE23 3300 6167 6391").WithNameOnAccount("Koen").Build())
                .WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Transaction)
                                                .WithInternalOrganisation(internalOrganisation2)
                                                .WithGeneralLedgerAccount(new GeneralLedgerAccountBuilder(this.Transaction)
                                                                                .WithAccountNumber("1")
                                                                                .WithName("name")
                                                                                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                                                                                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("desc").Build())
                                                                                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("desc").Build())
                                                                                .Build())
                                                .Build())
                .Build();

            internalOrganisation2.DefaultCollectionMethod = ownBankAccount;

            var customerRelationship2 = new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer2)
                .WithInternalOrganisation(internalOrganisation2)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            var partyFinancial = customer2.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship2.InternalOrganisation));
            partyFinancial.SubAccountNumber = 19;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenCustomerWithUnpaidInvoices_WhenDeriving_ThenAmountDueIsUpdated()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var partyFinancial = customer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));

            var billToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Mechelen").Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            good.VatRate = new VatRateBuilder(this.Transaction).WithRate(0).Build();

            this.Transaction.Derive();

            var invoice1 = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            var invoice2 = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(200M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(300M, partyFinancial.AmountDue);

            new ReceiptBuilder(this.Transaction)
                .WithAmount(50)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice1.SalesInvoiceItems[0]).WithAmountApplied(50).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(250, partyFinancial.AmountDue);

            new ReceiptBuilder(this.Transaction)
                .WithAmount(200)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice2.SalesInvoiceItems[0]).WithAmountApplied(200).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(50, partyFinancial.AmountDue);

            new ReceiptBuilder(this.Transaction)
                .WithAmount(50)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice1.SalesInvoiceItems[0]).WithAmountApplied(50).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(0, partyFinancial.AmountDue);
        }

        [Fact]
        public void GivenCustomerWithUnpaidInvoices_WhenDeriving_ThenAmountOverDueIsUpdated()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-31)).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var partyFinancial = customer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));

            var billToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Mechelen").Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            good.VatRate = new VatRateBuilder(this.Transaction).WithRate(0).Build();

            this.Transaction.Derive();

            var invoice1 = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanism)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-30))
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            var invoice2 = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanism)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-5))
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(200M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(100M, partyFinancial.AmountOverDue);

            new ReceiptBuilder(this.Transaction)
                .WithAmount(20)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice1.SalesInvoiceItems[0]).WithAmountApplied(20).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(80, partyFinancial.AmountOverDue);

            invoice2.InvoiceDate = this.Transaction.Now().AddDays(-10);

            this.Transaction.Derive();

            Assert.Equal(280, partyFinancial.AmountOverDue);
        }
    }

    public class CustomerRelationshipDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerRelationshipDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedCustomerDeriveParties()
        {
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var customer = new PersonBuilder(this.Transaction).Build();
            customerRelationship.Customer = customer;
            this.Transaction.Derive(false);

            Assert.Contains(customer, customerRelationship.Parties);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveParties()
        {
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            customerRelationship.InternalOrganisation = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(internalOrganisation, customerRelationship.Parties);
        }
    }
}
