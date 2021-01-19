// <copyright file="BasePriceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class BasePriceTests : DomainTest, IClassFixture<Fixture>
    {
        public BasePriceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("BasePrice.Part, BasePrice.Product, BasePrice.ProductFeature at least one"));
        }

        [Fact]
        public void ChangedPartThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Session).WithPart(new UnifiedGoodBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            basePrice.RemovePart();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("BasePrice.Part, BasePrice.Product, BasePrice.ProductFeature at least one"));
        }

        [Fact]
        public void ChangedOrderQuantityBreakThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();
            this.Session.Derive(false);

            basePrice.OrderQuantityBreak = new OrderQuantityBreakBuilder(this.Session).Build();

            var expectedMessage = $"{basePrice} { this.M.BasePrice.OrderQuantityBreak} { ErrorMessages.BasePriceOrderQuantityBreakNotAllowed}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedOrderValueThrowValidationError()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();
            this.Session.Derive(false);

            basePrice.OrderValue = new OrderValueBuilder(this.Session).Build();

            var expectedMessage = $"{basePrice} { this.M.BasePrice.OrderValue} { ErrorMessages.BasePriceOrderValueNotAllowed}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedProductDeriveProductBasePrices()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var product = new UnifiedGoodBuilder(this.Session).Build();
            basePrice.Product = product;
            this.Session.Derive(false);

            Assert.Contains(basePrice, product.BasePrices);
        }

        [Fact]
        public void ChangedProductFeatureDeriveProductBasePrices()
        {
            var basePrice = new BasePriceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var productFeature = new ColourBuilder(this.Session).Build();
            basePrice.ProductFeature = productFeature;
            this.Session.Derive(false);

            Assert.Contains(basePrice, productFeature.BasePrices);
        }
    }
}
