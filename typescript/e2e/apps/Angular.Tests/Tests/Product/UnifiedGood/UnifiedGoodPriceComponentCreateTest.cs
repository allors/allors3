// <copyright file="NonUnifiedGoodEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.unifiedgood.list;
using libs.workspace.angular.apps.src.lib.objects.unifiedgood.overview;

namespace Tests.UnifiedGood
{
    using System;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class UnifiedGoodPriceComponentCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly UnifiedGoodListComponent goods;

        public UnifiedGoodPriceComponentCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goods = this.Sidenav.NavigateToUnifiedGoods();
        }

        [Fact]
        public void Create()
        {
            var before = new BasePrices(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expectedPart = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault(v => v.QuantityOnHand > 0);

            var expected = new BasePriceBuilder(this.Transaction)
            .WithDefaults(internalOrganisation)
            .WithPart(expectedPart)
            .Build();

            this.Transaction.Derive();

            var expectedFromDate = expected.FromDate;
            var expectedThroughDate = expected.ThroughDate.Value;
            var expectedPrice = expected.Price;

            this.goods.Table.DefaultAction(expectedPart);
            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var priceComponentOverviewDetail = goodDetails.PricecomponentOverviewPanel.Click().CreateBasePrice();

            priceComponentOverviewDetail
                .FromDate.Set(expectedFromDate)
                .ThroughDate.Set(expectedThroughDate)
                .Price.Set(expectedPrice.ToString());

            this.Transaction.Rollback();
            priceComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new BasePrices(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var basePrice = after.Except(before).First();

            Assert.Equal(expectedFromDate.Date, basePrice.FromDate.Date);
            Assert.Equal(expectedThroughDate.Date, basePrice.ThroughDate.Value.Date);
            Assert.Equal(expectedPrice, basePrice.Price);
            Assert.Equal(expectedPart, basePrice.Part);
        }
    }
}
