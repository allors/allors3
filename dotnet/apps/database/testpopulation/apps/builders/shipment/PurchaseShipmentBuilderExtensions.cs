// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PurchaseShipmentBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class PurchaseShipmentBuilderExtensions
    {
        public static PurchaseShipmentBuilder WithDefaults(this PurchaseShipmentBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var supplier = faker.Random.ListItem(internalOrganisation.ActiveSuppliers.ToArray());

            @this.WithShipFromParty(supplier)
                .WithShipFromContactPerson(supplier.CurrentContacts.FirstOrDefault())
                .WithShipToParty(internalOrganisation)
                .WithShipToContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithShipmentMethod(faker.Random.ListItem(@this.Transaction.Extent<ShipmentMethod>()))
                .WithCarrier(faker.Random.ListItem(@this.Transaction.Extent<Carrier>()))
                .WithEstimatedShipDate(faker.Date.Between(start: @this.Transaction.Now(), end: @this.Transaction.Now().AddDays(5)))
                .WithEstimatedArrivalDate(faker.Date.Between(start: @this.Transaction.Now().AddDays(6), end: @this.Transaction.Now().AddDays(10)))
                .WithElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithEstimatedShipCost(faker.Finance.Amount(100, 1000, 2))
                .WithComment(faker.Lorem.Sentence());

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
