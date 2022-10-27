// <copyright file="InventoryItemTransactionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors.Database.Domain.TestPopulation;
    using Resources;
    using Xunit;

    public class InventoryItemTransactionOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveTransactionDate()
        {
            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(inventoryItemTransaction.ExistTransactionDate);
        }
    }

    public class InventoryItemTransactionOnInitTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionOnInitTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DerivePart()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            Assert.Equal(part, inventoryItemTransaction.Part);
        }

        [Fact]
        public void DeriveFacility()
        {
            var facility = new FacilityBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithDefaultFacility(facility).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithPart(part).Build();
            this.Derive();

            Assert.Equal(facility, inventoryItemTransaction.Facility);
        }

        [Fact]
        public void DeriveUnitOfMeasure()
        {
            var uom = new UnitsOfMeasure(this.Transaction).Meter;
            var part = new NonUnifiedPartBuilder(this.Transaction).WithUnitOfMeasure(uom).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithPart(part).Build();
            this.Derive();

            Assert.Equal(uom, inventoryItemTransaction.UnitOfMeasure);
        }

        [Fact]
        public void DeriveSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Theft)
                .WithSerialisedItem(serialisedItem)
                .Build();
            this.Derive();

            Assert.Equal(new InventoryTransactionReasons(this.Transaction).Theft.DefaultSerialisedInventoryItemState, inventoryItemTransaction.SerialisedInventoryItemState);
        }

        [Fact]
        public void DeriveNonSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Theft)
                .WithPart(part)
                .Build();
            this.Derive();

            Assert.Equal(new InventoryTransactionReasons(this.Transaction).Theft.DefaultNonSerialisedInventoryItemState, inventoryItemTransaction.NonSerialisedInventoryItemState);
        }

        [Fact]
        public void SyncInventoryItemSerialised()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithFacility(new FacilityBuilder(this.Transaction).Build())
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(part.InventoryItemsWherePart.FirstOrDefault(), inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void SyncInventoryItemNonSerialised()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithFacility(new FacilityBuilder(this.Transaction).Build())
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(part.InventoryItemsWherePart.FirstOrDefault(), inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void DeriveInventoryItemSerialisedMatch()
        {
            var facility = new FacilityBuilder(this.Transaction).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(facility)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                .Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).OutgoingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.True(part.InventoryItemsWherePart.Count() == 1);
            Assert.Equal(part.InventoryItemsWherePart.FirstOrDefault(), inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void DeriveInventoryItemNonSerialisedMatch()
        {
            var facility = new FacilityBuilder(this.Transaction).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(facility)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).OutgoingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.True(part.InventoryItemsWherePart.Count() == 1);
            Assert.Equal(part.InventoryItemsWherePart.FirstOrDefault(), inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void DeriveSerialisedInventoryItemSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Theft)
                .WithSerialisedItem(serialisedItem)
                .Build();
            this.Derive();

            Assert.Equal(new InventoryTransactionReasons(this.Transaction).Theft.DefaultSerialisedInventoryItemState, ((SerialisedInventoryItem)inventoryItemTransaction.InventoryItem).SerialisedInventoryItemState);
        }

        [Fact]
        public void DeriveNonSerialisedInventoryItemNonSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Theft)
                .WithPart(part)
                .Build();
            this.Derive();

            Assert.Equal(new InventoryTransactionReasons(this.Transaction).Theft.DefaultNonSerialisedInventoryItemState, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).NonSerialisedInventoryItemState);
        }
    }

    public class InventoryItemTransactionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidTransaction));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_2()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerialNumberRequired));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_3()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidTransaction));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_4()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).OutgoingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidTransaction));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_5()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemAlreadyInInventory));
        }
    }

    public class InventoryItemTransactionWorkEffortNumberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionWorkEffortNumberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AddedWorkEffortInventoryAssignmentInventoryItemTransactionDeriveWorkEffortNumber()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(2)
                .Build();

            this.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(worktask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();

            this.Derive();

            Assert.Equal(worktask.WorkEffortNumber, inventoryAssignment.InventoryItemTransactions.First().WorkEffortNumber);
        }

        [Fact]
        public void DeleteWorkEffortInventoryAssignmentDeriveWorkEffortNumber()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(2)
                .Build();

            this.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(worktask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();

            this.Derive();

            Assert.Equal(worktask.WorkEffortNumber, inventoryAssignment.InventoryItemTransactions.First().WorkEffortNumber);

            inventoryAssignment.Delete();

            this.Derive();

            Assert.Equal(worktask.WorkEffortNumber, part.InventoryItemTransactionsWherePart.Last().WorkEffortNumber);
        }
    }

    public class InventoryItemTransactionSalesOrderNumberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionSalesOrderNumberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AddedSalesOrderItemInventoryAssignmentInventoryItemTransactionDeriveWorkEffortNumber()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(item.Part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(item.QuantityOrdered)
                .Build();
            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var transaction = item.ReservedFromNonSerialisedInventoryItem.InventoryItemTransactionsWhereInventoryItem.Last();

            Assert.Equal(order.OrderNumber, transaction.SalesOrderNumber);
        }
    }
}
