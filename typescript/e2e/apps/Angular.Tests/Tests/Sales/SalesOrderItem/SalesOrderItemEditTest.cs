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
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class SalesOrderItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesOrderListComponent salesOrderListPage;
        private readonly Organisation internalOrganisation;

        public SalesOrderItemEditTest(Fixture fixture)
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
        public void EditWithDefaults()
        {
            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var disposableSalesOrder = this.internalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            var expected = disposableSalesOrder.SalesOrderItems.First(v => !(v.InvoiceItemType.IsProductItem || v.InvoiceItemType.IsPartItem));

            var salesOrderItem = salesOrder.SalesOrderItems.First(v => !(v.InvoiceItemType.IsProductItem || v.InvoiceItemType.IsPartItem));
            var id = salesOrderItem.Id;

            this.Transaction.Derive();

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

            salesOrderItemOverviewPanel.Table.DefaultAction(salesOrderItem);

            var salesOrderItemEdit = new SalesOrderItemEditComponent(this.Driver, this.M);

            salesOrderItemEdit.Description.Set(expected.Description);
            salesOrderItemEdit.Comment.Set(expected.Comment);
            salesOrderItemEdit.InternalComment.Set(expected.InternalComment);
            salesOrderItemEdit.OrderItemQuantityOrdered_1.Set(expected.QuantityOrdered.ToString());
            salesOrderItemEdit.PriceableAssignedUnitPrice_1.Set(expected.AssignedUnitPrice.ToString());

            this.Transaction.Rollback();
            salesOrderItemEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var actual = (SalesOrderItem)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedQuantityOrdered, actual.QuantityOrdered);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
        }

        /**
         * MinimalWithProductItemDefaults
         **/
        [Fact]
        public void EditWithSerialisedProductItemDefaults()
        {
            var salesOrder = new SalesOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var disposableSalesOrder = this.internalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            var expected = disposableSalesOrder.SalesOrderItems.First(v => v.InvoiceItemType.IsProductItem);

            var salesOrderItem = salesOrder.SalesOrderItems.First(v => v.InvoiceItemType.IsProductItem);
            var id = salesOrderItem.Id;

            this.Transaction.Derive();

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

            salesOrderItemOverviewPanel.Table.DefaultAction(salesOrderItem);

            var salesOrderItemEdit = new SalesOrderItemEditComponent(this.Driver, this.M);

            salesOrderItemEdit.Description.Set(expected.Description);
            salesOrderItemEdit.Comment.Set(expected.Comment);
            salesOrderItemEdit.InternalComment.Set(expected.InternalComment);
            salesOrderItemEdit.PriceableAssignedUnitPrice_2.Set(expected.AssignedUnitPrice.ToString());

            this.Transaction.Rollback();
            salesOrderItemEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrderItems(this.Transaction).Extent().ToArray();

            var actual = (SalesOrderItem)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
        }

        [Fact]
        public void EditWithNonSerialisedProductItemDefaults()
        {
        }

        [Fact]
        public void EditWithNonSerialisedPartItemDefaults()
        {
        }
    }
}
