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

            var endCustomer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());
            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(internalOrganisation)
                .WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(otherInternalOrganization)
                .WithAssignedBillToContactMechanism(otherInternalOrganization.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault())
                .WithBillToEndCustomer(endCustomer)
                .WithAssignedBillToEndCustomerContactMechanism(endCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault())
                .WithShipToEndCustomer(endCustomer)
                .WithAssignedShipToEndCustomerAddress(endCustomer.ShippingAddress)
                .WithShipToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault())
                .WithShipToCustomer(otherInternalOrganization)
                .WithAssignedShipToAddress(otherInternalOrganization.ShippingAddress)
                .WithShipToContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault())
                .WithSalesInvoiceType(salesInvoiceType)
                .WithTotalListPrice(faker.Random.Decimal())
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesInvoiceItem(salesInvoiceItem_Default)
                .WithSalesInvoiceItem(salesInvoiceItem_Product)
                .WithSalesInvoiceItem(salesInvoiceItem_Part)
                .WithAdvancePayment(faker.Random.Decimal())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build())
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithSalesExternalB2BInvoiceDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.FirstOrDefault(v => v.GetType().Name == nameof(Organisation));

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(internalOrganisation)
                .WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(customer.ShippingAddress)
                .WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithSalesInvoiceType(salesInvoiceType)
                .WithTotalListPrice(faker.Random.Decimal())
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesInvoiceItem(salesInvoiceItem_Default)
                .WithSalesInvoiceItem(salesInvoiceItem_Product)
                .WithSalesInvoiceItem(salesInvoiceItem_Part)
                .WithAdvancePayment(faker.Random.Decimal())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build())
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithSalesExternalB2CInvoiceDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.FirstOrDefault(v => v.GetType().Name == nameof(Person));

            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(internalOrganisation)
                .WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(customer.ShippingAddress)
                .WithSalesInvoiceType(salesInvoiceType)
                .WithTotalListPrice(faker.Random.Decimal())
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesInvoiceItem(salesInvoiceItem_Default)
                .WithSalesInvoiceItem(salesInvoiceItem_Product)
                .WithSalesInvoiceItem(salesInvoiceItem_Part)
                .WithAdvancePayment(faker.Random.Decimal())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build())
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithCreditNoteDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());

            var salesInvoiceItem_Default = new SalesInvoiceItemBuilder(@this.Transaction).WithDefaults().Build();
            var salesInvoiceItem_Product = new SalesInvoiceItemBuilder(@this.Transaction).WithProductItemDefaults().Build();
            var salesInvoiceItem_Part = new SalesInvoiceItemBuilder(@this.Transaction).WithPartItemDefaults().Build();

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).CreditNote;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(internalOrganisation)
                .WithAssignedBilledFromContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(customer.CurrentContacts.FirstOrDefault())
                .WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithBillToEndCustomer(customer)
                .WithAssignedBillToEndCustomerContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToEndCustomerContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithShipToEndCustomer(customer)
                .WithAssignedShipToEndCustomerAddress(customer.ShippingAddress)
                .WithShipToEndCustomerContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(customer.ShippingAddress)
                .WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithSalesInvoiceType(salesInvoiceType)
                .WithTotalListPrice(faker.Random.Decimal())
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesInvoiceItem(salesInvoiceItem_Default)
                .WithSalesInvoiceItem(salesInvoiceItem_Product)
                .WithSalesInvoiceItem(salesInvoiceItem_Part)
                .WithAdvancePayment(faker.Random.Decimal())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build())
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }

        public static SalesInvoiceBuilder WithDefaultsWithoutItems(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = internalOrganisation.ActiveCustomers.FirstOrDefault(v => v.GetType().Name == nameof(Organisation));

            var salesInvoiceType = new SalesInvoiceTypes(@this.Transaction).SalesInvoice;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(internalOrganisation)
                .WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(customer.ShippingAddress)
                .WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithSalesInvoiceType(salesInvoiceType)
                .WithTotalListPrice(faker.Random.Decimal())
                .WithAssignedPaymentMethod(paymentMethod)
                .WithAdvancePayment(faker.Random.Decimal())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaultsForPaymentNetDays().Build())
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            return @this;
        }
    }
}
