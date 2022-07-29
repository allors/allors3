// <copyright file="BasePriceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class BasePriceTests : DomainTest, IClassFixture<Fixture>
    {
        public BasePriceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("BasePrice.Product, BasePrice.ProductFeature at least one"));
        }

        [Fact]
        public void ChangedPartThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).WithProduct(new UnifiedGoodBuilder(this.Transaction).Build()).Build();
            this.Derive();

            basePrice.RemoveProduct();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("BasePrice.Product, BasePrice.ProductFeature at least one"));
        }

        [Fact]
        public void ChangedOrderQuantityBreakThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Derive();

            basePrice.OrderQuantityBreak = new OrderQuantityBreakBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.BasePriceOrderQuantityBreakNotAllowed));
        }

        [Fact]
        public void ChangedOrderValueThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Derive();

            basePrice.OrderValue = new OrderValueBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.BasePriceOrderValueNotAllowed));
        }

        [Fact]
        public void ChangedProductDeriveProductBasePrices()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Derive();

            var product = new UnifiedGoodBuilder(this.Transaction).Build();
            basePrice.Product = product;
            this.Derive();

            Assert.Contains(basePrice, product.BasePrices);
        }

        [Fact]
        public void ChangedProductFeatureDeriveProductBasePrices()
        {
            var basePrice = new BasePriceBuilder(this.Transaction).Build();
            this.Derive();

            var productFeature = new ColourBuilder(this.Transaction).Build();
            basePrice.ProductFeature = productFeature;
            this.Derive();

            Assert.Contains(basePrice, productFeature.BasePrices);
        }
    }
}
