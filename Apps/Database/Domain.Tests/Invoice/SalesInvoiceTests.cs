// <copyright file="SalesInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using TestPopulation;
    using Resources;
    using Xunit;
    using System.Collections.Generic;
    using Database.Derivations;

    public class SalesInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSalesInvoice_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceStates(this.Transaction).ReadyForPosting, invoice.SalesInvoiceState);
            Assert.Equal(invoice.LastSalesInvoiceState, invoice.SalesInvoiceState);
        }

        [Fact]
        public void GivenSalesInvoice_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Null(invoice.PreviousSalesInvoiceState);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism billToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new SalesInvoiceBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithBillToCustomer(customer);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithAssignedBillToContactMechanism(billToContactMechanism);
            var invoice = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).ReadyForPosting);
            Assert.Equal(invoice.SalesInvoiceState, invoice.LastSalesInvoiceState);

            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDerivingWithMultipleInternalOrganisations_ThenBilledFromMustExist()
        {
            var internalOrganisation = new Organisations(this.Transaction).FindBy(this.M.Organisation.IsInternalOrganisation, true);

            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(false)
                .WithName("internalOrganisation")
                .WithPreferredCurrency(new Currencies(this.Transaction).CurrencyByCode["EUR"])
                .WithPurchaseShipmentNumberPrefix("incoming shipmentno: ")
                .WithPurchaseInvoiceNumberPrefix("incoming invoiceno: ")
                .WithPurchaseOrderNumberPrefix("purchase orderno: ")
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism billToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            this.Transaction.Commit();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanism)
                .Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            invoice.BilledFrom = this.InternalOrganisation;

            Assert.False(this.Transaction.Derive(true).HasErrors);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDeriving_ThenBillToCustomerMustBeActiveCustomer()
        {
            var customer = new OrganisationBuilder(this.Transaction)
                .WithName("customer")
                .Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var expectedError = $"{salesInvoice} {this.M.SalesInvoice.BillToCustomer} {ErrorMessages.PartyIsNotACustomer}";
            Assert.Equal(expectedError, this.Transaction.Derive(false).Errors[0].Message);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDeriving_ThenShipToCustomerMustBeActiveCustomer()
        {
            var billtoCcustomer = new OrganisationBuilder(this.Transaction).WithName("billToCustomer").Build();
            var shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("shipToCustomer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(billtoCcustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var expectedError = $"{salesInvoice} {this.M.SalesInvoice.ShipToCustomer} {ErrorMessages.PartyIsNotACustomer}";
            Assert.Contains(expectedError, this.Transaction.Derive(false).Errors.Select(v => v.Message));

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).Build();

            Assert.DoesNotContain(expectedError, this.Transaction.Derive(false).Errors.Select(v => v.Message));
        }

        [Fact]
        public void GivenSalesInvoice_WhenGettingInvoiceNumberWithoutFormat_ThenInvoiceNumberShouldBeReturned()
        {
            var store = new Stores(this.Transaction).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.RemoveSalesInvoiceNumberPrefix();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice1 = new SalesInvoiceBuilder(this.Transaction)
                .WithStore(store)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("1", invoice1.InvoiceNumber);

            var invoice2 = new SalesInvoiceBuilder(this.Transaction)
                .WithStore(store)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("2", invoice2.InvoiceNumber);
        }

        [Fact]
        public void GivenBilledFromWithoutInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.RemoveSalesInvoiceNumberPrefix();
            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            Assert.Equal(int.Parse(invoice.InvoiceNumber), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenBilledFromWithInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.SalesInvoiceNumberPrefix = "prefix-";
            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            Assert.Equal(int.Parse(invoice.InvoiceNumber.Split('-')[1]), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenBilledFromWithParametrizedInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.SalesInvoiceNumberPrefix = "prefix-{year}-";
            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var number = int.Parse(invoice.InvoiceNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Transaction.Now().Date.Year.ToString(), number)), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenSingletonWithInvoiceSequenceFiscalYear_WhenCreatingInvoice_ThenInvoiceNumberFromFiscalYearMustBeUsed()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).RestartOnFiscalYear;
            var store = new StoreBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithName("new store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .Build();
            this.Transaction.Derive();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice1 = new SalesInvoiceBuilder(this.Transaction)
                .WithStore(store)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            invoice1.Send();

            Assert.False(store.ExistSalesInvoiceNumberCounter);
            Assert.Equal(this.Transaction.Now().Year, store.FiscalYearsStoreSequenceNumbers.First.FiscalYear);
            Assert.Equal("1", invoice1.InvoiceNumber);

            var invoice2 = new SalesInvoiceBuilder(this.Transaction)
                .WithStore(store)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            invoice2.Send();

            Assert.False(store.ExistSalesInvoiceNumberCounter);
            Assert.Equal(this.Transaction.Now().Year, store.FiscalYearsStoreSequenceNumbers.First.FiscalYear);
            Assert.Equal("2", invoice2.InvoiceNumber);
        }

        [Fact]
        public void GivenSalesInvoiceSend_WhenGettingInvoiceNumberWithFormat_ThenFormattedInvoiceNumberShouldBeReturned()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            var store = new Stores(this.Transaction).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.SalesInvoiceNumberPrefix = "the format is ";
            store.SalesInvoiceTemporaryCounter = new CounterBuilder(this.Transaction).WithUniqueId(Guid.NewGuid()).WithValue(10).Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithStore(store)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            invoice.Send();

            this.Transaction.Derive();

            Assert.Equal("the format is 1", invoice.InvoiceNumber);
        }

        [Fact]
        public void GivenSalesInvoiceNotSend_WhenGettingInvoiceNumberWithFormat_ThenTemporaryInvoiceNumberShouldBeReturned()
        {
            var store = new Stores(this.Transaction).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.SalesInvoiceNumberPrefix = "the format is ";
            store.SalesInvoiceTemporaryCounter = new CounterBuilder(this.Transaction).WithUniqueId(Guid.NewGuid()).WithValue(10).Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithStore(store)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("11", invoice.InvoiceNumber);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDeriving_ThenBilledFromContactMechanismMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Equal(this.InternalOrganisation.BillingAddress, invoice.DerivedBilledFromContactMechanism);
        }

        [Fact]
        public void GivenSalesInvoiceWithBillToCustomerWithBillingAsdress_WhenDeriving_ThendBillToContactMechanismMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism billToContactMechanism = new PostalAddressBuilder(this.Transaction).WithAddress1("Haverwerf 15").WithPostalAddressBoundary(mechelen).Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(billToContactMechanism)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);
            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Equal(billingAddress.ContactMechanism, invoice.DerivedBillToContactMechanism);
        }

        [Fact]
        public void GivenSalesInvoiceBuilderWithBillToCustomerWithPreferredCurrency_WhenBuilding_ThenDerivedCurrencyIsCustomersPreferredCurrency()
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            var customer = new OrganisationBuilder(this.Transaction)
                .WithName("customer")
                .WithPreferredCurrency(euro)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var billToContactMechanismMechelen = new WebAddressBuilder(this.Transaction).WithElectronicAddressString("dummy").Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanismMechelen)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(euro, invoice.DerivedCurrency);
        }

        [Fact]
        public void GivenSalesInvoiceWithShipToCustomerWithShippingAddress_WhenDeriving_ThenShipToAddressMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism shipToContactMechanism = new PostalAddressBuilder(this.Transaction).WithAddress1("Haverwerf 15").WithPostalAddressBoundary(mechelen).Build();

            var shippingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(shipToContactMechanism)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(shippingAddress);

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(shipToContactMechanism)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Equal(shippingAddress.ContactMechanism, invoice.DerivedShipToAddress);
        }

        [Fact]
        public void GivenSalesInvoiceBuilderWithoutBillToCustomer_WhenBuilding_ThenDerivedCurrencyIsSingletonsPreferredCurrency()
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            var customer = new OrganisationBuilder(this.Transaction)
                .WithName("customer")
                .WithLocale(new Locales(this.Transaction).DutchBelgium)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(euro, invoice.DerivedCurrency);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDeriving_ThenLocaleMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice1 = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).WithAssignedBillToContactMechanism(contactMechanism).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Equal(invoice1.BilledFrom.Locale, invoice1.DerivedLocale);

            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            customer.Locale = dutchLocale;

            var invoice2 = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).WithAssignedBillToContactMechanism(contactMechanism).Build();

            this.Transaction.Derive();

            Assert.Equal(dutchLocale, invoice2.DerivedLocale);
        }

        [Fact]
        public void GivenSalesInvoice_WhenDeriving_ThenTotalAmountMustBeDerived()
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable19)
                .Build();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(productItem)
                .WithProduct(good)
                .WithQuantity(1)
                .WithAssignedUnitPrice(8)
                .Build();

            invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(8, invoice.TotalExVat);
            Assert.Equal(1.68M, invoice.TotalVat);
            Assert.Equal(9.68M, invoice.TotalIncVat);
            Assert.Equal(1.52M, invoice.TotalIrpf);
            Assert.Equal(8.16M, invoice.GrandTotal);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(productItem)
                .WithProduct(good)
                .WithQuantity(1)
                .WithAssignedUnitPrice(8)
                .Build();

            var item3 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(productItem)
                .WithProduct(good)
                .WithQuantity(1)
                .WithAssignedUnitPrice(8)
                .Build();

            invoice.AddSalesInvoiceItem(item2);
            invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            Assert.Equal(24, invoice.TotalExVat);
            Assert.Equal(5.04M, invoice.TotalVat);
            Assert.Equal(29.04M, invoice.TotalIncVat);
            Assert.Equal(invoice.TotalListPrice, invoice.TotalExVat);
        }

        [Fact]
        public void GivenSalesInvoiceWithShippingAndHandlingAmount_WhenDeriving_ThenInvoiceTotalsMustIncludeShippingAndHandlingAmount()
        {
            var adjustment = new ShippingAndHandlingChargeBuilder(this.Transaction).WithAmount(7.5M).Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(new OrganisationBuilder(this.Transaction).WithName("customer").Build())
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(15).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build();
            invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(45, invoice.TotalBasePrice);
            Assert.Equal(0, invoice.TotalDiscount);
            Assert.Equal(0, invoice.TotalSurcharge);
            Assert.Equal(7.5m, invoice.TotalShippingAndHandling);
            Assert.Equal(0, invoice.TotalFee);
            Assert.Equal(52.5m, invoice.TotalExVat);
            Assert.Equal(11.03m, invoice.TotalVat);
            Assert.Equal(63.53m, invoice.TotalIncVat);
        }

        [Fact]
        public void GivenSalesInvoiceWithShippingAndHandlingPercentage_WhenDeriving_ThenSalesInvoiceTotalsMustIncludeShippingAndHandlingAmount()
        {
            var adjustment = new ShippingAndHandlingChargeBuilder(this.Transaction).WithPercentage(5).Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(new OrganisationBuilder(this.Transaction).WithName("customer").Build())
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(15).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build();
            invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(45, invoice.TotalBasePrice);
            Assert.Equal(0, invoice.TotalDiscount);
            Assert.Equal(0, invoice.TotalSurcharge);
            Assert.Equal(2.25m, invoice.TotalShippingAndHandling);
            Assert.Equal(0, invoice.TotalFee);
            Assert.Equal(47.25m, invoice.TotalExVat);
            Assert.Equal(9.92m, invoice.TotalVat);
            Assert.Equal(57.17m, invoice.TotalIncVat);
        }

        [Fact]
        public void GivenSalesInvoiceWithFeeAmount_WhenDeriving_ThenSalesInvoiceTotalsMustIncludeFeeAmount()
        {
            var adjustment = new FeeBuilder(this.Transaction).WithAmount(7.5M).Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(new OrganisationBuilder(this.Transaction).WithName("customer").Build())
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(15).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build();
            invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(45, invoice.TotalBasePrice);
            Assert.Equal(0, invoice.TotalDiscount);
            Assert.Equal(0, invoice.TotalSurcharge);
            Assert.Equal(0, invoice.TotalShippingAndHandling);
            Assert.Equal(7.5m, invoice.TotalFee);
            Assert.Equal(52.5m, invoice.TotalExVat);
            Assert.Equal(11.03m, invoice.TotalVat);
            Assert.Equal(63.53m, invoice.TotalIncVat);
        }

        [Fact]
        public void GivenSalesInvoiceWithFeePercentage_WhenDeriving_ThenSalesInvoiceTotalsMustIncludeFeeAmount()
        {
            var adjustment = new FeeBuilder(this.Transaction).WithPercentage(5).Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(new OrganisationBuilder(this.Transaction).WithName("customer").Build())
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(15).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build();
            invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(45, invoice.TotalBasePrice);
            Assert.Equal(0, invoice.TotalDiscount);
            Assert.Equal(0, invoice.TotalSurcharge);
            Assert.Equal(0, invoice.TotalShippingAndHandling);
            Assert.Equal(2.25m, invoice.TotalFee);
            Assert.Equal(47.25m, invoice.TotalExVat);
            Assert.Equal(9.92m, invoice.TotalVat);
            Assert.Equal(57.17m, invoice.TotalIncVat);
        }

        [Fact]
        public void GivenSalesInvoice_WhenShipToAndBillToAreSameCustomer_ThenDerivedCustomersIsSingleCustomer()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithShipToCustomer(customer)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            Assert.Single(invoice.Customers);
            Assert.Equal(customer, invoice.Customers.First);
        }

        [Fact]
        public void GivenSalesInvoice_WhenShipToAndBillToAreDifferentCustomers_ThenDerivedCustomersHoldsBothCustomers()
        {
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithShipToCustomer(shipToCustomer)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.ShipToCustomer).Build();

            this.Transaction.Derive();

            Assert.Equal(2, invoice.Customers.Count);
            Assert.Contains(billToCustomer, invoice.Customers);
            Assert.Contains(shipToCustomer, invoice.Customers);
        }

        [Fact]
        public void GivenSalesInvoice_WhenPartialPaymentIsReceived_ThenInvoiceStateIsSetToPartiallyPaid()
        {
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(2).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(90)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice.SalesInvoiceItems[0]).WithAmountApplied(90).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceStates(this.Transaction).PartiallyPaid, invoice.SalesInvoiceState);
        }

        [Fact]
        public void GiveninvoiceItem_WhenFullPaymentIsReceived_ThenInvoiceItemStateIsSetToPaid()
        {
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            good.VatRegime = new VatRegimes(this.Transaction).ZeroRated;

            this.Transaction.Derive();
            this.Transaction.Commit();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            invoice.AddSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build());

            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(100)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice.InvoiceItems[0]).WithAmountApplied(100).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceStates(this.Transaction).Paid, invoice.SalesInvoiceState);
        }

        [Fact]
        public void GiveninvoiceItem_WhenCancelled_ThenInvoiceItemsAreCancelled()
        {
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(invoice.BillToCustomer).Build();

            this.Transaction.Derive();

            invoice.CancelInvoice();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceStates(this.Transaction).Cancelled, invoice.SalesInvoiceState);
            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).CancelledByInvoice, invoice.SalesInvoiceItems[0].SalesInvoiceItemState);
            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).CancelledByInvoice, invoice.SalesInvoiceItems[1].SalesInvoiceItemState);
        }

        [Fact]
        public void GiveninvoiceItem_WhenWrittenOff_ThenInvoiceItemsAreWrittenOff()
        {
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            invoice.WriteOff();
            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceStates(this.Transaction).WrittenOff, invoice.SalesInvoiceState);
            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).WrittenOff, invoice.SalesInvoiceItems[0].SalesInvoiceItemState);
            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).WrittenOff, invoice.SalesInvoiceItems[1].SalesInvoiceItemState);
        }

        [Fact]
        public void GivenSalesOrder_WhenBillngForOrderItemsAndConfirmed_ThenInvoiceIsCreated()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsAutomaticallyShipped = true;
            store.IsImmediatelyPicked = true;
            store.BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var customer = new PersonBuilder(this.Transaction).WithLastName("person1").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good1.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive(true);

            order.Accept();
            this.Transaction.Derive(true);

            order.Invoice();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
            Assert.Equal(new SalesOrderPaymentStates(this.Transaction).NotPaid, order.SalesOrderPaymentState);
        }
    }

    public class SalesInvoiceOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderState()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(order.ExistSalesOrderState);
        }

        [Fact]
        public void DeriveInvoiceDate()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(invoice.ExistInvoiceDate);
        }

        [Fact]
        public void DeriveSalesInvoiceType()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Equal(invoice.SalesInvoiceType, new SalesInvoiceTypes(this.Transaction).SalesInvoice);
        }

        [Fact]
        public void DeriveBilledFrom()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Equal(invoice.BilledFrom, this.InternalOrganisation);
        }
    }

    public class SalesInvoiceReadyForPostingDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceReadyForPostingDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedLocaleDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            this.Transaction.GetSingleton().DefaultLocale = swedishLocale;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.Locale = swedishLocale;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedBillToCustomerLocaleDeriveDerivedLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.Locale = swedishLocale;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedLocale, customer.Locale);
        }

        [Fact]
        public void ChangedBilledFromLocaleDeriveDerivedLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            this.InternalOrganisation.RemoveLocale();
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.Locale = swedishLocale;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedLocale, this.InternalOrganisation.Locale);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            invoice.AssignedCurrency = swedishKrona;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedBillToCustomerPreferredCurrencyDeriveDerivedCurrency()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();
            customer.RemovePreferredCurrency();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.PreferredCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, customer.PreferredCurrency);
        }

        [Fact]
        public void ChangedBillToCustomerLocaleDeriveDerivedCurrency()
        {
            var newLocale = new LocaleBuilder(this.Transaction)
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();
            customer.RemovePreferredCurrency();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.Locale = newLocale;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, newLocale.Country.Currency);
        }

        [Fact]
        public void ChangedBilledFromPreferredCurrencyDeriveDerivedCurrency()
        {
            this.InternalOrganisation.RemovePreferredCurrency();
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.PreferredCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, this.InternalOrganisation.PreferredCurrency);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedVatRegime, invoice.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedIrpfRegime, invoice.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedAssignedBilledFromContactMechanismDeriveDerivedBilledFromContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedBilledFromContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBilledFromContactMechanism, invoice.AssignedBilledFromContactMechanism);
        }

        [Fact]
        public void ChangedBilledFromBillingAddressDeriveDerivedBilledFromContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.BillingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBilledFromContactMechanism, this.InternalOrganisation.BillingAddress);
        }

        [Fact]
        public void ChangedBilledFromGeneralCorrespondenceDeriveDerivedBilledFromContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.RemoveBillingAddress();
            this.InternalOrganisation.GeneralCorrespondence = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBilledFromContactMechanism, this.InternalOrganisation.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedBillToContactMechanismDeriveDerivedDerivedBillToContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedBillToContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBillToContactMechanism, invoice.AssignedBillToContactMechanism);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedBillToContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            invoice.BillToCustomer = invoice.BilledFrom.ActiveCustomers.First;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBillToContactMechanism, invoice.BillToCustomer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerBillingAddressDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.BillingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBillToContactMechanism, customer.BillingAddress);
        }

        [Fact]
        public void ChangedAssignedBillToEndCustomerContactMechanismDeriveDerivedDerivedBillToEndCustomerContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedBillToEndCustomerContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBillToEndCustomerContactMechanism, invoice.AssignedBillToEndCustomerContactMechanism);
        }

        [Fact]
        public void ChangedBillToEndCustomerDeriveBillToEndCustomerContactMechanism()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            invoice.BillToEndCustomer = invoice.BilledFrom.ActiveCustomers.First;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBillToEndCustomerContactMechanism, invoice.BillToEndCustomer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerBillingAddressDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToEndCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.BillingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedBillToEndCustomerContactMechanism, customer.BillingAddress);
        }

        [Fact]
        public void ChangedAssignedShipToEndCustomerAddressDeriveDerivedDerivedShipToEndCustomerAddress()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedShipToEndCustomerAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedShipToEndCustomerAddress, invoice.AssignedShipToEndCustomerAddress);
        }

        [Fact]
        public void ChangedShipToEndCustomerDeriveShipToEndCustomerAddress()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            invoice.ShipToEndCustomer = invoice.BilledFrom.ActiveCustomers.First;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedShipToEndCustomerAddress, invoice.ShipToEndCustomer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToEndCustomerShippingAddressDeriveDerivedShipToEndCustomerAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToEndCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedShipToEndCustomerAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDerivedDerivedShipToAddress()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedShipToAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedShipToAddress, invoice.AssignedShipToAddress);
        }

        [Fact]
        public void ChangedShipToCustomerDeriveShipToAddress()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            invoice.ShipToCustomer = invoice.BilledFrom.ActiveCustomers.First;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedShipToAddress, invoice.ShipToCustomer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToCustomerShippingAddressDeriveDerivedShipToAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedShipToAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedVatRegimeDeriveDerivedVatClauseIsNull()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Null(invoice.DerivedVatClause);
        }

        [Fact]
        public void ChangedVatRegimeDeriveDerivedVatClause()
        {
            var intraCommunautair = new VatRegimes(this.Transaction).IntraCommunautair;
            var assessable9 = new VatRegimes(this.Transaction).DutchReducedTariff;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            invoice.AssignedVatRegime = assessable9;
            this.Transaction.Derive(false);

            invoice.AssignedVatRegime = intraCommunautair;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedVatClause, intraCommunautair.VatClause);
        }

        [Fact]
        public void ChangedAssignedVatClauseDeriveDerivedVatClause()
        {
            var vatClause = new VatClauses(this.Transaction).BeArt14Par2;
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            invoice.AssignedVatClause = vatClause;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedVatClause, vatClause);
        }

        [Fact]
        public void ChangedInvoiceDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates[0].ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Transaction.Derive(false);

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Transaction.Derive(false);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(newVatRate, invoice.DerivedVatRate);

            invoice.InvoiceDate = this.Transaction.Now().AddDays(1).Date;
            this.Transaction.Derive(false);

            Assert.Equal(newVatRate, invoice.DerivedVatRate);
        }
    }

    public class SalesInvoiceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBilledFromValidationError()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.BilledFrom = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var expectedError = $"{invoice} {this.M.SalesInvoice.BilledFrom} {ErrorMessages.InternalOrganisationChanged}";
            Assert.Equal(expectedError, this.Transaction.Derive(false).Errors[0].Message);
        }

        [Fact]
        public void ChangedBilledFromDeriveStore()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(invoice.Store, new Stores(this.Transaction).Extent().First);
        }

        [Fact]
        public void ChangedStoreDeriveInvoiceNumber()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            var number = new Stores(this.Transaction).Extent().First.SalesInvoiceTemporaryCounter.Value;

            this.Transaction.Derive(false);

            Assert.Equal(invoice.InvoiceNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableInvoiceNumber()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            var number = new Stores(this.Transaction).Extent().First.SalesInvoiceTemporaryCounter.Value;

            this.Transaction.Derive(false);

            Assert.Equal(invoice.SortableInvoiceNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedInvoiceTermTermValueDerivePaymentDays()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            Assert.Equal(invoice.PaymentDays, int.Parse(invoice.SalesTerms.First(v => v.TermType.UniqueId.Equals(InvoiceTermTypes.PaymentNetDaysId)).TermValue));
        }

        [Fact]
        public void ChangedInvoiceTermTermValueDeriveDueDate()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            var paymentDays = invoice.SalesTerms.First(v => v.TermType.UniqueId.Equals(InvoiceTermTypes.PaymentNetDaysId));
            var days = int.Parse(paymentDays.TermValue);

            Assert.Equal(invoice.DueDate, invoice.InvoiceDate.AddDays(days));
        }

        [Fact]
        public void DeriveCustomers()
        {
            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer1).WithShipToCustomer(customer2).Build();

            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.Customers.Count);
            Assert.Contains(customer1, invoice.Customers);
            Assert.Contains(customer2, invoice.Customers);
        }

        [Fact]
        public void ValidateBillToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.CustomerRelationshipsWhereCustomer.First.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive(false);

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.BillToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ValidateShipToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.CustomerRelationshipsWhereCustomer.First.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive(false);

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void SyncInvoiceItemSyncedInvoice()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            foreach (InvoiceItem invoiceItem in invoice.InvoiceItems)
            {
                Assert.Equal(invoiceItem.SyncedInvoice, invoice);
            }
        }

        [Fact]
        public void ChangedRepeatingSalesInvoiceNextExecutionDateDeriveIsRepeatingInvoice()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithNextExecutionDate(this.Transaction.Now().AddDays(1))
                .Build();

            this.Transaction.Derive();

            Assert.True(invoice.IsRepeatingInvoice);
        }

        [Fact]
        public void ChangedRepeatingSalesInvoiceFinalExecutionDateDeriveIsRepeatingInvoice()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var repeatingSalesInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithNextExecutionDate(this.Transaction.Now().AddDays(-1))
                .Build();

            this.Transaction.Derive();

            Assert.True(invoice.IsRepeatingInvoice);

            repeatingSalesInvoice.FinalExecutionDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.False(invoice.IsRepeatingInvoice);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveCustomers()
        {
            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer1).Build();

            this.Transaction.Derive(false);

            invoice.BillToCustomer = customer2;
            this.Transaction.Derive(false);

            Assert.Single(invoice.Customers);
            Assert.Contains(customer2, invoice.Customers);
        }

        [Fact]
        public void ChangedShipToCustomerDeriveCustomers()
        {
            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(customer1).Build();

            this.Transaction.Derive(false);

            invoice.ShipToCustomer = customer2;
            this.Transaction.Derive(false);

            Assert.Single(invoice.Customers);
            Assert.Contains(customer2, invoice.Customers);
        }

        [Fact]
        public void ChangedCustomerRelationshipFromDateValidateBillToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.BillToCustomer} { ErrorMessages.PartyIsNotACustomer}";

            customer.CustomerRelationshipsWhereCustomer.First.FromDate = invoice.InvoiceDate.AddDays(+1);
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ChangedCustomerRelationshipThroughDateValidateBillToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.BillToCustomer} { ErrorMessages.PartyIsNotACustomer}";

            customer.CustomerRelationshipsWhereCustomer.First.ThroughDate = this.Transaction.Now().AddDays(-1);
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ChangedCustomerRelationshipFromDateValidateShipToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";

            customer.CustomerRelationshipsWhereCustomer.First.FromDate = invoice.InvoiceDate.AddDays(+1);
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ChangedCustomerRelationshipThroughDateValidateShipToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";

            customer.CustomerRelationshipsWhereCustomer.First.ThroughDate = this.Transaction.Now().AddDays(-1);
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ChangedInvoiceDateValidateBillToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.BillToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Contains(expectedMessage));

            invoice.InvoiceDate = customer.CustomerRelationshipsWhereCustomer.First.FromDate.AddDays(-1);
            errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ChangedInvoiceDateValidateShipToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(customer).Build();

            var expectedMessage = $"{invoice} {this.M.SalesInvoice.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Contains(expectedMessage));

            invoice.InvoiceDate = customer.CustomerRelationshipsWhereCustomer.First.FromDate.AddDays(-1);
            errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ChangedInvoiceItemSyncInvoiceItemSyncedInvoice()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var newItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(newItem);
            this.Transaction.Derive();

            Assert.Equal(newItem.SyncedInvoice, invoice);
        }
    }

    public class SalesInvoiceBillingDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceBillingDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderItemBillingInvoiceItemDeriveSalesOrders()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var salesOrder = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            salesOrder.Invoice();
            this.Transaction.Derive();

            var invoice = salesOrder.SalesInvoicesWhereSalesOrder.First;

            Assert.Single(invoice.SalesOrders);
        }

        [Fact]
        public void OnChangedShipmentItemBillingInvoiceItemDeriveSalesOrders()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Transaction).BillingForShipmentItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            shipment.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;

            Assert.Single(invoice.SalesOrders);
        }

        [Fact]
        public void OnChangedShipmentItemBillingInvoiceItemDeriveShipments()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Transaction).BillingForShipmentItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            shipment.Invoice();
            this.Transaction.Derive();

            var invoice = shipment.SalesInvoicesWhereShipment.First;

            Assert.Single(invoice.Shipments);
        }

        [Fact]
        public void ChangedWorkEffortBillingInvoiceItemDeriveWorkEfforts()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                .WithIdentification("Part")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(1)
                .Build();

            this.Transaction.Derive();

            new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(1)
                .Build();

            workTask.Complete();
            this.Transaction.Derive();

            workTask.Invoice();
            this.Transaction.Derive();

            var invoice = workTask.SalesInvoicesWhereWorkEffort.First;

            Assert.Single(invoice.WorkEfforts);
        }

        // billing time entry is not implemented
        //[Fact]
        //public void ChangedTimeEntryBillingInvoiceItemDeriveWorkEfforts()
        //{
        //    var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
        //    new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(this.InternalOrganisation).Build();
        //    this.Transaction.Derive();

        //    var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();
        //    this.Transaction.Derive();

        //    var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
        //    var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

        //    var timeEntry = new TimeEntryBuilder(this.Transaction)
        //        .WithRateType(new RateTypes(this.Transaction).StandardRate)
        //        .WithFromDate(yesterday)
        //        .WithThroughDate(laterYesterday)
        //        .WithTimeFrequency(new TimeFrequencies(this.Transaction).Day)
        //        .Build();

        //    employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

        //    this.Transaction.Derive();

        //    workTask.Complete();
        //    this.Transaction.Derive();

        //    timeEntry.Invoice();
        //    this.Transaction.Derive();

        //    var invoice = workTask.SalesInvoicesWhereWorkEffort.First;

        //    Assert.Single(invoice.WorkEfforts);
        //}
    }

    public class SalesInvoiceStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreateDeriveSalesInvoiceState()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).ReadyForPosting);
        }

        [Fact]
        public void OnCreateDeriveAmountPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.AmountPaid, invoice.AdvancePayment);
        }

        [Fact]
        public void ChangedSalesInvoiceItemStateDeriveSalesInvoiceItemStateNotPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var newItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(newItem);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            Assert.Equal(newItem.SalesInvoiceItemState, new SalesInvoiceItemStates(this.Transaction).NotPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemAmountPaidDeriveSalesInvoiceItemStatePartiallyPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new ReceiptBuilder(this.Transaction)
                .WithAmount(partialAmount)
                .WithEffectiveDate(this.Transaction.Now())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction)
                                            .WithAmountApplied(partialAmount)
                                            .WithInvoiceItem(invoiceItem)
                                            .Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoiceItem.SalesInvoiceItemState, new SalesInvoiceItemStates(this.Transaction).PartiallyPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemAmountPaidDeriveSalesInvoiceItemStatePaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var fullAmount = invoiceItem.TotalIncVat;
            new ReceiptBuilder(this.Transaction)
                .WithAmount(fullAmount)
                .WithEffectiveDate(this.Transaction.Now())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction)
                                            .WithAmountApplied(fullAmount)
                                            .WithInvoiceItem(invoiceItem)
                                            .Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoiceItem.SalesInvoiceItemState, new SalesInvoiceItemStates(this.Transaction).Paid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSalesInvoiceItemStateDeriveSalesInvoiceItemStatePaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var invoiceItem1 = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem1);
            this.Transaction.Derive();

            var invoiceItem2 = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem2);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var partialAmount = invoiceItem1.TotalIncVat;
            new ReceiptBuilder(this.Transaction)
                .WithAmount(partialAmount)
                .WithEffectiveDate(this.Transaction.Now())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction)
                                            .WithAmountApplied(partialAmount)
                                            .WithInvoiceItem(invoiceItem1)
                                            .Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).PartiallyPaid);

            invoiceItem2.AppsWriteOff();  //user can do this by reopening the invoice
            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).Paid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSalesInvoiceItemStateDeriveSalesInvoiceStateNotPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var newItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(newItem);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).NotPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSalesInvoiceItemStateDeriveSalesInvoiceStatePartiallyPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new ReceiptBuilder(this.Transaction)
                .WithAmount(partialAmount)
                .WithEffectiveDate(this.Transaction.Now())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction)
                                            .WithAmountApplied(partialAmount)
                                            .WithInvoiceItem(invoiceItem)
                                            .Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).PartiallyPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSalesInvoiceItemStateDeriveSalesInvoiceStatePaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var fullAmount = invoiceItem.TotalIncVat;
            new ReceiptBuilder(this.Transaction)
                .WithAmount(fullAmount)
                .WithEffectiveDate(this.Transaction.Now())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction)
                                            .WithAmountApplied(fullAmount)
                                            .WithInvoiceItem(invoiceItem)
                                            .Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).Paid);
        }

        [Fact]
        public void ChangedAdvancePaymentDeriveAmountPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            invoice.AdvancePayment = this.Transaction.Faker().Random.Decimal();
            this.Transaction.Derive();

            Assert.Equal(invoice.AmountPaid, invoice.AdvancePayment);
        }

        [Fact]
        public void ChangedPaymentApplicationAmountAppliedDeriveAmountPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();
            invoice.AdvancePayment = 0M;

            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(invoice.TotalIncVat - 1)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(invoice.TotalIncVat - 1).Build())
                .Build();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(1)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(1).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.AmountPaid, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemAmountPaidDeriveAmountPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();
            invoice.AdvancePayment = 0M;

            this.Transaction.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                    .WithAmount(invoiceItem.TotalIncVat)
                    .WithEffectiveDate(this.Transaction.Now())
                    .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction)
                                                .WithAmountApplied(invoiceItem.TotalIncVat)
                                                .WithInvoice(invoice)
                                                .Build())
                    .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.AmountPaid, invoiceItem.TotalIncVat);
        }

        [Fact]
        public void ChangedPaymentApplicationAmountAppliedDeriveInvoiceStatePartiallyPaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            invoice.AdvancePayment = 0M;

            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(invoice.TotalIncVat - 1)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(invoice.TotalIncVat - 1).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).PartiallyPaid);
        }

        [Fact]
        public void ChangedPaymentApplicationAmountAppliedDeriveInvoiceStatePaid()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            invoice.AdvancePayment = 0M;

            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(invoice.TotalIncVat)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(invoice.TotalIncVat).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(invoice.SalesInvoiceState, new SalesInvoiceStates(this.Transaction).Paid);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSalesInvoiceItemStateDeriveValidInvoiceItems()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaultsWithoutItems(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var invoiceItem1 = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem1);
            this.Transaction.Derive();

            var invoiceItem2 = new SalesInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            invoice.AddSalesInvoiceItem(invoiceItem2);
            this.Transaction.Derive();

            Assert.Equal(2, invoice.ValidInvoiceItems.Count);

            invoiceItem2.AppsWriteOff();  //user can do this by reopening the invoice
            this.Transaction.Derive();

            Assert.Single(invoice.ValidInvoiceItems);
        }
    }

    public class SalesInvoicePriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoicePriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedSalesInvoiceStateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceState(new SalesInvoiceStates(this.Transaction).WrittenOff)
                .WithInvoiceDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(0, invoice.TotalIncVat);

            invoiceItem.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Transaction).ReadyForPosting;
            this.Transaction.Derive(false);

            invoice.SalesInvoiceState = new SalesInvoiceStates(this.Transaction).ReadyForPosting;
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedValidInvoiceItemsCalculatePrice()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            Assert.True(invoice.TotalIncVat > 0);
            var totalIncVatBefore = invoice.TotalIncVat;

            invoice.SalesInvoiceItems.First.AppsWriteOff();  //user can do this by reopening the invoice
            this.Transaction.Derive();

            Assert.Equal(invoice.TotalIncVat, totalIncVatBefore - invoice.SalesInvoiceItems.First.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemsCalculatePriceForProductFeature()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProductFeature(productFeature)
                .WithPrice(0.2M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            var invoiceFeatureItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceFeatureItem);
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByPriceComponentFromDateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var basePrice = new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            Assert.Equal(0, invoice.TotalIncVat);

            basePrice.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Transaction.Derive(false);

            Assert.Equal(basePrice.Price, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByDiscountComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            new DiscountComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredBySurchargeComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            new SurchargeComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemQuantityCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Quantity = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.AssignedUnitPrice = 3;
            this.Transaction.Derive(false);

            Assert.Equal(3, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemProductCalculatePrice()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product1)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product1).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Product = product2;
            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemProductFeatureCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithProductFeature(productFeature)
                .WithPrice(1.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            invoiceItem.AddProductFeature(productFeature);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedBillToCustomerCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Transaction.Derive(false);

            Assert.NotEqual(customer1, customer2);

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theBad)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer1).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoice.BillToCustomer = customer2;
            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }
    }

    [Trait("Category", "Security")]
    public class SalesInvoiceDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.SalesInvoice.ObjectType, this.M.SalesInvoice.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedSalesInvoiceStateReadyForPostingDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceStateCancelledDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.CancelInvoice();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemSalesInvoiceItemStateReadyForPostingDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemSalesInvoiceItemStateCancelledDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.CancelFromInvoice();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRepeatingSalesInvoiceSourceDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new RepeatingSalesInvoiceBuilder(this.Transaction).WithSource(invoice).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedIsRepeatingInvoiceDeriveDeletePermissionDenied()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.IsRepeatingInvoice = true;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedIsRepeatingInvoiceDeriveDeletePermissionAllowed()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithIsRepeatingInvoice(true).Build();
            this.Transaction.Derive(false);

            invoice.IsRepeatingInvoice = false;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrdersDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AddSalesOrder(new SalesOrderBuilder(this.Transaction).Build());
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceDeriveDeletePermission()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.PurchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoice.DeniedPermissions);
        }
    }

    [Trait("Category", "Security")]
    public class SalesInvoiceSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenSalesInvoice_WhenObjectStateIsReadyForPosting_ThenCheckTransitions()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];
            Assert.True(acl.CanExecute(this.M.SalesInvoice.Send));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.WriteOff));
            Assert.True(acl.CanExecute(this.M.SalesInvoice.CancelInvoice));
            Assert.True(acl.CanExecute(this.M.SalesInvoice.Delete));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.SetPaid));
        }

        [Fact]
        public void GivenSalesInvoice_WhenObjectStateIsNotPaid_ThenCheckTransitions()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            invoice.Send();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(new SalesInvoiceStates(this.Transaction).NotPaid, invoice.SalesInvoiceState);

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Send));
            Assert.True(acl.CanExecute(this.M.SalesInvoice.WriteOff));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.CancelInvoice));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Delete));
            Assert.True(acl.CanExecute(this.M.SalesInvoice.SetPaid));
        }

        ////[Fact]
        ////public void GivenSalesInvoice_WhenObjectStateIsReceived_ThenCheckTransitions()
        ////{
        ////    SecurityPopulation securityPopulation = new SecurityPopulation(this.Transaction);
        ////    securityPopulation.CoreCreateUserGroups();
        ////    securityPopulation.CoreAssignDefaultPermissions();
        ////    new Invoices(this.Transaction).Populate();
        ////    new SalesInvoices(this.Transaction).Populate();

        ////    Person administrator = new PersonBuilder(this.Transaction).WithUserName("administrator").Build();
        ////    securityPopulation.CoreAdministrators.AddMember(administrator);

        ////    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("administrator", "Forms"), new string[0]);
        ////    SalesInvoice invoice = new SalesInvoiceBuilder(this.Transaction)
        ////        .WithInvoiceStatus(new InvoiceStatusBuilder(this.Transaction).WithObjectState(new Invoices(this.Transaction).Received).Build())
        ////        .Build();

        ////    AccessControlList acl = new AccessControlList(invoice, this.Transaction.GetUser()());
        ////    Assert.False(acl.CanExecute(Invoices.ReadyForPostingId));
        ////    Assert.False(acl.CanExecute(Invoices.ApproveId));
        ////    Assert.False(acl.CanExecute(Invoices.SendId));
        ////    Assert.False(acl.CanExecute(Invoices.WriteOffId));
        ////    Assert.False(acl.CanExecute(Invoices.CancelId));
        ////}

        [Fact]
        public void GivenSalesInvoice_WhenObjectStateIsPaid_ThenCheckTransitions()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            var invoiceItem = invoice.SalesInvoiceItems[0];
            var value = invoiceItem.TotalIncVat;

            new ReceiptBuilder(this.Transaction)
                .WithAmount(value)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(value).Build())
                .Build();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];

            Assert.Equal(new SalesInvoiceStates(this.Transaction).Paid, invoice.SalesInvoiceState);
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Send));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.WriteOff));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.CancelInvoice));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Delete));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.SetPaid));
        }

        [Fact]
        public void GivenSalesInvoice_WhenObjectStateIsPartiallyPaid_ThenCheckTransitions()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(90)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice.SalesInvoiceItems[0]).WithAmountApplied(90).Build())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Send));
            Assert.True(acl.CanExecute(this.M.SalesInvoice.WriteOff));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.CancelInvoice));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Delete));
            Assert.True(acl.CanExecute(this.M.SalesInvoice.SetPaid));
        }

        [Fact]
        public void GivenSalesInvoice_WhenObjectStateIsWrittenOff_ThenCheckTransitions()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            invoice.Send();
            invoice.WriteOff();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Send));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.WriteOff));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.CancelInvoice));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Delete));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.SetPaid));
        }

        [Fact]
        public void GivenSalesInvoice_WhenObjectStateIsCancelled_ThenCheckTransitions()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            this.Transaction.Derive();

            invoice.CancelInvoice();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];
            Assert.Equal(new SalesInvoiceStates(this.Transaction).Cancelled, invoice.SalesInvoiceState);
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Send));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.WriteOff));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.CancelInvoice));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.Delete));
            Assert.False(acl.CanExecute(this.M.SalesInvoice.SetPaid));
        }
    }
}
