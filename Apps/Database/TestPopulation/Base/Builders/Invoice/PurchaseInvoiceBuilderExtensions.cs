// <copyright file="PurchaseInvoiceBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public static partial class PurchaseInvoiceBuilderExtensions
    {
        public static PurchaseInvoiceBuilder WithInternalInvoiceDefaults(this PurchaseInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Session.Faker();

            var internalOrganisations = @this.Session.Extent<Organisation>();
            var otherInternalOrganization = internalOrganisations.Except(new List<Organisation> { internalOrganisation }).FirstOrDefault();
            var endCustomer = faker.Random.ListItem(internalOrganisation.ActiveCustomers);

            var purchaseInvoiceType = faker.Random.ListItem(@this.Session.Extent<PurchaseInvoiceType>());

            var paymentMethod = faker.Random.ListItem(@this.Session.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(internalOrganisation);
            @this.WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBilledTo(otherInternalOrganization);
            @this.WithBilledToContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault());
            @this.WithBillToEndCustomer(endCustomer);
            @this.WithAssignedBillToEndCustomerContactMechanism(endCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBillToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault());
            @this.WithShipToEndCustomer(endCustomer);
            @this.WithAssignedShipToEndCustomerAddress(endCustomer.ShippingAddress);
            @this.WithShipToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault());
            @this.WithShipToCustomer(otherInternalOrganization);
            @this.WithAssignedShipToCustomerAddress(otherInternalOrganization.ShippingAddress);
            @this.WithShipToCustomerContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault());
            @this.WithPurchaseInvoiceType(purchaseInvoiceType);
            @this.WithAssignedBillToCustomerPaymentMethod(paymentMethod);

            return @this;
        }

        public static PurchaseInvoiceBuilder WithExternalB2BInvoiceDefaults(this PurchaseInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Session.Faker();

            var supplier = internalOrganisation.ActiveSuppliers.Where(v => v.GetType().Name == typeof(Organisation).Name).FirstOrDefault();

            var purchaseInvoiceType = faker.Random.ListItem(@this.Session.Extent<PurchaseInvoiceType>());

            var paymentMethod = faker.Random.ListItem(@this.Session.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture));
            @this.WithBilledFrom(supplier);
            @this.WithAssignedBilledFromContactMechanism(supplier.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithBilledFromContactPerson(supplier.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithBilledTo(internalOrganisation);
            @this.WithBilledToContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithPurchaseInvoiceType(purchaseInvoiceType);
            @this.WithAssignedBillToCustomerPaymentMethod(paymentMethod);
            @this.WithAssignedVatRegime(faker.Random.ListItem(@this.Session.Extent<VatRegime>()));

            return @this;
        }
    }
}
