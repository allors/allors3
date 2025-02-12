// <copyright file="SurchargeAdjustmentCreateTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.productquote.list;
using libs.workspace.angular.apps.src.lib.objects.productquote.overview;
using libs.workspace.angular.apps.src.lib.objects.purchaseinvoice.list;
using libs.workspace.angular.apps.src.lib.objects.purchaseinvoice.overview;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.overview;
using libs.workspace.angular.apps.src.lib.objects.salesorder.list;
using libs.workspace.angular.apps.src.lib.objects.salesorder.overview;

namespace Tests.OrderAdjustmentTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    
    
    
    
    
    
    
    
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class SurchargeAdjustmentCreateTests : Test, IClassFixture<Fixture>
    {
        private ProductQuoteListComponent quoteListPage;
        private SalesOrderListComponent salesOrderListPage;
        private SalesInvoiceListComponent salesInvoiceListPage;
        private PurchaseInvoiceListComponent purchaseInvoiceListPage;

        public SurchargeAdjustmentCreateTests(Fixture fixture)
            : base(fixture)
        {
            this.Login();
        }

        [Fact]
        public void CreateAmountForProductQuote()
        {
            this.quoteListPage = this.Sidenav.NavigateToProductQuotes();

            var quote = new ProductQuotes(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithAmountDefaults().Build();
            quote.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistAmount);
            Assert.True(expected.ExistDescription);

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.quoteListPage.Table.DefaultAction(quote);
            var surchargeAdjustmentCreate = new ProductQuoteOverviewComponent(this.quoteListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Amount.Set(expectedAmount.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreatePercentageForProductQuote()
        {
            this.quoteListPage = this.Sidenav.NavigateToProductQuotes();

            var quote = new ProductQuotes(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentageDefaults().Build();
            quote.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistPercentage);
            Assert.True(expected.ExistDescription);

            var expectedPercentage = expected.Percentage;
            var expectedDescription = expected.Description;

            this.quoteListPage.Table.DefaultAction(quote);
            var surchargeAdjustmentCreate = new ProductQuoteOverviewComponent(this.quoteListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Percentage.Set(expectedPercentage.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedPercentage, actual.Percentage);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreateAmountForSalesOrder()
        {
            this.salesOrderListPage = this.Sidenav.NavigateToSalesOrders();

            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithAmountDefaults().Build();
            salesOrder.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistAmount);
            Assert.True(expected.ExistDescription);

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var surchargeAdjustmentCreate = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Amount.Set(expectedAmount.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreatePercentageForSalesOrder()
        {
            this.salesOrderListPage = this.Sidenav.NavigateToSalesOrders();

            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentageDefaults().Build();
            salesOrder.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistPercentage);
            Assert.True(expected.ExistDescription);

            var expectedPercentage = expected.Percentage;
            var expectedDescription = expected.Description;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var surchargeAdjustmentCreate = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Percentage.Set(expectedPercentage.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedPercentage, actual.Percentage);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreateAmountForSalesInvoice()
        {
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();

            var salesInvoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithAmountDefaults().Build();
            salesInvoice.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistAmount);
            Assert.True(expected.ExistDescription);

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.salesInvoiceListPage.Table.DefaultAction(salesInvoice);
            var surchargeAdjustmentCreate = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Amount.Set(expectedAmount.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreatePercentageForSalesInvoice()
        {
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();

            var salesInvoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentageDefaults().Build();
            salesInvoice.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistPercentage);
            Assert.True(expected.ExistDescription);

            var expectedPercentage = expected.Percentage;
            var expectedDescription = expected.Description;

            this.salesInvoiceListPage.Table.DefaultAction(salesInvoice);
            var surchargeAdjustmentCreate = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Percentage.Set(expectedPercentage.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedPercentage, actual.Percentage);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreateAmountForPurchaseInvoice()
        {
            this.purchaseInvoiceListPage = this.Sidenav.NavigateToPurchaseInvoices();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithAmountDefaults().Build();
            purchaseInvoice.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistAmount);
            Assert.True(expected.ExistDescription);

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.purchaseInvoiceListPage.Table.DefaultAction(purchaseInvoice);
            var surchargeAdjustmentCreate = new PurchaseInvoiceOverviewComponent(this.purchaseInvoiceListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Amount.Set(expectedAmount.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void CreatePercentageForPurchaseInvoice()
        {
            this.purchaseInvoiceListPage = this.Sidenav.NavigateToPurchaseInvoices();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();

            var before = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            var expected = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentageDefaults().Build();
            purchaseInvoice.AddOrderAdjustment(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistPercentage);
            Assert.True(expected.ExistDescription);

            var expectedPercentage = expected.Percentage;
            var expectedDescription = expected.Description;

            this.purchaseInvoiceListPage.Table.DefaultAction(purchaseInvoice);
            var surchargeAdjustmentCreate = new PurchaseInvoiceOverviewComponent(this.purchaseInvoiceListPage.Driver, this.M).OrderadjustmentOverviewPanel.Click().CreateSurchargeAdjustment();

            surchargeAdjustmentCreate
                .Percentage.Set(expectedPercentage.ToString())
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            surchargeAdjustmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SurchargeAdjustments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedPercentage, actual.Percentage);
            Assert.Equal(expectedDescription, actual.Description);
        }
    }
}
