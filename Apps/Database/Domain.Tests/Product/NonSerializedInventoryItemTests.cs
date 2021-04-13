// <copyright file="NonSerializedInventoryItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Domain.TestPopulation;
    using System;
    using System.Linq;
    using Xunit;

    public class NonSerialisedInventoryItemTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenInventoryItem_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.Transaction.Derive();

            var item = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(part)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new NonSerialisedInventoryItemStates(this.Transaction).Good, item.NonSerialisedInventoryItemState);
            Assert.Equal(item.LastNonSerialisedInventoryItemState, item.NonSerialisedInventoryItemState);
        }

        [Fact]
        public void GivenInventoryItem_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var item = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("1")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .Build())
                .Build();

            this.Transaction.Derive();

            Assert.Null(item.PreviousNonSerialisedInventoryItemState);
        }

        [Fact]
        public void GivenInventoryItem_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.Transaction.Derive();

            var item = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(part)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(0M, item.AvailableToPromise);
            Assert.Equal(0M, item.QuantityCommittedOut);
            Assert.Equal(0M, item.QuantityExpectedIn);
            Assert.Equal(0M, item.QuantityOnHand);
            Assert.Equal(new NonSerialisedInventoryItemStates(this.Transaction).Good, item.NonSerialisedInventoryItemState);
            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), item.Facility);
        }

        [Fact]
        public void GivenInventoryItemForPart_WhenDerived_ThenNameIsPartName()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("Part 1 at facility with state Good", part.InventoryItemsWherePart.Single().Name);
        }

        [Fact]
        public void GivenInventoryItemForPart_WhenDerived_ThenUnitOfMeasureIsPartUnitOfMeasure()
        {
            var uom = new UnitsOfMeasure(this.Transaction).Centimeter;
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .WithUnitOfMeasure(uom)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(part.UnitOfMeasure, part.InventoryItemsWherePart.Single().UnitOfMeasure);
        }

        [Fact]
        public void GivenInventoryItem_WhenQuantityOnHandIsRaised_ThenSalesOrderItemsWithQuantityShortFalledAreUpdated()
        {
            // Arrange
            var inventoryItemKinds = new InventoryItemKinds(this.Transaction);
            var unitsOfMeasure = new UnitsOfMeasure(this.Transaction);
            var varianceReasons = new InventoryTransactionReasons(this.Transaction);
            var contactMechanisms = new ContactMechanismPurposes(this.Transaction);

            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var category = new ProductCategoryBuilder(this.Transaction).WithName("category").Build();
            var finishedGood = this.CreatePart("1", inventoryItemKinds.NonSerialised);
            var good = this.CreateGood("10101", vatRegime, "good1", unitsOfMeasure.Piece, category, finishedGood);

            this.Transaction.Derive();

            this.CreateInventoryTransaction(5, varianceReasons.Unknown, finishedGood);

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = this.CreateShipTo(mechelenAddress, contactMechanisms.ShippingAddress, true);
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var order1 = this.CreateSalesOrder(customer, customer, this.Transaction.Now());
            var salesItem1 = this.CreateSalesOrderItem("item1", good, 10, 15);
            var salesItem2 = this.CreateSalesOrderItem("item2", good, 20, 15);

            order1.AddSalesOrderItem(salesItem1);
            order1.AddSalesOrderItem(salesItem2);

            var order2 = this.CreateSalesOrder(customer, customer, this.Transaction.Now().AddDays(1));
            var salesItem3 = this.CreateSalesOrderItem("item3", good, 10, 15);
            var salesItem4 = this.CreateSalesOrderItem("item4", good, 20, 15);

            order2.AddSalesOrderItem(salesItem3);
            order2.AddSalesOrderItem(salesItem4);

            this.Transaction.Derive();
            this.Transaction.Commit();

            // Act
            order1.SetReadyForPosting();
            this.Transaction.Derive(true);

            order1.Post();
            this.Transaction.Derive();

            order1.Accept();
            this.Transaction.Derive();

            Assert.Equal(0, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(5, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            order2.SetReadyForPosting();

            this.Transaction.Derive(true);

            order2.Post();
            this.Transaction.Derive();

            order2.Accept();
            this.Transaction.Derive();

            // Assert
            Assert.Equal(0, salesItem1.QuantityRequestsShipping);
            Assert.Equal(5, salesItem1.QuantityPendingShipment);
            Assert.Equal(10, salesItem1.QuantityReserved);
            Assert.Equal(5, salesItem1.QuantityShortFalled);

            Assert.Equal(0, salesItem2.QuantityRequestsShipping);
            Assert.Equal(0, salesItem2.QuantityPendingShipment);
            Assert.Equal(20, salesItem2.QuantityReserved);
            Assert.Equal(20, salesItem2.QuantityShortFalled);

            Assert.Equal(0, salesItem3.QuantityRequestsShipping);
            Assert.Equal(0, salesItem3.QuantityPendingShipment);
            Assert.Equal(10, salesItem3.QuantityReserved);
            Assert.Equal(10, salesItem3.QuantityShortFalled);

            Assert.Equal(0, salesItem4.QuantityRequestsShipping);
            Assert.Equal(0, salesItem4.QuantityPendingShipment);
            Assert.Equal(20, salesItem4.QuantityReserved);
            Assert.Equal(20, salesItem4.QuantityShortFalled);

            Assert.Equal(0, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(5, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            // Re-arrange
            this.CreateInventoryTransaction(15, varianceReasons.Unknown, finishedGood);

            // Act
            this.Transaction.Derive(true);
            this.Transaction.Commit();

            // Assert
            // Orderitems are sorted as follows: item1, item2, item3, item4
            Assert.Equal(0, salesItem1.QuantityRequestsShipping);
            Assert.Equal(10, salesItem1.QuantityPendingShipment);
            Assert.Equal(10, salesItem1.QuantityReserved);
            Assert.Equal(0, salesItem1.QuantityShortFalled);

            Assert.Equal(0, salesItem2.QuantityRequestsShipping);
            Assert.Equal(10, salesItem2.QuantityPendingShipment);
            Assert.Equal(20, salesItem2.QuantityReserved);
            Assert.Equal(10, salesItem2.QuantityShortFalled);

            Assert.Equal(0, salesItem3.QuantityRequestsShipping);
            Assert.Equal(0, salesItem3.QuantityPendingShipment);
            Assert.Equal(10, salesItem3.QuantityReserved);
            Assert.Equal(10, salesItem3.QuantityShortFalled);

            Assert.Equal(0, salesItem4.QuantityRequestsShipping);
            Assert.Equal(0, salesItem4.QuantityPendingShipment);
            Assert.Equal(20, salesItem4.QuantityReserved);
            Assert.Equal(20, salesItem4.QuantityShortFalled);

            Assert.Equal(0, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(20, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            // Re-arrange
            this.CreateInventoryTransaction(85, varianceReasons.Unknown, finishedGood);

            // Act
            this.Transaction.Derive();
            this.Transaction.Commit();

            // Assert
            // Orderitems are sorted as follows: item2, item1, item4, item 3
            Assert.Equal(0, salesItem1.QuantityRequestsShipping);
            Assert.Equal(10, salesItem1.QuantityPendingShipment);
            Assert.Equal(10, salesItem1.QuantityReserved);
            Assert.Equal(0, salesItem1.QuantityShortFalled);

            Assert.Equal(0, salesItem2.QuantityRequestsShipping);
            Assert.Equal(20, salesItem2.QuantityPendingShipment);
            Assert.Equal(20, salesItem2.QuantityReserved);
            Assert.Equal(0, salesItem2.QuantityShortFalled);

            Assert.Equal(0, salesItem3.QuantityRequestsShipping);
            Assert.Equal(10, salesItem3.QuantityPendingShipment);
            Assert.Equal(10, salesItem3.QuantityReserved);
            Assert.Equal(0, salesItem3.QuantityShortFalled);

            Assert.Equal(0, salesItem4.QuantityRequestsShipping);
            Assert.Equal(20, salesItem4.QuantityPendingShipment);
            Assert.Equal(20, salesItem4.QuantityReserved);
            Assert.Equal(0, salesItem4.QuantityShortFalled);

            Assert.Equal(45, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(105, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenInventoryItem_WhenQuantityOnHandIsDecreased_ThenSalesOrderItemsWithQuantityRequestsShippingAreUpdated()
        {
            // Arrange
            var inventoryItemKinds = new InventoryItemKinds(this.Transaction);
            var unitsOfMeasure = new UnitsOfMeasure(this.Transaction);
            var varianceReasons = new InventoryTransactionReasons(this.Transaction);
            var contactMechanisms = new ContactMechanismPurposes(this.Transaction);

            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var category = new ProductCategoryBuilder(this.Transaction).WithName("category").Build();
            var finishedGood = this.CreatePart("1", inventoryItemKinds.NonSerialised);
            var good = this.CreateGood("10101", vatRegime, "good1", unitsOfMeasure.Piece, category, finishedGood);

            this.Transaction.Derive();

            this.CreateInventoryTransaction(5, varianceReasons.Unknown, finishedGood);

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = this.CreateShipTo(mechelenAddress, contactMechanisms.ShippingAddress, true);
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = this.CreateSalesOrder(customer, customer, this.Transaction.Now(), false);
            var salesItem = this.CreateSalesOrderItem("item1", good, 10, 15);

            // Act
            order.AddSalesOrderItem(salesItem);
            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            // Assert
            Assert.Equal(5, salesItem.QuantityRequestsShipping);
            Assert.Equal(0, salesItem.QuantityPendingShipment);
            Assert.Equal(10, salesItem.QuantityReserved);
            Assert.Equal(5, salesItem.QuantityShortFalled);

            // Rearrange
            this.CreateInventoryTransaction(-2, varianceReasons.Unknown, finishedGood);

            // Act
            this.Transaction.Derive();

            // Assert
            Assert.Equal(3, salesItem.QuantityRequestsShipping);
            Assert.Equal(0, salesItem.QuantityPendingShipment);
            Assert.Equal(10, salesItem.QuantityReserved);
            Assert.Equal(7, salesItem.QuantityShortFalled);
        }

        [Fact]
        public void GivenInventoryItemForUnifiedGood_WhenQuantityOnHandIsRaised_ThenSalesOrderItemWithQuantityShortFalledIsUpdated()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => Equals(v.Name, "internalOrganisation"));
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(internalOrganisation).Build();
            var customer = internalOrganisation.CreateB2BCustomer(this.Transaction.Faker());

            this.Transaction.Derive();

            this.CreateInventoryTransaction(5, new InventoryTransactionReasons(this.Transaction).Unknown, unifiedGood);

            this.Transaction.Derive();

            var order = this.CreateSalesOrder(customer, customer, this.Transaction.Now());
            var salesItem1 = this.CreateSalesOrderItem("item1", unifiedGood, 10, 15);
            order.AddSalesOrderItem(salesItem1);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive(true);

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(0, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(5, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            Assert.Equal(0, salesItem1.QuantityRequestsShipping);
            Assert.Equal(5, salesItem1.QuantityPendingShipment);
            Assert.Equal(10, salesItem1.QuantityReserved);
            Assert.Equal(5, salesItem1.QuantityShortFalled);

            Assert.Equal(0, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(5, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            // Re-arrange
            this.CreateInventoryTransaction(15, new InventoryTransactionReasons(this.Transaction).Unknown, unifiedGood);

            // Act
            this.Transaction.Derive(true);

            Assert.Equal(0, salesItem1.QuantityRequestsShipping);
            Assert.Equal(10, salesItem1.QuantityPendingShipment);
            Assert.Equal(10, salesItem1.QuantityReserved);
            Assert.Equal(0, salesItem1.QuantityShortFalled);

            Assert.Equal(10, salesItem1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(20, salesItem1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        // [Fact]
        // public void ReportNonSerialisedInventory()
        // {
        //    var supplier = new OrganisationBuilder(this.DatabaseTransaction).WithName("supplier").Build();
        //    var internalOrganisation = Singleton.Instance(this.DatabaseTransaction).InternalOrganisation;

        // new SupplierRelationshipBuilder(this.DatabaseTransaction)
        //        .WithSingleton(internalOrganisation)
        //        .WithSupplier(supplier)
        //        .WithFromDate(this.Transaction.Now())
        //        .Build();

        // var rawMaterial = new RawMaterialBuilder(this.DatabaseTransaction)
        //        .WithName("raw material")
        //        .WithInventoryItemKind(new InventoryItemKinds(this.DatabaseTransaction).NonSerialised)
        //        .WithUnitOfMeasure(new UnitsOfMeasure(this.DatabaseTransaction).Piece)
        //        .Build();

        // var level1 = new ProductCategoryBuilder(this.DatabaseTransaction).WithDescription("level1").Build();
        //    var level2 = new ProductCategoryBuilder(this.DatabaseTransaction).WithDescription("level2").WithPrimaryParent(level1).Build();
        //    var level3 = new ProductCategoryBuilder(this.DatabaseTransaction).WithDescription("level3").WithPrimaryParent(level2).Build();
        //    var category = new ProductCategoryBuilder(this.DatabaseTransaction).WithDescription("category").Build();

        // var good = new NonUnifiedGoodBuilder(this.DatabaseTransaction)
        //        .WithName("Good")
        //        .WithSku("10101")
        //        .WithVatRate(new VatRateBuilder(this.DatabaseTransaction).WithRate(21).Build())
        //        .WithInventoryItemKind(new InventoryItemKinds(this.DatabaseTransaction).NonSerialised)
        //        .WithUnitOfMeasure(new UnitsOfMeasure(this.DatabaseTransaction).Piece)
        //        .WithProductCategory(level3)
        //        .WithProductCategory(category)
        //        .Build();

        // var purchasePrice = new ProductPurchasePriceBuilder(this.DatabaseTransaction)
        //        .WithFromDate(this.Transaction.Now())
        //        .WithCurrency(new Currencies(this.DatabaseTransaction).FindBy(M.Currency.IsoCode, "EUR"))
        //        .WithPrice(1)
        //        .WithUnitOfMeasure(new UnitsOfMeasure(this.DatabaseTransaction).Piece)
        //        .Build();

        // var goodItem = new NonSerialisedInventoryItemBuilder(this.DatabaseTransaction)
        //        .WithGood(good)
        //        .WithAvailableToPromise(120)
        //        .WithQuantityOnHand(120)
        //        .Build();

        // var damagedItem = new NonSerialisedInventoryItemBuilder(this.DatabaseTransaction)
        //        .WithGood(good)
        //        .WithAvailableToPromise(120)
        //        .WithQuantityOnHand(120)
        //        .WithCurrentObjectState(new NonSerialisedInventoryItemStates(this.DatabaseTransaction).SlightlyDamaged)
        //        .Build();

        // var partItem = (NonSerialisedInventoryItem)rawMaterial.InventoryItemsWherePart[0];

        // new SupplierOfferingBuilder(this.DatabaseTransaction)
        //        .WithProduct(good)
        //        .WithPart(rawMaterial)
        //        .WithSupplier(supplier)
        //        .WithProductPurchasePrice(purchasePrice)
        //        .Build();

        // var valueByParameter = new Dictionary<Predicate, object>();

        // var preparedExtent = new Reports(this.DatabaseTransaction).FindByName(Constants.REPORTNONSERIALIZEDINVENTORY).PersistentPreparedExtent;
        // var parameters = preparedExtent.Parameters;

        // var extent = preparedExtent.Execute(valueByParameter);

        // Assert.Equal(3, extent.Count);
        // Assert.Contains(goodItem, extent);
        // Assert.Contains(damagedItem, extent);
        // Assert.Contains(partItem, extent);

        // valueByParameter[parameters[1]] = new NonSerialisedInventoryItemStates(this.DatabaseTransaction).SlightlyDamaged;

        // extent = preparedExtent.Execute(valueByParameter);

        // Assert.Single(extent.Count);
        // Assert.Contains(damagedItem, extent);

        // valueByParameter.Clear();
        // valueByParameter[parameters[4]] = level1;

        // extent = preparedExtent.Execute(valueByParameter);

        // Assert.Equal(2, extent.Count);
        // Assert.Contains(goodItem, extent);
        // Assert.Contains(damagedItem, extent);
        // }
        private Part CreatePart(string partId, InventoryItemKind kind)
            => new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification(partId)
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(kind).Build();

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

        private PartyContactMechanism CreateShipTo(ContactMechanism mechanism, ContactMechanismPurpose purpose, bool isDefault)
            => new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(mechanism).WithContactPurpose(purpose).WithUseAsDefault(isDefault).Build();

        private SalesOrder CreateSalesOrder(Party billTo, Party shipTo, DateTime deliveryDate)
            => new SalesOrderBuilder(this.Transaction).WithBillToCustomer(billTo).WithShipToCustomer(shipTo).WithDeliveryDate(deliveryDate).Build();

        private SalesOrder CreateSalesOrder(Party billTo, Party shipTo, DateTime deliveryDate, bool partialShip)
            => new SalesOrderBuilder(this.Transaction).WithBillToCustomer(billTo).WithShipToCustomer(shipTo).WithDeliveryDate(deliveryDate).WithPartiallyShip(partialShip).Build();

        private SalesOrderItem CreateSalesOrderItem(string description, Product product, decimal quantityOrdered, decimal unitPrice)
            => new SalesOrderItemBuilder(this.Transaction)
                .WithDescription(description)
                .WithProduct(product)
                .WithQuantityOrdered(quantityOrdered)
                .WithAssignedUnitPrice(unitPrice)
                .Build();

        private InventoryItemTransaction CreateInventoryTransaction(int quantity, InventoryTransactionReason reason, Part part)
            => new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(quantity).WithReason(reason).WithPart(part).Build();
    }

    public class NonSerialisedInventoryItemOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveNonSerialisedInventoryItemState()
        {
            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(new NonSerialisedInventoryItemStates(this.Transaction).Good, inventoryItem.NonSerialisedInventoryItemState);
        }
    }

    public class NonSerialisedInventoryItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveName()
        {
            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Part = new UnifiedGoodBuilder(this.Transaction).WithName("partname").Build();
            this.Transaction.Derive(false);

            Assert.Equal("partname at  with state Good", inventoryItem.Name);
        }

        [Fact]
        public void ChangedFacilityDeriveName()
        {
            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Facility = new FacilityBuilder(this.Transaction).WithName("facilityname").Build();
            this.Transaction.Derive(false);

            Assert.Equal(" at facilityname with state Good", inventoryItem.Name);
        }
    }

    public class NonSerialisedInventoryItemQuantityOnHandRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemQuantityOnHandRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNonSerialisedInventoryItemStateDeriveQuantityOnHand()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithNonSerialisedInventoryItemState(new NonSerialisedInventoryItemStateBuilder(this.Transaction).Build())
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            // InventoryItemState is excluded from InventoryStrategy
            Assert.Equal(0, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).QuantityOnHand);
        }

        [Fact]
        public void ChangedInventoryItemTransactionInventoryItemDeriveQuantityOnHand()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).QuantityOnHand);
        }

        [Fact]
        public void ChangedPickListItemInventoryItemDeriveQuantityOnHand()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(100)
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

            Assert.Equal(99, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).QuantityOnHand);
        }
    }

    public class NonSerialisedInventoryItemQuantityCommittedOutRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemQuantityCommittedOutRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInventoryItemTransactionInventoryItemDeriveQuantityCommittedOut()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).SalesOrder)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).QuantityCommittedOut);
        }

        [Fact]
        public void ChangedPickListItemInventoryItemDeriveQuantityCommittedOut()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(100)
                .Build();
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).SalesOrder)
                .WithPart(part)
                .WithQuantity(100)
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

            Assert.Equal(99, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).QuantityCommittedOut);
        }
    }

    public class NonSerialisedInventoryItemAvailableToPromiseRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemAvailableToPromiseRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityOnHandDeriveAvailableToPromise()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).AvailableToPromise);
        }

        [Fact]
        public void ChangedQuantityCommittedOutDeriveAvailableToPromise()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(new FacilityBuilder(this.Transaction).Build())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithPart(part)
                .WithQuantity(100)
                .Build();
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithReason(new InventoryTransactionReasons(this.Transaction).SalesOrder)
                .WithPart(part)
                .WithQuantity(10)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(90, ((NonSerialisedInventoryItem)inventoryItemTransaction.InventoryItem).AvailableToPromise);
        }
    }

    public class NonSerialisedInventoryItemQuantityExpectedInRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonSerialisedInventoryItemQuantityExpectedInRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderItemDeriveQuantityExpectedIn()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).InProcess)
                .WithPart(part)
                .WithQuantityOrdered(1)
                .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, ((NonSerialisedInventoryItem)part.InventoryItemsWherePart.First).QuantityExpectedIn);
        }

        [Fact]
        public void ChangedPurchaseOrderItemPurchaseOrderItemStateDeriveQuantityExpectedIn()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).InProcess)
                .WithPart(part)
                .WithQuantityOrdered(1)
                .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Transaction.Derive(false);

            purchaseOrderItem.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Transaction).Cancelled;
            this.Transaction.Derive(false);

            Assert.Equal(0, ((NonSerialisedInventoryItem)part.InventoryItemsWherePart.First).QuantityExpectedIn);
        }

        [Fact]
        public void ChangedPurchaseOrderItemQuantityOrderedDeriveQuantityExpectedIn()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).InProcess)
                .WithPart(part)
                .WithQuantityOrdered(1)
                .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Transaction.Derive(false);

            purchaseOrderItem.QuantityOrdered = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, ((NonSerialisedInventoryItem)part.InventoryItemsWherePart.First).QuantityExpectedIn);
        }
    }
}
