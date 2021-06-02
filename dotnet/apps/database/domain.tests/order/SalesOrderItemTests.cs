// <copyright file="SalesOrderItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Domain.TestPopulation;
    using Derivations.Errors;
    using Resources;
    using Xunit;

    public class SalesOrderItemTests : DomainTest, IClassFixture<Fixture>
    {
        private ProductCategory productCategory;
        private ProductCategory ancestorProductCategory;
        private ProductCategory parentProductCategory;
        private Good good;
        private readonly Good variantGood;
        private readonly Good variantGood2;
        private Good virtualGood;
        private Part part;
        private Colour feature1;
        private Colour feature2;
        private Organisation shipToCustomer;
        private Organisation billToCustomer;
        private Organisation supplier;
        private City kiev;
        private PostalAddress shipToContactMechanismMechelen;
        private PostalAddress shipToContactMechanismKiev;
        private BasePrice currentBasePriceGeoBoundary;
        private BasePrice currentGoodBasePrice;
        private BasePrice currentGood1Feature1BasePrice;
        private BasePrice currentFeature2BasePrice;
        private SupplierOffering goodPurchasePrice;
        private SupplierOffering virtualGoodPurchasePrice;
        private SalesOrder order;
        private VatRegime vatRegime;

        public SalesOrderItemTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            this.supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            this.vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            this.kiev = new CityBuilder(this.Transaction).WithName("Kiev").Build();

            this.shipToContactMechanismMechelen = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            this.shipToContactMechanismKiev = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(this.kiev).WithAddress1("Dnieper").Build();
            this.shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("shipToCustomer").Build();
            this.shipToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.billToCustomer = new OrganisationBuilder(this.Transaction)
                .WithName("billToCustomer")
                .WithPreferredCurrency(euro)

                .Build();

            this.billToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("good")
                .WithPart(this.part)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(this.billToCustomer).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(this.shipToCustomer).Build();

            this.variantGood = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("variant good")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("2")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            this.variantGood2 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v10102")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("variant good2")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("3")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            this.virtualGood = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v10103")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("virtual good")
                .WithVariant(this.variantGood)
                .WithVariant(this.variantGood2)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            this.ancestorProductCategory = new ProductCategoryBuilder(this.Transaction)
                .WithName("ancestor")
                .Build();

            this.parentProductCategory = new ProductCategoryBuilder(this.Transaction)
                .WithName("parent")
                .WithPrimaryParent(this.ancestorProductCategory)
                .Build();

            this.productCategory = new ProductCategoryBuilder(this.Transaction)
                .WithName("gizmo")
                .Build();

            this.productCategory.AddSecondaryParent(this.parentProductCategory);

            this.goodPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.part)
                .WithSupplier(this.supplier)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithFromDate(this.Transaction.Now())
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.virtualGoodPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithCurrency(euro)
                .WithFromDate(this.Transaction.Now())
                .WithSupplier(this.supplier)
                .WithPrice(8)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            this.feature1 = new ColourBuilder(this.Transaction)
                .WithVatRegime(this.vatRegime)
                .WithName("white")
                .Build();

            this.feature2 = new ColourBuilder(this.Transaction)
                .WithName("black")
                .Build();

            this.currentBasePriceGeoBoundary = new BasePriceBuilder(this.Transaction)
                .WithDescription("current BasePriceGeoBoundary ")
                .WithGeographicBoundary(mechelen)
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Transaction.Now())
                .Build();

            // historic basePrice for good
            new BasePriceBuilder(this.Transaction).WithDescription("previous good")
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for good
            new BasePriceBuilder(this.Transaction).WithDescription("future good")
                .WithProduct(this.good)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentGoodBasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current good")
                .WithProduct(this.good)
                .WithPrice(10)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature1
            new BasePriceBuilder(this.Transaction).WithDescription("previous feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(0.5M)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for feature1
            new BasePriceBuilder(this.Transaction).WithDescription("future feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2.5M)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithDescription("current feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature2
            new BasePriceBuilder(this.Transaction).WithDescription("previous feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for feature2
            new BasePriceBuilder(this.Transaction)
                .WithDescription("future feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(4)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentFeature2BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(3)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for good with feature1
            new BasePriceBuilder(this.Transaction).WithDescription("previous good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(4)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for good with feature1
            new BasePriceBuilder(this.Transaction)
                .WithDescription("future good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(6)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentGood1Feature1BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(5)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithDescription("current variant good2")
                .WithProduct(this.variantGood2)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.order = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenOrderItemWithoutVatRegime_WhenDeriving_ThenDerivedVatRegimeIsFromOrder()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(salesOrder.AssignedVatRegime, orderItem.DerivedVatRegime);
        }

        [Fact]
        public void GivenOrderItemWithoutVatRate_WhenDeriving_ThenItemDerivedVatRateIsFromOrderVatRegime()
        {
            this.InstantiateObjects(this.Transaction);

            var expected = new VatRates(this.Transaction).Belgium21;

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(expected, orderItem.VatRate);
        }

        [Fact]
        public void GivenOrderItemWithAssignedDeliveryDate_WhenDeriving_ThenDeliveryDateIsOrderItemAssignedDeliveryDate()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.billToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(1)
                .WithAssignedDeliveryDate(this.Transaction.Now().AddMonths(1))
                .Build();

            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, orderItem.AssignedDeliveryDate);
        }

        [Fact]
        public void GivenOrderItemWithoutDeliveryDate_WhenDeriving_ThenDerivedDeliveryDateIsOrderDeliveryDate()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.billToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithDeliveryDate(this.Transaction.Now().AddMonths(1))
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(1)
                .Build();

            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, salesOrder.DeliveryDate);
        }

        [Fact]
        public void GivenOrderItem_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            this.InstantiateObjects(this.Transaction);

            var builder = new SalesOrderItemBuilder(this.Transaction);
            var orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithProduct(this.good);
            orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            this.Transaction.Rollback();

            builder.WithQuantityOrdered(1);
            orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            Assert.False(this.Transaction.Derive(false).HasErrors);

            builder.WithProductFeature(this.feature1);
            orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItem_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            this.InstantiateObjects(this.Transaction);

            var item = new SalesOrderItemBuilder(this.Transaction).Build();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Provisional, item.SalesOrderItemState);
        }

        [Fact]
        public void GivenOrderItemWithOrderedWithFeature_WhenDeriving_ThenOrderedWithFeatureOrderItemMustBeForProductFeature()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Transaction.Derive();

            var productOrderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            var productFeatureOrderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem)
                .WithProductFeature(this.feature1)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            productOrderItem.AddOrderedWithFeature(productFeatureOrderItem);
            salesOrder.AddSalesOrderItem(productOrderItem);
            salesOrder.AddSalesOrderItem(productFeatureOrderItem);

            Assert.False(this.Transaction.Derive(false).HasErrors);

            productFeatureOrderItem.RemoveProductFeature();
            productFeatureOrderItem.Product = this.good;

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItemWithoutCustomer_WhenDeriving_ShipToAddressIsNull()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(this.billToCustomer).Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Null(orderItem.DerivedShipToAddress);
            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItemWithoutShipFromAddress_WhenDeriving_ThenDerivedShipFromAddressIsFromOrder()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithAssignedShipFromAddress(this.shipToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(this.shipToContactMechanismMechelen, orderItem.DerivedShipFromAddress);
        }

        [Fact]
        public void GivenOrderItemWithoutShipToAddress_WhenDeriving_ThenDerivedShipToAddressIsFromOrder()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(this.shipToContactMechanismMechelen, orderItem.DerivedShipToAddress);
        }

        [Fact]
        public void GivenOrderItemWithoutShipToParty_WhenDeriving_ThenDerivedShipToPartyIsFromOrder()
        {
            this.InstantiateObjects(this.Transaction);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            Assert.Equal(this.shipToCustomer, orderItem.DerivedShipToParty);
        }

        [Fact]
        public void GivenOrderItemForGoodWithoutSelectedInventoryItem_WhenConfirming_ThenReservedFromNonSerialisedInventoryItemIsFromDefaultFacility()
        {
            this.InstantiateObjects(this.Transaction);

            var good2 = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good2PurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.part)
                .WithSupplier(this.supplier)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithFromDate(this.Transaction.Now())
                .WithPrice(7)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            //// good with part as inventory item
            var item1 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            var item2 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good2)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item1);
            this.order.AddSalesOrderItem(item2);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();

            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive(true);

            this.order.Accept();
            this.Transaction.Derive(true);

            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), item1.ReservedFromNonSerialisedInventoryItem.Facility);
            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), item2.ReservedFromNonSerialisedInventoryItem.Facility);
        }

        //[Fact]
        //public void GivenConfirmedOrderItemForGood_WhenReservedFromNonSerialisedInventoryItemChangesValue_ThenQuantitiesAreMovedFromOldToNewInventoryItem()
        //{
        //    this.InstantiateObjects(this.Transaction);

        //    new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

        //    this.Transaction.Derive();

        //    var secondWarehouse = new FacilityBuilder(this.Transaction)
        //        .WithName("affiliate warehouse")
        //        .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
        //        .WithOwner(this.InternalOrganisation)
        //        .Build();

        //    var order1 = new SalesOrderBuilder(this.Transaction)
        //        .WithShipToCustomer(this.shipToCustomer)
        //        .WithBillToCustomer(this.billToCustomer)
        //        .WithPartiallyShip(false)
        //        .Build();

        //this.Transaction.Derive();

        //    var salesOrderItem = new SalesOrderItemBuilder(this.Transaction)
        //        .WithProduct(this.good)
        //        .WithQuantityOrdered(3)
        //        .WithAssignedUnitPrice(5)
        //        .Build();

        //    order1.AddSalesOrderItem(salesOrderItem);

        //    this.Transaction.Derive();

        //    order1.Confirm();

        //    this.Transaction.Derive();

        //    order1.Send();

        //    this.Transaction.Derive(true);

        //    Assert.Equal(3, salesOrderItem.QuantityOrdered);
        //    Assert.Equal(0, salesOrderItem.QuantityShipped);
        //    Assert.Equal(0, salesOrderItem.QuantityPendingShipment);
        //    Assert.Equal(3, salesOrderItem.QuantityReserved);
        //    Assert.Equal(2, salesOrderItem.QuantityShortFalled);
        //    Assert.Equal(1, salesOrderItem.QuantityRequestsShipping);
        //    Assert.Equal(1, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
        //    Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
        //    Assert.Equal(1, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

        //    var previous = salesOrderItem.ReservedFromNonSerialisedInventoryItem;

        //    var transaction = new InventoryItemTransactionBuilder(this.Transaction).WithFacility(secondWarehouse).WithPart(this.part).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).Build();

        //    this.Transaction.Derive();

        //    var current = transaction.InventoryItem as NonSerialisedInventoryItem;

        //    salesOrderItem.ReservedFromNonSerialisedInventoryItem = current;

        //    this.Transaction.Derive();

        //    Assert.Equal(3, salesOrderItem.QuantityOrdered);
        //    Assert.Equal(0, salesOrderItem.QuantityShipped);
        //    Assert.Equal(0, salesOrderItem.QuantityPendingShipment);
        //    Assert.Equal(3, salesOrderItem.QuantityReserved);
        //    Assert.Equal(2, salesOrderItem.QuantityShortFalled);
        //    Assert.Equal(1, salesOrderItem.QuantityRequestsShipping);
        //    Assert.Equal(0, previous.QuantityCommittedOut);
        //    Assert.Equal(1, previous.AvailableToPromise);
        //    Assert.Equal(1, previous.QuantityOnHand);
        //    Assert.Equal(1, current.QuantityCommittedOut);
        //    Assert.Equal(0, current.AvailableToPromise);
        //    Assert.Equal(1, current.QuantityOnHand);
        //}

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenOrderItemIsCancelled_ThenNonSerialisedInventoryQuantitiesAreReleased()
        {
            this.InstantiateObjects(this.Transaction);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(3).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(salesOrderItem);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive(true);

            this.order.Accept();
            this.Transaction.Derive(true);

            Assert.Equal(salesOrderItem.QuantityOrdered, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);

            this.Transaction.Derive();

            salesOrderItem.Cancel();

            this.Transaction.Derive();

            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenOrderItemIsRejected_ThenNonSerialisedInventoryQuantitiesAreReleased()
        {
            this.InstantiateObjects(this.Transaction);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(3).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(salesOrderItem);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive(true);

            this.order.Accept();
            this.Transaction.Derive(true);

            Assert.Equal(salesOrderItem.QuantityOrdered, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);

            this.Transaction.Derive();

            salesOrderItem.Reject();

            this.Transaction.Derive();

            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenOrderItemForGoodWithEnoughStockAvailable_WhenConfirming_ThenQuantitiesReservedAndRequestsShippingAreEqualToQuantityOrdered()
        {
            this.InstantiateObjects(this.Transaction);

            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(100)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(100, item.QuantityOrdered);
            Assert.Equal(0, item.QuantityShipped);
            Assert.Equal(100, item.QuantityPendingShipment);
            Assert.Equal(100, item.QuantityReserved);
            Assert.Equal(0, item.QuantityShortFalled);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(100, item.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(10, item.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(110, item.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenOrderItemForGoodWithNotEnoughStockAvailable_WhenConfirming_ThenQuantitiesReservedAndRequestsShippingAreEqualToInventoryAvailableToPromise()
        {
            this.InstantiateObjects(this.Transaction);

            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(120)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(120, item1.QuantityOrdered);
            Assert.Equal(0, item1.QuantityShipped);
            Assert.Equal(110, item1.QuantityPendingShipment);
            Assert.Equal(120, item1.QuantityReserved);
            Assert.Equal(10, item1.QuantityShortFalled);
            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(110, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(110, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            var item2 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item2);
            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(120, item1.QuantityOrdered);
            Assert.Equal(0, item1.QuantityShipped);
            Assert.Equal(110, item1.QuantityPendingShipment);
            Assert.Equal(120, item1.QuantityReserved);
            Assert.Equal(10, item1.QuantityShortFalled);
            Assert.Equal(0, item1.QuantityRequestsShipping);

            Assert.Equal(10, item2.QuantityOrdered);
            Assert.Equal(0, item2.QuantityShipped);
            Assert.Equal(0, item2.QuantityPendingShipment);
            Assert.Equal(10, item2.QuantityReserved);
            Assert.Equal(10, item2.QuantityShortFalled);
            Assert.Equal(0, item2.QuantityRequestsShipping);
            Assert.Equal(110, item2.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item2.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(110, item2.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenShipmentIsCreated_ThenQuantitiesRequestsShippingIsSetToZero()
        {
            this.InstantiateObjects(this.Transaction);

            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(100)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(100, item.QuantityOrdered);
            Assert.Equal(0, item.QuantityShipped);
            Assert.Equal(100, item.QuantityPendingShipment);
            Assert.Equal(100, item.QuantityReserved);
            Assert.Equal(0, item.QuantityShortFalled);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(100, item.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(10, item.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(110, item.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            Assert.Equal(100, item.OrderShipmentsWhereOrderItem[0].Quantity);
        }

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenQuantityOrderedIsDecreased_ThenQuantitiesReservedAndRequestsShippingAndInventoryAvailableToPromiseDecreaseEqually()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.AutoGenerateCustomerShipment = false;

            this.InstantiateObjects(this.Transaction);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(100)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            this.Transaction.Commit();

            item.QuantityOrdered = 50;

            this.Transaction.Derive();

            Assert.Equal(50, item.QuantityOrdered);
            Assert.Equal(0, item.QuantityShipped);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(50, item.QuantityReserved);
            Assert.Equal(0, item.QuantityShortFalled);
            Assert.Equal(50, item.QuantityRequestsShipping);
            Assert.Equal(50, item.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(60, item.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(110, item.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenQuantityOrderedIsDecreased_ThenQuantityMayNotBeLessThanQuantityShipped()
        {
            this.InstantiateObjects(this.Transaction);

            var manual = new OrderKindBuilder(this.Transaction).WithDescription("manual").WithScheduleManually(true).Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(120)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(orderItem);
            this.order.OrderKind = manual;
            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipFromAddress(this.order.TakenBy.ShippingAddress)
                .WithShipToParty(this.order.ShipToCustomer)
                .WithShipToAddress(this.order.DerivedShipToAddress)
                .WithShipmentPackage(new ShipmentPackageBuilder(this.Transaction).Build())
                .WithShipmentMethod(this.order.DerivedShipmentMethod)
                .Build();

            this.Transaction.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithGood(orderItem.Product as Good)
                .WithQuantity(100)
                .WithReservedFromInventoryItem(orderItem.ReservedFromNonSerialisedInventoryItem)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            new OrderShipmentBuilder(this.Transaction)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipment.ShipmentItems.First)
                .WithQuantity(100)
                .Build();

            this.Transaction.Derive();

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Transaction.Derive();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem item in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(item).WithQuantity(item.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(100, orderItem.QuantityShipped);

            orderItem.QuantityOrdered = 90;
            var derivationLog = this.Transaction.Derive(false);

            Assert.True(derivationLog.HasErrors);
        }

        [Fact]
        public void GivenOrderItemWithPendingShipmentAndItemsShortFalled_WhenQuantityOrderedIsDecreased_ThenItemsShortFalledIsDecreasedAndShipmentIsLeftUnchanged()
        {
            this.InstantiateObjects(this.Transaction);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(30)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(20, item.QuantityShortFalled);

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;
            Assert.Equal(10, shipment.ShipmentItems[0].Quantity);

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            Assert.Equal(10, pickList.PickListItems[0].Quantity);

            item.QuantityOrdered = 11;

            this.Transaction.Derive();

            Assert.Equal(1, item.QuantityShortFalled);

            Assert.Equal(10, shipment.ShipmentItems[0].Quantity);
            Assert.Equal(10, pickList.PickListItems[0].Quantity);

            item.QuantityOrdered = 10;

            this.Transaction.Derive();

            Assert.Equal(0, item.QuantityShortFalled);

            Assert.Equal(10, shipment.ShipmentItems[0].Quantity);
            Assert.Equal(10, pickList.PickListItems[0].Quantity);
        }

        [Fact]
        public void GivenManuallyScheduledOrderItem_WhenScheduled_ThenQuantityCannotExceedInventoryAvailableToPromise()
        {
            this.InstantiateObjects(this.Transaction);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(3).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var manual = new OrderKindBuilder(this.Transaction).WithDescription("manual").WithScheduleManually(true).Build();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithOrderKind(manual)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(5)
                .WithAssignedUnitPrice(5)
                .Build();

            order1.AddSalesOrderItem(orderItem);
            this.Transaction.Derive();

            order1.SetReadyForPosting();
            this.Transaction.Derive();

            order1.Post();
            this.Transaction.Derive();

            order1.Accept();
            this.Transaction.Derive();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipFromAddress(this.order.TakenBy.ShippingAddress)
                .WithShipToParty(this.order.ShipToCustomer)
                .WithShipToAddress(this.order.DerivedShipToAddress)
                .WithShipmentPackage(new ShipmentPackageBuilder(this.Transaction).Build())
                .WithShipmentMethod(this.order.DerivedShipmentMethod)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithGood(orderItem.Product as Good)
                .WithQuantity(5)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            var orderShipment = new OrderShipmentBuilder(this.Transaction)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipment.ShipmentItems.First)
                .WithQuantity(5)
                .Build();

            var expectedMessage = $"{orderShipment}, {this.M.OrderShipment.Quantity}, {ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            this.Transaction.Rollback();

            shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithGood(orderItem.Product as Good)
                .WithQuantity(3)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            new OrderShipmentBuilder(this.Transaction)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipment.ShipmentItems.First)
                .WithQuantity(3)
                .Build();

            var derivationLog = this.Transaction.Derive();

            Assert.False(derivationLog.HasErrors);
            Assert.Equal(3, orderItem.QuantityPendingShipment);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.productCategory = (ProductCategory)transaction.Instantiate(this.productCategory);
            this.parentProductCategory = (ProductCategory)transaction.Instantiate(this.parentProductCategory);
            this.ancestorProductCategory = (ProductCategory)transaction.Instantiate(this.ancestorProductCategory);
            this.part = (Part)transaction.Instantiate(this.part);
            this.virtualGood = (Good)transaction.Instantiate(this.virtualGood);
            this.good = (Good)transaction.Instantiate(this.good);
            this.feature1 = (Colour)transaction.Instantiate(this.feature1);
            this.feature2 = (Colour)transaction.Instantiate(this.feature2);
            this.shipToCustomer = (Organisation)transaction.Instantiate(this.shipToCustomer);
            this.billToCustomer = (Organisation)transaction.Instantiate(this.billToCustomer);
            this.supplier = (Organisation)transaction.Instantiate(this.supplier);
            this.kiev = (City)transaction.Instantiate(this.kiev);
            this.shipToContactMechanismMechelen = (PostalAddress)transaction.Instantiate(this.shipToContactMechanismMechelen);
            this.shipToContactMechanismKiev = (PostalAddress)transaction.Instantiate(this.shipToContactMechanismKiev);
            this.currentBasePriceGeoBoundary = (BasePrice)transaction.Instantiate(this.currentBasePriceGeoBoundary);
            this.currentGoodBasePrice = (BasePrice)transaction.Instantiate(this.currentGoodBasePrice);
            this.currentGood1Feature1BasePrice = (BasePrice)transaction.Instantiate(this.currentGood1Feature1BasePrice);
            this.currentFeature2BasePrice = (BasePrice)transaction.Instantiate(this.currentFeature2BasePrice);
            this.goodPurchasePrice = (SupplierOffering)transaction.Instantiate(this.goodPurchasePrice);
            this.virtualGoodPurchasePrice = (SupplierOffering)transaction.Instantiate(this.virtualGoodPurchasePrice);
            this.currentGoodBasePrice = (BasePrice)transaction.Instantiate(this.currentGoodBasePrice);
            this.order = (SalesOrder)transaction.Instantiate(this.order);
            this.vatRegime = (VatRegime)transaction.Instantiate(this.vatRegime);
        }
    }

    public class SalesOrderItemOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderItemState()
        {
            var order = new SalesOrderItemBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(order.ExistSalesOrderItemState);
        }

        [Fact]
        public void DeriveSalesOrderItemShipmentState()
        {
            var order = new SalesOrderItemBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(order.ExistSalesOrderItemShipmentState);
        }

        [Fact]
        public void DeriveSalesOrderIteminvoiceState()
        {
            var order = new SalesOrderItemBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(order.ExistSalesOrderItemInvoiceState);
        }

        [Fact]
        public void DeriveSalesOrderItemPaymentState()
        {
            var order = new SalesOrderItemBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(order.ExistSalesOrderItemPaymentState);
        }

        [Fact]
        public void DeriveInvoiceItemType()
        {
            var order = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.True(order.ExistInvoiceItemType);
        }
    }

    public class SalesOrderItemProvisionalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemProvisionalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderItemStateDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithAssignedShipFromAddress(new PostalAddressBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithSalesOrderItemState(new SalesOrderItemStates(this.Transaction).Cancelled).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            orderItem.SalesOrderItemState = new SalesOrderItemStates(this.Transaction).Provisional;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, order.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderItemsDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithAssignedShipFromAddress(new PostalAddressBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, order.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedAssignedShipFromAddressDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedShipFromAddress(new PostalAddressBuilder(this.Transaction).Build()).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, orderItem.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedsalesOrderDerivedShipFromAddressDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedShipFromAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, order.DerivedShipFromAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).Build()).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipToAddress, orderItem.AssignedShipToAddress);
        }

        [Fact]
        public void ChangedShipToPartyShippingAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedShipToParty(customer).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipToAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedsalesOrderDerivedShipToAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedShipToAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedShipToAddress, order.DerivedShipToAddress);
        }

        [Fact]
        public void ChangedAssignedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedDeliveryDate(this.Transaction.Now().Date).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, orderItem.AssignedDeliveryDate);
        }

        [Fact]
        public void ChangedsalesOrderDerivedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.DeliveryDate = this.Transaction.Now().Date;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedVatRegime(new VatRegimes(this.Transaction).SpainReduced).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedVatRegime, orderItem.AssignedVatRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Transaction).SpainReduced;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedDerivedVatRegimeDeriveVatRate()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Transaction).SpainReduced;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.VatRate, order.AssignedVatRegime.VatRates[0]);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable15).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedIrpfRegime, orderItem.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedDerivedIrpfRegimeDeriveIrpfRate()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.IrpfRate, order.AssignedIrpfRegime.IrpfRates[0]);
        }

        [Fact]
        public void ChangedSalesOrderOrderDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates[0].ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Transaction.Derive(false);

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithOrderDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(newVatRate, orderItem.VatRate);

            order.OrderDate = this.Transaction.Now().AddDays(1).Date;
            this.Transaction.Derive(false);

            Assert.Equal(newVatRate, orderItem.VatRate);
        }
    }

    public class SalesOrderItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductDeriveInvoiceItemType()
        {
            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.Product = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(new InvoiceItemTypes(this.Transaction).ProductItem, item.InvoiceItemType);
        }

        [Fact]
        public void ChangedInvoiceItemTypeDeriveInvoiceItemType()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(new UnifiedGoodBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .Build();
            this.Transaction.Derive(false);

            item.RemoveInvoiceItemType();
            this.Transaction.Derive(false);

            Assert.Equal(new InvoiceItemTypes(this.Transaction).ProductItem, item.InvoiceItemType);
        }

        [Fact]
        public void ChangedSerialisedItemValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertExists: SalesOrderItem.NextSerialisedItemAvailability")));
        }

        [Fact]
        public void ChangedNextSerialisedItemAvailabilityValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Available)
                .Build();
            this.Transaction.Derive(false);

            item.RemoveNextSerialisedItemAvailability();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertExists: SalesOrderItem.NextSerialisedItemAvailability")));
        }

        [Fact]
        public void ChangedProductWhenSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithQuantityOrdered(2)
                .Build();
            this.Transaction.Derive(false);

            item.Product = product;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedWhenSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(product)
                .Build();
            this.Transaction.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedProductWhenNonSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithQuantityOrdered(0)
                .Build();
            this.Transaction.Derive(false);

            item.Product = product;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedWhenNonSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(product)
                .Build();
            this.Transaction.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .Build();
            this.Transaction.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorRequired()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(product)
                .Build();
            this.Transaction.Derive(false);

            item.QuantityOrdered = 0;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains("QuantityOrdered is Required"));
        }

        [Fact]
        public void ChangedAssignedUnitPriceValidationErrorRequired()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .Build();
            this.Transaction.Derive(false);

            item.AssignedUnitPrice = 0;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains("Price is Required"));
        }

        [Fact]
        public void ChangedProductValidationError()
        {
            var product1 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            var product2 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(product1)
                .Build();
            this.Transaction.Derive(false);

            item.Product = product2;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemProductChangeNotAllowed));
        }

        [Fact]
        public void SalesOrderItemWhereOrderedWithFeatureValidationErrorProductFeature()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.AddOrderedWithFeature(new SalesOrderItemBuilder(this.Transaction).Build());

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertExists: SalesOrderItem.ProductFeature")));
        }

        [Fact]
        public void SalesOrderItemWhereOrderedWithFeatureValidationErrorProduct()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.AddOrderedWithFeature(new SalesOrderItemBuilder(this.Transaction).WithProduct(product).Build());

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertNotExists: SalesOrderItem.Product")));
        }

        [Fact]
        public void ChangedProductFeatureValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .Build();
            this.Transaction.Derive(false);

            item.ProductFeature = new ColourBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertNotExists: SalesOrderItem.ProductFeature")));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            Assert.True(item.QuantityShipped > 0);

            item.QuantityOrdered = item.QuantityShipped - 1;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemLessThanAlreadeyShipped));
        }

        [Fact]
        public void ChangedProductFeatureValidationErrorAtMostOne()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(product)
                .Build();
            this.Transaction.Derive(false);

            item.ProductFeature = new ColourBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.Product\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedProductValidationErrorAtMostOne()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProductFeature(new ColourBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            item.Product = product;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.Product\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedSerialisedItemValidationErrorAtMostOne()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProductFeature(new ColourBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            item.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.SerialisedItem\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedSerialisedItemValidationErrorAtMostOne_2()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            item.ProductFeature = new ColourBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.SerialisedItem\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedReservedFromNonSerialisedInventoryItemValidationErrorAtMostOne()
        {
            var serialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(serialisedGood)
                .Build();
            var nonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(nonSerialisedGood)
                .Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithReservedFromSerialisedInventoryItem(serialisedInventoryItem)
                .Build();
            this.Transaction.Derive(false);

            item.ReservedFromNonSerialisedInventoryItem = nonSerialisedInventoryItem;

            var error = (DerivationErrorAtMostOne)this.Transaction.Derive(false).Errors.Single();
            Assert.Equal(2, error.RoleTypes.Length);
            Assert.Contains(this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem, error.RoleTypes);
            Assert.Contains(this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem, error.RoleTypes);
        }

        [Fact]
        public void ChangedReservedFromSerialisedInventoryItemValidationErrorAtMostOne()
        {
            var serialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(serialisedGood)
                .Build();
            var nonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(nonSerialisedGood)
                .Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithReservedFromNonSerialisedInventoryItem(nonSerialisedInventoryItem)
                .Build();
            this.Transaction.Derive(false);

            item.ReservedFromSerialisedInventoryItem = serialisedInventoryItem;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.ReservedFromSerialisedInventoryItem\nSalesOrderItem.ReservedFromNonSerialisedInventoryItem"));
        }

        [Fact]
        public void ChangedDiscountAdjustmentsValidationErrorAtMostOne()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithAssignedUnitPrice(1)
                .Build();
            this.Transaction.Derive(false);

            item.AddDiscountAdjustment(new DiscountAdjustmentBuilder(this.Transaction).Build());

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.AssignedUnitPrice\nSalesOrderItem.DiscountAdjustments\nSalesOrderItem.SurchargeAdjustments"));
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsValidationErrorAtMostOne()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithAssignedUnitPrice(1)
                .Build();
            this.Transaction.Derive(false);

            item.AddSurchargeAdjustment(new SurchargeAdjustmentBuilder(this.Transaction).Build());

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.AssignedUnitPrice\nSalesOrderItem.DiscountAdjustments\nSalesOrderItem.SurchargeAdjustments"));
        }

        [Fact]
        public void ChangedAssignedUnitPriceValidationErrorAtMostOne()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithSurchargeAdjustment(new SurchargeAdjustmentBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            item.AssignedUnitPrice = 1;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.AssignedUnitPrice\nSalesOrderItem.DiscountAdjustments\nSalesOrderItem.SurchargeAdjustments"));

            var error = (DerivationErrorAtMostOne)this.Transaction.Derive(false).Errors.Single();
            Assert.Equal(3, error.RoleTypes.Length);
            Assert.Contains(this.M.SalesOrderItem.AssignedUnitPrice, error.RoleTypes);
            Assert.Contains(this.M.SalesOrderItem.DiscountAdjustments, error.RoleTypes);
            Assert.Contains(this.M.SalesOrderItem.SurchargeAdjustments, error.RoleTypes);

        }

        [Fact]
        public void ChangedReservedFromSerialisedInventoryItemValidationError()
        {
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            var anotherNonSerialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var nonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(nonSerialisedGood)
                .Build();
            var anotherNonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(anotherNonSerialisedGood)
                .Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithReservedFromNonSerialisedInventoryItem(nonSerialisedInventoryItem)
                .Build();
            this.Transaction.Derive(false);

            order.AddSalesOrderItem(item);
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive(false);

            order.Post();
            this.Transaction.Derive(false);

            order.Accept();
            this.Transaction.Derive(false);

            item.ReservedFromNonSerialisedInventoryItem = anotherNonSerialisedInventoryItem;

            var error = this.Transaction.Derive(false).Errors.Single();
            Assert.Contains(ErrorMessages.ReservedFromNonSerialisedInventoryItem, error.Message);
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorSerializedItemQuantity()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }

        [Fact]
        public void ChangedSerialisedItemValidationErrorSerializedItemQuantity()
        {
            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithQuantityOrdered(2)
                .Build();
            this.Transaction.Derive(false);

            item.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }

        [Fact]
        public void ChangedSerialisedInventoryItemQuantityDeriveReservedFromSerialisedInventoryItem()
        {
            var serialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            serialisedGood.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(serialisedGood)
                .WithSerialisedItem(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            order.AddSalesOrderItem(item);
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithSerialisedItem(serialisedItem)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(item.ReservedFromSerialisedInventoryItem, serialisedItem.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => v.Quantity == 1));
        }

        [Fact]
        public void ChangedInventoryItemTransactionPartDeriveReservedFromNonSerialisedInventoryItem()
        {
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(nonSerialisedGood)
                .Build();
            this.Transaction.Derive(false);

            order.AddSalesOrderItem(item);
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithPart(nonSerialisedGood)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(item.ReservedFromNonSerialisedInventoryItem, nonSerialisedGood.InventoryItemsWherePart.First);
        }

        [Fact]
        public void ChangedProductDeriveSalesOrderSalesOrderItemsByProduct()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(product, order.SalesOrderItemsByProduct.First.Product);
        }
    }

    public class SalesOrderItemShipmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemShipmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderShipmentQuantityDeriveQuantityPendingShipment()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive(false);

            order.Post();
            this.Transaction.Derive(false);

            order.Accept();
            this.Transaction.Derive(false);

            order.Ship();
            this.Transaction.Derive(false);

            Assert.Equal(item.QuantityOrdered, item.QuantityPendingShipment);
        }

        [Fact]
        public void ChangedShipmentItemShipmentItemStateDeriveQuantityShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive(false);

            order.Post();
            this.Transaction.Derive(false);

            order.Accept();
            this.Transaction.Derive(false);

            order.Ship();
            this.Transaction.Derive(false);

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            Assert.Equal(item.QuantityOrdered, item.QuantityShipped);
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorQuantityPendingShipment()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive(false);

            order.Post();
            this.Transaction.Derive(false);

            order.Accept();
            this.Transaction.Derive(false);

            order.Ship();
            this.Transaction.Derive(false);

            item.QuantityOrdered -= 1;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorQuantityShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive(false);

            order.Post();
            this.Transaction.Derive(false);

            order.Accept();
            this.Transaction.Derive(false);

            order.Ship();
            this.Transaction.Derive(false);

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            item.QuantityOrdered -= 1;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining));
        }
    }

    public class SalesOrderItemStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateProvisional()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsReadyForPosting);

            order.Revise();
            this.Transaction.Derive();

            Assert.True(item.SalesOrderItemState.IsProvisional);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateReadyForPosting()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsReadyForPosting);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateRequestsApproval()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            this.InternalOrganisation.StoresWhereInternalOrganisation.First.OrderThreshold = order.TotalExVat + 1;

            order.SetReadyForPosting();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsRequestsApproval);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateAwaitingAcceptance()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsAwaitingAcceptance);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateInProcess()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsInProcess);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateOnHold()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Hold();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsOnHold);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateCancelled()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.Cancel();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsCancelled);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateRejected()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            order.Reject();
            this.Transaction.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsRejected);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateFinished()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First;
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive(false);

            order.Post();
            this.Transaction.Derive(false);

            order.Accept();
            this.Transaction.Derive(false);

            order.Ship();
            this.Transaction.Derive(false);

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive(false);

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            invoice.Send();
            this.Transaction.Derive(false);

            var paymentApplication = new PaymentApplicationBuilder(this.Transaction)
                .WithInvoice(invoice)
                .WithAmountApplied(invoice.TotalIncVat)
                .Build();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive();

            Assert.True(item.SalesOrderItemState.IsFinished);
        }

        [Fact]
        public void SalesOrderAddSalesOrderItemDeriveSalesOrderItemShipmentStateIsNotShipped()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(item);
            this.Transaction.Derive(false);

            Assert.True(item.SalesOrderItemShipmentState.IsNotShipped);
        }

        [Fact]
        public void ChangedQuantityPendingShipmentDeriveSalesOrderItemShipmentStateIsInProgress()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemShipmentState.IsInProgress);
        }

        [Fact]
        public void ChangedQuantityPendingShipmentDeriveSalesOrderItemShipmentStateIsPartiallyShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemShipmentState.IsPartiallyShipped);
        }

        [Fact]
        public void ChangedQuantityShippedDeriveSalesOrderItemShipmentStateIsShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemShipmentState.IsShipped);
        }

        [Fact]
        public void SalesOrderAddSalesOrderItemDeriveSalesOrderItemPaymentStateIsNotPaid()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(item);
            this.Transaction.Derive(false);

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsNotPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemPaymentStateIsPartiallyPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Transaction.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Transaction)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat - 1)
                .Build();
            new ReceiptBuilder(this.Transaction)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemPaymentStateIsPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Transaction.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Transaction)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat)
                .Build();
            new ReceiptBuilder(this.Transaction)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPaid);
        }

        [Fact]
        public void ChangedOrderShipmentDeriveSalesOrderItemPaymentStateIsPartiallyPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForShipmentItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            shipment.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Transaction.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Transaction)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat - 1)
                .Build();
            new ReceiptBuilder(this.Transaction)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedOrderShipmentDeriveSalesOrderItemPaymentStateIsPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForShipmentItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Transaction.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Transaction.Derive();

            shipment.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Transaction.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Transaction)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat)
                .Build();
            new ReceiptBuilder(this.Transaction)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPaid);
        }

        [Fact]
        public void SalesOrderAddSalesOrderItemDeriveSalesOrderItemInvoiceStateIsNotInvoiced()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(item);
            this.Transaction.Derive(false);

            Assert.True(order.SalesOrderItems.First.SalesOrderItemInvoiceState.IsNotInvoiced);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemInvoiceStateIsPartiallyInvoiced()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            item.AssignedUnitPrice += 1;
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemInvoiceState.IsPartiallyInvoiced);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemInvoiceStateIsInvoiced()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemInvoiceState.IsInvoiced);
        }
    }

    public class SalesOrderItemPriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemPriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityOrderedCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            orderItem.QuantityOrdered = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            orderItem.AssignedUnitPrice = 3;
            this.Transaction.Derive(false);

            Assert.Equal(3, order.TotalIncVat);
        }

        [Fact]
        public void ChangedProductCalculatePrice()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product1)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product1).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            orderItem.Product = product2;
            this.Transaction.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedProductFeatureCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithProductFeature(productFeature)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            var orderFeatureItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantityOrdered(1).Build();
            orderItem.AddOrderedWithFeature(orderFeatureItem);
            order.AddSalesOrderItem(orderFeatureItem);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, order.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, order.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderBillToCustomerCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Transaction.Derive(false);

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPartyClassification(theBad)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer1).WithOrderDate(this.Transaction.Now()).WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            order.BillToCustomer = customer2;
            this.Transaction.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderOrderDateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var baseprice = new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-2))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now().AddDays(-1)).WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            baseprice.ThroughDate = this.Transaction.Now().AddDays(-2);

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddSeconds(-1))
                .Build();
            this.Transaction.Derive(false);

            order.OrderDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderDerivationTriggerCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var basePrice = new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);

            var expectedMessage = $"{orderItem}, {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            Assert.Equal(0, order.TotalExVat);

            basePrice.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Transaction.Derive(false);

            Assert.Equal(basePrice.Price, order.TotalExVat);
        }

        [Fact]
        public void ChangedDerivationTriggerCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var break1 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(10)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for quantity break 1")
                .WithOrderQuantityBreak(break1)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(item1);
            this.Transaction.Derive(false);

            Assert.Equal(0, item1.UnitDiscount);

            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(49).Build();
            order.AddSalesOrderItem(item2);
            this.Transaction.Derive(false);

            Assert.Equal(1, item1.UnitDiscount);
        }
    }

    public class SalesOrderItemQuantitiesRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemQuantitiesRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderItemInventoryAssignmentsDeriveQuantityCommittedOut()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(item.QuantityOrdered, item.QuantityCommittedOut);
        }

        [Fact]
        public void ChangedReservedFromNonSerialisedInventoryItemQuantityOnHandDeriveQuantityRequestsShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(item.QuantityOrdered, item.QuantityRequestsShipping);
        }

        [Fact]
        public void ChangedSalesOrderItemStateDeriveQuantityRequestsShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Revise();
            this.Transaction.Derive();

            item.QuantityOrdered -= 1;
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept(); // set to processing with adjusted quantity ordered
            this.Transaction.Derive();

            Assert.Equal(item.QuantityOrdered, item.QuantityRequestsShipping);
        }

        [Fact]
        public void ChangedQuantityPendingShipmentDeriveQuantityRequestsShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Transaction.Derive(false);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithGood(item.Product as Good)
                .WithQuantity(1)
                .WithReservedFromInventoryItem(item.ReservedFromNonSerialisedInventoryItem)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            new OrderShipmentBuilder(this.Transaction)
                .WithOrderItem(item)
                .WithShipmentItem(shipmentItem)
                .WithQuantity(1)
                .Build();

            this.Transaction.Derive(false);

            Assert.Equal(item.QuantityOrdered - 1, item.QuantityRequestsShipping);
        }
    }

    public class SalesOrderItemInventoryAssignmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemInventoryAssignmentRuleTests(Fixture fixture) : base(fixture) { }
    }

    [Trait("Category", "Security")]
    public class SalesOrderItemDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemDeniedPermissionRuleTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deletePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Delete);
            Assert.DoesNotContain(deletePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.SalesOrderItemState = new SalesOrderItemStates(this.Transaction).RequestsApproval;
            this.Transaction.Derive(false);

            var deletePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Delete);
            Assert.Contains(deletePermission, item.DeniedPermissions);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveDeletePermissionDenied()
        {
            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItemBilling = new OrderItemBillingBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deletePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Delete);
            Assert.DoesNotContain(deletePermission, orderItem.DeniedPermissions);

            orderItemBilling.OrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void ChangedOrderShipmentOrderItemDeriveDeletePermissionDenied()
        {
            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderShipment = new OrderShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deletePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Delete);
            Assert.DoesNotContain(deletePermission, orderItem.DeniedPermissions);

            orderShipment.OrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void ChangedOrderRequirementCommitmentOrderItemDeriveDeletePermissionDenied()
        {
            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderRequirementCommitment = new OrderRequirementCommitmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deletePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Delete);
            Assert.DoesNotContain(deletePermission, orderItem.DeniedPermissions);

            orderRequirementCommitment.OrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void ChangedWorkEffortOrderItemFulfillmentDeriveDeletePermissionDenied()
        {
            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deletePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Delete);
            Assert.DoesNotContain(deletePermission, orderItem.DeniedPermissions);

            workTask.OrderItemFulfillment = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemInvoiceStateInvoicedDeriveDeniablePermission()
        {
            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.SalesOrderItemInvoiceState = new SalesOrderItemInvoiceStates(this.Transaction).Invoiced;
            this.Transaction.Derive(false);

            var deniablePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Cancel);
            Assert.Contains(deniablePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemShipmentStateInvoicedDeriveDeniablePermission()
        {
            var item = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            item.SalesOrderItemShipmentState = new SalesOrderItemShipmentStates(this.Transaction).Shipped;
            this.Transaction.Derive(false);

            var deniablePermission = new Permissions(this.Transaction).Get(this.M.SalesOrderItem, this.M.SalesOrderItem.Cancel);
            Assert.Contains(deniablePermission, item.DeniedPermissions);
        }
    }

    [Trait("Category", "Security")]
    public class SalesOrderItemSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        private ProductCategory productCategory;
        private ProductCategory ancestorProductCategory;
        private ProductCategory parentProductCategory;
        private Good good;
        private readonly Good variantGood;
        private readonly Good variantGood2;
        private Good virtualGood;
        private Part part;
        private Colour feature1;
        private Colour feature2;
        private Organisation shipToCustomer;
        private Organisation billToCustomer;
        private Organisation supplier;
        private City kiev;
        private PostalAddress shipToContactMechanismMechelen;
        private PostalAddress shipToContactMechanismKiev;
        private BasePrice currentBasePriceGeoBoundary;
        private BasePrice currentGoodBasePrice;
        private BasePrice currentGood1Feature1BasePrice;
        private BasePrice currentFeature2BasePrice;
        private SupplierOffering goodPurchasePrice;
        private SupplierOffering virtualGoodPurchasePrice;
        private SalesOrder order;
        private VatRegime vatRegime;

        public override Config Config => new Config { SetupSecurity = true };

        public SalesOrderItemSecurityTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            this.supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            this.vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            this.kiev = new CityBuilder(this.Transaction).WithName("Kiev").Build();

            this.shipToContactMechanismMechelen = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            this.shipToContactMechanismKiev = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(this.kiev).WithAddress1("Dnieper").Build();
            this.shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("shipToCustomer").Build();
            this.shipToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.billToCustomer = new OrganisationBuilder(this.Transaction)
                .WithName("billToCustomer")
                .WithPreferredCurrency(euro)

                .Build();

            this.billToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            this.good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("good")
                .WithPart(this.part)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(this.billToCustomer).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(this.shipToCustomer).Build();

            this.variantGood = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("variant good")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("2")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            this.variantGood2 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v10102")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("variant good2")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("3")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            this.virtualGood = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("v10103")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(this.vatRegime)
                .WithName("virtual good")
                .WithVariant(this.variantGood)
                .WithVariant(this.variantGood2)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            this.ancestorProductCategory = new ProductCategoryBuilder(this.Transaction)
                .WithName("ancestor")
                .Build();

            this.parentProductCategory = new ProductCategoryBuilder(this.Transaction)
                .WithName("parent")
                .WithPrimaryParent(this.ancestorProductCategory)
                .Build();

            this.productCategory = new ProductCategoryBuilder(this.Transaction)
                .WithName("gizmo")
                .Build();

            this.productCategory.AddSecondaryParent(this.parentProductCategory);

            this.goodPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.part)
                .WithSupplier(this.supplier)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithFromDate(this.Transaction.Now())
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.virtualGoodPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithCurrency(euro)
                .WithFromDate(this.Transaction.Now())
                .WithSupplier(this.supplier)
                .WithPrice(8)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            this.feature1 = new ColourBuilder(this.Transaction)
                .WithVatRegime(this.vatRegime)
                .WithName("white")
                .Build();

            this.feature2 = new ColourBuilder(this.Transaction)
                .WithName("black")
                .Build();

            this.currentBasePriceGeoBoundary = new BasePriceBuilder(this.Transaction)
                .WithDescription("current BasePriceGeoBoundary ")
                .WithGeographicBoundary(mechelen)
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Transaction.Now())
                .Build();

            // historic basePrice for good
            new BasePriceBuilder(this.Transaction).WithDescription("previous good")
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for good
            new BasePriceBuilder(this.Transaction).WithDescription("future good")
                .WithProduct(this.good)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentGoodBasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current good")
                .WithProduct(this.good)
                .WithPrice(10)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature1
            new BasePriceBuilder(this.Transaction).WithDescription("previous feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(0.5M)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for feature1
            new BasePriceBuilder(this.Transaction).WithDescription("future feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2.5M)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithDescription("current feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature2
            new BasePriceBuilder(this.Transaction).WithDescription("previous feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for feature2
            new BasePriceBuilder(this.Transaction)
                .WithDescription("future feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(4)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentFeature2BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(3)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for good with feature1
            new BasePriceBuilder(this.Transaction).WithDescription("previous good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(4)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for good with feature1
            new BasePriceBuilder(this.Transaction)
                .WithDescription("future good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(6)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentGood1Feature1BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(5)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithDescription("current variant good2")
                .WithProduct(this.variantGood2)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.order = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCreated_ThenItemMayBeDeletedCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Provisional, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Delete));
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsConfirmed_ThenItemMayBeCancelledOrRejectedButNotDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.order.SetReadyForPosting();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsPartiallyShipped_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)this.order.DerivedShipToAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Transaction.Derive();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemShipmentStates(this.Transaction).PartiallyShipped, item.SalesOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCancelled_ThenItemMayNotBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            item.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Cancelled, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCancelled_ThenItemCanBeDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            item.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Cancelled, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsRejected_ThenItemMayNotBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            item.Reject();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Rejected, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsRejected_ThenCanBeDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            item.Reject();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Rejected, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCompleted_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            this.order.Ship();

            this.Transaction.Derive();

            var shipment = (CustomerShipment)this.order.DerivedShipToAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;
            pickList.SetPicked();

            this.Transaction.Derive();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();

            this.Transaction.Derive();

            shipment.Invoice();

            this.Transaction.Derive();

            ((SalesInvoiceItem)shipment.ShipmentItems[0].ShipmentItemBillingsWhereShipmentItem[0].InvoiceItem).SalesInvoiceWhereSalesInvoiceItem.Send();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Completed, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsFinished_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SalesOrderState = new SalesOrderStates(this.Transaction).Finished;

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsPartiallyShipped_ThenProductChangeIsNotAllowed()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)this.order.DerivedShipToAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Transaction.Derive();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemShipmentStates(this.Transaction).PartiallyShipped, item.SalesOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanWrite(this.M.SalesOrderItem.Product));
        }

        [Fact]
        public void GivenOrderItem_WhenShippingInProgress_ThenCancelIsNotAllowed()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(this.part).Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForPosting();
            this.Transaction.Derive();

            this.order.Post();
            this.Transaction.Derive();

            this.order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderItemShipmentStates(this.Transaction).InProgress, item.SalesOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.productCategory = (ProductCategory)transaction.Instantiate(this.productCategory);
            this.parentProductCategory = (ProductCategory)transaction.Instantiate(this.parentProductCategory);
            this.ancestorProductCategory = (ProductCategory)transaction.Instantiate(this.ancestorProductCategory);
            this.part = (Part)transaction.Instantiate(this.part);
            this.virtualGood = (Good)transaction.Instantiate(this.virtualGood);
            this.good = (Good)transaction.Instantiate(this.good);
            this.feature1 = (Colour)transaction.Instantiate(this.feature1);
            this.feature2 = (Colour)transaction.Instantiate(this.feature2);
            this.shipToCustomer = (Organisation)transaction.Instantiate(this.shipToCustomer);
            this.billToCustomer = (Organisation)transaction.Instantiate(this.billToCustomer);
            this.supplier = (Organisation)transaction.Instantiate(this.supplier);
            this.kiev = (City)transaction.Instantiate(this.kiev);
            this.shipToContactMechanismMechelen = (PostalAddress)transaction.Instantiate(this.shipToContactMechanismMechelen);
            this.shipToContactMechanismKiev = (PostalAddress)transaction.Instantiate(this.shipToContactMechanismKiev);
            this.currentBasePriceGeoBoundary = (BasePrice)transaction.Instantiate(this.currentBasePriceGeoBoundary);
            this.currentGoodBasePrice = (BasePrice)transaction.Instantiate(this.currentGoodBasePrice);
            this.currentGood1Feature1BasePrice = (BasePrice)transaction.Instantiate(this.currentGood1Feature1BasePrice);
            this.currentFeature2BasePrice = (BasePrice)transaction.Instantiate(this.currentFeature2BasePrice);
            this.goodPurchasePrice = (SupplierOffering)transaction.Instantiate(this.goodPurchasePrice);
            this.virtualGoodPurchasePrice = (SupplierOffering)transaction.Instantiate(this.virtualGoodPurchasePrice);
            this.currentGoodBasePrice = (BasePrice)transaction.Instantiate(this.currentGoodBasePrice);
            this.order = (SalesOrder)transaction.Instantiate(this.order);
            this.vatRegime = (VatRegime)transaction.Instantiate(this.vatRegime);
        }
    }
}
