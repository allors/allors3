
// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using TestPopulation;
    using Xunit;

    public class NonUnifiedPartTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostDeriveAssertExistPart()
        {
            this.Session.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedPartDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Session.GetSingleton().Settings;
            settings.UsePartNumberCounter = false;

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.False(nonUnifiedPart.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Session)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build();

            nonUnifiedPart.AddProductIdentification(goodIdentification);
            this.Session.Derive(false);

            Assert.True(nonUnifiedPart.ExistProductNumber);
        }

        [Fact]
        public void ChangedPartIdentificationDeriveName()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(nonUnifiedPart.ProductIdentifications.First.Identification, nonUnifiedPart.Name);
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

    public class NonUnifiedPartQuantitiesDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartQuantitiesDerivationTests(Fixture fixture) : base(fixture) { }

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

    public class NonUnifiedPartSuppliedByDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartSuppliedByDerivationTests(Fixture fixture) : base(fixture) { }

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

    [Trait("Category", "Security")]
    public class NonUnifiedPartDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.NonUnifiedPart.ObjectType, this.M.NonUnifiedPart.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortInventoryProducedPartDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            new WorkEffortInventoryProducedBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPartStandardPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortPartStandardBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ManufacturingBomBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ManufacturingBomBuilder(this.Session).WithComponentPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedInventoryItemTransactionPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }
    }
}
