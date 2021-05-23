
// <copyright file="DeliverableBasedServiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class DeliverableBasedServiceTests : DomainTest, IClassFixture<Fixture>
    {
        public DeliverableBasedServiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var service = new DeliverableBasedServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variant = new DeliverableBasedServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            service.AddVariant(variant);
            this.Transaction.Derive(false);

            Assert.Equal(variant.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new DeliverableBasedServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var service = new DeliverableBasedServiceBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            service.RemoveVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var service = new DeliverableBasedServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variant = new DeliverableBasedServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            service.AddVariant(variant);
            this.Transaction.Derive(false);

            Assert.Equal(variant.BasePrices.First, pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantService = new DeliverableBasedServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var service = new DeliverableBasedServiceBuilder(this.Transaction).WithVariant(variantService).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(variantService.VirtualProductPriceComponents.First, pricecomponent);
        }
    }
}
