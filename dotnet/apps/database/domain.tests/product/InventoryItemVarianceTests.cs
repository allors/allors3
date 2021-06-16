// <copyright file="InventoryItemVarianceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class InventoryItemVarianceTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemVarianceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenInventoryItem_WhenPositiveVariance_ThenQuantityOnHandIsRaised()
        {
            var nonSerialized = new InventoryItemKinds(this.Transaction).NonSerialised;
            var unknown = new InventoryTransactionReasons(this.Transaction).Unknown;
            var vatRate21 = new VatRateBuilder(this.Transaction).WithRate(21).Build();
            var piece = new UnitsOfMeasure(this.Transaction).Piece;
            var category = this.Transaction.Extent<ProductCategory>().First;

            var finishedGood = this.CreatePart("FG1", nonSerialized);

            this.Transaction.Derive(true);
            this.Transaction.Commit();

            var inventoryItem = (NonSerialisedInventoryItem)finishedGood.InventoryItemsWherePart.First();

            Assert.Equal(0, finishedGood.QuantityOnHand);
            Assert.Equal(0, inventoryItem.QuantityOnHand);

            var transaction = this.CreateInventoryTransaction(10, unknown, finishedGood);
            this.Transaction.Derive(true);

            Assert.Equal(10, finishedGood.QuantityOnHand);
            Assert.Equal(10, inventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenSerializedInventoryItems_WhenVarianceContainsInvalidQuantity_ThenDerivationExceptionRaised()
        {
            // Arrange
            var kinds = new InventoryItemKinds(this.Transaction);
            var unitsOfMeasure = new UnitsOfMeasure(this.Transaction);
            var unknown = new InventoryTransactionReasons(this.Transaction).Unknown;

            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var category = new ProductCategoryBuilder(this.Transaction).WithName("category").Build();
            var finishedGood = this.CreatePart("FG1", kinds.Serialised);
            var good = this.CreateGood("10101", vatRegime, "good1", unitsOfMeasure.Piece, category, finishedGood);
            var serialItem = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").Build();
            var variance = this.CreateInventoryTransaction(10, unknown, finishedGood, serialItem);

            // Act
            var derivation = this.Derive();

            // Assert
            Assert.True(derivation.HasErrors);

            //TODO: Derivation error
            //Assert.Contains(M.InventoryItemTransaction.Quantity, derivation.Errors.SelectMany(e => e.RoleTypes));

            // Re-Arrange
            variance.Quantity = -10;

            // Act
            derivation = this.Derive();

            // Assert
            Assert.True(derivation.HasErrors);

            //TODO: Derivation error
            //Assert.Contains(M.InventoryItemTransaction.Quantity, derivation.Errors.SelectMany(e => e.RoleTypes));
        }

        private Part CreatePart(string partId, InventoryItemKind kind)
            => new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification(partId)
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build()).WithInventoryItemKind(kind).Build();

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

        private InventoryItemTransaction CreateInventoryTransaction(int quantity, InventoryTransactionReason reason, Part part)
           => new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(quantity).WithReason(reason).WithPart(part).Build();

        private InventoryItemTransaction CreateInventoryTransaction(int quantity, InventoryTransactionReason reason, Part part, SerialisedItem serialisedItem)
           => new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(quantity).WithReason(reason).WithPart(part).WithSerialisedItem(serialisedItem).Build();
    }
}
