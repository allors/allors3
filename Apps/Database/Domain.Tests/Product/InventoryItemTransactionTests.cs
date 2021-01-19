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
            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(inventoryItemTransaction.ExistTransactionDate);
        }
    }

    public class InventoryItemTransactionOnInitTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionOnInitTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DerivePart()
        {
            var part = new NonUnifiedPartBuilder(this.Session).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.Equal(part, inventoryItemTransaction.Part);
        }

        [Fact]
        public void DeriveFacility()
        {
            var facility = new FacilityBuilder(this.Session).Build();
            var part = new NonUnifiedPartBuilder(this.Session).WithDefaultFacility(facility).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session).WithPart(part).Build();
            this.Session.Derive(false);

            Assert.Equal(facility, inventoryItemTransaction.Facility);
        }

        [Fact]
        public void DeriveUnitOfMeasure()
        {
            var uom = new UnitsOfMeasure(this.Session).Meter;
            var part = new NonUnifiedPartBuilder(this.Session).WithUnitOfMeasure(uom).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session).WithPart(part).Build();
            this.Session.Derive(false);

            Assert.Equal(uom, inventoryItemTransaction.UnitOfMeasure);
        }

        [Fact]
        public void DeriveSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).Theft)
                .WithSerialisedItem(serialisedItem)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(new InventoryTransactionReasons(this.Session).Theft.DefaultSerialisedInventoryItemState, inventoryItemTransaction.SerialisedInventoryItemState);
        }

        [Fact]
        public void DeriveNonSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).Theft)
                .WithPart(part)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(new InventoryTransactionReasons(this.Session).Theft.DefaultNonSerialisedInventoryItemState, inventoryItemTransaction.NonSerialisedInventoryItemState);
        }

        [Fact]
        public void SyncInventoryItemSerialised()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithFacility(new FacilityBuilder(this.Session).Build())
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void SyncInventoryItemNonSerialised()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithFacility(new FacilityBuilder(this.Session).Build())
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void DeriveInventoryItemSerialisedMatch()
        {
            var facility = new FacilityBuilder(this.Session).Build();

            var part = new NonUnifiedPartBuilder(this.Session)
                .WithDefaultFacility(facility)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised)
                .Build();

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).OutgoingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.True(part.InventoryItemsWherePart.Count == 1);
            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void DeriveInventoryItemNonSerialisedMatch()
        {
            var facility = new FacilityBuilder(this.Session).Build();

            var part = new NonUnifiedPartBuilder(this.Session)
                .WithDefaultFacility(facility)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).OutgoingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.True(part.InventoryItemsWherePart.Count == 1);
            Assert.Equal(part.InventoryItemsWherePart.First, inventoryItemTransaction.InventoryItem);
        }

        [Fact]
        public void DeriveSerialisedInventoryItemSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).Theft)
                .WithSerialisedItem(serialisedItem)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(new InventoryTransactionReasons(this.Session).Theft.DefaultSerialisedInventoryItemState, ((SerialisedInventoryItem)inventoryItemTransaction.InventoryItem).SerialisedInventoryItemState);
        }

        [Fact]
        public void DeriveNonSerialisedInventoryItemNonSerialisedInventoryItemState()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).Theft)
                .WithPart(part)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(new InventoryTransactionReasons(this.Session).Theft.DefaultNonSerialisedInventoryItemState, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).NonSerialisedInventoryItemState);
        }
    }

    public class InventoryItemTransactionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTransactionDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.Quantity} Serialised Inventory Items only accept Quantities of -1, 0, and 1.";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_2()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.SerialisedItem} The Serial Number is required for Inventory Item Transactions involving Serialised Inventory Items.";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_3()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.Reason} Invalid transaction";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_4()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).OutgoingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.Reason} Invalid transaction";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError_5()
        {
            var part = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();

            var expectedMessage = $"{inventoryItemTransaction} { this.M.InventoryItemTransaction.SerialisedItem} Serialised item already in inventory";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
