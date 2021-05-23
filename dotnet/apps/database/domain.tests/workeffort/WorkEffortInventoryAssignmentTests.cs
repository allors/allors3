// <copyright file="WorkEffortInventoryAssignmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class WorkEffortInventoryAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkEffort_WhenAddingInventoryAssignment_ThenInventoryConsumptionCreated()
        {
            // Arrange
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var reasons = new InventoryTransactionReasons(this.Transaction);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(11)
                .Build();

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Empty(workEffort.WorkEffortInventoryAssignmentsWhereAssignment);
            Assert.True(workEffort.WorkEffortState.IsCreated);

            // Re-arrange
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            // Act
            this.Transaction.Derive(true);

            // Assert
            var transactions = inventoryAssignment.InventoryItemTransactions;

            Assert.Single(transactions);
            var transaction = transactions[0];
            Assert.Equal(part, transaction.Part);
            Assert.Equal(10, transaction.Quantity);
            Assert.Equal(reasons.Consumption, transaction.Reason);

            Assert.Equal(0, part.QuantityCommittedOut);
            Assert.Equal(1, part.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenChangingPart_ThenInventoryConsumptionChange()
        {
            // Arrange
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part1 = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();
            var part2 = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part2)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part1.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            // Act
            this.Transaction.Derive(true);

            // Assert
            var transactions = inventoryAssignment.InventoryItemTransactions.ToArray();

            Assert.Single(transactions);
            Assert.Equal(part1, transactions[0].Part);
            Assert.Equal(10, transactions[0].Quantity);
            Assert.Equal(reasons.Consumption, transactions[0].Reason);

            // Re-arrange
            inventoryAssignment.InventoryItem = part2.InventoryItemsWherePart.First;

            // Act
            this.Transaction.Derive(true);

            // Assert
            var part1Transactions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Part.Equals(part1)).ToArray();
            var part2Transactions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Part.Equals(part2)).ToArray();

            Assert.Equal(0, part1Transactions.Sum(t => t.Quantity));
            Assert.Equal(10, part2Transactions.Sum(t => t.Quantity));

            Assert.Equal(10, part1.QuantityOnHand);
            Assert.Equal(0, part2.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenCancelling_ThenInventoryConsumptionCancelled()
        {
            // Arrange
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            // Act
            workEffort.Cancel();
            this.Transaction.Derive(true);

            // Assert
            var transactions = inventoryAssignment.InventoryItemTransactions;

            Assert.Equal(2, transactions.Count);

            var consumption = transactions.First(t => t.Reason.Equals(reasons.Consumption) && (t.Quantity > 0));
            var consumptionCancellation = transactions.First(t => t.Reason.Equals(reasons.Consumption) && (t.Quantity < 0));

            Assert.Equal(10, consumption.Quantity);
            Assert.Equal(-10, consumptionCancellation.Quantity);

            Assert.Equal(10, part.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenCompleting_ThenInventoryTransactionsCreated()
        {
            // Arrange
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction).WithPart(part).WithQuantity(100).WithReason(reasons.PhysicalCount).Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            // Act
            workEffort.Complete();
            this.Transaction.Derive(true);

            // Assert
            var transactions = inventoryAssignment.InventoryItemTransactions;

            Assert.Single(transactions);

            var consumption = transactions.First(t => t.Reason.Equals(reasons.Consumption));

            Assert.Equal(10, consumption.Quantity);

            Assert.Equal(90, part.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenCompletingThenCancelling_ThenInventoryTransactionsCancelled()
        {
            // Arrange
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            // Act
            workEffort.Complete();
            this.Transaction.Derive(true);

            workEffort.Cancel();
            this.Transaction.Derive(true);

            // Assert
            var transactions = inventoryAssignment.InventoryItemTransactions;
            var consumption = transactions.First(t => t.Reason.Equals(reasons.Consumption) && (t.Quantity > 0));
            var consumptionCancellation = transactions.First(t => t.Reason.Equals(reasons.Consumption) && (t.Quantity < 0));

            Assert.Equal(2, transactions.Count);

            Assert.Equal(10, consumption.Quantity);
            Assert.Equal(-10, consumptionCancellation.Quantity);

            Assert.Equal(10, part.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenChangingPartAndQuantityAndFinishing_ThenOldInventoryCancelledAndNewInventoryCreated()
        {
            // Arrange
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part1 = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();
            var part2 = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part2)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part1.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            // Act
            inventoryAssignment.InventoryItem = part2.InventoryItemsWherePart.First;
            inventoryAssignment.Quantity = 5;

            workEffort.Complete();
            this.Transaction.Derive(true);

            // Assert
            var part1Transactions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Part.Equals(part1)).ToArray();
            var part2Transactions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Part.Equals(part2)).ToArray();

            var part1Consumptions = part1Transactions.Where(t => t.Reason.Equals(reasons.Consumption));
            var part2Consumptions = part2Transactions.Where(t => t.Reason.Equals(reasons.Consumption));

            Assert.Equal(0, part1Consumptions.Sum(r => r.Quantity));
            Assert.Equal(5, part2Consumptions.Sum(c => c.Quantity));

            Assert.Equal(10, part1.QuantityOnHand);
            Assert.Equal(5, part2.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenChangingPartAndQuantityAndReopening_ThenOldInventoryCancelledAndNewInventoryCreated()
        {
            // Arrange
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part1 = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();
            var part2 = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(10)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part2)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(5)
                .Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part1.InventoryItemsWherePart.First)
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);

            workEffort.Complete();
            this.Transaction.Derive(true);

            workEffort.Reopen();
            this.Transaction.Derive(true);

            // Act
            inventoryAssignment.InventoryItem = part2.InventoryItemsWherePart.First;
            inventoryAssignment.Quantity = 4;

            this.Transaction.Derive(true);

            // Assert
            var part1Transactions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Part.Equals(part1)).ToArray();
            var part2Transactions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Part.Equals(part2)).ToArray();

            var part1Consumptions = part1Transactions.Where(t => t.Reason.Equals(reasons.Consumption));
            var part2Consumptions = part2Transactions.Where(t => t.Reason.Equals(reasons.Consumption));

            Assert.Equal(0, part1Consumptions.Sum(c => c.Quantity));
            Assert.Equal(4, part2Consumptions.Sum(r => r.Quantity));

            Assert.Equal(10, part1.QuantityOnHand);
            Assert.Equal(1, part2.QuantityOnHand);
        }

        [Fact]
        public void GivenWorkEffortWithInventoryAssignment_WhenChangingQuantity_ThenInventoryTransactionsCreated()
        {
            // Arrage
            var reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(20)
                .Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(5)
                .Build();

            // Act
            this.Transaction.Derive(true);

            // Assert
            var consumption = inventoryAssignment.InventoryItemTransactions.First(t => t.Reason.Equals(reasons.Consumption) && (t.Quantity > 0));
            Assert.Equal(5, consumption.Quantity);

            // Re-arrange
            inventoryAssignment.Quantity = 10;

            // Act
            this.Transaction.Derive(true);

            // Assert
            var consumptions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Reason.Equals(reasons.Consumption));
            Assert.Equal(10, consumptions.Sum(r => r.Quantity));

            // Re-arrange
            workEffort.Complete();

            // Act
            this.Transaction.Derive(true);

            // Assert
            consumptions = inventoryAssignment.InventoryItemTransactions.Where(t => t.Reason.Equals(reasons.Consumption));

            Assert.Equal(2, inventoryAssignment.InventoryItemTransactions.Count);

            Assert.Equal(10, consumptions.Sum(r => r.Quantity));
            Assert.Equal(10, part.QuantityOnHand);
        }
    }

    public class WorkEffortInventoryAssignmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInventoryItemCreateInventoryItemTransaction()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part1 = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            var part2 = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part2)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part1.InventoryItemsWherePart.First)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(2, part1.QuantityOnHand);
            Assert.Equal(3, part2.QuantityOnHand);

            inventoryAssignment.InventoryItem = part2.InventoryItemsWherePart.First;
            this.Transaction.Derive(false);

            Assert.Equal(3, part1.QuantityOnHand);
            Assert.Equal(2, part2.QuantityOnHand);
        }
    }

    public class WorkEffortInventoryAssignmentCostOfGoodsSoldRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentCostOfGoodsSoldRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInventoryItemDeriveCostOfGoodsSold()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part1 = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part1.InventoryItemsWherePart.First)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(20, inventoryAssignment.CostOfGoodsSold);
        }

        [Fact]
        public void ChangedQuantityDeriveCostOfGoodsSold()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.Quantity = 3;
            this.Transaction.Derive(false);

            Assert.Equal(30, inventoryAssignment.CostOfGoodsSold);
        }
    }

    public class WorkEffortInventoryAssignmentDerivedBillableQuantityRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentDerivedBillableQuantityRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedBillableQuantityDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.AssignedBillableQuantity = 1;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedQuantityDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.Quantity = 1;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }
    }

    public class WorkEffortInventoryAssignmentUnitSellingPriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentUnitSellingPriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedUnitSellingPriceDeriveUnitSellingPrice()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.AssignedUnitSellingPrice = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedInventoryItemDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPart(part).WithPrice(11).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }
    }
}
