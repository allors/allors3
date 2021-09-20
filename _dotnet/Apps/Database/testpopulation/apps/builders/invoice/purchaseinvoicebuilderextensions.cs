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
            var faker = @this.Transaction.Faker();

            var internalOrganisations = @this.Transaction.Extent<Organisation>();
            var otherInternalOrganization = internalOrganisations.Except(new List<Organisation> { internalOrganisation }).FirstOrDefault();
            var endCustomer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());

            var purchaseInvoiceType = faker.Random.ListItem(@this.Transaction.Extent<PurchaseInvoiceType>());

            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(internalOrganisation)
                .WithAssignedBilledFromContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBilledTo(otherInternalOrganization)
                .WithBilledToContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault())
                .WithBillToEndCustomer(endCustomer)
                .WithAssignedBillToEndCustomerContactMechanism(endCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault())
                .WithShipToEndCustomer(endCustomer)
                .WithAssignedShipToEndCustomerAddress(endCustomer.ShippingAddress)
                .WithShipToEndCustomerContactPerson(endCustomer.CurrentContacts.FirstOrDefault())
                .WithShipToCustomer(otherInternalOrganization)
                .WithAssignedShipToCustomerAddress(otherInternalOrganization.ShippingAddress)
                .WithShipToCustomerContactPerson(otherInternalOrganization.CurrentContacts.FirstOrDefault())
                .WithPurchaseInvoiceType(purchaseInvoiceType)
                .WithAssignedBillToCustomerPaymentMethod(paymentMethod);

            return @this;
        }

        public static PurchaseInvoiceBuilder WithExternalB2BInvoiceDefaults(this PurchaseInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var supplier = internalOrganisation.ActiveSuppliers.FirstOrDefault(v => v.GetType().Name == nameof(Domain.Organisation));

            var purchaseInvoiceType = faker.Random.ListItem(@this.Transaction.Extent<PurchaseInvoiceType>());

            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithCustomerReference(faker.Random.String(16).ToUpper(CultureInfo.CurrentCulture))
                .WithBilledFrom(supplier)
                .WithAssignedBilledFromContactMechanism(supplier.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBilledFromContactPerson(supplier.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBilledTo(internalOrganisation)
                .WithBilledToContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithPurchaseInvoiceType(purchaseInvoiceType)
                .WithAssignedBillToCustomerPaymentMethod(paymentMethod)
                .WithAssignedVatRegime(faker.Random.ListItem(@this.Transaction.Extent<VatRegime>()));

            return @this;
        }
    }
}
