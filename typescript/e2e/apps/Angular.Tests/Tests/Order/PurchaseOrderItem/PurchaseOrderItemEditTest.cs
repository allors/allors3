// <copyright file="PurchaseOrderItemCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.purchaseorder.list;
using libs.workspace.angular.apps.src.lib.objects.purchaseorder.overview;
using libs.workspace.angular.apps.src.lib.objects.purchaseorderitem.edit;

namespace Tests.PurchaseOrderItemTests
{
    using System.Linq;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class PurchaseOrderItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PurchaseOrderListComponent purchaseOrderListPage;
        private readonly Organisation internalOrganisation;

        public PurchaseOrderItemEditTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.Login();
            this.purchaseOrderListPage = this.Sidenav.NavigateToPurchaseOrders();
        }

        /**
         * MinimalWithDefaults except Product/Part Item
         **/
        [Fact]
        public void EditWithNonSerializedPartDefaults()
        {
            var purchaseOrder = new PurchaseOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            var disposablePurchaseOrder = this.internalOrganisation.CreatePurchaseOrderWithNonSerializedItem(this.Transaction.Faker());
            var expected = disposablePurchaseOrder.PurchaseOrderItems.First(v => v.InvoiceItemType.IsPartItem);

            var purchaseOrderItem = purchaseOrder.PurchaseOrderItems.First(v => v.InvoiceItemType.IsPartItem);
            var id = purchaseOrderItem.Id;

            this.Transaction.Derive();

            var expectedDescription = expected.Description;
            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedPart = expected.Part;
            var expectedQuantityOrdered = expected.QuantityOrdered;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;
            var expectedMessage = expected.Message;

            this.purchaseOrderListPage.Table.DefaultAction(purchaseOrder);
            var purchaseOrderOverview = new PurchaseOrderOverviewComponent(this.purchaseOrderListPage.Driver, this.M);
            var purchaseOrderItemOverviewPanel = purchaseOrderOverview.PurchaseorderitemOverviewPanel.Click();

            purchaseOrderItemOverviewPanel.Table.DefaultAction(purchaseOrderItem);

            var purchaseOrderItemEdit = new PurchaseOrderItemEditComponent(this.Driver, this.M);

            purchaseOrderItemEdit.OrderItemDescription_1.Set(expected.Description);
            purchaseOrderItemEdit.Comment.Set(expected.Comment);
            purchaseOrderItemEdit.InternalComment.Set(expected.InternalComment);
            purchaseOrderItemEdit.QuantityOrdered.Set(expected.QuantityOrdered.ToString());
            purchaseOrderItemEdit.AssignedUnitPrice.Set(expected.AssignedUnitPrice.ToString());
            purchaseOrderItemEdit.Message.Set(expected.Message);

            this.Transaction.Rollback();
            purchaseOrderItemEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            var actual = (PurchaseOrderItem)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedQuantityOrdered, actual.QuantityOrdered);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
            Assert.Equal(expectedMessage, actual.Message);
        }

        /**
         * MinimalWithProductItemDefaults
         **/
        [Fact]
        public void EditWithSerialisedPartDefaults()
        {
            var purchaseOrder = new PurchaseOrders(this.Transaction).Extent().FirstOrDefault();

            var before = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            var disposablePurchaseOrder = this.internalOrganisation.CreatePurchaseOrderWithSerializedItem();
            var expected = disposablePurchaseOrder.PurchaseOrderItems.First(v => v.InvoiceItemType.IsProductItem);

            var purchaseOrderItem = purchaseOrder.PurchaseOrderItems.First(v => v.InvoiceItemType.IsProductItem);
            var id = purchaseOrderItem.Id;

            this.Transaction.Derive();

            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedPart = expected.Part;
            var expectedQuantityOrdered = expected.QuantityOrdered;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;
            var expectedMessage = expected.Message;

            this.purchaseOrderListPage.Table.DefaultAction(purchaseOrder);
            var purchaseOrderOverview = new PurchaseOrderOverviewComponent(this.purchaseOrderListPage.Driver, this.M);
            var purchaseOrderItemOverviewPanel = purchaseOrderOverview.PurchaseorderitemOverviewPanel.Click();

            purchaseOrderItemOverviewPanel.Table.DefaultAction(purchaseOrderItem);

            var purchaseOrderItemEdit = new PurchaseOrderItemEditComponent(this.Driver, this.M);

            purchaseOrderItemEdit.Comment.Set(expected.Comment);
            purchaseOrderItemEdit.InternalComment.Set(expected.InternalComment);
            purchaseOrderItemEdit.AssignedUnitPrice.Set(expected.AssignedUnitPrice.ToString());
            purchaseOrderItemEdit.Message.Set(expected.Message);

            this.Transaction.Rollback();
            purchaseOrderItemEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            var actual = (PurchaseOrderItem)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedQuantityOrdered, actual.QuantityOrdered);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
            Assert.Equal(expectedMessage, actual.Message);
        }
    }
}
