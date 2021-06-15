// <copyright file="PriceComponentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Xunit;

    public class PriceComponentTests : DomainTest, IClassFixture<Fixture>
    {
        public PriceComponentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenBasePrice_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var colorFeature = new ColourBuilder(this.Transaction)
                .WithVatRegime(vatRegime)
                .WithName("black")
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new BasePriceBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithProduct(good);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithProductFeature(colorFeature);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(this.Transaction.Now());
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBasePriceForVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsUpdated()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var physicalGood = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var virtualGood = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithName("virtual gizmo")
                .WithVatRegime(vatRegime)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            physicalGood.AddVariant(virtualGood);

            this.Transaction.Derive();

            var basePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("baseprice")
                .WithPrice(10)
                .WithProduct(physicalGood)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Single(virtualGood.VirtualProductPriceComponents);
            Assert.Contains(basePrice, virtualGood.VirtualProductPriceComponents);
            Assert.False(physicalGood.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenBasePriceForNonVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsNull()
        {
            var physicalGood = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new BasePriceBuilder(this.Transaction)
                .WithDescription("baseprice")
                .WithPrice(10)
                .WithProduct(physicalGood)
                .WithFromDate(this.Transaction.Now())
                .Build();

            Assert.False(physicalGood.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenDiscount_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var colorFeature = new ColourBuilder(this.Transaction)
                .WithVatRegime(vatRegime)
                .WithName("black")
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new DiscountComponentBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(this.Transaction.Now());
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            builder.WithProduct(good);
            builder.Build();

            this.Transaction.Rollback();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            builder.WithProductFeature(colorFeature);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPercentage(10);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenDiscountForVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsUpdated()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var virtualService = new DeliverableBasedServiceBuilder(this.Transaction)
                .WithName("virtual service")
                .WithVatRegime(vatRegime)
                .Build();

            var physicalService = new DeliverableBasedServiceBuilder(this.Transaction)
                .WithName("real service")
                .WithVatRegime(vatRegime)
                .Build();

            physicalService.AddVariant(virtualService);

            this.Transaction.Derive();

            var discount = new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount")
                .WithPrice(10)
                .WithProduct(physicalService)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Single(virtualService.VirtualProductPriceComponents);
            Assert.Contains(discount, virtualService.VirtualProductPriceComponents);
            Assert.False(physicalService.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenDiscountForNonVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsNull()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var physicalService = new DeliverableBasedServiceBuilder(this.Transaction)
                .WithName("real service")
                .WithVatRegime(vatRegime)
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount")
                .WithPrice(10)
                .WithProduct(physicalService)
                .WithFromDate(this.Transaction.Now())
                .Build();

            Assert.False(physicalService.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenSurcharge_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var colorFeature = new ColourBuilder(this.Transaction)
                .WithVatRegime(vatRegime)
                .WithName("black")
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new SurchargeComponentBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(this.Transaction.Now());
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithProduct(good);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            builder.WithProductFeature(colorFeature);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPercentage(10);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSurchargeForVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsUpdated()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var virtualService = new TimeAndMaterialsServiceBuilder(this.Transaction)
                .WithName("virtual service")
                .WithVatRegime(vatRegime)
                .Build();

            var physicalService = new TimeAndMaterialsServiceBuilder(this.Transaction)
                .WithName("real service")
                .WithVatRegime(vatRegime)
                .Build();

            physicalService.AddVariant(virtualService);

            this.Transaction.Derive();

            var surcharge = new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge")
                .WithPrice(10)
                .WithProduct(physicalService)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Single(virtualService.VirtualProductPriceComponents);
            Assert.Contains(surcharge, virtualService.VirtualProductPriceComponents);
            Assert.False(physicalService.ExistVirtualProductPriceComponents);
        }

        [Fact]
        public void GivenSurchargeForNonVirtualProduct_WhenDeriving_ThenProductVirtualProductPriceComponentIsNull()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var physicalService = new TimeAndMaterialsServiceBuilder(this.Transaction)
                .WithName("real service")
                .WithVatRegime(vatRegime)
                .Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge")
                .WithPrice(10)
                .WithProduct(physicalService)
                .WithFromDate(this.Transaction.Now())
                .Build();

            Assert.False(physicalService.ExistVirtualProductPriceComponents);
        }
    }

    public class PriceComponentDerivationsTests : DomainTest, IClassFixture<Fixture>
    {
        public PriceComponentDerivationsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPriceDeriveCurrency()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            basePrice.Price = 1;
            this.Transaction.Derive(false);

            Assert.Equal(basePrice.PricedBy.PreferredCurrency, basePrice.Currency);
        }

        [Fact]
        public void ChangedPriceThrowValidationError()
        {
            this.InternalOrganisation.RemovePreferredCurrency();

            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            basePrice.Price = 1;

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorRequired>();
            Assert.Equal(new IRoleType[]
            {
                this.M.BasePrice.Currency,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnCreatedDerivePricedBy()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation, basePrice.PricedBy);
        }
    }
}


