
// <copyright file="TimeAndMaterialsServiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using TestPopulation;
    using Xunit;

    public class TimeAndMaterialsServiceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public TimeAndMaterialsServiceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var service = new TimeAndMaterialsServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variant = new TimeAndMaterialsServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            service.AddVariant(variant);
            this.Transaction.Derive(false);

            Assert.Equal(variant.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new TimeAndMaterialsServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var service = new TimeAndMaterialsServiceBuilder(this.Transaction).WithVariant(variantGood).Build();
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
            var service = new TimeAndMaterialsServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variant = new TimeAndMaterialsServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            service.AddVariant(variant);
            this.Transaction.Derive(false);

            Assert.Equal(variant.BasePrices.First, pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantService = new TimeAndMaterialsServiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var service = new TimeAndMaterialsServiceBuilder(this.Transaction).WithVariant(variantService).Build();
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
