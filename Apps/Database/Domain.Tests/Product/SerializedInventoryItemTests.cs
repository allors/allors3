// <copyright file="SerializedInventoryItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;
    using System.Linq;
    using Allors.Database.Derivations;
    using System.Collections.Generic;

    public class SerialisedInventoryItemTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedInventoryItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenInventoryItem_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            // Arrange
            var serialItem = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("part")
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithSerialisedItem(serialItem)
                .Build();

            this.Transaction.Derive(true);
            this.Transaction.Commit();

            var builder = new SerialisedInventoryItemBuilder(this.Transaction).WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First).WithPart(part);
            builder.Build();

            // Act
            var derivation = this.Transaction.Derive(false);

            // Assert
            Assert.True(derivation.HasErrors);

            // Re-arrange
            this.Transaction.Rollback();

            builder.WithSerialisedItem(serialItem);
            builder.Build();

            // Act
            derivation = this.Transaction.Derive(false);

            // Assert
            Assert.False(derivation.HasErrors);
        }

        [Fact]
        public void GivenInventoryItem_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            // Arrange
            var goodOrder = new SerialisedInventoryItemStates(this.Transaction).Good;
            var warehouse = new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse);
            var kinds = new InventoryItemKinds(this.Transaction);

            var serialItem = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").Build();
            var finishedGood = this.CreatePart("1", kinds.Serialised);
            finishedGood.AddSerialisedItem(serialItem);

            this.Transaction.Derive(true);

            var serialInventoryItem = new SerialisedInventoryItemBuilder(this.Transaction).WithSerialisedItem(serialItem).WithPart(finishedGood).Build();

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(goodOrder, serialInventoryItem.SerialisedInventoryItemState);
            Assert.Equal(warehouse, serialInventoryItem.Facility);
        }

        [Fact]
        public void GivenFinishedGoodWithSerializedInventory_WhenDeriving_ThenQuantityOnHandUpdated()
        {
            // Arrange
            var goodOrder = new SerialisedInventoryItemStates(this.Transaction).Good;
            var warehouse = new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse);

            var kinds = new InventoryItemKinds(this.Transaction);
            var unitsOfMeasure = new UnitsOfMeasure(this.Transaction);
            var unknown = new InventoryTransactionReasons(this.Transaction).Unknown;

            var vatRegime = new VatRegimes(this.Transaction).ZeroRated;
            var category = new ProductCategoryBuilder(this.Transaction).WithName("category").Build();
            var serialPart = this.CreatePart("FG1", kinds.Serialised);
            var serialItem1 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").Build();
            var serialItem2 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("2").Build();
            var serialItem3 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("3").Build();

            serialPart.AddSerialisedItem(serialItem1);
            serialPart.AddSerialisedItem(serialItem2);
            serialPart.AddSerialisedItem(serialItem3);

            var good = this.CreateGood("10101", vatRegime, "good1", unitsOfMeasure.Piece, category, serialPart);

            // Act
            this.Transaction.Derive(true);

            this.CreateInventoryTransaction(1, unknown, serialPart, serialItem1);
            this.CreateInventoryTransaction(1, unknown, serialPart, serialItem2);
            this.CreateInventoryTransaction(1, unknown, serialPart, serialItem3);

            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(3, serialPart.QuantityOnHand);
        }

        [Fact]
        public void GivenSerializedItemInMultipleFacilities_WhenDeriving_ThenMultipleQuantityOnHandTracked()
        {
            // Arrange
            var warehouseType = new FacilityTypes(this.Transaction).Warehouse;
            var warehouse1 = this.CreateFacility("WH1", warehouseType, this.InternalOrganisation);
            var warehouse2 = this.CreateFacility("WH2", warehouseType, this.InternalOrganisation);

            var serialized = new InventoryItemKinds(this.Transaction).Serialised;
            var piece = new UnitsOfMeasure(this.Transaction).Piece;
            var unknown = new InventoryTransactionReasons(this.Transaction).Unknown;

            var vatRegime = new VatRegimes(this.Transaction).ZeroRated;
            var category = new ProductCategoryBuilder(this.Transaction).WithName("category").Build();
            var finishedGood = this.CreatePart("FG1", serialized);
            var serialItem1 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").Build();
            var serialItem2 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("2").Build();

            finishedGood.AddSerialisedItem(serialItem1);
            finishedGood.AddSerialisedItem(serialItem2);

            var good = this.CreateGood("10101", vatRegime, "good1", piece, category, finishedGood);

            // Act
            this.Transaction.Derive(true);

            this.CreateInventoryTransaction(1, unknown, finishedGood, serialItem1, warehouse1);
            this.CreateInventoryTransaction(1, unknown, finishedGood, serialItem2, warehouse2);

            this.Transaction.Derive(true);

            // Assert
            var item1 = (SerialisedInventoryItem)new InventoryItems(this.Transaction).Extent().First(i => i.Facility.Equals(warehouse1));
            Assert.Equal(1, item1.QuantityOnHand);

            var item2 = (SerialisedInventoryItem)new InventoryItems(this.Transaction).Extent().First(i => i.Facility.Equals(warehouse2));
            Assert.Equal(1, item2.QuantityOnHand);

            Assert.Equal(2, finishedGood.QuantityOnHand);
        }

        private Facility CreateFacility(string name, FacilityType type, InternalOrganisation owner)
            => new FacilityBuilder(this.Transaction).WithName(name).WithFacilityType(type).WithOwner(owner).Build();

        private Good CreateGood(string sku, VatRegime vatRegime, string name, UnitOfMeasure uom, ProductCategory category, Part part)
            => new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new SkuIdentificationBuilder(this.Transaction)
                    .WithIdentification(sku)
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build())
                .WithVatRegime(vatRegime)
                .WithName(name)
                .WithUnitOfMeasure(uom)
                .WithPart(part)
                .Build();

        private Part CreatePart(string partId, InventoryItemKind kind)
            => new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification(partId)
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(kind).Build();

        private InventoryItemTransaction CreateInventoryTransaction(int quantity, InventoryTransactionReason reason, Part part, SerialisedItem serialisedItem)
           => new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(quantity).WithReason(reason).WithPart(part).WithSerialisedItem(serialisedItem).Build();

        private InventoryItemTransaction CreateInventoryTransaction(int quantity, InventoryTransactionReason reason, Part part, SerialisedItem serialisedItem, Facility facility)
           => new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(quantity).WithReason(reason).WithPart(part).WithSerialisedItem(serialisedItem).WithFacility(facility).Build();
    }

    public class SerialisedInventoryItemOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedInventoryItemOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveNonSerialisedInventoryItemState()
        {
            var inventoryItem = new SerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(new SerialisedInventoryItemStates(this.Transaction).Good, inventoryItem.SerialisedInventoryItemState);
        }
    }

    public class SerialisedInventoryItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedInventoryItemDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveName()
        {
            var inventoryItem = new SerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Part = new UnifiedGoodBuilder(this.Transaction).WithName("partname").Build();
            this.Transaction.Derive(false);

            Assert.Equal("partname at  with state In good order", inventoryItem.Name);
        }

        [Fact]
        public void ChangedFacilityDeriveName()
        {
            var inventoryItem = new SerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Facility = new FacilityBuilder(this.Transaction).WithName("facilityname").Build();
            this.Transaction.Derive(false);

            Assert.Equal(" at facilityname with state In good order", inventoryItem.Name);
        }
    }

    public class SerialisedInventoryItemQuantitiesDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedInventoryItemQuantitiesDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNonSerialisedInventoryItemStateDeriveQuantity()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                .WithSerialisedItem(serialisedItem)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStateBuilder(this.Transaction).Build())
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            // InventoryItemState is excluded from InventoryStrategy
            Assert.Equal(0, ((SerialisedInventoryItem)inventoryItemTransaction.InventoryItem).Quantity);
        }

        [Fact]
        public void ChangedInventoryItemTransactionInventoryItemDeriveQuantity()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                .WithSerialisedItem(serialisedItem)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, ((SerialisedInventoryItem)inventoryItemTransaction.InventoryItem).Quantity);
        }

        [Fact]
        public void ChangedPickListItemInventoryItemDeriveQuantity()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                .WithSerialisedItem(serialisedItem)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .Build();
            this.Transaction.Derive(false);

            var picklistItem = new PickListItemBuilder(this.Transaction)
                .WithInventoryItem(inventoryItemTransaction.InventoryItem)
                .WithQuantityPicked(1)
                .Build();
            picklist.AddPickListItem(picklistItem);

            new ItemIssuanceBuilder(this.Transaction)
                .WithPickListItem(picklistItem)
                .WithShipmentItem(new ShipmentItemBuilder(this.Transaction).WithShipmentItemState(new ShipmentItemStates(this.Transaction).Created).Build())
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(0, ((SerialisedInventoryItem)inventoryItemTransaction.InventoryItem).Quantity);
        }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                .WithSerialisedItem(serialisedItem)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(2)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains("Invalid transaction"));
        }
    }
}
