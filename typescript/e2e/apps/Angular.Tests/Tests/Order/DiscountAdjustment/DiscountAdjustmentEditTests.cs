// <copyright file="DiscountAdjustmentEditTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.orderadjustment.edit;
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
    public class DiscountAdjustmentEditTests : Test, IClassFixture<Fixture>
    {
        private ProductQuoteListComponent quoteListPage;
        private SalesOrderListComponent salesOrderListPage;
        private SalesInvoiceListComponent salesInvoiceListPage;
        private PurchaseInvoiceListComponent purchaseInvoiceListPage;

        public DiscountAdjustmentEditTests(Fixture fixture)
            : base(fixture)
        {
            this.Login();
        }

        [Fact]
        public void EditForProductQuote()
        {
            this.quoteListPage = this.Sidenav.NavigateToProductQuotes();

            var quote = new ProductQuotes(this.Session).Extent().FirstOrDefault();
            quote.AddOrderAdjustment(new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build());

            this.Session.Derive();
            this.Session.Commit();

            var before = new OrderAdjustments(this.Session).Extent().ToArray();

            var expected = new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build();

            var discountAdjustment = quote.OrderAdjustments.First();
            var id = discountAdjustment.Id;

            this.Session.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.quoteListPage.Table.DefaultAction(quote);
            var quoteOverview = new ProductQuoteOverviewComponent(this.quoteListPage.Driver, this.M);
            var adjustmentOverviewPanel = quoteOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(discountAdjustment);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Session.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new OrderAdjustments(this.Session).Extent().ToArray();

            var actual = (DiscountAdjustment)this.Session.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void EditForSalesOrder()
        {
            this.salesOrderListPage = this.Sidenav.NavigateToSalesOrders();

            var salesOrder = new SalesOrders(this.Session).Extent().FirstOrDefault();
            salesOrder.AddOrderAdjustment(new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build());

            this.Session.Derive();
            this.Session.Commit();

            var before = new OrderAdjustments(this.Session).Extent().ToArray();

            var expected = new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build();

            var discountAdjustment = salesOrder.OrderAdjustments.First();
            var id = discountAdjustment.Id;

            this.Session.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var adjustmentOverviewPanel = salesOrderOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(discountAdjustment);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Session.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new OrderAdjustments(this.Session).Extent().ToArray();

            var actual = (DiscountAdjustment)this.Session.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void EditForSalesInvoice()
        {
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();

            var salesInvoice = new SalesInvoices(this.Session).Extent().FirstOrDefault();
            salesInvoice.AddOrderAdjustment(new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build());

            this.Session.Derive();
            this.Session.Commit();

            var before = new OrderAdjustments(this.Session).Extent().ToArray();

            var expected = new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build();

            var discountAdjustment = salesInvoice.OrderAdjustments.First();
            var id = discountAdjustment.Id;

            this.Session.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.salesInvoiceListPage.Table.DefaultAction(salesInvoice);
            var salesInvoiceOverview = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M);
            var adjustmentOverviewPanel = salesInvoiceOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(discountAdjustment);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Session.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new OrderAdjustments(this.Session).Extent().ToArray();

            var actual = (DiscountAdjustment)this.Session.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void EditForPurchaseInvoice()
        {
            this.purchaseInvoiceListPage = this.Sidenav.NavigateToPurchaseInvoices();

            var purchaseInvoice = new PurchaseInvoices(this.Session).Extent().FirstOrDefault();
            purchaseInvoice.AddOrderAdjustment(new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build());

            this.Session.Derive();
            this.Session.Commit();

            var before = new OrderAdjustments(this.Session).Extent().ToArray();

            var expected = new DiscountAdjustmentBuilder(this.Session).WithAmountDefaults().Build();

            var discountAdjustment = purchaseInvoice.OrderAdjustments.First();
            var id = discountAdjustment.Id;

            this.Session.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.purchaseInvoiceListPage.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceOverview = new PurchaseInvoiceOverviewComponent(this.purchaseInvoiceListPage.Driver, this.M);
            var adjustmentOverviewPanel = purchaseInvoiceOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(discountAdjustment);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Session.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new OrderAdjustments(this.Session).Extent().ToArray();

            var actual = (DiscountAdjustment)this.Session.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }
    }
}
