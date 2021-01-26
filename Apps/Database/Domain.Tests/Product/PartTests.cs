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
            var finishedGood = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            Assert.Equal(new InventoryItemKinds(this.Session).NonSerialised, finishedGood.InventoryItemKind);
        }

        [Fact]
        public void GivenNewPart_WhenDeriving_ThenInventoryItemIsCreated()
        {
            var finishedGood = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            this.Session.Derive();

            Assert.Single(finishedGood.InventoryItemsWherePart);
            Assert.Equal(new Facilities(this.Session).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Session).Warehouse), finishedGood.InventoryItemsWherePart.First.Facility);
        }

        [Fact]
        public void OnInitAddProductIdentification()
        {
            this.Session.GetSingleton().Settings.UsePartNumberCounter = true;

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Single(nonUnifiedPart.ProductIdentifications);
        }
    }

    public class PartDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNameDeriveDisplayName()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.Name = "anotherName";
            this.Session.Derive(false);

            Assert.Equal("anotherName", nonUnifiedPart.DisplayName);
        }

        [Fact]
        public void ChangedUnitOfMeasureDeriveNonSerialisedInventoryItemWherePart()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            var piece = new UnitsOfMeasure(this.Session).Piece;
            nonUnifiedPart.UnitOfMeasure = piece;
            this.Session.Derive(false);

            Assert.NotNull(nonUnifiedPart.InventoryItemsWherePart.Single(v => v.UnitOfMeasure.Equals(piece)));
        }

        [Fact]
        public void ChangedDefaultFacilityDeriveNonSerialisedInventoryItemWherePart()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            var facility = new FacilityBuilder(this.Session).Build();
            nonUnifiedPart.DefaultFacility = facility;
            this.Session.Derive(false);

            Assert.NotNull(nonUnifiedPart.InventoryItemsWherePart.Single(v => v.Facility.Equals(facility)));
        }

        [Fact]
        public void ChangedProductTypeDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Session).Build();
            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Session).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Session.Derive(false);

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Empty(nonUnifiedPart.SerialisedItemCharacteristics);

            nonUnifiedPart.ProductType = productType;
            this.Session.Derive(false);

            Assert.Equal(characteristicType, nonUnifiedPart.SerialisedItemCharacteristics.First.SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedProductTypeSerialisedItemCharacteristicTypesDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Session).Build();

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithProductType(productType).Build();
            this.Session.Derive(false);

            Assert.Empty(nonUnifiedPart.SerialisedItemCharacteristics);

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Session).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Session.Derive(false);
            this.Session.Derive(false);

            Assert.Equal(characteristicType, nonUnifiedPart.SerialisedItemCharacteristics.First.SerialisedItemCharacteristicType);
        }
    }

    public class PartQuantitiesDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartQuantitiesDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNonSerialisedInventoryItemQuantityOnHandDeriveQuantityOnHand()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.First).QuantityOnHand = 1;
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.QuantityOnHand);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemQuantityDeriveQuantityOnHand()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            nonUnifiedPart.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.QuantityOnHand);
        }

        [Fact]
        public void ChangedNonSerialisedInventoryItemAvailableToPromiseDeriveAvailableToPromise()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.First).AvailableToPromise = 1;
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.AvailableToPromise);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemAvailableToPromiseDeriveAvailableToPromise()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            nonUnifiedPart.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(this.Session).Good)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.AvailableToPromise);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemSerialisedInventoryItemStateDeriveAvailableToPromise()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            nonUnifiedPart.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment)
                .WithSerialisedItem(serialisedItem)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(this.Session).Scrap)
                .WithQuantity(1)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(0, nonUnifiedPart.AvailableToPromise);

            serialisedItem.SerialisedInventoryItemsWhereSerialisedItem.First.SerialisedInventoryItemState = new SerialisedInventoryItemStates(this.Session).Good;
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.AvailableToPromise);
        }

        [Fact]
        public void ChangedNonSerialisedInventoryItemQuantityCommittedOutDeriveQuantityCommittedOut()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.First).QuantityCommittedOut = 1;
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.QuantityCommittedOut);
        }

        [Fact]
        public void ChangedNonSerialisedInventoryItemQuantityExpectedInDeriveQuantityExpectedIn()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            ((NonSerialisedInventoryItem)nonUnifiedPart.InventoryItemsWherePart.First).QuantityExpectedIn = 1;
            this.Session.Derive(false);

            Assert.Equal(1, nonUnifiedPart.QuantityExpectedIn);
        }
    }

    public class PartSuppliedByDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartSuppliedByDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSupplierOfferingPartDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SupplierOfferingBuilder(this.Session).WithPart(nonUnifiedPart).WithSupplier(new OrganisationBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            Assert.Single(nonUnifiedPart.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingPartDeriveSuppliedBy_2()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SupplierOfferingBuilder(this.Session).WithPart(unifiedGood).WithSupplier(new OrganisationBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            Assert.Single(unifiedGood.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingAllVersionsDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var supplier = new OrganisationBuilder(this.Session).Build();
            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithFromDate(this.Session.Now().AddDays(-1))
                .WithPart(nonUnifiedPart)
                .WithSupplier(supplier)
                .Build();
            this.Session.Derive(false);

            Assert.Single(nonUnifiedPart.SuppliedBy);

            supplierOffering.Part = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(supplier, nonUnifiedPart.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingAllVersionsDeriveSuppliedBy_2()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var supplier = new OrganisationBuilder(this.Session).Build();
            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithFromDate(this.Session.Now().AddDays(-1))
                .WithPart(unifiedGood)
                .WithSupplier(supplier)
                .Build();
            this.Session.Derive(false);

            Assert.Single(unifiedGood.SuppliedBy);

            supplierOffering.Part = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(supplier, unifiedGood.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingFromDateDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithFromDate(this.Session.Now().AddDays(1))
                .WithPart(nonUnifiedPart)
                .WithSupplier(new OrganisationBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            Assert.Empty(nonUnifiedPart.SuppliedBy);

            supplierOffering.FromDate = this.Session.Now().AddDays(-1);
            this.Session.Derive(false);

            Assert.Single(nonUnifiedPart.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingThroughDateDeriveSuppliedBy()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithFromDate(this.Session.Now().AddDays(-1))
                .WithPart(nonUnifiedPart)
                .WithSupplier(new OrganisationBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            Assert.Single(nonUnifiedPart.SuppliedBy);

            supplierOffering.ThroughDate = this.Session.Now().AddDays(-1);
            this.Session.Derive(false);

            Assert.Empty(nonUnifiedPart.SuppliedBy);
        }
    }

}
