// <copyright file="SupplierOfferingTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class SupplierOfferingTests : DomainTest, IClassFixture<Fixture>
    {
        public SupplierOfferingTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSupplierOffering_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.Transaction.Commit();

            var builder = new SupplierOfferingBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPrice(1);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithSupplier(supplier);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Pack);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            builder.WithPart(part);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenNewGood_WhenDeriving_ThenNonSerialisedInventryItemIsCreatedForEveryFacility()
        {
            var settings = this.Transaction.GetSingleton().Settings;

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var internalOrganisation = this.InternalOrganisation;
            var before = settings.DefaultFacility.InventoryItemsWhereFacility.Count;

            var secondFacility = new FacilityBuilder(this.Transaction)
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithOwner(this.InternalOrganisation)
                .WithName("second facility")
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithName("good1")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(1)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, good.Part.InventoryItemsWherePart.Count);
            Assert.Equal(before + 1, settings.DefaultFacility.InventoryItemsWhereFacility.Count);
            Assert.Single(secondFacility.InventoryItemsWhereFacility);
        }

        [Fact]
        public void GivenNewGoodBasedOnPart_WhenDeriving_ThenNonSerialisedInventryItemIsCreatedForEveryPartAndFacility()
        {
            var settings = this.Transaction.GetSingleton().Settings;
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var before = settings.DefaultFacility.InventoryItemsWhereFacility.Count;

            var secondFacility = new FacilityBuilder(this.Transaction)
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithName("second facility")
                .WithOwner(this.InternalOrganisation)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithName("good1")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithFromDate(this.Transaction.Now())
                .WithPrice(1)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, good.Part.InventoryItemsWherePart.Count);
            Assert.Equal(before + 1, settings.DefaultFacility.InventoryItemsWhereFacility.Count);
            Assert.Single(secondFacility.InventoryItemsWhereFacility);
        }

        [Fact]
        public void GivenSupplierOffering_WhenCalculatingUnitSellingPrice_ThenConsiderHighestHistoricalPurchaseRate()
        {
            var settings = this.Transaction.GetSingleton().Settings;

            var supplier_1 = new OrganisationBuilder(this.Transaction).WithName("supplier uno").Build();
            var supplier_2 = new OrganisationBuilder(this.Transaction).WithName("supplier dos").Build();
            var supplier_3 = new OrganisationBuilder(this.Transaction).WithName("supplier tres").Build();
            var supplier_4 = new OrganisationBuilder(this.Transaction).WithName("supplier cuatro").Build();

            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => Equals(v.Name, "internalOrganisation"));

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_1)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(this.Transaction.Now().AddYears(-3))
                .Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_2)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(this.Transaction.Now().AddYears(-2))
                .Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_3)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_4)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(this.Transaction.Now().AddMonths(-6))
                .Build();

            var finishedGood = new NonUnifiedPartBuilder(this.Transaction)
                .WithNonSerialisedDefaults(internalOrganisation)
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment).WithPart(finishedGood).Build();

            this.Transaction.Derive();

            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");
            var piece = new UnitsOfMeasure(this.Transaction).Piece;

            new BasePriceBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithFromDate(this.Transaction.Now())
                .WithPrice(100)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_1)
                .WithFromDate(this.Transaction.Now().AddMonths(-6))
                .WithThroughDate(this.Transaction.Now().AddMonths(-3))
                .WithUnitOfMeasure(piece)
                .WithPrice(100)
                .WithCurrency(euro)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_2)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithUnitOfMeasure(piece)
                .WithPrice(120)
                .WithCurrency(euro)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_3)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(piece)
                .WithPrice(99)
                .WithCurrency(euro)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_4)
                .WithFromDate(this.Transaction.Now().AddDays(7))
                .WithThroughDate(this.Transaction.Now().AddDays(30))
                .WithUnitOfMeasure(piece)
                .WithPrice(135)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();

            var customer = internalOrganisation.CreateB2BCustomer(this.Transaction.Faker());

            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithName("Activity")
                .WithCustomer(customer)
                .WithTakenBy(internalOrganisation)
                .Build();

            var workEffortInventoryAssignement = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(finishedGood.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();

            this.Transaction.Derive();

            /*Purchase price times InternalSurchargePercentage
            var sellingPrice = Math.Round(135 * (1 + (this.Transaction.GetSingleton().Settings.PartSurchargePercentage / 100)), 2);*/

            Assert.Equal(100, workEffortInventoryAssignement.UnitSellingPrice);
        }
    }

    public class SupplierOfferingDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SupplierOfferingDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSupplierOffering_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            new SupplierOfferingBuilder(this.Transaction).WithPart(part).Build();
            this.Transaction.Derive(false);

            var warehouses = this.Transaction.Extent<Facility>();
            warehouses.Filter.AddEquals(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse);

            foreach (Facility facility in warehouses)
            {
                var inventoryItem = part.InventoryItemsWherePart.Where(v => v.Facility.Equals(facility));
                Assert.NotNull(inventoryItem);
            }
        }
    }
}
