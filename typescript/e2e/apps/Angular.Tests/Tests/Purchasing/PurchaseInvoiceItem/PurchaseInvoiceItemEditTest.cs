// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.PurchaseInvoiceItemTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.purchaseinvoice.list;
    using libs.workspace.angular.apps.src.lib.objects.purchaseinvoice.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PurchaseInvoiceItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PurchaseInvoiceListComponent purchaseInvoices;

        public PurchaseInvoiceItemEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.purchaseInvoices = this.Sidenav.NavigateToPurchaseInvoices();
        }

        [Fact]
        public void EditWithProductFeature()
        {
            var before = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var expected = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;

            this.purchaseInvoices.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceItemDetails = new PurchaseInvoiceOverviewComponent(this.purchaseInvoices.Driver, this.M);
            var purchaseInvoiceItemDetail = purchaseInvoiceItemDetails.PurchaseinvoiceitemOverviewPanel.Click().CreatePurchaseInvoiceItem();

            purchaseInvoiceItemDetail
                .InvoiceItemType.Select(expectedInvoiceItemType);

            this.Transaction.Rollback();
            purchaseInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var purchaseInvoiceItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, purchaseInvoiceItem.InvoiceItemType);
        }

        [Fact]
        public void EditWithPartItem()
        {
            var before = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var expected = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithQuantity(10M)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();

            this.Transaction.Derive();

            var expectedQuantity = expected.Quantity;
            var expectedInvoiceItemType = expected.InvoiceItemType;


            this.purchaseInvoices.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceItemDetails = new PurchaseInvoiceOverviewComponent(this.purchaseInvoices.Driver, this.M);
            var purchaseInvoiceItemDetail = purchaseInvoiceItemDetails.PurchaseinvoiceitemOverviewPanel.Click().CreatePurchaseInvoiceItem();

            purchaseInvoiceItemDetail
                .InvoiceItemType.Select(expectedInvoiceItemType)
                .Quantity.Set(expectedQuantity.ToString());

            this.Transaction.Rollback();
            purchaseInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var purchaseInvoiceItem = after.Except(before).First();

            Assert.Equal(expectedQuantity, purchaseInvoiceItem.Quantity);
            Assert.Equal(expectedInvoiceItemType, purchaseInvoiceItem.InvoiceItemType);
        }

        [Fact]
        public void EditWithProduct()
        {
            var before = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var expected = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;


            this.purchaseInvoices.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceItemDetails = new PurchaseInvoiceOverviewComponent(this.purchaseInvoices.Driver, this.M);
            var purchaseInvoiceItemDetail = purchaseInvoiceItemDetails.PurchaseinvoiceitemOverviewPanel.Click().CreatePurchaseInvoiceItem();

            purchaseInvoiceItemDetail
                .InvoiceItemType.Select(expectedInvoiceItemType);

            this.Transaction.Rollback();
            purchaseInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var purchaseInvoiceItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, purchaseInvoiceItem.InvoiceItemType);
        }

        [Fact]
        public void EditWithService()
        {
            var before = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var expected = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;


            this.purchaseInvoices.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceItemDetails = new PurchaseInvoiceOverviewComponent(this.purchaseInvoices.Driver, this.M);
            var purchaseInvoiceItemDetail = purchaseInvoiceItemDetails.PurchaseinvoiceitemOverviewPanel.Click().CreatePurchaseInvoiceItem();

            purchaseInvoiceItemDetail
                .InvoiceItemType.Select(expectedInvoiceItemType);

            this.Transaction.Rollback();
            purchaseInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var purchaseInvoiceItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, purchaseInvoiceItem.InvoiceItemType);
        }

        [Fact]
        public void EditWithTime()
        {
            var before = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var expected = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;


            this.purchaseInvoices.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceItemDetails = new PurchaseInvoiceOverviewComponent(this.purchaseInvoices.Driver, this.M);
            var purchaseInvoiceItemDetail = purchaseInvoiceItemDetails.PurchaseinvoiceitemOverviewPanel.Click().CreatePurchaseInvoiceItem();

            purchaseInvoiceItemDetail
                .InvoiceItemType.Select(expectedInvoiceItemType);

            this.Transaction.Rollback();
            purchaseInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseInvoiceItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var purchaseInvoiceItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, purchaseInvoiceItem.InvoiceItemType);
        }
    }
}
