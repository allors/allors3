// <copyright file="WorkEffortPurchaseOrderItemAssignmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class WorkEffortPurchaseOrderItemAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortPurchaseOrderItemAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderItemDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            assignment.PurchaseOrderItem = orderItem;
            this.Derive();

            Assert.Equal(order, assignment.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderItemUnitPriceDeriveUnitPurchasePrice()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).WithPurchaseOrderItem(orderItem).Build();
            this.Derive();

            orderItem.AssignedUnitPrice = 1;
            this.Derive();

            Assert.Equal(1, assignment.UnitPurchasePrice);
        }
    }

    public class WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedUnitSellingPriceDeriveUnitSellingPrice()
        {
            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            assignment.AssignedUnitSellingPrice = 11;
            this.Derive();

            Assert.Equal(11, assignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedAssignmentDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPart(part).WithPrice(11).Build();
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).WithPurchaseOrderItem(orderItem).Build();
            this.Derive();

            purchaseOrderItemAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkeffortTakenByDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPart(part).WithPrice(11).Build();
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithTakenBy(anotherInternalOrganisation)
                .Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Derive();

            workEffort.TakenBy = this.InternalOrganisation;
            this.Derive();

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedPriceComponentPricedByDeriveUnitSellingPrice()
        {
            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var priceComponent = new BasePriceBuilder(this.Transaction)
                .WithPricedBy(anotherInternalOrganisation)
                .WithPart(part)
                .WithPrice(11)
                .Build();
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction).WithOrderedBy(this.InternalOrganisation).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Derive();

            priceComponent.PricedBy = this.InternalOrganisation;
            this.Derive();

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedPriceComponentFromDateByDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var priceComponent = new BasePriceBuilder(this.Transaction)
                .WithPart(part)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Derive();

            priceComponent.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Derive();

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedPriceComponentThroughDateByDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var priceComponent = new BasePriceBuilder(this.Transaction)
                .WithPart(part)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now().AddHours(-1))
                .WithThroughDate(this.Transaction.Now().AddHours(-1))
                .Build();
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Derive();

            priceComponent.RemoveThroughDate();
            this.Derive();

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }
    }
}
