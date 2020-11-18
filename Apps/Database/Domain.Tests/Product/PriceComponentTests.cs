// <copyright file="PriceComponentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PriceComponentTests : DomainTest, IClassFixture<Fixture>
    {
        public PriceComponentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenBasePrice_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            var colorFeature = new ColourBuilder(this.Session)
                .WithVatRate(vatRate21)
                .WithName("black")
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var builder = new BasePriceBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithProduct(good);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithProductFeature(colorFeature);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFromDate(this.Session.Now());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceForVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsUpdated()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var virtualGood = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithName("virtual gizmo")
                .WithVatRate(vatRate21)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            var physicalGood = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            virtualGood.AddVariant(physicalGood);

            this.Session.Derive();

            var basePrice = new BasePriceBuilder(this.Session)
                .WithDescription("baseprice")
                .WithPrice(10)
                .WithProduct(virtualGood)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Single(physicalGood.VirtualProductPriceComponents);
            Assert.Contains(basePrice, physicalGood.VirtualProductPriceComponents);
            Assert.False(virtualGood.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenBasePriceForNonVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsNull()
        {
            var physicalGood = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            new BasePriceBuilder(this.Session)
                .WithDescription("baseprice")
                .WithPrice(10)
                .WithProduct(physicalGood)
                .WithFromDate(this.Session.Now())
                .Build();

            Assert.False(physicalGood.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenDiscount_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            var colorFeature = new ColourBuilder(this.Session)
             .WithVatRate(vatRate21)
             .WithName("black")
             .Build();

            this.Session.Derive();
            this.Session.Commit();

            var builder = new DiscountComponentBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFromDate(this.Session.Now());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithProduct(good);
            builder.Build();

            this.Session.Rollback();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithProductFeature(colorFeature);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPercentage(10);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenDiscountForVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsUpdated()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var virtualService = new DeliverableBasedServiceBuilder(this.Session)
                .WithName("virtual service")
                .WithVatRate(vatRate21)
                .Build();

            var physicalService = new DeliverableBasedServiceBuilder(this.Session)
                .WithName("real service")
                .WithVatRate(vatRate21)
                .Build();

            virtualService.AddVariant(physicalService);

            this.Session.Derive();

            var discount = new DiscountComponentBuilder(this.Session)
                .WithDescription("discount")
                .WithPrice(10)
                .WithProduct(virtualService)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Single(physicalService.VirtualProductPriceComponents);
            Assert.Contains(discount, physicalService.VirtualProductPriceComponents);
            Assert.False(virtualService.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenDiscountForNonVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsNull()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var physicalService = new DeliverableBasedServiceBuilder(this.Session)
                .WithName("real service")
                .WithVatRate(vatRate21)
                .Build();

            new DiscountComponentBuilder(this.Session)
                .WithDescription("discount")
                .WithPrice(10)
                .WithProduct(physicalService)
                .WithFromDate(this.Session.Now())
                .Build();

            Assert.False(physicalService.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenSurcharge_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            var colorFeature = new ColourBuilder(this.Session)
                .WithVatRate(vatRate21)
                .WithName("black")
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var builder = new SurchargeComponentBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFromDate(this.Session.Now());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithProduct(good);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithProductFeature(colorFeature);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPercentage(10);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSurchargeForVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsUpdated()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var virtualService = new TimeAndMaterialsServiceBuilder(this.Session)
                .WithName("virtual service")
                .WithVatRate(vatRate21)
                .Build();

            var physicalService = new TimeAndMaterialsServiceBuilder(this.Session)
                .WithName("real service")
                .WithVatRate(vatRate21)
                .Build();

            virtualService.AddVariant(physicalService);

            this.Session.Derive();

            var surcharge = new SurchargeComponentBuilder(this.Session)
                .WithDescription("surcharge")
                .WithPrice(10)
                .WithProduct(virtualService)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Single(physicalService.VirtualProductPriceComponents);
            Assert.Contains(surcharge, physicalService.VirtualProductPriceComponents);
            Assert.False(virtualService.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenSurchargeForNonVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsNull()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var physicalService = new TimeAndMaterialsServiceBuilder(this.Session)
                .WithName("real service")
                .WithVatRate(vatRate21)
                .Build();

            new SurchargeComponentBuilder(this.Session)
                .WithDescription("surcharge")
                .WithPrice(10)
                .WithProduct(physicalService)
                .WithFromDate(this.Session.Now())
                .Build();

            Assert.False(physicalService.ExistVirtualProductPriceComponents);
        }
    }

    public class PriceComponentDerivationsTests : DomainTest, IClassFixture<Fixture>
    {
        public PriceComponentDerivationsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenBasePriceWhenDerivingThenRequiredRelationsMustExist()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(basePrice.PricedBy, new Organisations(this.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray().First());
        }

        [Fact]
        public void GivenBasePriceWhenDerivingeWithPriceByThenPricedByEqualOrganisation()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();

            var basePrice = new BasePriceBuilder(this.Session).WithPricedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Equal(basePrice.PricedBy, organisation);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingeWithTwoInternalOrganisationsThenPricedByMustNull()
        {
            var organisation1 = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var organisation2 = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var organisation3 = new OrganisationBuilder(this.Session).Build();

            var basePrice = new BasePriceBuilder(this.Session).WithPricedBy(organisation3).Build();
            this.Session.Derive(false);

            Assert.NotEqual(basePrice.PricedBy, organisation1);
            Assert.NotEqual(basePrice.PricedBy, organisation2);
            Assert.Equal(basePrice.PricedBy, organisation3);
            Assert.True(new Organisations(this.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray().Length > 1);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingThenHasErrors()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithPartThenHasErrors()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithName("Part").Build();
            var basePrice = new BasePriceBuilder(this.Session).WithPart(part).Build();

            Assert.Equal(basePrice.Part, part);
            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithProductThenHasErrors()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithName("Product").Build();
            var basePrice = new BasePriceBuilder(this.Session).WithProduct(product).Build();

            Assert.Equal(basePrice.Product, product);
            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithProductFeatureThenHasErrors()
        {
            var Colour = new ColourBuilder(this.Session).WithName("Colour").Build();
            var basePrice = new BasePriceBuilder(this.Session).WithProductFeature(Colour).Build();

            Assert.Equal(basePrice.ProductFeature, Colour);
            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithPartWithProductWithProductFeatureThenHasNoErrors()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithName("part").Build();
            var product = new UnifiedGoodBuilder(this.Session).WithName("product").Build();
            var Colour = new ColourBuilder(this.Session).WithName("Colour").Build();

            var basePrice = new BasePriceBuilder(this.Session).WithPart(part).WithProduct(product).WithProductFeature(Colour).Build();

            Assert.Equal(basePrice.Part, part);
            Assert.Equal(basePrice.Product, product);
            Assert.Equal(basePrice.ProductFeature, Colour);
            Assert.False(this.Session.Derive(true).HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithPartWithProductWithProductFeatureThenHasErrors()
        {
            var Colour = new ColourBuilder(this.Session).WithName("Colour").Build();
            var orderQuantityBreak = new OrderQuantityBreakBuilder(this.Session).Build();

            var basePrice = new BasePriceBuilder(this.Session).WithOrderQuantityBreak(orderQuantityBreak).WithProductFeature(Colour).Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }


        [Fact]
        public void GivenBasePriceWhenDerivingWithPartWithProductWithOrderValueThenHasErrors()
        {
            var Colour = new ColourBuilder(this.Session).WithName("Colour").Build();
            var orderValue = new OrderValueBuilder(this.Session).Build();

            var basePrice = new BasePriceBuilder(this.Session).WithOrderValue(orderValue).WithProductFeature(Colour).Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithPriceWithCurrencyThenCurrenyMustExist()
        {
            var Colour = new ColourBuilder(this.Session).WithName("Colour").Build();
            var belgium = new Countries(this.Session).CountryByIsoCode["BE"];
            var euro = belgium.Currency;

            var basePrice = new BasePriceBuilder(this.Session)
                .WithCurrency(euro)
                .WithPrice(1)
                .WithProductFeature(Colour)
                .Build();

            Assert.Equal(1, basePrice.Price);
            Assert.Equal(euro, basePrice.Currency);
            Assert.False(this.Session.Derive().HasErrors);
        }

        [Fact]
        public void GivenBasePriceWhenDerivingWithPriceWithPricedByThenCurrenyMustExist()
        {
            var Colour = new ColourBuilder(this.Session).WithName("Colour").Build();
            var belgium = new Countries(this.Session).CountryByIsoCode["BE"];
            var euro = belgium.Currency;
            var organisation = new OrganisationBuilder(this.Session).WithPreferredCurrency(euro).Build();

            var basePrice = new BasePriceBuilder(this.Session)
                .WithPricedBy(organisation)
                .WithPrice(1)
                .WithProductFeature(Colour)
                .Build();

            var validation = this.Session.Derive(false);

            Assert.Equal(1, basePrice.Price);
            Assert.Equal(organisation.PreferredCurrency, basePrice.Currency);
            Assert.False(validation.HasErrors);
        }
    }
}


