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
    }

    public class WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivationTests(Fixture fixture) : base(fixture) { }

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
        public void ChangedAssignmentDeriveUnitSellingPrice()
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

        [Fact]
        public void ChangedWorkeffortTakenByDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPart(part).WithPrice(11).Build();
            this.Transaction.Derive(false);

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithTakenBy(anotherInternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Transaction.Derive(false);

            workEffort.TakenBy = this.InternalOrganisation;
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            var order = new PurchaseOrderBuilder(this.Transaction).WithOrderedBy(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Transaction.Derive(false);

            priceComponent.PricedBy = this.InternalOrganisation;
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Transaction.Derive(false);

            priceComponent.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithPurchaseOrderItem(orderItem)
                .Build();
            this.Transaction.Derive(false);

            priceComponent.RemoveThroughDate();
            this.Transaction.Derive(false);

            Assert.Equal(11, purchaseOrderItemAssignment.UnitSellingPrice);
        }
    }
}
