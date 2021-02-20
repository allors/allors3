// <copyright file="SalesInvoiceBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public static partial class SalesInvoiceBuilderExtensions
    {
        public static SalesInvoiceBuilder WithSalesInternalInvoiceDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var internalOrganisations = @this.Transaction.Extent<Organisation>();

            var otherInternalOrganization = internalOrganisations.Except(new List<Organisation> { internalOrganisation }).FirstOrDefault();

            var endCustomer = faker.Random.ListItem(internalOrganisation.ActiveCustomers);
            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(internalOrganisation);
            @this.WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBillToCustomer(otherInternalOrganization);
            @this.WithAssignedBillToContactMechanism(otherInternalOrganization.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault());
            @this.WithBillToEndCustomer(endCustomer);
            @this.WithAssignedBillToEndCustomerContactMechanism(endCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault());
            @this.WithShipToEndCustomer(endCustomer);
            @this.WithAssignedShipToEndCustomerAddress(endCustomer.ShippingAddress);
            @this.WithShipToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault());
            @this.WithShipToCustomer(otherInternalOrganization);
            @this.WithAssignedShipToAddress(otherInternalOrganization.ShippingAddress);
            @this.WithShipToContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault());
            @this.WithSalesInvoiceType(salesInvoiceType);
            @this.WithTotalListPrice(faker.Random.Decimal());
            @this.WithAssignedPaymentMethod(paymentMethod);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Default);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Product);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Part);
            @this.WithAdvancePayment(faker.Random.Decimal());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build());
            @this.WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithSalesExternalB2BInvoiceDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.Where(v => v.GetType().Name == typeof(Organisation).Name).FirstOrDefault();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(internalOrganisation);
            @this.WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBillToCustomer(customer);
            @this.WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithShipToCustomer(customer);
            @this.WithAssignedShipToAddress(customer.ShippingAddress);
            @this.WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithSalesInvoiceType(salesInvoiceType);
            @this.WithTotalListPrice(faker.Random.Decimal());
            @this.WithAssignedPaymentMethod(paymentMethod);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Default);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Product);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Part);
            @this.WithAdvancePayment(faker.Random.Decimal());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build());
            @this.WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithSalesExternalB2CInvoiceDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.Where(v => v.GetType().Name == typeof(Person).Name).FirstOrDefault();

            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(internalOrganisation);
            @this.WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBillToCustomer(customer);
            @this.WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithShipToCustomer(customer);
            @this.WithAssignedShipToAddress(customer.ShippingAddress);
            @this.WithSalesInvoiceType(salesInvoiceType);
            @this.WithTotalListPrice(faker.Random.Decimal());
            @this.WithAssignedPaymentMethod(paymentMethod);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Default);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Product);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Part);
            @this.WithAdvancePayment(faker.Random.Decimal());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build());
            @this.WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithCreditNoteDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers);

            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).CreditNote;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(internalOrganisation);
            @this.WithAssignedBilledFromContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBillToCustomer(customer.CurrentContacts.FirstOrDefault());
            @this.WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithBillToEndCustomer(customer);
            @this.WithAssignedBillToEndCustomerContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToEndCustomerContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithShipToEndCustomer(customer);
            @this.WithAssignedShipToEndCustomerAddress(customer.ShippingAddress);
            @this.WithShipToEndCustomerContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithShipToCustomer(customer);
            @this.WithAssignedShipToAddress(customer.ShippingAddress);
            @this.WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithSalesInvoiceType(salesInvoiceType);
            @this.WithTotalListPrice(faker.Random.Decimal());
            @this.WithAssignedPaymentMethod(paymentMethod);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Default);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Product);
            @this.WithSalesInvoiceItem(salesInvoiceItem_Part);
            @this.WithAdvancePayment(faker.Random.Decimal());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build());
            @this.WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithDefaultsWithoutItems(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.Where(v => v.GetType().Name == typeof(Organisation).Name).FirstOrDefault();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(internalOrganisation);
            @this.WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBillToCustomer(customer);
            @this.WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithShipToCustomer(customer);
            @this.WithAssignedShipToAddress(customer.ShippingAddress);
            @this.WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithSalesInvoiceType(salesInvoiceType);
            @this.WithTotalListPrice(faker.Random.Decimal());
            @this.WithAssignedPaymentMethod(paymentMethod);
            @this.WithAdvancePayment(faker.Random.Decimal());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build());
            @this.WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }
    }
}