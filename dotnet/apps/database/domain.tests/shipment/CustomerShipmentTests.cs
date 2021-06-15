// <copyright file="CustomerShipmentTests.cs" company="Allors bvba">
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
    using Xunit;

    public class CustomerShipmentTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerShipmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCustomerShipment_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
            Assert.Equal(shipment.LastShipmentState, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipment_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive();

            Assert.Null(shipment.PreviousShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(new PostalAddresses(this.Transaction).Extent().First)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Boat)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
            Assert.Equal(shipment.ShipFromParty, shipment.ShipFromParty);
            Assert.Equal(new Stores(this.Transaction).FindBy(this.M.Store.Name, "store"), shipment.Store);
        }

        [Fact]
        public void GivenCustomerShipment_WhenGettingShipmentNumberWithoutFormat_ThenShipmentNumberShouldBeReturned()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var store = new StoreBuilder(this.Transaction).WithName("store")
                .WithDefaultFacility(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse))
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithIsImmediatelyPacked(true)
                .Build();

            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipment1 = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).WithLastName("person1").Build())
                .WithShipToAddress(shipToAddress)
                .WithStore(store)
                .WithShipmentMethod(store.DefaultShipmentMethod)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("1", shipment1.ShipmentNumber);

            var shipment2 = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).WithLastName("person1").Build())
                .WithStore(store)
                .WithShipmentMethod(store.DefaultShipmentMethod)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("2", shipment2.ShipmentNumber);
        }

        [Fact]
        public void GivenCustomerShipment_WhenGettingShipmentNumberWithFormat_ThenFormattedShipmentNumberShouldBeReturned()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.InternalOrganisation.CustomerShipmentSequence = new CustomerShipmentSequences(this.Transaction).EnforcedSequence;
            var store = new StoreBuilder(this.Transaction).WithName("store")
                .WithDefaultFacility(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse))
                .WithCustomerShipmentNumberPrefix("the format is ")
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithIsImmediatelyPacked(true)
                .Build();

            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipment1 = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).WithLastName("person1").Build())
                .WithShipToAddress(shipToAddress)
                .WithStore(store)
                .WithShipmentMethod(store.DefaultShipmentMethod)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("the format is 1", shipment1.ShipmentNumber);

            var shipment2 = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).WithLastName("person1").Build())
                .WithShipToAddress(shipToAddress)
                .WithStore(store)
                .WithShipmentMethod(store.DefaultShipmentMethod)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("the format is 2", shipment2.ShipmentNumber);
        }

        [Fact]
        public void GivenShipFromWithoutShipmentNumberPrefix_WhenDeriving_ThenSortableShipmentNumberIsSet()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.RemoveCustomerShipmentNumberPrefix();
            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).WithLastName("person1").Build())
                .WithShipToAddress(shipToAddress)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(shipment.ShipmentNumber), shipment.SortableShipmentNumber);
        }

        [Fact]
        public void GivenShipFromWithShipmentNumberPrefix_WhenDeriving_ThenSortableShipmentNumberIsSet()
        {
            this.InternalOrganisation.CustomerShipmentSequence = new CustomerShipmentSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.CustomerShipmentNumberPrefix = "prefix-";
            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).WithLastName("person1").Build())
                .WithShipToAddress(shipToAddress)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(shipment.ShipmentNumber.Split('-')[1]), shipment.SortableShipmentNumber);
        }

        [Fact]
        public void GivenCustomerShipmentWithShipToCustomerWithShippingAddress_WhenDeriving_ThenShipToAddressMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithAddress1("Haverwerf 15").WithPostalAddressBoundary(mechelen).Build();

            var shippingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(shippingAddress);

            this.Transaction.Derive();

            var customerShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(shippingAddress.ContactMechanism, customerShipment.ShipToAddress);
        }

        [Fact]
        public void GivenCustomerShipment_WhenAllItemsArePutIntoShipmentPackages_ThenShipmentStateIsPacked()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithAddress1("Haverwerf 15").WithPostalAddressBoundary(mechelen).Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
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

            customer.PickListsWhereShipToParty.First.SetPicked();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipment_WhenAddingAndRemovingPackages_ThenPackageSequenceNumberIsRecalculated()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToAddress(new PostalAddresses(this.Transaction).Extent().First)
                .WithShipToParty(new Organisations(this.Transaction).Extent().First)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Boat)
                .Build();

            var package1 = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package1);

            this.Transaction.Derive();

            Assert.Equal(1, package1.SequenceNumber);

            var package2 = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package2);

            this.Transaction.Derive();

            Assert.Equal(2, package2.SequenceNumber);

            var package3 = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package3);

            this.Transaction.Derive();

            Assert.Equal(3, package3.SequenceNumber);

            shipment.RemoveShipmentPackage(package1);

            this.Transaction.Derive();

            Assert.Equal(2, package2.SequenceNumber);
            Assert.Equal(3, package3.SequenceNumber);

            var package4 = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package4);

            this.Transaction.Derive();

            Assert.Equal(2, package2.SequenceNumber);
            Assert.Equal(3, package3.SequenceNumber);
            Assert.Equal(4, package4.SequenceNumber);

            shipment.RemoveShipmentPackage(package4);

            this.Transaction.Derive();

            var package5 = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package5);

            this.Transaction.Derive();

            Assert.Equal(2, package2.SequenceNumber);
            Assert.Equal(3, package3.SequenceNumber);
            Assert.Equal(4, package5.SequenceNumber);
        }

        [Fact]
        public void GivenCustomerShipment_WhenDeriving_ThenTotalShipmentValueIsCalculated()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;
            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good2.Part).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item1.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;
            Assert.Equal(75, shipment.ShipmentValue);

            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(10).WithAssignedUnitPrice(10).Build();
            order.AddSalesOrderItem(item2);

            //item2.Confirm();

            //this.Transaction.Derive();

            //Assert.Equal(175, shipment.ShipmentValue);
        }

        [Fact]
        public void GivenShipmentThreshold_WhenNewCustomerShipmentIsBelowThreshold_ThenShipmentIsSetOnHold()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;
            store.IsImmediatelyPicked = false;

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

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipment_WhenShipmentValueFallsBelowThreshold_ThenShipmentAndPendigPickListAreSetOnHold()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;
            store.IsImmediatelyPicked = false;

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

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentOnHold_WhenShipmentValueRisesAboveThreshold_ThenShipmentIsReleased()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;
            store.IsImmediatelyPicked = false;

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            item.QuantityOrdered = 10;

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentOnHold_WhenTrySetStateToShipped_ThenActionIsNotAllowed()
        {
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
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(10).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

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

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);

            shipment.Hold();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentOnHold_WhenShipmentIsReleased_ThenShipmentObjecStateIsSetToCreated()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;
            store.IsImmediatelyPicked = false;

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

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.ReleasedManually = true;

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentOnHoldWithPickedPickList_WhenShipmentIsReleased_ThenShipmentObjecStateIsSetToPicked()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;

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

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(10).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);

            shipment.Hold();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.Continue();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentOnHoldWithAllItemsPacked_WhenShipmentIsReleased_ThenShipmentObjecStateIsSetToPacked()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.ShipmentThreshold = 100;

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
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

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

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.Continue();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerBuyingFromDifferentStores_WhenShipping_ThenDifferentShipmentIsCreatedForEachStore()
        {
            new StoreBuilder(this.Transaction).WithName("second store")
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
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive(true);

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithStore(new Stores(this.Transaction).FindBy(this.M.Store.Name, "store"))
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
            this.Transaction.Derive(true);

            order1.Accept();
            this.Transaction.Derive();

            Assert.Single(customer.ShipmentsWhereShipToParty);

            var order2 = new SalesOrderBuilder(this.Transaction)
                .WithStore(new Stores(this.Transaction).FindBy(this.M.Store.Name, "store"))
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var order2Item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order2.AddSalesOrderItem(order2Item);

            this.Transaction.Derive(true);

            order2.SetReadyForPosting();
            this.Transaction.Derive(true);

            order2.Post();
            this.Transaction.Derive(true);

            order2.Accept();
            this.Transaction.Derive();

            Assert.Single(customer.ShipmentsWhereShipToParty);

            var order3 = new SalesOrderBuilder(this.Transaction)
                .WithStore(new Stores(this.Transaction).FindBy(this.M.Store.Name, "second store"))
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

            var order3Item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order3.AddSalesOrderItem(order3Item);

            this.Transaction.Derive(true);

            order3.SetReadyForPosting();
            this.Transaction.Derive(true);

            order3.Post();
            this.Transaction.Derive(true);

            order3.Accept();
            this.Transaction.Derive();

            Assert.Equal(2, customer.ShipmentsWhereShipToParty.Count);
        }

        [Fact]
        public void GivenCustomerShipment_WhenStateIsSetToShipped_ThenInvoiceIsCreated()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var billToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).WithPartyContactMechanism(billToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item1.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();

            this.Transaction.Derive();

            var salesInvoiceitem =
                (SalesInvoiceItem)shipment.ShipmentItems[0].ShipmentItemBillingsWhereShipmentItem[0].InvoiceItem;
            var invoice = salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;

            Assert.NotNull(invoice);
        }

        [Fact]
        public void GivenCustomerShipmentWithoutOrder_WhenStateIsSetToShipped_ThenInventoryIsUpdated()
        {
            var good = new UnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithPart(good)
                .Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(mechelenAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithGood(good).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);

            this.Transaction.Derive();

            var inventory = good.InventoryItemsWherePart.First as NonSerialisedInventoryItem;

            Assert.Equal(100, good.QuantityOnHand);
            Assert.Equal(100, good.AvailableToPromise);
            Assert.Equal(100, inventory.QuantityOnHand);
            Assert.Equal(0, shipmentItem.QuantityPicked);
            Assert.Equal(0, shipmentItem.QuantityShipped);

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Transaction.Derive();

            Assert.Equal(90, good.QuantityOnHand);
            Assert.Equal(90, good.AvailableToPromise);
            Assert.Equal(90, inventory.QuantityOnHand);
            Assert.Equal(10, shipmentItem.QuantityPicked);
            Assert.Equal(0, shipmentItem.QuantityShipped);

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            shipment.AddShipmentPackage(package);

            this.Transaction.Derive();

            Assert.Equal(90, good.QuantityOnHand);
            Assert.Equal(90, good.AvailableToPromise);
            Assert.Equal(90, inventory.QuantityOnHand);
            Assert.Equal(10, shipmentItem.QuantityPicked);
            Assert.Equal(0, shipmentItem.QuantityShipped);

            shipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(90, good.QuantityOnHand);
            Assert.Equal(90, good.AvailableToPromise);
            Assert.Equal(90, inventory.QuantityOnHand);
            Assert.Equal(10, shipmentItem.QuantityPicked);
            Assert.Equal(10, shipmentItem.QuantityShipped);
        }

        [Fact]
        public void GivenCustomerShipmentContainingOrderOnHold_WhenTrySetStateToShipped_ThenActionIsNotAllowed()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            this.Transaction.Derive();
            this.Transaction.Commit();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

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

            order.Hold();

            this.Transaction.Derive();

            shipment.Ship();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentWithQuantityPackagedDifferentFromShippingQuantity_WhenTrySetStateToShipped_ThenActionIsNotAllowed()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

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
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

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
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity - 1).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();

            Assert.Equal(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);
        }

        [Fact]
        public void GivenCustomerShipmentWithValueBelowThreshold_WhenShippingToBelgium_ThenInvoiceIncludesCosts()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            new ShippingAndHandlingComponentBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeographicBoundary(mechelen)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipmentValue(new ShipmentValueBuilder(this.Transaction).WithThroughAmount(300M).Build())
                .WithCost(15M)
                .Build();

            new ShippingAndHandlingComponentBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithShipmentValue(new ShipmentValueBuilder(this.Transaction).WithThroughAmount(300M).Build())
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithCost(20M)
                .Build();

            new ShippingAndHandlingComponentBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithShipmentValue(new ShipmentValueBuilder(this.Transaction).WithThroughAmount(300M).Build())
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).FirstClassAir)
                .WithCost(50M)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            var billToContactMechanismMechelen = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Mechelen").Build();
            var internalOrganisation = this.InternalOrganisation;
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanismMechelen)
                .WithShipToCustomer(customer)
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

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

            var invoice = customer.SalesInvoicesWhereBillToCustomer.First;
            Assert.Equal(15M, invoice.TotalShippingAndHandling);
        }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            new CustomerShipmentBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.Equals("CustomerShipment.ShipToParty is required"));
        }
    }

    public class CustomerShipmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerShipmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            store.RemoveCustomerShipmentNumberPrefix();
            var number = this.InternalOrganisation.StoresWhereInternalOrganisation.First.CustomerShipmentNumberCounter.Value;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithStore(store).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            var number = store.CustomerShipmentNumberCounter.Value;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithStore(store).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipToAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipFromAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipmentStateCreateInvoice()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsAutomaticallyShipped = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Transaction).BillingForShipmentItems;

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            salesOrder.SalesOrderState = new SalesOrderStates(this.Transaction).OnHold;
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new OrderShipmentBuilder(this.Transaction).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Transaction.Derive(false);

            Assert.True(shipmentItem.ExistShipmentItemBillingsWhereShipmentItem);
        }
    }

    public class CustomerShipmentStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerShipmentStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentItemsDeriveShipmentStateCancelled()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedShipmentItemQuantityDeriveShipmentStateCancelled()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);

            shipmentItem.Quantity = 0;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedDerivationTriggerPartyDeriveShipmentStateCancelled()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction).WithShipToParty(shipment.ShipToParty).WithStore(shipment.Store).Build();
            this.Transaction.Derive(false);

            shipmentItem.Quantity = 0;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);

            picklist.Delete();
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedPickListShipToPartyDeriveShipmentStateCancelled()
        {
            var shipToParty = new PersonBuilder(this.Transaction).Build();
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(shipToParty).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction).WithShipToParty(shipment.ShipToParty).WithStore(shipment.Store).Build();
            this.Transaction.Derive(false);

            shipmentItem.Quantity = 0;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);

            picklist.RemoveShipToParty();
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedPickListPickListStateDeriveShipmentStateCancelled()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction).WithShipToParty(shipment.ShipToParty).WithStore(shipment.Store).Build();
            this.Transaction.Derive(false);

            shipmentItem.Quantity = 0;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);

            picklist.PickListState = new PickListStates(this.Transaction).Picked;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedShipmentStateDeriveShipmentStateCancelled()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction).WithShipToParty(shipment.ShipToParty).WithStore(shipment.Store).Build();
            this.Transaction.Derive(false);

            shipmentItem.Quantity = 0;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);

            picklist.PickListState = new PickListStates(this.Transaction).Picked;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Cancelled, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedPickListPickListStateDeriveShipmentStatePicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipmentState(new ShipmentStates(this.Transaction).Picking)
                .WithShipToParty(new PersonBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction).WithShipToParty(shipment.ShipToParty).WithStore(shipment.Store).Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);

            picklist.PickListState = new PickListStates(this.Transaction).Picked;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedShipmentStateDeriveShipmentStatePicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Picking;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Picked, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedShipmentStateDeriveShipmentStatePacked()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsImmediatelyPacked = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(new PersonBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(1).Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Picked;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedQuantityDeriveShipmentStatePacked()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsImmediatelyPacked = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipmentState(new ShipmentStates(this.Transaction).Picked)
                .WithShipToParty(new PersonBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(2).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(1).Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);

            shipmentItem.Quantity = 1;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedPackagingContentQuantityDeriveShipmentStatePacked()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsImmediatelyPacked = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipmentState(new ShipmentStates(this.Transaction).Picked)
                .WithShipToParty(new PersonBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);

            new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(1).Build();
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Packed, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedShipmentValueDeriveShipmentStateOnHold()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 10;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipmentValue(10).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.ShipmentValue = 9;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedStoreShipmentThresholdDeriveShipmentStateOnHold()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 10;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipmentValue(10).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 11;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedReleasedManuallyDeriveShipmentStateOnHold()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 10;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipmentValue(9).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.ReleasedManually = true;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedShipmentValueDeriveShipmentStateCreated()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 10;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipmentValue(9).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            shipment.ShipmentValue = 11;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedStoreShipmentThresholdDeriveShipmentStateCreated()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 10;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipmentValue(9).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).OnHold, shipment.ShipmentState);

            this.InternalOrganisation.StoresWhereInternalOrganisation.First.ShipmentThreshold = 9;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
        }
    }

    public class CustomerShipmentShipmentValueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerShipmentShipmentValueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderShipmentShipmentItemDeriveShipmentValue()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new OrderShipmentBuilder(this.Transaction).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, shipment.ShipmentValue);
        }

        [Fact]
        public void ChangedSalesOrderItemUnitPriceDeriveShipmentValue()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new OrderShipmentBuilder(this.Transaction).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, shipment.ShipmentValue);

            orderItem.AssignedUnitPrice = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, shipment.ShipmentValue);
        }

        [Fact]
        public void ChangedOrderShipmentQuantityDeriveShipmentValue()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var orderShipment = new OrderShipmentBuilder(this.Transaction).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, shipment.ShipmentValue);

            orderShipment.Quantity = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, shipment.ShipmentValue);
        }
    }

    public class CustomerShipmentShipRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerShipmentShipRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentStateDeriveShipmentState()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsAutomaticallyShipped = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Packed;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Shipped, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedPicklistPickListStateDeriveShipmentState()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsAutomaticallyShipped = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).OnHold)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Packed;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Shipped, shipment.ShipmentState);

            picklist.PickListState = new PickListStates(this.Transaction).Picked;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Shipped, shipment.ShipmentState);
        }

        [Fact]
        public void ChangedSalesOrderSalesOrderStateDeriveShipmentState()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.IsAutomaticallyShipped = true;

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            salesOrder.SalesOrderState = new SalesOrderStates(this.Transaction).OnHold;
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new OrderShipmentBuilder(this.Transaction).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            var picklist = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .WithShipToParty(shipment.ShipToParty)
                .WithStore(shipment.Store)
                .Build();
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Packed;
            this.Transaction.Derive(false);

            Assert.NotEqual(new ShipmentStates(this.Transaction).Shipped, shipment.ShipmentState);

            salesOrder.SalesOrderState = new SalesOrderStates(this.Transaction).InProcess;
            this.Transaction.Derive(false);

            Assert.Equal(new ShipmentStates(this.Transaction).Shipped, shipment.ShipmentState);
        }
    }

    [Trait("Category", "Security")]
    public class CustomerShipmentSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerShipmentSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenCustomerShipment_WhenObjectStateIsCreated_ThenCheckTransitions()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[shipment];
            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
            Assert.True(acl.CanExecute(this.M.CustomerShipment.Cancel));
        }

        [Fact]
        public void GivenCustomerShipment_WhenObjectStateIsCancelled_ThenCheckTransitions()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            User user = this.OrderProcessor;
            this.Transaction.SetUser(user);

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            shipment.Cancel();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[shipment];
            Assert.False(acl.CanExecute(this.M.CustomerShipment.Cancel));
        }

        [Fact]
        public void GivenCustomerShipment_WhenObjectStateIsShipped_ThenCheckTransitions()
        {
            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var assessable = new VatRegimes(this.Transaction).BelgiumStandard;

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var billToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).WithPartyContactMechanism(billToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedBillToContactMechanism(mechelenAddress)
                .WithAssignedVatRegime(assessable)
                .Build();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item1.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[shipment];
            Assert.Equal(new ShipmentStates(this.Transaction).Shipped, shipment.ShipmentState);
            Assert.False(acl.CanExecute(this.M.CustomerShipment.Cancel));
            Assert.False(acl.CanWrite(this.M.Shipment.HandlingInstruction));
            Assert.True(acl.CanWrite(this.M.Shipment.ElectronicDocuments));
        }
    }
}
