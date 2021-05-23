// <copyright file="NonUnifiedGoodBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class NonUnifiedGoodBuilderExtensions
    {
        public static NonUnifiedGoodBuilder WithNonSerialisedDefaults(this NonUnifiedGoodBuilder @this, Organisation internalOrganisation)
        {
            var m = @this.Transaction.Database.Context().M;
            var faker = @this.Transaction.Faker();

            var dutchLocale = new Locales(@this.Transaction).DutchNetherlands;

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

            @this.WithName(faker.Commerce.ProductName());
            @this.WithPart(new NonUnifiedPartBuilder(@this.Transaction).WithNonSerialisedDefaults(internalOrganisation).Build());
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
            @this.WithVatRegime(faker.Random.ListItem(@this.Transaction.Extent<VatRegime>()));

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

        public static NonUnifiedGoodBuilder WithSerialisedDefaults(this NonUnifiedGoodBuilder @this, Organisation internalOrganisation)
        {
            var m = @this.Transaction.Database.Context().M;
            var faker = @this.Transaction.Faker();

            var dutchLocale = new Locales(@this.Transaction).DutchNetherlands;

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
                    .WithName("serialisedProductType")
                    .WithSerialisedItemCharacteristicType(size)
                    .WithSerialisedItemCharacteristicType(weight)
                    .Build();
            }

            @this.WithName(faker.Commerce.ProductName());
            @this.WithPart(new NonUnifiedPartBuilder(@this.Transaction).WithSerialisedDefaults(internalOrganisation, faker).Build());
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
            @this.WithVatRegime(faker.Random.ListItem(@this.Transaction.Extent<VatRegime>()));

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
