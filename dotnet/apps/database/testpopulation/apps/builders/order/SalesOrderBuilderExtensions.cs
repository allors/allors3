// <copyright file="SalesOrderBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class SalesOrderBuilderExtensions
    {
        /**
         *
         * All invloved parties (Buyer and Seller) must be an Organisation to use
         * WithOrganisationInternalDefaults method
         *
         **/
        public static SalesOrderBuilder WithOrganisationInternalDefaults(this SalesOrderBuilder @this, Organisation sellerOrganisation)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();

            var internalOrganisations = @this.Transaction.Extent<Organisation>();
            // Organisation of type Internal Organisation
            internalOrganisations.Filter.AddEquals(m.Organisation.IsInternalOrganisation, true);

            // Filter out the sellerOrganisation
            var shipToCustomer = internalOrganisations.Except(new List<Organisation> { sellerOrganisation }).FirstOrDefault();
            var billToCustomer = shipToCustomer;
            var endCustomer = faker.Random.ListItem(shipToCustomer.ActiveCustomers.Where(v => v.GetType().Name == "Organisation").ToList());
            var endContact = endCustomer is Person endContactPerson ? endContactPerson : endCustomer.CurrentContacts.FirstOrDefault();
            var shipToContact = shipToCustomer.CurrentContacts.FirstOrDefault();
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithPartiallyShip(true)
                .WithCustomerReference(faker.Random.Words(10))
                .WithTakenBy(sellerOrganisation)
                .WithAssignedTakenByContactMechanism(sellerOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithTakenByContactPerson(sellerOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(billToCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToContactPerson(billToCustomer.CurrentContacts.FirstOrDefault())
                .WithBillToEndCustomer(endCustomer)
                .WithAssignedBillToEndCustomerContactMechanism(endCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToEndCustomerContactPerson(endContact)
                .WithShipToEndCustomer(endCustomer)
                .WithAssignedShipToEndCustomerAddress(endCustomer.ShippingAddress)
                .WithShipToEndCustomerContactPerson(endContact)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToCustomer.ShippingAddress)
                .WithAssignedShipFromAddress(sellerOrganisation.ShippingAddress)
                .WithShipToContactPerson(shipToContact)
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }

        /**
         *
         * All invloved parties (Buyer and Seller) must be an Organisation to use
         * WithOrganisationExternalDefaults method
         *
         **/
        public static SalesOrderBuilder WithOrganisationExternalDefaults(this SalesOrderBuilder @this, Organisation sellerOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var shipToCustomer = faker.Random.ListItem(sellerOrganisation.ActiveCustomers.Where(v => v.GetType().Name == "Organisation").ToList());
            var billToCustomer = shipToCustomer;
            var shipToContact = shipToCustomer is Person shipToContactPerson ? shipToContactPerson : shipToCustomer.CurrentContacts.FirstOrDefault();
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithPartiallyShip(true)
                .WithCustomerReference(faker.Random.Words(10))
                .WithTakenBy(sellerOrganisation)
                .WithAssignedTakenByContactMechanism(sellerOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithTakenByContactPerson(sellerOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(billToCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithBillToContactPerson(billToCustomer.CurrentContacts.FirstOrDefault())
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToCustomer.ShippingAddress)
                .WithAssignedShipFromAddress(sellerOrganisation.ShippingAddress)
                .WithShipToContactPerson(shipToContact)
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }

        /**
         *
         * All invloved parties (Buyer and Seller) must be an Individual to use
         * WithPersonExternalDefaults method
         *
         **/
        public static SalesOrderBuilder WithPersonExternalDefaults(this SalesOrderBuilder @this, Organisation sellerOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var shipToCustomer = faker.Random.ListItem(sellerOrganisation.ActiveCustomers.Where(v => v.GetType().Name == "Person").ToList());
            var billToCustomer = shipToCustomer;
            var paymentMethod = faker.Random.ListItem(@this.Transaction.Extent<PaymentMethod>());

            @this.WithPartiallyShip(true)
                .WithCustomerReference(faker.Random.Words(16))
                .WithTakenBy(sellerOrganisation)
                .WithAssignedTakenByContactMechanism(sellerOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithTakenByContactPerson(sellerOrganisation.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(billToCustomer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToCustomer.ShippingAddress)
                .WithAssignedShipFromAddress(sellerOrganisation.ShippingAddress)
                .WithAssignedPaymentMethod(paymentMethod)
                .WithSalesTerm(new IncoTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new InvoiceTermBuilder(@this.Transaction).WithDefaults().Build())
                .WithSalesTerm(new OrderTermBuilder(@this.Transaction).WithDefaults().Build());

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
