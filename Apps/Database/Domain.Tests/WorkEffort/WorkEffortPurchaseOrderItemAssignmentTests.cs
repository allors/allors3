// <copyright file="WorkEffortPurchaseOrderItemAssignmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class WorkEffortPurchaseOrderItemAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortPurchaseOrderItemAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderItemDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            assignment.PurchaseOrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Equal(order, assignment.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderItemUnitPriceDeriveUnitPurchasePrice()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).WithPurchaseOrderItem(orderItem).Build();
            this.Transaction.Derive(false);

            orderItem.AssignedUnitPrice = 1;
            this.Transaction.Derive(false);

            Assert.Equal(1, assignment.UnitPurchasePrice);
        }

        [Fact]
        public void ChangedAssignedUnitSellingPriceDeriveUnitSellingPrice()
        {
            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            assignment.AssignedUnitSellingPrice = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, assignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedInventoryItemDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPart(part).WithPrice(11).Build();
            this.Transaction.Derive(false);

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).WithPurchaseOrderItem(orderItem).Build();
            this.Transaction.Derive(false);

            purchaseOrderItemAssignment.Assignment = workEffort;
            this.Transaction.Derive(false);

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }
    }
}
