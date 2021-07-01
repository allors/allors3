// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerialisedItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System;

    public static partial class SerialisedItemBuilderExtensions
    {
        public static SerialisedItemBuilder WithDefaults(this SerialisedItemBuilder @this, Organisation internalOrganisation)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();

            var availability = faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>());
            var serviceDate = faker.Date.Past(refDate: @this.Transaction.Now());
            var acquiredDate = faker.Date.Past(refDate: serviceDate);
            var replacementValue = Convert.ToDecimal(faker.Commerce.Price());
            var expectedSalesPrice = Convert.ToDecimal(faker.Commerce.Price(replacementValue + 1000, replacementValue + 10000));

            @this.WithName(faker.Lorem.Word())
                .WithSerialisedItemAvailability(availability)
                .WithSerialisedItemState(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemState>()))
                .WithDescription(faker.Lorem.Sentence())
                .WithKeywords(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithAcquiredDate(acquiredDate)
                .WithLastServiceDate(serviceDate)
                .WithNextServiceDate(faker.Date.Future(refDate: serviceDate))
                .WithSerialNumber(faker.Random.AlphaNumeric(12))
                .WithOwnership(faker.Random.ListItem(@this.Transaction.Extent<Ownership>()))
                .WithManufacturingYear(serviceDate.Year - 5)
                .WithAssignedPurchasePrice(Convert.ToDecimal(faker.Commerce.Price(replacementValue)))
                .WithExpectedSalesPrice(expectedSalesPrice)
                .WithPrimaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build());
            if (@this.SecondaryPhotos != null)
            {
                @this.WithSecondaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                    .WithSecondaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build());
            }
            @this.WithAdditionalPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAdditionalPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAdditionalPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithPrivatePhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAvailableForSale(faker.Random.Bool())
                .WithBuyer(internalOrganisation)
                .WithSeller(internalOrganisation);
            @this.OwnedBy = (availability.IsSold ? new Organisations(@this.Transaction).FindBy(m.Organisation.IsInternalOrganisation, false) : internalOrganisation) ?? internalOrganisation;

            if (availability.IsInRent)
            {
                @this.WithRentedBy(new Organisations(@this.Transaction).FindBy(m.Organisation.IsInternalOrganisation, false))
                    .WithRentalFromDate(faker.Date.Between(start: acquiredDate, end: acquiredDate.AddDays(10)))
                    .WithRentalThroughDate(faker.Date.Future(refDate: acquiredDate.AddYears(2)))
                    .WithExpectedReturnDate(faker.Date.Between(start: acquiredDate.AddYears(2).AddDays(1), end: acquiredDate.AddYears(2).AddDays(10)));
            }

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Word()).WithLocale(additionalLocale).Build())
                    .WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build())
                    .WithLocalisedKeyword(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }

        public static SerialisedItemBuilder WithForSaleDefaults(this SerialisedItemBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var serviceDate = faker.Date.Past(refDate: @this.Transaction.Now());
            var acquiredDate = faker.Date.Past(refDate: serviceDate);
            var replacementValue = Convert.ToDecimal(faker.Commerce.Price());
            var expectedSalesPrice = Convert.ToDecimal(faker.Commerce.Price(replacementValue + 1000, replacementValue + 10000));

            @this.WithName(faker.Lorem.Word())
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(@this.Transaction).Available)
                .WithSerialisedItemState(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemState>()))
                .WithDescription(faker.Lorem.Sentence())
                .WithKeywords(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithAcquiredDate(acquiredDate)
                .WithLastServiceDate(serviceDate)
                .WithNextServiceDate(faker.Date.Future(refDate: serviceDate))
                .WithSerialNumber(faker.Random.AlphaNumeric(12))
                .WithOwnership(faker.Random.ListItem(@this.Transaction.Extent<Ownership>()))
                .WithManufacturingYear(serviceDate.Year - 5)
                .WithAssignedPurchasePrice(Convert.ToDecimal(faker.Commerce.Price(replacementValue)))
                .WithExpectedSalesPrice(expectedSalesPrice)
                .WithPrimaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithSecondaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithSecondaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAdditionalPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAdditionalPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAdditionalPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithPrivatePhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 800, height: 600)).Build())
                .WithAvailableForSale(true)
                .WithOwnedBy(internalOrganisation);

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Word()).WithLocale(additionalLocale).Build())
                    .WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build())
                    .WithLocalisedKeyword(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
