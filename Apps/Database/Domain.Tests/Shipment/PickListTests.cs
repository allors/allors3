// <copyright file="PickListTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PickListTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPickListBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var pickList = new PickListBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            Assert.Equal(new PickListStates(this.Transaction).Created, pickList.PickListState);
        }

        [Fact]
        public void GivenPickList_WhenPicked_ThenInventoryIsAdjustedAndOrderItemsQuantityPickedIsSet()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen)
                .WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var customer = new PersonBuilder(this.Transaction).WithLastName("person1")
                .WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good1.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithPrice(7)
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var good1Inventory = (NonSerialisedInventoryItem)good1.Part.InventoryItemsWherePart.First;
            var good2Inventory = (NonSerialisedInventoryItem)good2.Part.InventoryItemsWherePart.First;

            var colorWhite = new ColourBuilder(this.Transaction)
                .WithName("white")
                .Build();
            var extraLarge = new SizeBuilder(this.Transaction)
                .WithName("Extra large")
                .Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();

            var item2 = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem)
                .WithProductFeature(colorWhite).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();

            var item3 = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem)
                .WithProductFeature(extraLarge).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();

            item1.AddOrderedWithFeature(item2);
            item1.AddOrderedWithFeature(item3);

            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();

            var item5 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();

            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);
            order.AddSalesOrderItem(item5);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = good1.Part.InventoryItemsWherePart[0].PickListItemsWhereInventoryItem[0].PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            //// item5: only 4 out of 5 are actually picked
            foreach (PickListItem pickListItem in pickList.PickListItems)
            {
                if (pickListItem.Quantity == 5)
                {
                    pickListItem.QuantityPicked = 4;
                }
            }

            pickList.SetPicked();

            this.Transaction.Derive();

            //// all orderitems have same physical finished good, so there is only one picklist item.
            Assert.Equal(1, item1.QuantityReserved);
            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(2, item4.QuantityReserved);
            Assert.Equal(0, item4.QuantityRequestsShipping);
            Assert.Equal(5, item5.QuantityReserved);
            Assert.Equal(0, item5.QuantityRequestsShipping);
            Assert.Equal(97, good1Inventory.QuantityOnHand);
            Assert.Equal(0, good1Inventory.QuantityCommittedOut);
            Assert.Equal(96, good2Inventory.QuantityOnHand);
            Assert.Equal(1, good2Inventory.QuantityCommittedOut);
        }

        [Fact]
        public void GivenPickList_WhenActualQuantityPickedIsLess_ThenShipmentItemQuantityIsAdjusted()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen)
                .WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var customer = new PersonBuilder(this.Transaction).WithLastName("person1")
                .WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good1.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good2.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress[0];
            shipment.Pick();
            this.Transaction.Derive();

            var pickList = good1.Part.InventoryItemsWherePart[0].PickListItemsWhereInventoryItem[0].PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            //// item3: only 4 out of 5 are actually picked
            PickListItem adjustedPicklistItem = null;
            foreach (PickListItem pickListItem in pickList.PickListItems)
            {
                if (pickListItem.Quantity == 5)
                {
                    adjustedPicklistItem = pickListItem;
                }
            }

            var itemIssuance = adjustedPicklistItem.ItemIssuancesWherePickListItem[0];
            var shipmentItem = adjustedPicklistItem.ItemIssuancesWherePickListItem[0].ShipmentItem;

            Assert.Equal(2, shipment.ShipmentItems.Count);
            Assert.Equal(5, itemIssuance.Quantity);
            Assert.Equal(5, shipmentItem.Quantity);
            Assert.Equal(5, item3.QuantityPendingShipment);

            adjustedPicklistItem.QuantityPicked = 4;

            pickList.SetPicked();
            this.Transaction.Derive();

            // When SalesOrder is derived 1 quantity is requested for shipping (because inventory is available and quantity ordered (5) is greater then quantity pending shipment (4)
            // A new shipment item is created with quantity 1 and QuantityPendingShipment remains 5
            Assert.Equal(4, itemIssuance.Quantity);
            Assert.Equal(4, shipmentItem.Quantity);
            Assert.Equal(3, shipment.ShipmentItems.Count);
            Assert.Equal(1, shipment.ShipmentItems.Last().Quantity);
            Assert.Equal(5, item3.QuantityPendingShipment);
        }

        [Fact]
        public void GivenSalesOrder_WhenShipmentIsCreated_ThenOrderItemsAreAddedToPickList()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen)
                .WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("person1")
                .WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good1.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithPrice(7)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good2.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithPrice(7)
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var good1Inventory = good1.Part.InventoryItemsWherePart.First;
            var good2Inventory = good2.Part.InventoryItemsWherePart.First;

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1)
                .WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2)
                .WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(5)
                .WithAssignedUnitPrice(15).Build();

            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress[0];
            shipment.Pick();
            this.Transaction.Derive();

            var pickList = good1.Part.InventoryItemsWherePart[0].PickListItemsWhereInventoryItem[0]
                .PickListWherePickListItem;

            Assert.Equal(2, pickList.PickListItems.Count);

            var extent1 = pickList.PickListItems;
            extent1.Filter.AddEquals(this.M.PickListItem.InventoryItem, good1Inventory);
            Assert.Equal(3, extent1.First.Quantity);

            var extent2 = pickList.PickListItems;
            extent2.Filter.AddEquals(this.M.PickListItem.InventoryItem, good2Inventory);
            Assert.Equal(5, extent2.First.Quantity);
        }

        [Fact]
        public void GivenMultipleOrders_WhenCombinedPickListIsPicked_ThenSingleShipmentIsPickedState()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen)
                .WithAddress1("Haverwerf").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var customer = new PersonBuilder(this.Transaction).WithLastName("person1")
                .WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer)
                .Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good1.Part)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithPrice(7)
                .WithSupplier(supplier)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good2.Part)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .WithPrice(7)
                .WithSupplier(supplier)
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1)
                .WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2)
                .WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(5)
                .WithAssignedUnitPrice(15).Build();

            order1.AddSalesOrderItem(item1);
            order1.AddSalesOrderItem(item2);
            order1.AddSalesOrderItem(item3);

            this.Transaction.Derive();

            order1.SetReadyForPosting();
            this.Transaction.Derive();

            order1.Post();
            this.Transaction.Derive();

            order1.Accept();
            this.Transaction.Derive();

            var order2 = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var itema = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1)
                .WithAssignedUnitPrice(15).Build();
            var itemb = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(1)
                .WithAssignedUnitPrice(15).Build();
            order2.AddSalesOrderItem(itema);
            order2.AddSalesOrderItem(itemb);

            this.Transaction.Derive();

            order2.SetReadyForPosting();
            this.Transaction.Derive();

            order2.Post();
            this.Transaction.Derive();

            order2.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress[0];
            shipment.Pick();
            this.Transaction.Derive();

            var pickList = good1.Part.InventoryItemsWherePart[0].PickListItemsWhereInventoryItem[0]
                .PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            this.Transaction.Derive();

            Assert.Single(customer.ShipmentsWhereShipToParty);

            var customerShipment = (CustomerShipment)customer.ShipmentsWhereShipToParty.First;
            Assert.Equal(new ShipmentStates(this.Transaction).Picked, customerShipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerBuyingFromDifferentStores_WhenShipping_ThenPickListIsCreatedForEachStore()
        {
            var store1 = new Stores(this.Transaction).FindBy(this.M.Store.Name, "store");

            var store2 = new StoreBuilder(this.Transaction).WithName("second store")
                .WithDefaultFacility(new Facilities(this.Transaction).Extent().First)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithSalesOrderNumberPrefix("")
                .WithCustomerShipmentNumberPrefix("")
                .WithIsImmediatelyPacked(true)
                .Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive(true);

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithStore(store1)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .WithShipToCustomer(customer)
                .Build();

            var order1Item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order1.AddSalesOrderItem(order1Item);

            this.Transaction.Derive(true);

            order1.SetReadyForPosting();
            this.Transaction.Derive(true);

            order1.Post();
            this.Transaction.Derive();

            order1.Accept();
            this.Transaction.Derive();

            Assert.Single(customer.ShipmentsWhereShipToParty);

            var order2 = new SalesOrderBuilder(this.Transaction)
                .WithStore(store1)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var order2Item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            order2.AddSalesOrderItem(order2Item);

            this.Transaction.Derive(true);

            order2.SetReadyForPosting();
            this.Transaction.Derive(true);

            order2.Post();
            this.Transaction.Derive();

            order2.Accept();
            this.Transaction.Derive();

            Assert.Single(customer.ShipmentsWhereShipToParty);

            var store1Shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress.First(v => v.Store.Equals(store1));

            store1Shipment.Pick();
            this.Transaction.Derive();

            var order3 = new SalesOrderBuilder(this.Transaction)
                .WithStore(store2)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var order3Item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order3.AddSalesOrderItem(order3Item);

            this.Transaction.Derive(true);

            order3.SetReadyForPosting();
            this.Transaction.Derive(true);

            order3.Post();
            this.Transaction.Derive();

            order3.Accept();
            this.Transaction.Derive();

            var store2Shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress.First(v => v.Store.Equals(store2));

            store2Shipment.Pick();
            this.Transaction.Derive();

            var store1PickList = customer.PickListsWhereShipToParty.FirstOrDefault(v => v.Store.Equals(store1));
            var store2PickList = customer.PickListsWhereShipToParty.FirstOrDefault(v => v.Store.Equals(store2));

            Assert.Equal(2, customer.PickListsWhereShipToParty.Count);
            Assert.NotNull(store1PickList);
            Assert.Equal(3, store1PickList.PickListItems[0].Quantity);
            Assert.NotNull(store2PickList);
            Assert.Equal(5, store2PickList.PickListItems[0].Quantity);
        }
    }

    public class PickListDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDerivePickListState()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsImmediatelyPicked = true;
            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            Assert.Equal(new PickListStates(this.Transaction).Picked, pickList.PickListState);
        }
    }

    [Trait("Category", "Security")]
    public class PickListSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenPickList_WhenObjectStateIsCreated_ThenCheckTransitions()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var pickList = new PickListBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[pickList];
            Assert.True(acl.CanExecute(this.M.PickList.Cancel));
        }

        [Fact]
        public void GivenPickList_WhenObjectStateIsCancelled_ThenCheckTransitions()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            User user = this.OrderProcessor;
            this.Transaction.SetUser(user);

            var pickList = new PickListBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            pickList.Cancel();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[pickList];
            Assert.False(acl.CanExecute(this.M.PickList.Cancel));
            Assert.False(acl.CanExecute(this.M.PickList.SetPicked));
        }

        [Fact]
        public void GivenPickList_WhenObjectStateIsPicked_ThenCheckTransitions()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            User user = this.OrderProcessor;
            this.Transaction.SetUser(user);

            var pickList = new PickListBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            pickList.SetPicked();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[pickList];
            Assert.False(acl.CanExecute(this.M.PickList.Cancel));
            Assert.False(acl.CanExecute(this.M.PickList.SetPicked));
        }
    }
}
