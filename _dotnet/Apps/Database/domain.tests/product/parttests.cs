// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PartTests : DomainTest, IClassFixture<Fixture>
    {
        public PartTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPart_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var finishedGood = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            Assert.Equal(new InventoryItemKinds(this.Transaction).NonSerialised, finishedGood.InventoryItemKind);
        }

        [Fact]
        public void GivenNewPart_WhenDeriving_ThenInventoryItemIsCreated()
        {
            var finishedGood = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.Transaction.Derive();

            Assert.Single(finishedGood.InventoryItemsWherePart);
            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), finishedGood.InventoryItemsWherePart.First().Facility);
        }

        [Fact]
        public void OnInitAddProductIdentification()
        {
            this.Transaction.GetSingleton().Settings.UsePartNumberCounter = true;

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Single(nonUnifiedPart.ProductIdentifications);
        }
    }

    public class PartRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNameDeriveDisplayName()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.Name = "anotherName";
            this.Derive();

            Assert.Equal("anotherName", nonUnifiedPart.DisplayName);
        }

        [Fact]
        public void ChangedUnitOfMeasureDeriveNonSerialisedInventoryItemWherePart()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            var piece = new UnitsOfMeasure(this.Transaction).Piece;
            nonUnifiedPart.UnitOfMeasure = piece;
            this.Derive();

            Assert.NotNull(nonUnifiedPart.InventoryItemsWherePart.Single(v => v.UnitOfMeasure.Equals(piece)));
        }

        [Fact]
        public void ChangedDefaultFacilityDeriveNonSerialisedInventoryItemWherePart()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            var facility = new FacilityBuilder(this.Transaction).Build();
            nonUnifiedPart.DefaultFacility = facility;
            this.Derive();

            Assert.NotNull(nonUnifiedPart.InventoryItemsWherePart.Single(v => v.Facility.Equals(facility)));
        }

        [Fact]
        public void ChangedProductTypeDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();
            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Derive();

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Empty(nonUnifiedPart.SerialisedItemCharacteristics);

            nonUnifiedPart.ProductType = productType;
            this.Derive();

            Assert.Equal(characteristicType, nonUnifiedPart.SerialisedItemCharacteristics.First().SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedProductTypeSerialisedItemCharacteristicTypesDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithProductType(productType).Build();
            this.Derive();

            Assert.Empty(nonUnifiedPart.SerialisedItemCharacteristics);

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Derive();
            this.Derive();

            Assert.Equal(characteristicType, nonUnifiedPart.SerialisedItemCharacteristics.First().SerialisedItemCharacteristicType);
        }
    }

    public class PartQuantitiesRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartQuantitiesRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNonSerialisedInventoryItemQuantityOnHandDeriveQuantityOnHand()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.FirstOrDefault()).QuantityOnHand = 1;
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.QuantityOnHand);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemQuantityDeriveQuantityOnHand()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            nonUnifiedPart.AddSerialisedItem(serialisedItem);
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.QuantityOnHand);
        }

        [Fact]
        public void ChangedNonSerialisedInventoryItemAvailableToPromiseDeriveAvailableToPromise()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.FirstOrDefault()).AvailableToPromise = 1;
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.AvailableToPromise);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemAvailableToPromiseDeriveAvailableToPromise()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            nonUnifiedPart.AddSerialisedItem(serialisedItem);
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(this.Transaction).Good)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.AvailableToPromise);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemSerialisedInventoryItemStateDeriveAvailableToPromise()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            nonUnifiedPart.AddSerialisedItem(serialisedItem);
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(this.Transaction).Scrap)
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(0, nonUnifiedPart.AvailableToPromise);

            serialisedItem.SerialisedInventoryItemsWhereSerialisedItem.First().SerialisedInventoryItemState = new SerialisedInventoryItemStates(this.Transaction).Good;
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.AvailableToPromise);
        }

        [Fact]
        public void ChangedNonSerialisedInventoryItemQuantityCommittedOutDeriveQuantityCommittedOut()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.FirstOrDefault()).QuantityCommittedOut = 1;
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.QuantityCommittedOut);
        }

        [Fact]
        public void ChangedNonSerialisedInventoryItemQuantityExpectedInDeriveQuantityExpectedIn()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.FirstOrDefault()).QuantityExpectedIn = 1;
            this.Derive();

            Assert.Equal(1, nonUnifiedPart.QuantityExpectedIn);
        }
    }

    public class PartSuppliedByRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartSuppliedByRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSupplierOfferingPartDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction).WithPart(nonUnifiedPart).WithSupplier(new OrganisationBuilder(this.Transaction).Build()).Build();
            this.Derive();

            Assert.Single(nonUnifiedPart.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingPartDeriveSuppliedBy_2()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction).WithPart(unifiedGood).WithSupplier(new OrganisationBuilder(this.Transaction).Build()).Build();
            this.Derive();

            Assert.Single(unifiedGood.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingAllVersionsDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithPart(nonUnifiedPart)
                .WithSupplier(supplier)
                .Build();
            this.Derive();

            Assert.Single(nonUnifiedPart.SuppliedBy);

            supplierOffering.Part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(supplier, nonUnifiedPart.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingAllVersionsDeriveSuppliedBy_2()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithPart(unifiedGood)
                .WithSupplier(supplier)
                .Build();
            this.Derive();

            Assert.Single(unifiedGood.SuppliedBy);

            supplierOffering.Part = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(supplier, unifiedGood.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingFromDateDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .WithPart(nonUnifiedPart)
                .WithSupplier(new OrganisationBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            Assert.Empty(nonUnifiedPart.SuppliedBy);

            supplierOffering.FromDate = this.Transaction.Now().AddDays(-1);
            this.Derive();

            Assert.Single(nonUnifiedPart.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingThroughDateDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithPart(nonUnifiedPart)
                .WithSupplier(new OrganisationBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            Assert.Single(nonUnifiedPart.SuppliedBy);

            supplierOffering.ThroughDate = this.Transaction.Now().AddDays(-1);
            this.Derive();

            Assert.Empty(nonUnifiedPart.SuppliedBy);
        }
    }

}
