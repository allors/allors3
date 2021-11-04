// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.PartyContactMachanismTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.good.list;
    using libs.workspace.angular.apps.src.lib.objects.unifiedgood.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class ProductIdentificationEditTest : Test, IClassFixture<Fixture>
    {
        private readonly GoodListComponent goods;

        public ProductIdentificationEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goods = this.Sidenav.NavigateToGoods();
        }

        [Fact]
        public void EditWithProduct()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateProductNumber();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithPart()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreatePartNumber();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithSku()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateSkuIdentification();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithUpca()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Upca)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateUpcaIdentification();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithManufacturer()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Manufacturer)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateManufacturerIdentification();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithIsbn()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Isbn)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateIsbnIdentification();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithEan()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Ean)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateEanIdentification();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }

        [Fact]
        public void EditWithUpce()
        {
            var before = new ProductIdentifications(this.Transaction).Extent().ToArray();

            var good = new UnifiedGoods(this.Transaction).Extent().FirstOrDefault();

            var expected = new PartNumberBuilder(this.Transaction)
                .WithIdentification("test")
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Upce)
                .Build();

            this.Transaction.Derive();

            var expectedIdentification = expected.Identification;
            var expectedProductIdentificationType = expected.ProductIdentificationType;

            this.goods.Table.DefaultAction(good);

            var goodDetails = new UnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var productIdentificationComponentOverviewDetail = goodDetails.ProductidentificationPanel.Click().CreateUpceIdentification();

            productIdentificationComponentOverviewDetail
                .ProductIdentificationType.Select(expectedProductIdentificationType)
                .Identification.Set(expectedIdentification);

            this.Transaction.Rollback();
            productIdentificationComponentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductIdentifications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productIdentification = after.Except(before).First();

            Assert.Equal(expectedIdentification, productIdentification.Identification);
            Assert.Equal(expectedProductIdentificationType, productIdentification.ProductIdentificationType);
        }
    }
}
