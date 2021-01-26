
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
            var service = new TimeAndMaterialsServiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            var variant = new TimeAndMaterialsServiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            service.AddVariant(variant);
            this.Session.Derive(false);

            Assert.Equal(variant.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new TimeAndMaterialsServiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var service = new TimeAndMaterialsServiceBuilder(this.Session).WithVariant(variantGood).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            service.RemoveVariant(variantGood);
            this.Session.Derive(false);

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var service = new TimeAndMaterialsServiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            var variant = new TimeAndMaterialsServiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            service.AddVariant(variant);
            this.Session.Derive(false);

            Assert.Equal(variant.BasePrices.First, pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantService = new TimeAndMaterialsServiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var service = new TimeAndMaterialsServiceBuilder(this.Session).WithVariant(variantService).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(service)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            Assert.Equal(variantService.VirtualProductPriceComponents.First, pricecomponent);
        }
    }
}
