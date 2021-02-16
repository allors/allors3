// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomerShipmentBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class CustomerShipmentBuilderExtensions
    {
        public static CustomerShipmentBuilder WithDefaults(this CustomerShipmentBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers);
            var shipmentItem = new ShipmentItemBuilder(@this.Transaction).WithSerializedUnifiedGoodDefaults(internalOrganisation).Build();

            @this.WithShipFromParty(internalOrganisation);
            @this.WithShipFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault());
            @this.WithShipToParty(customer);
            @this.WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithShipmentMethod(faker.Random.ListItem(@this.Transaction.Extent<ShipmentMethod>()));
            @this.WithCarrier(faker.Random.ListItem(@this.Transaction.Extent<Carrier>()));
            @this.WithEstimatedReadyDate(@this.Transaction.Now());
            @this.WithEstimatedShipDate(faker.Date.Between(start: @this.Transaction.Now(), end: @this.Transaction.Now().AddDays(5)));
            @this.WithLatestCancelDate(faker.Date.Between(start: @this.Transaction.Now(), end: @this.Transaction.Now().AddDays(2)));
            @this.WithEstimatedArrivalDate(faker.Date.Between(start: @this.Transaction.Now().AddDays(6), end: @this.Transaction.Now().AddDays(10)));

            @this.WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());
            @this.WithEstimatedShipCost(faker.Finance.Amount(100, 1000, 2));
            @this.WithHandlingInstruction(faker.Lorem.Paragraph());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithShipmentItem(shipmentItem);

            foreach (Locale additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
