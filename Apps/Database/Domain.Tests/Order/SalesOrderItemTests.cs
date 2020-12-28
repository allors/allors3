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
        private VatRate vatRate21;

        public SalesOrderItemTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");

            this.supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();

            this.vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            this.kiev = new CityBuilder(this.Session).WithName("Kiev").Build();

            this.shipToContactMechanismMechelen = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            this.shipToContactMechanismKiev = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(this.kiev).WithAddress1("Dnieper").Build();
            this.shipToCustomer = new OrganisationBuilder(this.Session).WithName("shipToCustomer").Build();
            this.shipToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Session)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.billToCustomer = new OrganisationBuilder(this.Session)
                .WithName("billToCustomer")
                .WithPreferredCurrency(euro)

                .Build();

            this.billToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Session)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.part = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            this.good = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("good")
                .WithPart(this.part)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            new SupplierRelationshipBuilder(this.Session)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Session.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Session).WithCustomer(this.billToCustomer).Build();

            new CustomerRelationshipBuilder(this.Session).WithCustomer(this.shipToCustomer).Build();

            this.variantGood = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("variant good")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Session)
                    .WithProductIdentification(new PartNumberBuilder(this.Session)
                        .WithIdentification("2")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build())
                .Build();

            this.variantGood2 = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v10102")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("variant good2")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Session)
                    .WithProductIdentification(new PartNumberBuilder(this.Session)
                        .WithIdentification("3")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build())
                .Build();

            this.virtualGood = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v10103")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("virtual good")
                .WithVariant(this.variantGood)
                .WithVariant(this.variantGood2)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            this.ancestorProductCategory = new ProductCategoryBuilder(this.Session)
                .WithName("ancestor")
                .Build();

            this.parentProductCategory = new ProductCategoryBuilder(this.Session)
                .WithName("parent")
                .WithPrimaryParent(this.ancestorProductCategory)
                .Build();

            this.productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("gizmo")
                .Build();

            this.productCategory.AddSecondaryParent(this.parentProductCategory);

            this.goodPurchasePrice = new SupplierOfferingBuilder(this.Session)
                .WithPart(this.part)
                .WithSupplier(this.supplier)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithFromDate(this.Session.Now())
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.virtualGoodPurchasePrice = new SupplierOfferingBuilder(this.Session)
                .WithCurrency(euro)
                .WithFromDate(this.Session.Now())
                .WithSupplier(this.supplier)
                .WithPrice(8)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            this.feature1 = new ColourBuilder(this.Session)
                .WithVatRate(this.vatRate21)
                .WithName("white")
                .Build();

            this.feature2 = new ColourBuilder(this.Session)
                .WithName("black")
                .Build();

            this.currentBasePriceGeoBoundary = new BasePriceBuilder(this.Session)
                .WithDescription("current BasePriceGeoBoundary ")
                .WithGeographicBoundary(mechelen)
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Session.Now())
                .Build();

            // historic basePrice for good
            new BasePriceBuilder(this.Session).WithDescription("previous good")
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for good
            new BasePriceBuilder(this.Session).WithDescription("future good")
                .WithProduct(this.good)
                .WithPrice(11)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            this.currentGoodBasePrice = new BasePriceBuilder(this.Session)
                .WithDescription("current good")
                .WithProduct(this.good)
                .WithPrice(10)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature1
            new BasePriceBuilder(this.Session).WithDescription("previous feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(0.5M)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for feature1
            new BasePriceBuilder(this.Session).WithDescription("future feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2.5M)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithDescription("current feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature2
            new BasePriceBuilder(this.Session).WithDescription("previous feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for feature2
            new BasePriceBuilder(this.Session)
                .WithDescription("future feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(4)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            this.currentFeature2BasePrice = new BasePriceBuilder(this.Session)
                .WithDescription("current feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(3)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for good with feature1
            new BasePriceBuilder(this.Session).WithDescription("previous good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(4)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for good with feature1
            new BasePriceBuilder(this.Session)
                .WithDescription("future good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(6)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            this.currentGood1Feature1BasePrice = new BasePriceBuilder(this.Session)
                .WithDescription("current good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(5)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithDescription("current variant good2")
                .WithProduct(this.variantGood2)
                .WithPrice(11)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            this.order = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Session.Derive();
            this.Session.Commit();
        }

        [Fact]
        public void GivenOrderItemWithoutVatRegime_WhenDeriving_ThenDerivedVatRegimeIsFromOrder()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Export)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(salesOrder.AssignedVatRegime, orderItem.DerivedVatRegime);
        }

        [Fact]
        public void GivenOrderItemWithoutVatRate_WhenDeriving_ThenItemDerivedVatRateIsFromOrderVatRegime()
        {
            this.InstantiateObjects(this.Session);

            var expected = new VatRegimes(this.Session).Export.VatRate;

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Export)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(expected, orderItem.VatRate);
        }

        [Fact]
        public void GivenOrderItemWithAssignedDeliveryDate_WhenDeriving_ThenDeliveryDateIsOrderItemAssignedDeliveryDate()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.billToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Export)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(1)
                .WithAssignedDeliveryDate(this.Session.Now().AddMonths(1))
                .Build();

            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, orderItem.AssignedDeliveryDate);
        }

        [Fact]
        public void GivenOrderItemWithoutDeliveryDate_WhenDeriving_ThenDerivedDeliveryDateIsOrderDeliveryDate()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.billToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Export)
                .WithDeliveryDate(this.Session.Now().AddMonths(1))
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(1)
                .Build();

            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, salesOrder.DeliveryDate);
        }

        [Fact]
        public void GivenOrderItem_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            this.InstantiateObjects(this.Session);

            var builder = new SalesOrderItemBuilder(this.Session);
            var orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithProduct(this.good);
            orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            this.Session.Rollback();

            builder.WithQuantityOrdered(1);
            orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithProductFeature(this.feature1);
            orderItem = builder.Build();

            this.order.AddSalesOrderItem(orderItem);

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItem_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            this.InstantiateObjects(this.Session);

            var item = new SalesOrderItemBuilder(this.Session).Build();

            Assert.Equal(new SalesOrderItemStates(this.Session).Provisional, item.SalesOrderItemState);
        }

        [Fact]
        public void GivenOrderItemWithOrderedWithFeature_WhenDeriving_ThenOrderedWithFeatureOrderItemMustBeForProductFeature()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Session.Derive();

            var productOrderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            var productFeatureOrderItem = new SalesOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem)
                .WithProductFeature(this.feature1)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            productOrderItem.AddOrderedWithFeature(productFeatureOrderItem);
            salesOrder.AddSalesOrderItem(productOrderItem);
            salesOrder.AddSalesOrderItem(productFeatureOrderItem);

            Assert.False(this.Session.Derive(false).HasErrors);

            productFeatureOrderItem.RemoveProductFeature();
            productFeatureOrderItem.Product = this.good;

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItemWithoutCustomer_WhenDeriving_ShipToAddressIsNull()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session).WithBillToCustomer(this.billToCustomer).Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Null(orderItem.DerivedShipToAddress);
            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItemWithoutShipFromAddress_WhenDeriving_ThenDerivedShipFromAddressIsFromOrder()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithAssignedShipFromAddress(this.shipToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(this.shipToContactMechanismMechelen, orderItem.DerivedShipFromAddress);
        }

        [Fact]
        public void GivenOrderItemWithoutShipToAddress_WhenDeriving_ThenDerivedShipToAddressIsFromOrder()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(this.shipToContactMechanismMechelen, orderItem.DerivedShipToAddress);
        }

        [Fact]
        public void GivenOrderItemWithoutShipToParty_WhenDeriving_ThenDerivedShipToPartyIsFromOrder()
        {
            this.InstantiateObjects(this.Session);

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(this.good).WithQuantityOrdered(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            Assert.Equal(this.shipToCustomer, orderItem.DerivedShipToParty);
        }

        [Fact]
        public void GivenOrderItemForGoodWithoutSelectedInventoryItem_WhenConfirming_ThenReservedFromNonSerialisedInventoryItemIsFromDefaultFacility()
        {
            this.InstantiateObjects(this.Session);

            var good2 = new Goods(this.Session).FindBy(this.M.Good.Name, "good2");

            new SupplierRelationshipBuilder(this.Session)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Session.Now())
                .Build();

            var good2PurchasePrice = new SupplierOfferingBuilder(this.Session)
                .WithPart(this.part)
                .WithSupplier(this.supplier)
                .WithCurrency(new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithFromDate(this.Session.Now())
                .WithPrice(7)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            //// good with part as inventory item
            var item1 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            var item2 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good2)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item1);
            this.order.AddSalesOrderItem(item2);

            this.Session.Derive();

            this.order.SetReadyForPosting();

            this.Session.Derive();

            this.order.Post();
            this.Session.Derive(true);

            this.order.Accept();
            this.Session.Derive(true);

            Assert.Equal(new Facilities(this.Session).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Session).Warehouse), item1.ReservedFromNonSerialisedInventoryItem.Facility);
            Assert.Equal(new Facilities(this.Session).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Session).Warehouse), item2.ReservedFromNonSerialisedInventoryItem.Facility);
        }

        //[Fact]
        //public void GivenConfirmedOrderItemForGood_WhenReservedFromNonSerialisedInventoryItemChangesValue_ThenQuantitiesAreMovedFromOldToNewInventoryItem()
        //{
        //    this.InstantiateObjects(this.Session);

        //    new InventoryItemTransactionBuilder(this.Session).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

        //    this.Session.Derive();

        //    var secondWarehouse = new FacilityBuilder(this.Session)
        //        .WithName("affiliate warehouse")
        //        .WithFacilityType(new FacilityTypes(this.Session).Warehouse)
        //        .WithOwner(this.InternalOrganisation)
        //        .Build();

        //    var order1 = new SalesOrderBuilder(this.Session)
        //        .WithShipToCustomer(this.shipToCustomer)
        //        .WithBillToCustomer(this.billToCustomer)
        //        .WithPartiallyShip(false)
        //        .Build();

            //this.Session.Derive();

        //    var salesOrderItem = new SalesOrderItemBuilder(this.Session)
        //        .WithProduct(this.good)
        //        .WithQuantityOrdered(3)
        //        .WithAssignedUnitPrice(5)
        //        .Build();

        //    order1.AddSalesOrderItem(salesOrderItem);

        //    this.Session.Derive();

        //    order1.Confirm();

        //    this.Session.Derive();

        //    order1.Send();

        //    this.Session.Derive(true);

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

        //    var transaction = new InventoryItemTransactionBuilder(this.Session).WithFacility(secondWarehouse).WithPart(this.part).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).Build();

        //    this.Session.Derive();

        //    var current = transaction.InventoryItem as NonSerialisedInventoryItem;

        //    salesOrderItem.ReservedFromNonSerialisedInventoryItem = current;

        //    this.Session.Derive();

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
            this.InstantiateObjects(this.Session);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(3).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(salesOrderItem);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive(true);

            this.order.Accept();
            this.Session.Derive(true);

            Assert.Equal(salesOrderItem.QuantityOrdered, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);

            this.Session.Derive();

            salesOrderItem.Cancel();

            this.Session.Derive();

            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenOrderItemIsRejected_ThenNonSerialisedInventoryQuantitiesAreReleased()
        {
            this.InstantiateObjects(this.Session);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(3).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(salesOrderItem);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive(true);

            this.order.Accept();
            this.Session.Derive(true);

            Assert.Equal(salesOrderItem.QuantityOrdered, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);

            this.Session.Derive();

            salesOrderItem.Reject();

            this.Session.Derive();

            Assert.Equal(0, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(3, salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenOrderItemForGoodWithEnoughStockAvailable_WhenConfirming_ThenQuantitiesReservedAndRequestsShippingAreEqualToQuantityOrdered()
        {
            this.InstantiateObjects(this.Session);

            var store = this.Session.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(100)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

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
            this.InstantiateObjects(this.Session);

            var store = this.Session.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Session).IncomingShipment).WithPart(this.part).Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(120)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item1);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            Assert.Equal(120, item1.QuantityOrdered);
            Assert.Equal(0, item1.QuantityShipped);
            Assert.Equal(110, item1.QuantityPendingShipment);
            Assert.Equal(120, item1.QuantityReserved);
            Assert.Equal(10, item1.QuantityShortFalled);
            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(110, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(110, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            var item2 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item2);
            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

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
            this.InstantiateObjects(this.Session);

            var store = this.Session.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(100)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

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
            var store = this.Session.Extent<Store>().First;
            store.AutoGenerateCustomerShipment = false;

            this.InstantiateObjects(this.Session);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(100)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            this.Session.Commit();

            item.QuantityOrdered = 50;

            this.Session.Derive();

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
            this.InstantiateObjects(this.Session);

            var manual = new OrderKindBuilder(this.Session).WithDescription("manual").WithScheduleManually(true).Build();

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            var orderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(120)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(orderItem);
            this.order.OrderKind = manual;
            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            var shipment = new CustomerShipmentBuilder(this.Session)
                .WithShipFromAddress(this.order.TakenBy.ShippingAddress)
                .WithShipToParty(this.order.ShipToCustomer)
                .WithShipToAddress(this.order.DerivedShipToAddress)
                .WithShipmentPackage(new ShipmentPackageBuilder(this.Session).Build())
                .WithShipmentMethod(this.order.DerivedShipmentMethod)
                .Build();

            this.Session.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithGood(orderItem.Product as Good)
                .WithQuantity(100)
                .WithReservedFromInventoryItem(orderItem.ReservedFromNonSerialisedInventoryItem)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            new OrderShipmentBuilder(this.Session)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipment.ShipmentItems.First)
                .WithQuantity(100)
                .Build();

            this.Session.Derive();

            shipment.Pick();
            this.Session.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Session.Derive();

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem item in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(item).WithQuantity(item.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();
            this.Session.Derive();

            Assert.Equal(100, orderItem.QuantityShipped);

            orderItem.QuantityOrdered = 90;
            var derivationLog = this.Session.Derive(false);

            Assert.True(derivationLog.HasErrors);
        }

        [Fact]
        public void GivenOrderItemWithPendingShipmentAndItemsShortFalled_WhenQuantityOrderedIsDecreased_ThenItemsShortFalledIsDecreasedAndShipmentIsLeftUnchanged()
        {
            this.InstantiateObjects(this.Session);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(30)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            Assert.Equal(20, item.QuantityShortFalled);

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;
            Assert.Equal(10, shipment.ShipmentItems[0].Quantity);

            shipment.Pick();
            this.Session.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            Assert.Equal(10, pickList.PickListItems[0].Quantity);

            item.QuantityOrdered = 11;

            this.Session.Derive();

            Assert.Equal(1, item.QuantityShortFalled);

            Assert.Equal(10, shipment.ShipmentItems[0].Quantity);
            Assert.Equal(10, pickList.PickListItems[0].Quantity);

            item.QuantityOrdered = 10;

            this.Session.Derive();

            Assert.Equal(0, item.QuantityShortFalled);

            Assert.Equal(10, shipment.ShipmentItems[0].Quantity);
            Assert.Equal(10, pickList.PickListItems[0].Quantity);
        }

        [Fact]
        public void GivenManuallyScheduledOrderItem_WhenScheduled_ThenQuantityCannotExceedInventoryAvailableToPromise()
        {
            this.InstantiateObjects(this.Session);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(3).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var manual = new OrderKindBuilder(this.Session).WithDescription("manual").WithScheduleManually(true).Build();

            var order1 = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismMechelen)
                .WithOrderKind(manual)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(5)
                .WithAssignedUnitPrice(5)
                .Build();

            order1.AddSalesOrderItem(orderItem);
            this.Session.Derive();

            order1.SetReadyForPosting();
            this.Session.Derive();

            order1.Post();
            this.Session.Derive();

            order1.Accept();
            this.Session.Derive();

            var shipment = new CustomerShipmentBuilder(this.Session)
                .WithShipFromAddress(this.order.TakenBy.ShippingAddress)
                .WithShipToParty(this.order.ShipToCustomer)
                .WithShipToAddress(this.order.DerivedShipToAddress)
                .WithShipmentPackage(new ShipmentPackageBuilder(this.Session).Build())
                .WithShipmentMethod(this.order.DerivedShipmentMethod)
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithGood(orderItem.Product as Good)
                .WithQuantity(5)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            var orderShipment = new OrderShipmentBuilder(this.Session)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipment.ShipmentItems.First)
                .WithQuantity(5)
                .Build();

            var expectedMessage = $"{orderShipment} {this.M.OrderShipment.Quantity} {ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            this.Session.Rollback();

            shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithGood(orderItem.Product as Good)
                .WithQuantity(3)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            new OrderShipmentBuilder(this.Session)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipment.ShipmentItems.First)
                .WithQuantity(3)
                .Build();

            var derivationLog = this.Session.Derive();

            Assert.False(derivationLog.HasErrors);
            Assert.Equal(3, orderItem.QuantityPendingShipment);
        }

        private void InstantiateObjects(ISession session)
        {
            this.productCategory = (ProductCategory)session.Instantiate(this.productCategory);
            this.parentProductCategory = (ProductCategory)session.Instantiate(this.parentProductCategory);
            this.ancestorProductCategory = (ProductCategory)session.Instantiate(this.ancestorProductCategory);
            this.part = (Part)session.Instantiate(this.part);
            this.virtualGood = (Good)session.Instantiate(this.virtualGood);
            this.good = (Good)session.Instantiate(this.good);
            this.feature1 = (Colour)session.Instantiate(this.feature1);
            this.feature2 = (Colour)session.Instantiate(this.feature2);
            this.shipToCustomer = (Organisation)session.Instantiate(this.shipToCustomer);
            this.billToCustomer = (Organisation)session.Instantiate(this.billToCustomer);
            this.supplier = (Organisation)session.Instantiate(this.supplier);
            this.kiev = (City)session.Instantiate(this.kiev);
            this.shipToContactMechanismMechelen = (PostalAddress)session.Instantiate(this.shipToContactMechanismMechelen);
            this.shipToContactMechanismKiev = (PostalAddress)session.Instantiate(this.shipToContactMechanismKiev);
            this.currentBasePriceGeoBoundary = (BasePrice)session.Instantiate(this.currentBasePriceGeoBoundary);
            this.currentGoodBasePrice = (BasePrice)session.Instantiate(this.currentGoodBasePrice);
            this.currentGood1Feature1BasePrice = (BasePrice)session.Instantiate(this.currentGood1Feature1BasePrice);
            this.currentFeature2BasePrice = (BasePrice)session.Instantiate(this.currentFeature2BasePrice);
            this.goodPurchasePrice = (SupplierOffering)session.Instantiate(this.goodPurchasePrice);
            this.virtualGoodPurchasePrice = (SupplierOffering)session.Instantiate(this.virtualGoodPurchasePrice);
            this.currentGoodBasePrice = (BasePrice)session.Instantiate(this.currentGoodBasePrice);
            this.order = (SalesOrder)session.Instantiate(this.order);
            this.vatRate21 = (VatRate)session.Instantiate(this.vatRate21);
        }
    }

    public class SalesOrderItemBuildDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemBuildDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderItemState()
        {
            var order = new SalesOrderItemBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderItemState);
        }

        [Fact]
        public void DeriveSalesOrderItemShipmentState()
        {
            var order = new SalesOrderItemBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderItemShipmentState);
        }

        [Fact]
        public void DeriveSalesOrderIteminvoiceState()
        {
            var order = new SalesOrderItemBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderItemInvoiceState);
        }

        [Fact]
        public void DeriveSalesOrderItemPaymentState()
        {
            var order = new SalesOrderItemBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderItemPaymentState);
        }

        [Fact]
        public void DeriveInvoiceItemType()
        {
            var order = new SalesOrderItemBuilder(this.Session)
                .WithProduct(new UnifiedGoodBuilder(this.Session).Build())
                .Build();

            this.Session.Derive(false);

            Assert.True(order.ExistInvoiceItemType);
        }
    }

    public class SalesOrderItemProvisionalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemProvisionalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderSalesOrderItemsDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Session).WithAssignedShipFromAddress(new PostalAddressBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, order.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedAssignedShipFromAddressDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithAssignedShipFromAddress(new PostalAddressBuilder(this.Session).Build()).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, orderItem.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedsalesOrderDerivedShipFromAddressDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.AssignedShipFromAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedShipFromAddress, order.DerivedShipFromAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).Build()).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedShipToAddress, orderItem.AssignedShipToAddress);
        }

        [Fact]
        public void ChangedShipToPartyShippingAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var orderItem = new SalesOrderItemBuilder(this.Session).WithAssignedShipToParty(customer).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedShipToAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedsalesOrderDerivedShipToAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.AssignedShipToAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedShipToAddress, order.DerivedShipToAddress);
        }

        [Fact]
        public void ChangedAssignedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithAssignedDeliveryDate(this.Session.Now().Date).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, orderItem.AssignedDeliveryDate);
        }

        [Fact]
        public void ChangedsalesOrderDerivedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.DeliveryDate = this.Session.Now().Date;
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithAssignedVatRegime(new VatRegimes(this.Session).Assessable10).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedVatRegime, orderItem.AssignedVatRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Session).Assessable10;
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedDerivedVatRegimeDeriveVatRate()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Session).Assessable10;
            this.Session.Derive(false);

            Assert.Equal(orderItem.VatRate, order.AssignedVatRegime.VatRate);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithAssignedIrpfRegime(new IrpfRegimes(this.Session).Assessable15).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedIrpfRegime, orderItem.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(orderItem.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedDerivedIrpfRegimeDeriveIrpfRate()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(orderItem.IrpfRate, order.AssignedIrpfRegime.IrpfRate);
        }
    }

    public class SalesOrderItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductDeriveInvoiceItemType()
        {
            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.Product = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(new InvoiceItemTypes(this.Session).ProductItem, item.InvoiceItemType);
        }

        [Fact]
        public void ChangedInvoiceItemTypeDeriveInvoiceItemType()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(new UnifiedGoodBuilder(this.Session).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem)
                .Build();
            this.Session.Derive(false);

            item.RemoveInvoiceItemType();
            this.Session.Derive(false);

            Assert.Equal(new InvoiceItemTypes(this.Session).ProductItem, item.InvoiceItemType);
        }

        [Fact]
        public void ChangedSerialisedItemValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.SerialisedItem = new SerialisedItemBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertExists: SalesOrderItem.NextSerialisedItemAvailability")));
        }

        [Fact]
        public void ChangedNextSerialisedItemAvailabilityValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Session).Build())
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Session).Available)
                .Build();
            this.Session.Derive(false);

            item.RemoveNextSerialisedItemAvailability();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertExists: SalesOrderItem.NextSerialisedItemAvailability")));
        }

        [Fact]
        public void ChangedProductWhenSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithQuantityOrdered(2)
                .Build();
            this.Session.Derive(false);

            item.Product = product;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedWhenSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(product)
                .Build();
            this.Session.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedProductWhenNonSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithQuantityOrdered(0)
                .Build();
            this.Session.Derive(false);

            item.Product = product;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedWhenNonSerialisedValidationError()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(product)
                .Build();
            this.Session.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).Service)
                .Build();
            this.Session.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorRequired()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(product)
                .Build();
            this.Session.Derive(false);

            item.QuantityOrdered = 0;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains("QuantityOrdered is Required"));
        }

        [Fact]
        public void ChangedAssignedUnitPriceValidationErrorRequired()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).Service)
                .Build();
            this.Session.Derive(false);

            item.AssignedUnitPrice = 0;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains("Price is Required"));
        }

        [Fact]
        public void ChangedProductValidationError()
        {
            var product1 = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            var product2 = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(product1)
                .Build();
            this.Session.Derive(false);

            item.Product = product2;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemProductChangeNotAllowed));
        }

        [Fact]
        public void SalesOrderItemWhereOrderedWithFeatureValidationErrorProductFeature()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.AddOrderedWithFeature(new SalesOrderItemBuilder(this.Session).Build());

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertExists: SalesOrderItem.ProductFeature")));
        }

        [Fact]
        public void SalesOrderItemWhereOrderedWithFeatureValidationErrorProduct()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.AddOrderedWithFeature(new SalesOrderItemBuilder(this.Session).WithProduct(product).Build());

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertNotExists: SalesOrderItem.Product")));
        }

        [Fact]
        public void ChangedProductFeatureValidationError()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .Build();
            this.Session.Derive(false);

            item.ProductFeature = new ColourBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals("AssertNotExists: SalesOrderItem.ProductFeature")));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Ship();
            this.Session.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            Assert.True(item.QuantityShipped > 0);

            item.QuantityOrdered = item.QuantityShipped - 1;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemLessThanAlreadeyShipped));
        }

        [Fact]
        public void ChangedProductFeatureValidationErrorAtMostOne()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(product)
                .Build();
            this.Session.Derive(false);

            item.ProductFeature = new ColourBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.Product\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedProductValidationErrorAtMostOne()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProductFeature(new ColourBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            item.Product = product;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.Product\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedSerialisedItemValidationErrorAtMostOne()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProductFeature(new ColourBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            item.SerialisedItem = new SerialisedItemBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.SerialisedItem\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedSerialisedItemValidationErrorAtMostOne_2()
        {
            var product = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            item.ProductFeature = new ColourBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.SerialisedItem\nSalesOrderItem.ProductFeature"));
        }

        [Fact]
        public void ChangedReservedFromNonSerialisedInventoryItemValidationErrorAtMostOne()
        {
            var serialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Session)
                .WithPart(serialisedGood)
                .Build();
            var nonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Session)
                .WithPart(nonSerialisedGood)
                .Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithReservedFromSerialisedInventoryItem(serialisedInventoryItem)
                .Build();
            this.Session.Derive(false);

            item.ReservedFromNonSerialisedInventoryItem = nonSerialisedInventoryItem;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.ReservedFromSerialisedInventoryItem\nSalesOrderItem.ReservedFromNonSerialisedInventoryItem"));
        }

        [Fact]
        public void ChangedReservedFromSerialisedInventoryItemValidationErrorAtMostOne()
        {
            var serialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Session)
                .WithPart(serialisedGood)
                .Build();
            var nonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Session)
                .WithPart(nonSerialisedGood)
                .Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithReservedFromNonSerialisedInventoryItem(nonSerialisedInventoryItem)
                .Build();
            this.Session.Derive(false);

            item.ReservedFromSerialisedInventoryItem = serialisedInventoryItem;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.ReservedFromSerialisedInventoryItem\nSalesOrderItem.ReservedFromNonSerialisedInventoryItem"));
        }

        [Fact]
        public void ChangedDiscountAdjustmentsValidationErrorAtMostOne()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithAssignedUnitPrice(1)
                .Build();
            this.Session.Derive(false);

            item.AddDiscountAdjustment(new DiscountAdjustmentBuilder(this.Session).Build());

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.AssignedUnitPrice\nSalesOrderItem.DiscountAdjustments\nSalesOrderItem.SurchargeAdjustments"));
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsValidationErrorAtMostOne()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithAssignedUnitPrice(1)
                .Build();
            this.Session.Derive(false);

            item.AddSurchargeAdjustment(new SurchargeAdjustmentBuilder(this.Session).Build());

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.AssignedUnitPrice\nSalesOrderItem.DiscountAdjustments\nSalesOrderItem.SurchargeAdjustments"));
        }

        [Fact]
        public void ChangedAssignedUnitPriceValidationErrorAtMostOne()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithSurchargeAdjustment(new SurchargeAdjustmentBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            item.AssignedUnitPrice = 1;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SalesOrderItem.AssignedUnitPrice\nSalesOrderItem.DiscountAdjustments\nSalesOrderItem.SurchargeAdjustments"));
        }

        [Fact]
        public void ChangedReservedFromSerialisedInventoryItemValidationError()
        {
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            var anotherNonSerialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            var nonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Session)
                .WithPart(nonSerialisedGood)
                .Build();
            var anotherNonSerialisedInventoryItem = new NonSerialisedInventoryItemBuilder(this.Session)
                .WithPart(anotherNonSerialisedGood)
                .Build();
            this.Session.Derive(false);

            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithReservedFromNonSerialisedInventoryItem(nonSerialisedInventoryItem)
                .Build();
            this.Session.Derive(false);

            order.AddSalesOrderItem(item);
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            item.ReservedFromNonSerialisedInventoryItem = anotherNonSerialisedInventoryItem;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.ReservedFromNonSerialisedInventoryItem));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorSerializedItemQuantity()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            item.QuantityOrdered = 2;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }

        [Fact]
        public void ChangedSerialisedItemValidationErrorSerializedItemQuantity()
        {
            var item = new SalesOrderItemBuilder(this.Session)
                .WithQuantityOrdered(2)
                .Build();
            this.Session.Derive(false);

            item.SerialisedItem = new SerialisedItemBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }

        [Fact]
        public void ChangedSerialisedInventoryItemQuantityDeriveReservedFromSerialisedInventoryItem()
        {
            var serialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            serialisedGood.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            var order = new SalesOrderBuilder(this.Session).WithTakenBy(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(serialisedGood)
                .WithSerialisedItem(serialisedItem)
                .Build();
            this.Session.Derive(false);

            order.AddSalesOrderItem(item);
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount)
                .WithSerialisedItem(serialisedItem)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(item.ReservedFromSerialisedInventoryItem, serialisedItem.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => v.Quantity == 1));
        }

        [Fact]
        public void ChangedInventoryItemTransactionPartDeriveReservedFromNonSerialisedInventoryItem()
        {
            var nonSerialisedGood = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();
            this.Session.Derive(false);

            var order = new SalesOrderBuilder(this.Session).WithTakenBy(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(nonSerialisedGood)
                .Build();
            this.Session.Derive(false);

            order.AddSalesOrderItem(item);
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount)
                .WithPart(nonSerialisedGood)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(item.ReservedFromNonSerialisedInventoryItem, nonSerialisedGood.InventoryItemsWherePart.First);
        }

        [Fact]
        public void ChangedProductDeriveSalesOrderSalesOrderItemsByProduct()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(product, order.SalesOrderItemsByProduct.First.Product);
        }
    }

    public class SalesOrderItemShipmentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemShipmentDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderShipmentQuantityDeriveQuantityPendingShipment()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.Ship();
            this.Session.Derive(false);

            Assert.Equal(item.QuantityOrdered, item.QuantityPendingShipment);
        }

        [Fact]
        public void ChangedShipmentItemShipmentItemStateDeriveQuantityShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.Ship();
            this.Session.Derive(false);

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            Assert.Equal(item.QuantityOrdered, item.QuantityShipped);
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorQuantityPendingShipment()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.Ship();
            this.Session.Derive(false);

            item.QuantityOrdered -= 1;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining));
        }

        [Fact]
        public void ChangedQuantityOrderedValidationErrorQuantityShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.Ship();
            this.Session.Derive(false);

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            item.QuantityOrdered -= 1;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining));
        }
    }

    public class SalesOrderItemStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateProvisional()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsReadyForPosting);

            order.Revise();
            this.Session.Derive();

            Assert.True(item.SalesOrderItemState.IsProvisional);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateReadyForPosting()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsReadyForPosting);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateRequestsApproval()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            this.InternalOrganisation.StoresWhereInternalOrganisation.First.OrderThreshold = order.TotalExVat + 1;

            order.SetReadyForPosting();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsRequestsApproval);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateAwaitingAcceptance()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsAwaitingAcceptance);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateInProcess()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsInProcess);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateOnHold()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Hold();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsOnHold);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateCancelled()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.Cancel();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsCancelled);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateRejected()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.Reject();
            this.Session.Derive();

            var item = order.SalesOrderItems.First;
            Assert.True(item.SalesOrderItemState.IsRejected);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveSalesOrderItemStateFinished()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First;
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.Ship();
            this.Session.Derive(false);

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive(false);

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            invoice.Send();
            this.Session.Derive(false);

            var paymentApplication = new PaymentApplicationBuilder(this.Session)
                .WithInvoice(invoice)
                .WithAmountApplied(invoice.TotalIncVat)
                .Build();

            new ReceiptBuilder(this.Session)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Session.Now())
                .Build();
            this.Session.Derive();

            Assert.True(item.SalesOrderItemState.IsFinished);
        }

        [Fact]
        public void SalesOrderAddSalesOrderItemDeriveSalesOrderItemShipmentStateIsNotShipped()
        {
            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(item);
            this.Session.Derive(false);

            Assert.True(item.SalesOrderItemShipmentState.IsNotShipped);
        }

        [Fact]
        public void ChangedQuantityPendingShipmentDeriveSalesOrderItemShipmentStateIsInProgress()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Ship();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemShipmentState.IsInProgress);
        }

        [Fact]
        public void ChangedQuantityPendingShipmentDeriveSalesOrderItemShipmentStateIsPartiallyShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Ship();
            this.Session.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemShipmentState.IsPartiallyShipped);
        }

        [Fact]
        public void ChangedQuantityShippedDeriveSalesOrderItemShipmentStateIsShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Ship();
            this.Session.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemShipmentState.IsShipped);
        }

        [Fact]
        public void SalesOrderAddSalesOrderItemDeriveSalesOrderItemPaymentStateIsNotPaid()
        {
            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(item);
            this.Session.Derive(false);

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsNotPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemPaymentStateIsPartiallyPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Session.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Session)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat - 1)
                .Build();
            new ReceiptBuilder(this.Session)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Session.Now())
                .Build();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemPaymentStateIsPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Session.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Session)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat)
                .Build();
            new ReceiptBuilder(this.Session)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Session.Now())
                .Build();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPaid);
        }

        [Fact]
        public void ChangedOrderShipmentDeriveSalesOrderItemPaymentStateIsPartiallyPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForShipmentItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Ship();
            this.Session.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            shipment.Invoice();
            this.Session.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Session.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Session)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat - 1)
                .Build();
            new ReceiptBuilder(this.Session)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Session.Now())
                .Build();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedOrderShipmentDeriveSalesOrderItemPaymentStateIsPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForShipmentItems;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Ship();
            this.Session.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            ((CustomerShipment)shipment).Pick();
            this.Session.Derive();

            ((CustomerShipment)shipment).Ship();
            this.Session.Derive();

            shipment.Invoice();
            this.Session.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.First;
            var invoiceItem = invoice.InvoiceItems.First();

            invoice.Send();
            this.Session.Derive();

            var paymentApplication = new PaymentApplicationBuilder(this.Session)
                .WithInvoiceItem(invoiceItem)
                .WithAmountApplied(invoiceItem.TotalIncVat)
                .Build();
            new ReceiptBuilder(this.Session)
                .WithAmount(paymentApplication.AmountApplied)
                .WithPaymentApplication(paymentApplication)
                .WithEffectiveDate(this.Session.Now())
                .Build();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemPaymentState.IsPaid);
        }

        [Fact]
        public void SalesOrderAddSalesOrderItemDeriveSalesOrderItemInvoiceStateIsNotInvoiced()
        {
            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var item = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(item);
            this.Session.Derive(false);

            Assert.True(order.SalesOrderItems.First.SalesOrderItemInvoiceState.IsNotInvoiced);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemInvoiceStateIsPartiallyInvoiced()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive();

            item.AssignedUnitPrice += 1;
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemInvoiceState.IsPartiallyInvoiced);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveSalesOrderItemInvoiceStateIsInvoiced()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive();

            Assert.True(order.SalesOrderItems.First.SalesOrderItemInvoiceState.IsInvoiced);
        }
    }

    public class SalesOrderItemPriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemPriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityOrderedCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            orderItem.QuantityOrdered = 2;
            this.Session.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            orderItem.AssignedUnitPrice = 3;
            this.Session.Derive(false);

            Assert.Equal(3, order.TotalIncVat);
        }

        [Fact]
        public void ChangedProductCalculatePrice()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Session).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product1)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product2)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product1).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            orderItem.Product = product2;
            this.Session.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedProductFeatureCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            var productFeature = new ColourBuilder(this.Session)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithProductFeature(productFeature)
                .WithPrice(0.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            var orderFeatureItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(productFeature).WithQuantityOrdered(1).Build();
            orderItem.AddOrderedWithFeature(orderFeatureItem);
            order.AddSalesOrderItem(orderFeatureItem);
            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);

            discount.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(0.8M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithAmount(0.5M).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.5M, order.TotalIncVat);

            discount.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(0.6M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(1.2M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithAmount(0.5M).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.5M, order.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(1.4M, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderBillToCustomerCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Session).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Session).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Session.Derive(false);

            new BasePriceBuilder(this.Session)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPartyClassification(theBad)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer1).WithOrderDate(this.Session.Now()).WithAssignedVatRegime(new VatRegimes(this.Session).Exempt).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            order.BillToCustomer = customer2;
            this.Session.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderOrderDateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();
            var baseprice = new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddDays(-2))
                .Build();
            
            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now().AddDays(-1)).WithAssignedVatRegime(new VatRegimes(this.Session).Exempt).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.TotalIncVat);

            baseprice.ThroughDate = this.Session.Now().AddDays(-2);

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddSeconds(-1))
                .Build();
            this.Session.Derive(false);

            order.OrderDate = this.Session.Now();
            this.Session.Derive(false);

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderDerivationTriggerCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var basePrice = new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddDays(1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);

            var expectedMessage = $"{orderItem}, {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            Assert.Equal(0, order.TotalExVat);

            basePrice.FromDate = this.Session.Now().AddMinutes(-1);
            this.Session.Derive(false);

            Assert.Equal(basePrice.Price, order.TotalExVat);
        }

        [Fact]
        public void ChangedDerivationTriggerCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();
            var break1 = new OrderQuantityBreakBuilder(this.Session).WithFromAmount(50).WithThroughAmount(99).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(10)
                .WithFromDate(this.Session.Now().AddDays(-1))
                .Build();

            new DiscountComponentBuilder(this.Session)
                .WithDescription("discount good for quantity break 1")
                .WithOrderQuantityBreak(break1)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(item1);
            this.Session.Derive(false);

            Assert.Equal(0, item1.UnitDiscount);

            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(49).Build();
            order.AddSalesOrderItem(item2);
            this.Session.Derive(false);

            Assert.Equal(1, item1.UnitDiscount);
        }
    }

    public class SalesOrderItemQuantitiesDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemQuantitiesDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderItemInventoryAssignmentsDeriveQuantityCommittedOut()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(item.QuantityOrdered, item.QuantityCommittedOut);
        }

        [Fact]
        public void ChangedReservedFromNonSerialisedInventoryItemQuantityOnHandDeriveQuantityRequestsShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(item.QuantityOrdered, item.QuantityRequestsShipping);
        }

        [Fact]
        public void ChangedSalesOrderItemStateDeriveQuantityRequestsShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Revise();
            this.Session.Derive();

            item.QuantityOrdered -= 1;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept(); // set to processing with adjusted quantity ordered
            this.Session.Derive();

            Assert.Equal(item.QuantityOrdered, item.QuantityRequestsShipping);
        }

        [Fact]
        public void ChangedQuantityPendingShipmentDeriveQuantityRequestsShipping()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var shipment = new CustomerShipmentBuilder(this.Session).Build();

            var shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithGood(item.Product as Good)
                .WithQuantity(1)
                .WithReservedFromInventoryItem(item.ReservedFromNonSerialisedInventoryItem)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            new OrderShipmentBuilder(this.Session)
                .WithOrderItem(item)
                .WithShipmentItem(shipmentItem)
                .WithQuantity(1)
                .Build();

            this.Session.Derive(false);

            Assert.Equal(item.QuantityOrdered - 1, item.QuantityRequestsShipping);
        }
    }

    public class SalesOrderItemInventoryAssignmentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemInventoryAssignmentDerivationTests(Fixture fixture) : base(fixture) { }
    }

    [Trait("Category", "Security")]
    public class SalesOrderItemDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void OnChangedSalesOrderItemStateProvisionalDeriveDeletePermission()
        {
            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var deletePermission = new Permissions(this.Session).Get(this.M.SalesOrderItem.ObjectType, this.M.SalesOrderItem.Delete);
            Assert.DoesNotContain(deletePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemStateCancelledDeriveDeletePermission()
        {
            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.SalesOrderItemState = new SalesOrderItemStates(this.Session).RequestsApproval;
            this.Session.Derive(false);

            var deletePermission = new Permissions(this.Session).Get(this.M.SalesOrderItem.ObjectType, this.M.SalesOrderItem.Delete);
            Assert.Contains(deletePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemInvoiceStateInvoicedDeriveDeniablePermission()
        {
            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.SalesOrderItemInvoiceState = new SalesOrderItemInvoiceStates(this.Session).Invoiced;
            this.Session.Derive(false);

            var deniablePermission = new Permissions(this.Session).Get(this.M.SalesOrderItem.ObjectType, this.M.SalesOrderItem.Cancel);
            Assert.Contains(deniablePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemShipmentStateInvoicedDeriveDeniablePermission()
        {
            var item = new SalesOrderItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            item.SalesOrderItemShipmentState = new SalesOrderItemShipmentStates(this.Session).Shipped;
            this.Session.Derive(false);

            var deniablePermission = new Permissions(this.Session).Get(this.M.SalesOrderItem.ObjectType, this.M.SalesOrderItem.Cancel);
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
        private VatRate vatRate21;

        public override Config Config => new Config { SetupSecurity = true };

        public SalesOrderItemSecurityTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");

            this.supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();

            this.vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            this.kiev = new CityBuilder(this.Session).WithName("Kiev").Build();

            this.shipToContactMechanismMechelen = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            this.shipToContactMechanismKiev = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(this.kiev).WithAddress1("Dnieper").Build();
            this.shipToCustomer = new OrganisationBuilder(this.Session).WithName("shipToCustomer").Build();
            this.shipToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Session)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.billToCustomer = new OrganisationBuilder(this.Session)
                .WithName("billToCustomer")
                .WithPreferredCurrency(euro)

                .Build();

            this.billToCustomer.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Session)
                                                            .WithContactMechanism(this.shipToContactMechanismKiev)
                                                            .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                                                            .WithUseAsDefault(true)
                                                            .Build());

            this.part = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            this.good = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("good")
                .WithPart(this.part)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            new SupplierRelationshipBuilder(this.Session)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Session.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Session).WithCustomer(this.billToCustomer).Build();

            new CustomerRelationshipBuilder(this.Session).WithCustomer(this.shipToCustomer).Build();

            this.variantGood = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("variant good")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Session)
                    .WithProductIdentification(new PartNumberBuilder(this.Session)
                        .WithIdentification("2")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build())
                .Build();

            this.variantGood2 = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v10102")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("variant good2")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Session)
                    .WithProductIdentification(new PartNumberBuilder(this.Session)
                        .WithIdentification("3")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build())
                .Build();

            this.virtualGood = new NonUnifiedGoodBuilder(this.Session)
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("v10103")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(this.vatRate21)
                .WithName("virtual good")
                .WithVariant(this.variantGood)
                .WithVariant(this.variantGood2)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            this.ancestorProductCategory = new ProductCategoryBuilder(this.Session)
                .WithName("ancestor")
                .Build();

            this.parentProductCategory = new ProductCategoryBuilder(this.Session)
                .WithName("parent")
                .WithPrimaryParent(this.ancestorProductCategory)
                .Build();

            this.productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("gizmo")
                .Build();

            this.productCategory.AddSecondaryParent(this.parentProductCategory);

            this.goodPurchasePrice = new SupplierOfferingBuilder(this.Session)
                .WithPart(this.part)
                .WithSupplier(this.supplier)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithFromDate(this.Session.Now())
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.virtualGoodPurchasePrice = new SupplierOfferingBuilder(this.Session)
                .WithCurrency(euro)
                .WithFromDate(this.Session.Now())
                .WithSupplier(this.supplier)
                .WithPrice(8)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            this.feature1 = new ColourBuilder(this.Session)
                .WithVatRate(this.vatRate21)
                .WithName("white")
                .Build();

            this.feature2 = new ColourBuilder(this.Session)
                .WithName("black")
                .Build();

            this.currentBasePriceGeoBoundary = new BasePriceBuilder(this.Session)
                .WithDescription("current BasePriceGeoBoundary ")
                .WithGeographicBoundary(mechelen)
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Session.Now())
                .Build();

            // historic basePrice for good
            new BasePriceBuilder(this.Session).WithDescription("previous good")
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for good
            new BasePriceBuilder(this.Session).WithDescription("future good")
                .WithProduct(this.good)
                .WithPrice(11)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            this.currentGoodBasePrice = new BasePriceBuilder(this.Session)
                .WithDescription("current good")
                .WithProduct(this.good)
                .WithPrice(10)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature1
            new BasePriceBuilder(this.Session).WithDescription("previous feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(0.5M)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for feature1
            new BasePriceBuilder(this.Session).WithDescription("future feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2.5M)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithDescription("current feature1")
                .WithProductFeature(this.feature1)
                .WithPrice(2)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature2
            new BasePriceBuilder(this.Session).WithDescription("previous feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for feature2
            new BasePriceBuilder(this.Session)
                .WithDescription("future feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(4)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            this.currentFeature2BasePrice = new BasePriceBuilder(this.Session)
                .WithDescription("current feature2")
                .WithProductFeature(this.feature2)
                .WithPrice(3)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for good with feature1
            new BasePriceBuilder(this.Session).WithDescription("previous good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(4)
                .WithFromDate(this.Session.Now().AddYears(-1))
                .WithThroughDate(this.Session.Now().AddDays(-1))
                .Build();

            // future basePrice for good with feature1
            new BasePriceBuilder(this.Session)
                .WithDescription("future good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(6)
                .WithFromDate(this.Session.Now().AddYears(1))
                .Build();

            this.currentGood1Feature1BasePrice = new BasePriceBuilder(this.Session)
                .WithDescription("current good/feature1")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(5)
                .WithFromDate(this.Session.Now())
                .WithThroughDate(this.Session.Now().AddYears(1).AddDays(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithDescription("current variant good2")
                .WithProduct(this.variantGood2)
                .WithPrice(11)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            this.order = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(this.shipToCustomer)
                .WithBillToCustomer(this.billToCustomer)
                .Build();

            this.Session.Derive();
            this.Session.Commit();
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCreated_ThenItemMayBeDeletedCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();
            this.Session.Commit();

            Assert.Equal(new SalesOrderItemStates(this.Session).Provisional, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Delete));
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsConfirmed_ThenItemMayBeCancelledOrRejectedButNotDeleted()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.order.SetReadyForPosting();

            this.Session.Derive();
            this.Session.Commit();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsPartiallyShipped_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            var shipment = (CustomerShipment)this.order.DerivedShipToAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Session.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Session.Derive();

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();
            this.Session.Derive();

            Assert.Equal(new SalesOrderItemShipmentStates(this.Session).PartiallyShipped, item.SalesOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCancelled_ThenItemMayNotBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            item.Cancel();

            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).Cancelled, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCancelled_ThenItemCanBeDeleted()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            item.Cancel();

            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).Cancelled, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsRejected_ThenItemMayNotBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            item.Reject();

            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).Rejected, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsRejected_ThenCanBeDeleted()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            item.Reject();

            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).Rejected, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.SalesOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCompleted_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(110).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            this.order.Ship();

            this.Session.Derive();

            var shipment = (CustomerShipment)this.order.DerivedShipToAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Session.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;
            pickList.SetPicked();

            this.Session.Derive();

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();

            this.Session.Derive();

            shipment.Invoice();

            this.Session.Derive();

            ((SalesInvoiceItem)shipment.ShipmentItems[0].ShipmentItemBillingsWhereShipmentItem[0].InvoiceItem).SalesInvoiceWhereSalesInvoiceItem.Send();

            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).Completed, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsFinished_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SalesOrderState = new SalesOrderStates(this.Session).Finished;

            this.Session.Derive();

            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item.SalesOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsPartiallyShipped_ThenProductChangeIsNotAllowed()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            var shipment = (CustomerShipment)this.order.DerivedShipToAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Session.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Session.Derive();

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();
            this.Session.Derive();

            Assert.Equal(new SalesOrderItemShipmentStates(this.Session).PartiallyShipped, item.SalesOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanWrite(this.M.SalesOrderItem.Product));
        }

        [Fact]
        public void GivenOrderItem_WhenShippingInProgress_ThenCancelIsNotAllowed()
        {
            var administrator = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(administrator);

            this.Session.Derive();
            this.Session.Commit();

            this.InstantiateObjects(this.Session);

            User user = this.Administrator;
            this.Session.SetUser(user);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(this.part).Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(this.good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddSalesOrderItem(item);

            this.Session.Derive();

            this.order.SetReadyForPosting();
            this.Session.Derive();

            this.order.Post();
            this.Session.Derive();

            this.order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderItemShipmentStates(this.Session).InProgress, item.SalesOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.SalesOrderItem.Cancel));
        }

        private void InstantiateObjects(ISession session)
        {
            this.productCategory = (ProductCategory)session.Instantiate(this.productCategory);
            this.parentProductCategory = (ProductCategory)session.Instantiate(this.parentProductCategory);
            this.ancestorProductCategory = (ProductCategory)session.Instantiate(this.ancestorProductCategory);
            this.part = (Part)session.Instantiate(this.part);
            this.virtualGood = (Good)session.Instantiate(this.virtualGood);
            this.good = (Good)session.Instantiate(this.good);
            this.feature1 = (Colour)session.Instantiate(this.feature1);
            this.feature2 = (Colour)session.Instantiate(this.feature2);
            this.shipToCustomer = (Organisation)session.Instantiate(this.shipToCustomer);
            this.billToCustomer = (Organisation)session.Instantiate(this.billToCustomer);
            this.supplier = (Organisation)session.Instantiate(this.supplier);
            this.kiev = (City)session.Instantiate(this.kiev);
            this.shipToContactMechanismMechelen = (PostalAddress)session.Instantiate(this.shipToContactMechanismMechelen);
            this.shipToContactMechanismKiev = (PostalAddress)session.Instantiate(this.shipToContactMechanismKiev);
            this.currentBasePriceGeoBoundary = (BasePrice)session.Instantiate(this.currentBasePriceGeoBoundary);
            this.currentGoodBasePrice = (BasePrice)session.Instantiate(this.currentGoodBasePrice);
            this.currentGood1Feature1BasePrice = (BasePrice)session.Instantiate(this.currentGood1Feature1BasePrice);
            this.currentFeature2BasePrice = (BasePrice)session.Instantiate(this.currentFeature2BasePrice);
            this.goodPurchasePrice = (SupplierOffering)session.Instantiate(this.goodPurchasePrice);
            this.virtualGoodPurchasePrice = (SupplierOffering)session.Instantiate(this.virtualGoodPurchasePrice);
            this.currentGoodBasePrice = (BasePrice)session.Instantiate(this.currentGoodBasePrice);
            this.order = (SalesOrder)session.Instantiate(this.order);
            this.vatRate21 = (VatRate)session.Instantiate(this.vatRate21);
        }
    }
}
