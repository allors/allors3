// <copyright file="NonUnifiedPartBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;
    using Bogus;

    public static partial class NonUnifiedPartBuilderExtensions
    {
        public static NonUnifiedPartBuilder WithNonSerialisedDefaults(this NonUnifiedPartBuilder @this, Organisation internalOrganisation)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();

            var dutchLocale = new Locales(@this.Transaction).DutchNetherlands;
            var brand = new BrandBuilder(@this.Transaction).WithDefaults().Build();

            var nonSerialisedProductType = new ProductTypes(@this.Transaction).FindBy(m.ProductType.Name, "nonSerialisedProductType");

            if (nonSerialisedProductType == null)
            {
                var size = new SerialisedItemCharacteristicTypeBuilder(@this.Transaction)
                    .WithName("Size")
                    .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText("Afmeting").WithLocale(dutchLocale).Build())
                    .Build();

                var weight = new SerialisedItemCharacteristicTypeBuilder(@this.Transaction)
                    .WithName("Weight")
                    .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText("Gewicht").WithLocale(dutchLocale).Build())
                    .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Kilogram)
                    .Build();

                nonSerialisedProductType = new ProductTypeBuilder(@this.Transaction)
                    .WithName("nonSerialisedProductType")
                    .WithSerialisedItemCharacteristicType(size)
                    .WithSerialisedItemCharacteristicType(weight)
                    .Build();
            }

            @this.WithInventoryItemKind(new InventoryItemKinds(@this.Transaction).NonSerialised)
                .WithName(faker.Commerce.ProductName())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithKeywords(faker.Lorem.Sentence())
                .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Piece)
                .WithPrimaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithPublicElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithPrivateElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc2.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithProductIdentification(new SkuIdentificationBuilder(@this.Transaction).WithDefaults().Build())
                .WithProductIdentification(new EanIdentificationBuilder(@this.Transaction).WithDefaults().Build())
                .WithProductIdentification(new ManufacturerIdentificationBuilder(@this.Transaction).WithDefaults().Build())
                .WithDefaultFacility(internalOrganisation.FacilitiesWhereOwner?.FirstOrDefault())
                .WithProductType(nonSerialisedProductType)
                .WithBrand(brand)
                .WithModel(brand.Models.FirstOrDefault())
                .WithHsCode(faker.Random.Number(99999999).ToString())
                .WithManufacturedBy(new OrganisationBuilder(@this.Transaction).WithManufacturerDefaults(faker).Build())
                .WithReorderLevel(faker.Random.Number(99))
                .WithReorderQuantity(faker.Random.Number(999));

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Word()).WithLocale(additionalLocale).Build())
                    .WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build())
                    .WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build())
                    .WithLocalisedKeyword(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());

                var localisedDocument = new MediaBuilder(@this.Transaction).WithInFileName($"doc1.{additionalLocale.Country.IsoCode}.pdf").WithInData(faker.Random.Bytes(1000)).Build();
                @this.WithPublicLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build())
                    .WithPrivateLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build());
            }

            return @this;
        }

        public static NonUnifiedPartBuilder WithSerialisedDefaults(this NonUnifiedPartBuilder @this, Organisation internalOrganisation, Faker faker)
        {
            var m = @this.Transaction.Database.Services().M;
            var dutchLocale = new Locales(@this.Transaction).DutchNetherlands;
            var brand = new BrandBuilder(@this.Transaction).WithDefaults().Build();

            var serialisedProductType = new ProductTypes(@this.Transaction).FindBy(m.ProductType.Name, "serialisedProductType");

            if (serialisedProductType == null)
            {
                var size = new SerialisedItemCharacteristicTypeBuilder(@this.Transaction)
                    .WithName("Size")
                    .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText("Afmeting").WithLocale(dutchLocale).Build())
                    .Build();

                var weight = new SerialisedItemCharacteristicTypeBuilder(@this.Transaction)
                    .WithName("Weight")
                    .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText("Gewicht").WithLocale(dutchLocale).Build())
                    .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Kilogram)
                    .Build();

                serialisedProductType = new ProductTypeBuilder(@this.Transaction)
                    .WithName("serialisedProductType")
                    .WithSerialisedItemCharacteristicType(size)
                    .WithSerialisedItemCharacteristicType(weight)
                    .Build();
            }

            @this.WithInventoryItemKind(new InventoryItemKinds(@this.Transaction).Serialised)
                .WithName(faker.Commerce.ProductName())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithKeywords(faker.Lorem.Sentence())
                .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Piece)
                .WithPrimaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithPublicElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithPrivateElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc2.en.pdf").WithInData(faker.Random.Bytes(1000)).Build())
                .WithProductIdentification(new SkuIdentificationBuilder(@this.Transaction).WithDefaults().Build())
                .WithProductIdentification(new EanIdentificationBuilder(@this.Transaction).WithDefaults().Build())
                .WithProductIdentification(new ManufacturerIdentificationBuilder(@this.Transaction).WithDefaults().Build())
                .WithDefaultFacility(internalOrganisation.FacilitiesWhereOwner?.FirstOrDefault())
                .WithProductType(serialisedProductType)
                .WithBrand(brand)
                .WithModel(brand.Models.FirstOrDefault())
                .WithHsCode(faker.Random.Number(99999999).ToString())
                .WithManufacturedBy(new OrganisationBuilder(@this.Transaction).WithManufacturerDefaults(faker).Build());

            foreach (var additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Word()).WithLocale(additionalLocale).Build())
                    .WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build())
                    .WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build())
                    .WithLocalisedKeyword(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());

                var localisedDocument = new MediaBuilder(@this.Transaction).WithInFileName($"doc1.{additionalLocale.Country.IsoCode}.pdf").WithInData(faker.Random.Bytes(1000)).Build();
                @this.WithPublicLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build())
                    .WithPrivateLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
