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
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            assignment.PurchaseOrderItem = orderItem;
            this.Session.Derive(false);

            Assert.Equal(order, assignment.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderItemUnitPriceDeriveUnitPurchasePrice()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Session).WithPurchaseOrderItem(orderItem).Build();
            this.Session.Derive(false);

            orderItem.AssignedUnitPrice = 1;
            this.Session.Derive(false);

            Assert.Equal(1, assignment.UnitPurchasePrice);
        }

        [Fact]
        public void ChangedAssignedUnitSellingPriceDeriveUnitSellingPrice()
        {
            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            assignment.AssignedUnitSellingPrice = 11;
            this.Session.Derive(false);

            Assert.Equal(11, assignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedInventoryItemDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            new BasePriceBuilder(this.Session).WithPart(part).WithPrice(11).Build();
            this.Session.Derive(false);

            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Session).WithPurchaseOrderItem(orderItem).Build();
            this.Session.Derive(false);

            purchaseOrderItemAssignment.Assignment = workEffort;
            this.Session.Derive(false);

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }
    }
}
