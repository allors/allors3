// <copyright file="ProductTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ProductTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenDeliverableBasedService_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            this.Session.Derive();
            this.Session.Commit();

            var builder = new DeliverableBasedServiceBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithVatRate(vatRate21);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("service");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenTimeAndMaterialsService_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();

            this.Session.Derive();
            this.Session.Commit();

            var builder = new TimeAndMaterialsServiceBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("good");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenGood_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var finishedGood = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var builder = new NonUnifiedGoodBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("good");
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithVatRate(vatRate21);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPart(finishedGood);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }
    }

    public class ProductDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            var variantGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddVariant(variantGood);
            this.Session.Derive(false);

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithVariant(variantGood).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            nonUnifiedGood.RemoveVariant(variantGood);
            this.Session.Derive(false);

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            var variantGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddVariant(variantGood);
            this.Session.Derive(false);

            Assert.Equal(variantGood.BasePrices.First, pricecomponent);
        }
    }
}
