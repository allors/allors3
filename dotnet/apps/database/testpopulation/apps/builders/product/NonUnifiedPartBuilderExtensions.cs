// <copyright file="NonUnifiedPartBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
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

            @this.WithInventoryItemKind(new InventoryItemKinds(@this.Transaction).NonSerialised);
            @this.WithName(faker.Commerce.ProductName());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithKeywords(faker.Lorem.Sentence());
            @this.WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Piece);
            @this.WithPrimaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build());
            @this.WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build());
            @this.WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build());
            @this.WithPublicElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());
            @this.WithPrivateElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc2.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());
            @this.WithProductIdentification(new SkuIdentificationBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithProductIdentification(new EanIdentificationBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithProductIdentification(new ManufacturerIdentificationBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithDefaultFacility(internalOrganisation.FacilitiesWhereOwner?.First);
            @this.WithProductType(nonSerialisedProductType);
            @this.WithBrand(brand);
            @this.WithModel(brand.Models.First);
            @this.WithHsCode(faker.Random.Number(99999999).ToString());
            @this.WithManufacturedBy(new OrganisationBuilder(@this.Transaction).WithManufacturerDefaults(faker).Build());
            @this.WithReorderLevel(faker.Random.Number(99));
            @this.WithReorderQuantity(faker.Random.Number(999));

            foreach (Locale additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Word()).WithLocale(additionalLocale).Build());
                @this.WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
                @this.WithLocalisedKeyword(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());

                var localisedDocument = new MediaBuilder(@this.Transaction).WithInFileName($"doc1.{additionalLocale.Country.IsoCode}.pdf").WithInData(faker.Random.Bytes(1000)).Build();
                @this.WithPublicLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build());
                @this.WithPrivateLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build());
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

            @this.WithInventoryItemKind(new InventoryItemKinds(@this.Transaction).Serialised);
            @this.WithName(faker.Commerce.ProductName());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithKeywords(faker.Lorem.Sentence());
            @this.WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Piece);
            @this.WithPrimaryPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build());
            @this.WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build());
            @this.WithPhoto(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build());
            @this.WithPublicElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc1.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());
            @this.WithPrivateElectronicDocument(new MediaBuilder(@this.Transaction).WithInFileName("doc2.en.pdf").WithInData(faker.Random.Bytes(1000)).Build());
            @this.WithProductIdentification(new SkuIdentificationBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithProductIdentification(new EanIdentificationBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithProductIdentification(new ManufacturerIdentificationBuilder(@this.Transaction).WithDefaults().Build());
            @this.WithDefaultFacility(internalOrganisation.FacilitiesWhereOwner?.First);
            @this.WithProductType(serialisedProductType);
            @this.WithBrand(brand);
            @this.WithModel(brand.Models.First);
            @this.WithHsCode(faker.Random.Number(99999999).ToString());
            @this.WithManufacturedBy(new OrganisationBuilder(@this.Transaction).WithManufacturerDefaults(faker).Build());

            foreach (Locale additionalLocale in @this.Transaction.GetSingleton().AdditionalLocales)
            {
                @this.WithLocalisedName(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Word()).WithLocale(additionalLocale).Build());
                @this.WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
                @this.WithLocalisedComment(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());
                @this.WithLocalisedKeyword(new LocalisedTextBuilder(@this.Transaction).WithText(faker.Lorem.Sentence()).WithLocale(additionalLocale).Build());

                var localisedDocument = new MediaBuilder(@this.Transaction).WithInFileName($"doc1.{additionalLocale.Country.IsoCode}.pdf").WithInData(faker.Random.Bytes(1000)).Build();
                @this.WithPublicLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build());
                @this.WithPrivateLocalisedElectronicDocument(new LocalisedMediaBuilder(@this.Transaction).WithMedia(localisedDocument).WithLocale(additionalLocale).Build());
            }

            return @this;
        }
    }
}
