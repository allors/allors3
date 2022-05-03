// <copyright file="MiscellaneousChargeEditTests.cs" company="Allors bvba">
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
    public class MiscellaneousChargeEditTests : Test, IClassFixture<Fixture>
    {
        private ProductQuoteListComponent quoteListPage;
        private SalesOrderListComponent salesOrderListPage;
        private SalesInvoiceListComponent salesInvoiceListPage;
        private PurchaseInvoiceListComponent purchaseInvoiceListPage;

        public MiscellaneousChargeEditTests(Fixture fixture)
            : base(fixture)
        {
            this.Login();
        }

        [Fact]
        public void EditForProductQuote()
        {
            this.quoteListPage = this.Sidenav.NavigateToProductQuotes();

            var quote = new ProductQuotes(this.Transaction).Extent().FirstOrDefault();
            quote.AddOrderAdjustment(new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var expected = new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build();

            var miscellaneousCharge = quote.OrderAdjustments.First();
            var id = miscellaneousCharge.Id;

            this.Transaction.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.quoteListPage.Table.DefaultAction(quote);
            var quoteOverview = new ProductQuoteOverviewComponent(this.quoteListPage.Driver, this.M);
            var adjustmentOverviewPanel = quoteOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(miscellaneousCharge);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Transaction.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var actual = (MiscellaneousCharge)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void EditForSalesOrder()
        {
            this.salesOrderListPage = this.Sidenav.NavigateToSalesOrders();

            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();
            salesOrder.AddOrderAdjustment(new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var expected = new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build();

            var miscellaneousCharge = salesOrder.OrderAdjustments.First();
            var id = miscellaneousCharge.Id;

            this.Transaction.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var adjustmentOverviewPanel = salesOrderOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(miscellaneousCharge);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Transaction.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var actual = (MiscellaneousCharge)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void EditForSalesInvoice()
        {
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();

            var salesInvoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();
            salesInvoice.AddOrderAdjustment(new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var expected = new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build();

            var miscellaneousCharge = salesInvoice.OrderAdjustments.First();
            var id = miscellaneousCharge.Id;

            this.Transaction.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.salesInvoiceListPage.Table.DefaultAction(salesInvoice);
            var salesInvoiceOverview = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M);
            var adjustmentOverviewPanel = salesInvoiceOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(miscellaneousCharge);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Transaction.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var actual = (MiscellaneousCharge)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }

        [Fact]
        public void EditForPurchaseInvoice()
        {
            this.purchaseInvoiceListPage = this.Sidenav.NavigateToPurchaseInvoices();

            var purchaseInvoice = new PurchaseInvoices(this.Transaction).Extent().FirstOrDefault();
            purchaseInvoice.AddOrderAdjustment(new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var expected = new MiscellaneousChargeBuilder(this.Transaction).WithAmountDefaults().Build();

            var miscellaneousCharge = purchaseInvoice.OrderAdjustments.First();
            var id = miscellaneousCharge.Id;

            this.Transaction.Derive();

            var expectedAmount = expected.Amount;
            var expectedDescription = expected.Description;

            this.purchaseInvoiceListPage.Table.DefaultAction(purchaseInvoice);
            var purchaseInvoiceOverview = new PurchaseInvoiceOverviewComponent(this.purchaseInvoiceListPage.Driver, this.M);
            var adjustmentOverviewPanel = purchaseInvoiceOverview.OrderadjustmentOverviewPanel.Click();

            adjustmentOverviewPanel.Table.DefaultAction(miscellaneousCharge);

            var adjustmentEdit = new OrderAdjustmentEditComponent(this.Driver, this.M);

            adjustmentEdit.Amount.Set(expected.Amount.ToString());
            adjustmentEdit.Description.Set(expected.Description);

            this.Transaction.Rollback();
            adjustmentEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrderAdjustments(this.Transaction).Extent().ToArray();

            var actual = (MiscellaneousCharge)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedAmount, actual.Amount);
            Assert.Equal(expectedDescription, actual.Description);
        }
    }
}
