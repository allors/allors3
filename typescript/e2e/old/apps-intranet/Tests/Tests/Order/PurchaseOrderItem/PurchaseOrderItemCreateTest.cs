// <copyright file="PurchaseOrderItemCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.purchaseorder.list;
using libs.workspace.angular.apps.src.lib.objects.purchaseorder.overview;

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
    public class PurchaseOrderItemCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PurchaseOrderListComponent purchaseOrderListPage;
        private readonly PurchaseOrder purchaseOrder;
        private readonly PurchaseOrderItem nonSerializedPartItem;
        private readonly PurchaseOrderItem serializedPartItem;
        private readonly Organisation internalOrganisation;

        public PurchaseOrderItemCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.purchaseOrder = this.internalOrganisation.CreatePurchaseOrderWithBothItems(this.Transaction.Faker());
            this.nonSerializedPartItem = this.purchaseOrder.PurchaseOrderItems.First(v => !v.ExistSerialisedItem);
            this.serializedPartItem = this.purchaseOrder.PurchaseOrderItems.First(v => v.ExistSerialisedItem);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.purchaseOrderListPage = this.Sidenav.NavigateToPurchaseOrders();
        }

        /**
         * MinimalWithDefaults except Product/Part Item
         **/
        [Fact]
        public void CreateWithNonSerializedPartDefaults()
        {
            var before = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            var expected = new PurchaseOrderItemBuilder(this.Transaction).WithNonSerializedPartDefaults(this.nonSerializedPartItem.Part, this.internalOrganisation).Build();
            this.purchaseOrder.AddPurchaseOrderItem(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistDescription);
            Assert.True(expected.ExistComment);
            Assert.True(expected.ExistInternalComment);
            Assert.True(expected.ExistInvoiceItemType);
            Assert.True(expected.ExistPart);
            Assert.True(expected.ExistQuantityOrdered);
            Assert.True(expected.ExistAssignedUnitPrice);
            Assert.True(expected.ExistMessage);

            var expectedDescription = expected.Description;
            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedPart = expected.Part;
            var expectedQuantityOrdered = expected.QuantityOrdered;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;
            var expectedMessage = expected.Message;

            this.purchaseOrderListPage.Table.DefaultAction(this.purchaseOrder);
            var purchaseOrderOverview = new PurchaseOrderOverviewComponent(this.purchaseOrderListPage.Driver, this.M);
            var purchaseOrderItemOverviewPanel = purchaseOrderOverview.PurchaseorderitemOverviewPanel.Click();

            var purchaseOrderItemCreate = purchaseOrderItemOverviewPanel.CreatePurchaseOrderItem();

            purchaseOrderItemCreate.InvoiceItemType.Select(expected.InvoiceItemType);
            purchaseOrderItemCreate.PurchaseOrderItemPart_2.Select(expected.Part.Name);
            purchaseOrderItemCreate.OrderItemDescription_1.Set(expected.Description);
            purchaseOrderItemCreate.Comment.Set(expected.Comment);
            purchaseOrderItemCreate.InternalComment.Set(expected.InternalComment);
            purchaseOrderItemCreate.QuantityOrdered.Set(expected.QuantityOrdered.ToString());
            purchaseOrderItemCreate.AssignedUnitPrice.Set(expected.AssignedUnitPrice.ToString());
            purchaseOrderItemCreate.Message.Set(expected.Message);

            this.Transaction.Rollback();
            purchaseOrderItemCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedPart, actual.Part);
            Assert.Equal(expectedQuantityOrdered, actual.QuantityOrdered);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
            Assert.Equal(expectedMessage, actual.Message);
        }

        /**
         * MinimalWithProductItemDefaults
         **/
        [Fact]
        public void CreateWithSerializedPartDefaults()
        {
            var before = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            var expected = new PurchaseOrderItemBuilder(this.Transaction).WithSerializedPartDefaults(this.serializedPartItem.Part, this.serializedPartItem.SerialisedItem, this.internalOrganisation).Build();
            this.purchaseOrder.AddPurchaseOrderItem(expected);

            this.Transaction.Derive();

            Assert.True(expected.ExistComment);
            Assert.True(expected.ExistInternalComment);
            Assert.True(expected.ExistInvoiceItemType);
            Assert.True(expected.ExistPart);
            Assert.True(expected.ExistSerialisedItem);
            Assert.True(expected.ExistAssignedUnitPrice);
            Assert.True(expected.ExistMessage);

            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;
            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedPart = expected.Part;
            var expectedSerialisedItem = expected.SerialisedItem;
            var expectedAssignedUnitPrice = expected.AssignedUnitPrice;
            var expectedMessage = expected.Message;

            this.purchaseOrderListPage.Table.DefaultAction(this.purchaseOrder);
            var purchaseOrderOverview = new PurchaseOrderOverviewComponent(this.purchaseOrderListPage.Driver, this.M);
            var purchaseOrderItemOverviewPanel = purchaseOrderOverview.PurchaseorderitemOverviewPanel.Click();

            var purchaseOrderItemCreate = purchaseOrderItemOverviewPanel.CreatePurchaseOrderItem();
            purchaseOrderItemCreate.InvoiceItemType.Select(expected.InvoiceItemType);
            purchaseOrderItemCreate.Comment.Set(expected.Comment);
            purchaseOrderItemCreate.InternalComment.Set(expected.InternalComment);
            purchaseOrderItemCreate.PurchaseOrderItemPart_1.Select(expected.Part.Name);
            purchaseOrderItemCreate.PurchaseOrderItemSerialisedItem_1.Select(expected.SerialisedItem);
            purchaseOrderItemCreate.AssignedUnitPrice.Set(expected.AssignedUnitPrice.ToString());
            purchaseOrderItemCreate.Message.Set(expected.Message);

            this.Transaction.Rollback();
            purchaseOrderItemCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseOrderItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedComment, actual.Comment);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
            Assert.Equal(expectedInvoiceItemType, actual.InvoiceItemType);
            Assert.Equal(expectedPart, actual.Part);
            Assert.Equal(expectedSerialisedItem, actual.SerialisedItem);
            Assert.Equal(expectedAssignedUnitPrice, actual.AssignedUnitPrice);
            Assert.Equal(expectedMessage, actual.Message);
        }
    }
}
