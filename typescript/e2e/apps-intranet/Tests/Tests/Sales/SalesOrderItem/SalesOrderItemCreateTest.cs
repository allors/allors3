// <copyright file="SalesOrderItemCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.salesorder.list;
using libs.workspace.angular.apps.src.lib.objects.salesorder.overview;
using libs.workspace.angular.apps.src.lib.objects.salesorderitem.edit;

namespace Tests.SalesOrderItemTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class SalesOrderItemCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesOrderListComponent salesOrderListPage;
        private readonly Organisation internalOrganisation;

        public SalesOrderItemCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.Login();
            this.salesOrderListPage = this.Sidenav.NavigateToSalesOrders();
        }

        /**
         * MinimalWithDefaults except Product/Part Item
         **/
        [Fact]
        public void CreateWithDefaults()
        {
            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var expected = new SalesOrderItemBuilder(this.Transaction).WithDefaults().Build();
            salesOrder.AddSalesOrderItem(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistDescription);
            Assert.True(expected.ExistComment);
            Assert.True(expected.ExistInternalComment);
            Assert.True(expected.ExistInvoiceItemType);
            Assert.True(expected.ExistAssignedUnitPrice);

            var expectedDescription = expected.Description;
            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var salesOrderItemOverviewPanel = salesOrderOverview.SalesorderitemOverviewPanel.Click();

            var salesOrderItemCreate = salesOrderItemOverviewPanel
                .CreateSalesOrderItem()
                .BuildForDefaults(expected);

            this.Transaction.Rollback();
            salesOrderItemCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrderItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
        }

        /**
         * MinimalWithProductItemDefaults
         **/
        [Fact]
        public void CreateWithProductItemDefaults()
        {
            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var expected = new SalesOrderItemBuilder(this.Transaction).WithSerialisedProductDefaults().Build();
            salesOrder.AddSalesOrderItem(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistDescription);
            Assert.True(expected.ExistComment);
            Assert.True(expected.ExistInternalComment);
            Assert.True(expected.ExistInvoiceItemType);
            Assert.True(expected.ExistProduct);
            Assert.True(expected.ExistSerialisedItem);
            Assert.True(expected.ExistQuantityOrdered);
            Assert.True(expected.ExistAssignedUnitPrice);

            var expectedDescription = expected.Description;
            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedProduct = expected.Product;
            var expectedSerialisedItem = expected.SerialisedItem;
            var expectedQuantityOrdered = expected.QuantityOrdered;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var salesOrderItemOverviewPanel = salesOrderOverview.SalesorderitemOverviewPanel.Click();

            var salesOrderItemCreate = salesOrderItemOverviewPanel
                .CreateSalesOrderItem()
                .BuildForProductItemDefaults(expected);

            this.Transaction.Rollback();
            salesOrderItemCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrderItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedProduct, actual.Product);
            Assert.Equal(expectedSerialisedItem, actual.SerialisedItem);
            Assert.Equal(expectedQuantityOrdered, actual.QuantityOrdered);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
        }

        /**
         * MinimalWithPartItemDefaults
         **/
        [Fact]
        public void CreateWithPartItemDefaults()
        {
            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var expected = new SalesOrderItemBuilder(this.Transaction).WithPartItemDefaults().Build();
            salesOrder.AddSalesOrderItem(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistDescription);
            Assert.True(expected.ExistComment);
            Assert.True(expected.ExistInternalComment);
            Assert.True(expected.ExistInvoiceItemType);
            Assert.True(expected.ExistProduct);
            Assert.True(expected.ExistSerialisedItem);
            Assert.True(expected.ExistQuantityOrdered);
            Assert.True(expected.ExistAssignedUnitPrice);

            var expectedDescription = expected.Description;
            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedProduct = expected.Product;
            var expectedSerialisedItem = expected.SerialisedItem;
            var expectedQuantityOrdered = expected.QuantityOrdered;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var salesOrderItemOverviewPanel = salesOrderOverview.SalesorderitemOverviewPanel.Click();

            var salesOrderItemCreate = salesOrderItemOverviewPanel
                .CreateSalesOrderItem()
                .BuildForProductItemDefaults(expected);

            this.Transaction.Rollback();
            salesOrderItemCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrderItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedProduct, actual.Product);
            Assert.Equal(expectedSerialisedItem, actual.SerialisedItem);
            Assert.Equal(expectedQuantityOrdered, actual.QuantityOrdered);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
        }
    }
}
