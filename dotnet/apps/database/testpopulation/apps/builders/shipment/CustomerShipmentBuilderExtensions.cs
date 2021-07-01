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

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());
            var shipmentItem = new ShipmentItemBuilder(@this.Transaction).WithSerializedUnifiedGoodDefaults(internalOrganisation).Build();

            @this.WithShipFromParty(internalOrganisation)
                .WithShipFromContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithShipToParty(customer)
                .WithShipToContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithShipmentMethod(faker.Random.ListItem(@this.Transaction.Extent<ShipmentMethod>()))
                .WithCarrier(faker.Random.ListItem(@this.Transaction.Extent<Carrier>()))
                .WithEstimatedReadyDate(@this.Transaction.Now())
                .WithEstimatedShipDate(faker.Date.Between(start: @this.Transaction.Now(), end: @this.Transaction.Now().AddDays(5)))
                .WithLatestCancelDate(faker.Date.Between(start: @this.Transaction.Now(), end: @this.Transaction.Now().AddDays(2)))
                .WithEstimatedArrivalDate(faker.Date.Between(start: @this.Transaction.Now().AddDays(6), end: @this.Transaction.Now().AddDays(10)))
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithEstimatedShipCost(faker.Finance.Amount(100, 1000, 2))
                .WithHandlingInstruction(faker.Lorem.Paragraph())
                .WithComment(faker.Lorem.Sentence())
                .WithShipmentItem(shipmentItem);

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
