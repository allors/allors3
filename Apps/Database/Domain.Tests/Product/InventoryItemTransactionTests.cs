// <copyright file="InventoryItemTransactionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class InventoryItemTransactionOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveTransactionDate()
        {
            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            Assert.Equal(part, inventoryItemTransaction.Part);
        }

        [Fact]
        public void DeriveFacility()
        {
            var facility = new FacilityBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithDefaultFacility(facility).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithPart(part).Build();
            this.Transaction.Derive(false);

            Assert.Equal(facility, inventoryItemTransaction.Facility);
        }

        [Fact]
        public void DeriveUnitOfMeasure()
        {
            var uom = new UnitsOfMeasure(this.Transaction).Meter;
            var part = new NonUnifiedPartBuilder(this.Transaction).WithUnitOfMeasure(uom).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithPart(part).Build();
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
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
            this.Transaction.Derive(false);

            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
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
            this.Transaction.Derive(false);

            Assert.True(part.InventoryItemsWherePart.Count == 1);
            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
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
            this.Transaction.Derive(false);

            Assert.True(part.InventoryItemsWherePart.Count == 1);
            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
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
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.Quantity} Serialised Inventory Items only accept Quantities of -1, 0, and 1.";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_2()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.SerialisedItem} The Serial Number is required for Inventory Item Transactions involving Serialised Inventory Items.";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_3()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.Reason} Invalid transaction";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_4()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).OutgoingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.Reason} Invalid transaction";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_5()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.SerialisedItem} Serialised item already in inventory";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
