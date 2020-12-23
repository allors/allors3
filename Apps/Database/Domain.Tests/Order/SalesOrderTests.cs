// <copyright file="SalesOrderTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Resources;
    using Xunit;
    using System.Collections.Generic;
    using Database.Derivations;

    public class SalesOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSalesOrderBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Provisional, order.SalesOrderState);
            Assert.True(order.PartiallyShip);
            Assert.Equal(this.Session.Now().Date, order.OrderDate.Date);
            Assert.Equal(this.Session.Now().Date, order.EntryDate.Date);
            Assert.Equal(order.PreviousBillToCustomer, order.BillToCustomer);
            Assert.Equal(order.PreviousShipToCustomer, order.ShipToCustomer);
            Assert.Equal(order.DerivedVatRegime, order.BillToCustomer.VatRegime);
            Assert.Equal(new Stores(this.Session).FindBy(this.M.Store.Name, "store"), order.Store);
            Assert.Equal(order.Store.DefaultCollectionMethod, order.DerivedPaymentMethod);
            Assert.Equal(order.Store.DefaultShipmentMethod, order.DerivedShipmentMethod);
        }

        [Fact]
        public void GivenSalesOrder_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Provisional, order.SalesOrderState);
            Assert.Equal(order.LastSalesOrderState, order.SalesOrderState);
        }

        [Fact]
        public void GivenSalesOrder_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Null(order.PreviousSalesOrderState);
        }

        [Fact]
        public void GivenSalesOrderBuilder_WhenBuild_ThenOrderMustBeValid()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            new SalesOrderBuilder(this.Session).WithBillToCustomer(billToCustomer).WithTakenBy(this.InternalOrganisation).Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSalesOrderForItemsThatAreAvailable_WhenShipped_ThenOrderIsCompleted()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var billToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).WithPartyContactMechanism(billToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithPart(good1.Part).WithReason(new InventoryTransactionReasons(this.Session).Unknown).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithPart(good2.Part).WithReason(new InventoryTransactionReasons(this.Session).Unknown).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(mechelenAddress)
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress[0];

            shipment.Pick();
            this.Session.Derive();

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            // var derivation = new Allors.Domain.Logging.Derivation(this.Session, new DerivationConfig { DerivationLogFunc = () => new DerivationLog() });
            // derivation.Derive();

            // var list = ((DerivationLog)derivation.DerivationLog).List;
            ////list.RemoveAll(v => !v.StartsWith("Dependency"));

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

            Assert.Equal(new SalesOrderStates(this.Session).Completed, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Completed, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Completed, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Completed, item3.SalesOrderItemState);
        }

        [Fact]
        public void GivenSalesOrderShippedInMultipleParts_WhenPaymentsAreReceived_ThenObjectStateCorrespondingSalesOrderIsUpdated()
        {
            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var billToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).WithPartyContactMechanism(billToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).WithComment("item1").Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).WithComment("item2").Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).WithComment("item3").Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var shipment = (CustomerShipment)item1.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

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

            shipment.SetPacked();
            this.Session.Derive();

            shipment.Ship();
            this.Session.Derive();

            var salesInvoiceitem = (SalesInvoiceItem)shipment.ShipmentItems[0].ShipmentItemBillingsWhereShipmentItem[0].InvoiceItem;
            var invoice1 = (SalesInvoice)salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;
            invoice1.Send();

            new ReceiptBuilder(this.Session)
                .WithAmount(15)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Session).WithInvoiceItem(invoice1.SalesInvoiceItems[0]).WithAmountApplied(15).Build())
                .WithEffectiveDate(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item3.SalesOrderItemState);

            Assert.Equal(1, item1.QuantityShipped);
            Assert.Equal(0, item1.QuantityCommittedOut);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();

            this.Session.Derive();

            shipment = (CustomerShipment)item2.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Session.Derive();

            pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            this.Session.Derive();

            package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();

            this.Session.Derive();

            salesInvoiceitem = (SalesInvoiceItem)shipment.ShipmentItems[0].ShipmentItemBillingsWhereShipmentItem[0].InvoiceItem;
            var invoice2 = (SalesInvoice)salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;
            invoice2.Send();

            new ReceiptBuilder(this.Session)
                .WithAmount(30)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Session).WithInvoiceItem(invoice2.SalesInvoiceItems[0]).WithAmountApplied(30).Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item3.SalesOrderItemState);

            Assert.Equal(1, item1.QuantityShipped);
            Assert.Equal(0, item1.QuantityCommittedOut);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityShortFalled);

            Assert.Equal(2, item2.QuantityShipped);
            Assert.Equal(0, item2.QuantityCommittedOut);
            Assert.Equal(0, item2.QuantityPendingShipment);
            Assert.Equal(0, item2.QuantityRequestsShipping);
            Assert.Equal(0, item2.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            shipment = (CustomerShipment)item3.OrderShipmentsWhereOrderItem[0].ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Session.Derive();

            pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            this.Session.Derive();

            package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();

            this.Session.Derive();

            salesInvoiceitem =
                (SalesInvoiceItem)shipment.ShipmentItems[0].ShipmentItemBillingsWhereShipmentItem[0].InvoiceItem;
            var invoice3 = salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;

            new ReceiptBuilder(this.Session)
                .WithAmount(75)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Session).WithInvoiceItem(invoice3.SalesInvoiceItems[0]).WithAmountApplied(75).Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Finished, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Finished, item3.SalesOrderItemState);
        }

        [Fact]
        public void GivenPendingShipmentAndAssignedPickList_WhenNewOrderIsConfirmed_ThenNewPickListIsCreatedAndSingleOrderShipmentIsUpdated()
        {
            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order1 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(mechelenAddress)
                .WithAssignedVatRegime(assessable)
                .Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            order1.AddSalesOrderItem(item);

            this.Session.Derive();

            order1.SetReadyForPosting();
            this.Session.Derive();

            order1.Post();
            this.Session.Derive();

            order1.Accept();
            this.Session.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem.Single().ShipmentItem.ShipmentWhereShipmentItem;
            Assert.Equal(3, shipment.ShipmentItems[0].Quantity);

            shipment.Pick();
            this.Session.Derive();

            var pickList1 = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            Assert.Equal(3, pickList1.PickListItems[0].Quantity);

            pickList1.Picker = this.OrderProcessor;

            this.Session.Derive();

            var order2 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(mechelenAddress)
                .WithAssignedVatRegime(assessable)
                .Build();

            this.Session.Derive();

            item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(5)
                .Build();

            order2.AddSalesOrderItem(item);

            this.Session.Derive();

            order2.SetReadyForPosting();
            this.Session.Derive();

            order2.Post();
            this.Session.Derive();

            order2.Accept();
            this.Session.Derive();

            shipment.Pick();
            this.Session.Derive();

            Assert.Equal(5, shipment.ShipmentItems.First.Quantity);

            var pickList2 = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[1].PickListItem.PickListWherePickListItem;
            Assert.Equal(2, pickList2.PickListItems[0].Quantity);
        }

        [Fact]
        public void GivenSalesOrderOnHold_WhenInventoryBecomesAvailable_ThenOrderIsNotSelectedForShipment()
        {
            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Hold();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityCommittedOut);
            Assert.Equal(10, item.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityCommittedOut);
            Assert.Equal(10, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderOnHold_WhenOrderIsContinued_ThenOrderIsSelectedForShipment()
        {
            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Hold();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(10, item.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(10, item.QuantityShortFalled);

            order.Continue();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
            Assert.Equal(10, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderNotPartiallyShipped_WhenInComplete_ThenOrderIsNotSelectedForShipment()
        {
            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithPartiallyShip(false)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good1)
                .WithQuantityOrdered(20)
                .WithAssignedUnitPrice(5)
                .Build();

            var item2 = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good2)
                .WithQuantityOrdered(20)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.False(customer.ExistShipmentsWhereShipToParty);

            Assert.Equal(10, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(10, item1.QuantityShortFalled);

            Assert.Equal(10, item2.QuantityRequestsShipping);
            Assert.Equal(0, item2.QuantityPendingShipment);
            Assert.Equal(10, item2.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();

            this.Session.Derive();

            Assert.False(customer.ExistShipmentsWhereShipToParty);

            Assert.Equal(20, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(0, item1.QuantityShortFalled);

            Assert.Equal(10, item2.QuantityRequestsShipping);
            Assert.Equal(0, item2.QuantityPendingShipment);
            Assert.Equal(10, item2.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            Assert.True(customer.ExistShipmentsWhereShipToParty);

            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(20, item1.QuantityPendingShipment);
            Assert.Equal(0, item1.QuantityShortFalled);

            Assert.Equal(0, item2.QuantityRequestsShipping);
            Assert.Equal(20, item2.QuantityPendingShipment);
            Assert.Equal(0, item2.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderForStoreExceedingCreditLimit_WhenOrderIsConfirmed_ThenOrderRequestsApproval()
        {
            var store = this.Session.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var productItem = new InvoiceItemTypes(this.Session).ProductItem;
            var contactMechanism = new ContactMechanisms(this.Session).Extent().First;

            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(customer)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(this.Session.Now().AddYears(-2))
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var invoice = new SalesInvoiceBuilder(this.Session)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Session).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithInvoiceDate(this.Session.Now().AddYears(-1))
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Session).WithProduct(good).WithQuantity(10).WithAssignedUnitPrice(100).WithInvoiceItemType(productItem).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).RequestsApproval, order.SalesOrderState);
            Assert.Equal(0, item.QuantityReserved);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);

            order.Approve();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
            Assert.Equal(10, item.QuantityReserved);
            Assert.Equal(10, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderForCustomerExceedingCreditLimit_WhenOrderIsSetReadyForPosting_ThenOrderRequestsApproval()
        {
            var store = this.Session.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

            var productItem = new InvoiceItemTypes(this.Session).ProductItem;
            var contactMechanism = new ContactMechanisms(this.Session).Extent().First;

            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            var customerRelationship = new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(customer)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(this.Session.Now().AddYears(-2))
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var partyFinancial = customer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));
            partyFinancial.CreditLimit = 100M;

            this.Session.Derive();
            this.Session.Commit();

            var invoice = new SalesInvoiceBuilder(this.Session)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Session).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithInvoiceDate(this.Session.Now().AddYears(-1))
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Session).WithProduct(good).WithQuantity(10).WithAssignedUnitPrice(11).WithInvoiceItemType(productItem).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).RequestsApproval, order.SalesOrderState);
            Assert.Equal(0, item.QuantityReserved);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);

            order.Approve();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
            Assert.Equal(10, item.QuantityReserved);
            Assert.Equal(10, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderBelowOrderThreshold_WhenOrderIsConfirmed_ThenOrderIsNotShipped()
        {
            new Stores(this.Session).Extent().First.OrderThreshold = 1;

            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(customer)
                .WithInternalOrganisation(this.InternalOrganisation)
                .Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(0.1M)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).RequestsApproval, order.SalesOrderState);
        }

        [Fact]
        public void GivenSalesOrderWithManualShipmentSchedule_WhenOrderIsConfirmed_ThenInventoryIsNotReservedAndOrderIsNotShipped()
        {
            var assessable = new VatRegimes(this.Session).Assessable21;
            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();
            assessable.VatRate = vatRate0;

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            var inventoryItem = (NonSerialisedInventoryItem)good.Part.InventoryItemsWherePart.First;

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var manual = new OrderKindBuilder(this.Session).WithDescription("manual").WithScheduleManually(true).Build();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithOrderKind(manual)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(50)
                .WithAssignedUnitPrice(50)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
            Assert.Equal(0, item.QuantityReserved);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
            Assert.Equal(100, inventoryItem.QuantityOnHand);
            Assert.Equal(100, inventoryItem.AvailableToPromise);
        }

        [Fact]
        public void GivenConfirmedOrder_WhenOrderIsRejected_ThenNonSerialisedInventoryQuantitiesAreReleased()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .WithAssignedBillToContactMechanism(shipToContactMechanism)
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(3, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(97, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            order.Reject();

            this.Session.Derive();

            Assert.Equal(0, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenConfirmedOrder_WhenOrderIsCancelled_ThenNonSerialisedInventoryQuantitiesAreReleased()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .WithAssignedBillToContactMechanism(shipToContactMechanism)
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(3, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(97, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            order.Cancel();

            this.Session.Derive();

            Assert.Equal(0, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
        }

        [Fact]
        public void GivenSalesOrder_WhenConfirmed_ThenCurrentOrderStatusMustBeDerived()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Provisional, order.SalesOrderState);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).InProcess, order.SalesOrderState);
        }

        [Fact]
        public void GivenSalesOrderWithCancelledItem_WhenDeriving_ThenCancelledItemIsNotInValidOrderItems()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            item4.Cancel();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count);
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
        }

        [Fact]
        public void GivenSalesOrder_WhenGettingOrderNumberWithoutFormat_ThenOrderNumberShouldBeReturned()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var store = new Stores(this.Session).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.RemoveSalesOrderNumberPrefix();

            this.Session.Derive();

            var order1 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Session.Derive();

            Assert.Equal("1", order1.OrderNumber);

            var order2 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Session.Derive();

            Assert.Equal("2", order2.OrderNumber);
        }

        [Fact]
        public void GivenSalesOrder_WhenGettingOrderNumberWithFormat_ThenFormattedOrderNumberShouldBeReturned()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var store = new Stores(this.Session).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.SalesOrderNumberPrefix = "the format is ";

            this.Session.Derive();

            var order1 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Session.Derive();

            Assert.Equal("the format is 1", order1.OrderNumber);

            var order2 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Session.Derive();

            Assert.Equal("the format is 2", order2.OrderNumber);
        }

        [Fact]
        public void GivenIssuerWithoutOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.RemoveSalesOrderNumberPrefix();
            new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Session.Derive();

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive();

            Assert.Equal(int.Parse(order.OrderNumber), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenIssuerWithOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.SalesOrderNumberPrefix = "prefix-";
            new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Session.Derive();

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive();

            Assert.Equal(int.Parse(order.OrderNumber.Split('-')[1]), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.SalesOrderNumberPrefix = "prefix-{year}-";
            new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Session.Derive();

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive();

            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), order.OrderNumber.Split('-').Last())), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenSalesOrder_WhenDeriving_ThenTakenByContactMechanismMustExist()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var orderContact = new EmailAddressBuilder(this.Session).WithElectronicAddressString("orders@acme.com").Build();

            var orderFrom = new PartyContactMechanismBuilder(this.Session)
                .WithUseAsDefault(true)
                .WithContactMechanism(orderContact)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).OrderAddress)
                .Build();

            internalOrganisation.AddPartyContactMechanism(orderFrom);

            this.Session.Derive();

            var order1 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(internalOrganisation.OrderAddress, order1.DerivedTakenByContactMechanism);
        }

        [Fact]
        public void GivenSalesOrder_WhenDeriving_ThenBillFromContactMechanismMustExist()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var billingContact = new EmailAddressBuilder(this.Session)
                .WithElectronicAddressString("orders@acme.com")
                .Build();

            var billingFrom = new PartyContactMechanismBuilder(this.Session)
                .WithUseAsDefault(true)
                .WithContactMechanism(billingContact)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                .Build();

            internalOrganisation.AddPartyContactMechanism(billingFrom);

            this.Session.Derive();

            var order1 = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(internalOrganisation.BillingAddress, order1.DerivedTakenByContactMechanism);
        }

        [Fact]
        public void GivenSalesOrderWithBillToCustomerWithPreferredCurrency_WhenBuild_ThenCurrencyIsFromCustomer()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var englischLocale = new Locales(this.Session).EnglishGreatBritain;
            var poundSterling = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "GBP");

            var customer = new OrganisationBuilder(this.Session).WithName("customer").WithLocale(englischLocale).WithPreferredCurrency(poundSterling).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(poundSterling, order.DerivedCurrency);

            customer.PreferredCurrency = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");

            this.Session.Derive();

            Assert.Equal(customer.PreferredCurrency, order.DerivedCurrency);
        }

        [Fact]
        public void GivenSalesOrder_WhenDeriving_ThenLocaleMustExist()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .Build();

            this.Session.Derive();

            Assert.Equal(order.TakenBy.Locale, order.DerivedLocale);
        }

        [Fact]
        public void GivenSalesOrderWithShippingAndHandlingAmount_WhenDeriving_ThenOrderTotalsMustIncludeShippingAndHandlingAmount()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var euro = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var adjustment = new ShippingAndHandlingChargeBuilder(this.Session).WithAmount(7.5M).Build();

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Session.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Session.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Session.Derive();

            Assert.Equal(45, order.TotalBasePrice);
            Assert.Equal(0, order.TotalDiscount);
            Assert.Equal(0, order.TotalSurcharge);
            Assert.Equal(7.5m, order.TotalShippingAndHandling);
            Assert.Equal(0, order.TotalFee);
            Assert.Equal(52.5m, order.TotalExVat);
            Assert.Equal(11.03m, order.TotalVat);
            Assert.Equal(63.53m, order.TotalIncVat);
        }

        [Fact]
        public void GivenSalesOrderWithShippingAndHandlingPercentage_WhenDeriving_ThenOrderTotalsMustIncludeShippingAndHandlingAmount()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var euro = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var adjustment = new ShippingAndHandlingChargeBuilder(this.Session).WithPercentage(5).Build();

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Session.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Session.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Session.Derive();

            Assert.Equal(45, order.TotalBasePrice);
            Assert.Equal(0, order.TotalDiscount);
            Assert.Equal(0, order.TotalSurcharge);
            Assert.Equal(2.25m, order.TotalShippingAndHandling);
            Assert.Equal(0, order.TotalFee);
            Assert.Equal(47.25m, order.TotalExVat);
            Assert.Equal(9.92m, order.TotalVat);
            Assert.Equal(57.17m, order.TotalIncVat);
        }

        [Fact]
        public void GivenSalesOrderWithFeeAmount_WhenDeriving_ThenOrderTotalsMustIncludeFeeAmount()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var euro = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var adjustment = new FeeBuilder(this.Session).WithAmount(7.5M).Build();

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new SupplierOfferingBuilder(this.Session)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Session.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Session.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Session.Derive();

            Assert.Equal(45, order.TotalBasePrice);
            Assert.Equal(0, order.TotalDiscount);
            Assert.Equal(0, order.TotalSurcharge);
            Assert.Equal(0, order.TotalShippingAndHandling);
            Assert.Equal(7.5m, order.TotalFee);
            Assert.Equal(52.5m, order.TotalExVat);
            Assert.Equal(11.03m, order.TotalVat);
            Assert.Equal(63.53m, order.TotalIncVat);
        }

        [Fact]
        public void GivenSalesOrderWithoutShipFromAddress_WhenDeriving_ThenUseTakenByShipFromAddress()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var adjustment = new FeeBuilder(this.Session).WithAmount(7.5M).Build();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Session.Derive();

            Assert.NotNull(this.InternalOrganisation.ShippingAddress);
            Assert.Equal(order.DerivedShipFromAddress, this.InternalOrganisation.ShippingAddress);
        }

        [Fact]
        public void GivenSalesOrderWithShipFromAddress_WhenDeriving_ThenUseOrderShipFromAddress()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipFrom = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var adjustment = new FeeBuilder(this.Session).WithAmount(7.5M).Build();

            var order = new SalesOrderBuilder(this.Session)
                .WithAssignedShipFromAddress(shipFrom)
                .WithBillToCustomer(billToCustomer)
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Session.Derive();

            Assert.Equal(order.DerivedShipFromAddress, shipFrom);
        }

        [Fact]
        public void GivenSalesOrderWithFeePercentage_WhenDeriving_ThenOrderTotalsMustIncludeFeeAmount()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var euro = new Currencies(this.Session).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var adjustment = new FeeBuilder(this.Session).WithPercentage(5).Build();

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Session.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Session.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Session.Derive();

            Assert.Equal(45, order.TotalBasePrice);
            Assert.Equal(0, order.TotalDiscount);
            Assert.Equal(0, order.TotalSurcharge);
            Assert.Equal(0, order.TotalShippingAndHandling);
            Assert.Equal(2.25m, order.TotalFee);
            Assert.Equal(47.25m, order.TotalExVat);
            Assert.Equal(9.92m, order.TotalVat);
            Assert.Equal(57.17m, order.TotalIncVat);
        }

        [Fact]
        public void GivenSalesOrder_WhenConfirming_ThenInventoryItemsQuantityCommittedOutAndAvailableToPromiseMustBeUpdated()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(6, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(4, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(3, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(7, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
        }

        [Fact]
        public void GivenSalesOrder_WhenChangingItemQuantityToZero_ThenItemIsInvalid()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Session.Derive();
            Assert.Equal(4, order.ValidOrderItems.Count);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            item4.QuantityOrdered = 0;

            var expectedMessage = $"{item4} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void GivenSalesOrder_WhenOrderItemIsWithoutBasePrice_ThenExceptionIsThrown()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            item4.RemoveAssignedUnitPrice();

            var expectedMessage = $"{item4} {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
        }

        [Fact]
        public void GivenSalesOrder_WhenConfirming_ThenAllValidItemsAreInConfirmedState()
        {
            var billToCustomer = new PersonBuilder(this.Session).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Session).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            item4.Cancel();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count);
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).InProcess, item3.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Session).Cancelled, item4.SalesOrderItemState);
        }

        [Fact]
        public void GivenSalesOrder_WhenConfirmed_ThenShipmentItemsAreCreated()
        {
            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderKind(new OrderKindBuilder(this.Session).WithDescription("auto").WithScheduleManually(false).Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var shipment = customer.ShipmentsWhereShipToParty.First;

            Assert.Equal(2, shipment.ShipmentItems.Count);
            Assert.Equal(1, item1.OrderShipmentsWhereOrderItem[0].Quantity);
            Assert.Equal(2, item2.OrderShipmentsWhereOrderItem[0].Quantity);
            Assert.Equal(5, item3.OrderShipmentsWhereOrderItem[0].Quantity);
        }

        [Fact]
        public void GivenSalesOrderForSerialisedItem_WhenConfirmed_ThenShipmentItemIsCreated()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();

            var good1 = new NonUnifiedGoodBuilder(this.Session)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(vatRate21)
                .WithPart(new NonUnifiedPartBuilder(this.Session)
                    .WithProductIdentification(new PartNumberBuilder(this.Session)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised)
                    .Build())
                .Build();

            var serialisedItem1 = new SerialisedItemBuilder(this.Session).WithName("name").WithSerialNumber("1").WithAvailableForSale(true).Build();
            good1.Part.AddSerialisedItem(serialisedItem1);

            new SerialisedInventoryItemBuilder(this.Session).WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First).WithPart(good1.Part).WithSerialisedItem(serialisedItem1).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithPartyContactMechanism(shipToMechelen).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Session.Derive();
            order.SetReadyForPosting();

            //var derivation = new Logging.Derivation(this.Session, new DerivationConfig
            //    {
            //        DerivationLogFunc = () => new CustomListDerivationLog(),
            //    }
            //);

            //derivation.Derive();

            //var list = ((CustomListDerivationLog)derivation.DerivationLog).List;
            //list.RemoveAll(v => !v.StartsWith("Dependency"));

            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var shipment = customer.ShipmentsWhereShipToParty.First;

            Assert.Single(shipment.ShipmentItems);
            Assert.Equal(1, item1.OrderShipmentsWhereOrderItem[0].Quantity);
        }

        [Fact]
        public void GivenSalesOrderWithMultipleRecipients_WhenConfirmed_ThenShipmentIsCreatedForEachRecipientAndPickListIsCreated()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var baal = new CityBuilder(this.Session).WithName("Baal").Build();
            var baalAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(baal).WithAddress1("Haverwerf 15").Build();
            var shipToBaal = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(baalAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var person1 = new PersonBuilder(this.Session).WithLastName("person1").WithPartyContactMechanism(shipToMechelen).Build();
            var person2 = new PersonBuilder(this.Session).WithLastName("person2").WithPartyContactMechanism(shipToBaal).Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(person1).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(person2).WithInternalOrganisation(this.InternalOrganisation).Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            this.Session.Derive();

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            var colorBlack = new ColourBuilder(this.Session)
                .WithName("Black")
                .Build();

            var extraLarge = new SizeBuilder(this.Session)
                .WithName("Extra large")
                .Build();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(person1)
                .WithShipToCustomer(person1)
                .WithAssignedShipToAddress(mechelenAddress)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Export)
                .Build();

            this.Session.Derive();

            var item1 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(colorBlack).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(extraLarge).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            item1.AddOrderedWithFeature(item2);
            item1.AddOrderedWithFeature(item3);
            var item4 = new SalesOrderItemBuilder(this.Session).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item5 = new SalesOrderItemBuilder(this.Session).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).WithAssignedShipToParty(person2).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);
            order.AddSalesOrderItem(item5);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var shipmentToMechelen = mechelenAddress.ShipmentsWhereShipToAddress[0];

            var shipmentToBaal = baalAddress.ShipmentsWhereShipToAddress[0];

            this.Session.Derive();

            Assert.Equal(mechelenAddress, shipmentToMechelen.ShipToAddress);
            Assert.Single(shipmentToMechelen.ShipmentItems);
            Assert.Equal(3, shipmentToMechelen.ShipmentItems[0].Quantity);

            Assert.Equal(baalAddress, shipmentToBaal.ShipToAddress);
            Assert.Single(shipmentToBaal.ShipmentItems);
            Assert.Equal(good2, shipmentToBaal.ShipmentItems[0].Good);
            Assert.Equal(5, shipmentToBaal.ShipmentItems[0].Quantity);
        }

        [Fact]
        public void GivenSalesOrder_WhenShipToAndBillToAreSameCustomer_ThenDerivedCustomersIsSingleCustomer()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(customer)
                .WithBillToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Single(order.Customers);
            Assert.Equal(customer, order.Customers.First);
        }

        [Fact]
        public void GivenSalesOrder_WhenShipToAndBillToAreDifferentCustomers_ThenDerivedCustomersHoldsBothCustomers()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var billToCustomer = new OrganisationBuilder(this.Session).WithName("customer").Build();
            var shipToCustomer = new OrganisationBuilder(this.Session).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(shipToCustomer)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(2, order.Customers.Count);
            Assert.Contains(billToCustomer, order.Customers);
            Assert.Contains(shipToCustomer, order.Customers);
        }

        [Fact]
        public void GivenSettingSerialisedItemSoldOnSalesOrderAccept_WhenAcceptingSalesOrder_ThenSerialisedItemStateIsChanged()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Session).SalesOrderAccept);

            this.Session.Derive();

            var vatRate0 = new VatRateBuilder(this.Session).WithRate(0).Build();

            var good = new NonUnifiedGoodBuilder(this.Session)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Session)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build())
                .WithVatRate(vatRate0)
                .WithPart(new NonUnifiedPartBuilder(this.Session)
                    .WithProductIdentification(new PartNumberBuilder(this.Session)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised)
                    .Build())
                .Build();

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithName("name").WithSerialNumber("1").WithAvailableForSale(true).Build();
            good.Part.AddSerialisedItem(serialisedItem);

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var billToCustomer = new OrganisationBuilder(this.Session).WithName("customer").Build();
            var shipToCustomer = new OrganisationBuilder(this.Session).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Session.Derive();

            var address = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var salesOrder = new SalesOrderBuilder(this.Session)
                .WithShipToCustomer(shipToCustomer)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(address)
                .WithAssignedShipToAddress(address)
                .Build();

            this.Session.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithSerialisedItem(serialisedItem)
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Session).Sold)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(1)
                .Build();

            salesOrder.AddSalesOrderItem(orderItem);

            this.Session.Derive();

            salesOrder.SetReadyForPosting();
            this.Session.Derive();

            Assert.NotEqual(new SerialisedItemAvailabilities(this.Session).Sold, serialisedItem.SerialisedItemAvailability);

            salesOrder.Post();
            this.Session.Derive();

            salesOrder.Accept();
            this.Session.Derive();

            Assert.Equal(new SerialisedItemAvailabilities(this.Session).Sold, serialisedItem.SerialisedItemAvailability);
        }
    }

    public class SalesOrderBuildDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderBuildDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderState()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderState);
        }

        [Fact]
        public void DeriveSalesOrderShipmentState()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderShipmentState);
        }

        [Fact]
        public void DeriveSalesOrderPaymentState()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistSalesOrderPaymentState);
        }

        [Fact]
        public void DeriveOrderDate()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistOrderDate);
        }

        [Fact]
        public void DeriveEntryDate()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.True(order.ExistEntryDate);
        }

        [Fact]
        public void DeriveTakenBy()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Equal(order.TakenBy, this.InternalOrganisation);
        }

        [Fact]
        public void DeriveStore()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Equal(order.Store, this.InternalOrganisation.StoresWhereInternalOrganisation.First);
        }

        [Fact]
        public void DeriveOriginFacility()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Equal(order.OriginFacility, this.InternalOrganisation.StoresWhereInternalOrganisation.First.DefaultFacility);
        }
    }

    public class SalesOrderProvisionalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderProvisionalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedLocaleFromBillToCustomerLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Session)
               .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.Locale = swedishLocale;

            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.BillToCustomer = customer;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedLocale, customer.Locale);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedLocaleFromTakenByLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Session)
               .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            this.Session.GetSingleton().DefaultLocale = swedishLocale;

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();

            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.BillToCustomer = customer;
            this.Session.Derive(false);

            Assert.False(customer.ExistLocale);
            Assert.Equal(order.DerivedLocale, order.TakenBy.Locale);
        }

        [Fact]
        public void ChangedLocaleDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Session)
               .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            this.Session.GetSingleton().DefaultLocale = swedishLocale;

            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.Locale = swedishLocale;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Session).ServiceB2B;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedVatRegime()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();
            this.Session.Derive(false);

            customer.VatRegime = new VatRegimes(this.Session).Assessable10;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedVatRegime, customer.VatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedIrpfRegime()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();
            this.Session.Derive(false);

            customer.IrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedIrpfRegime, customer.IrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var swedishKrona = new Currencies(this.Session).FindBy(M.Currency.IsoCode, "SEK");
            order.AssignedCurrency = swedishKrona;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, order.AssignedCurrency);
        }

        [Fact]
        public void ChangedTakenByPreferredCurrencyDeriveDerivedCurrency()
        {
            Assert.True(this.InternalOrganisation.ExistPreferredCurrency);

            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, this.InternalOrganisation.PreferredCurrency);
        }

        [Fact]
        public void ChangedBillToCustomerLocaleDeriveDerivedCurrency()
        {
            var se = new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE");
            var newLocale = new LocaleBuilder(this.Session)
                .WithCountry(se)
                .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemovePreferredCurrency();
            customer.Locale = newLocale;

            order.BillToCustomer = customer;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, se.Currency);
        }

        [Fact]
        public void ChangedBillToCustomerPreferredCurrencyDeriveDerivedCurrency()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();
            customer.RemovePreferredCurrency();

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();
            this.Session.Derive(false);

            var swedishKrona = new Currencies(this.Session).FindBy(M.Currency.IsoCode, "SEK");
            customer.PreferredCurrency = swedishKrona;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveCurrencyFromBillToCustomerLocale()
        {
            var newLocale = new LocaleBuilder(this.Session)
                .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE"))
                .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.Locale = newLocale;
            customer.RemovePreferredCurrency();

            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.BillToCustomer = customer;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, newLocale.Country.Currency);
        }

        [Fact]
        public void ChangedAssignedTakenByContactMechanismDeriveDerivedTakenByContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedTakenByContactMechanism = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedTakenByContactMechanism, order.AssignedTakenByContactMechanism);
        }

        [Fact]
        public void ChangedTakenByOrderAddressDeriveDerivedTakenByContactMechanism()
        {
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Session).WithTakenBy(internalOrganisation).Build();
            this.Session.Derive(false);

            internalOrganisation.OrderAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedTakenByContactMechanism, internalOrganisation.OrderAddress);
        }

        [Fact]
        public void ChangedTakenByBillingAddressDeriveDerivedTakenByContactMechanism()
        {
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Session).WithTakenBy(internalOrganisation).Build();
            this.Session.Derive(false);

            internalOrganisation.BillingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedTakenByContactMechanism, internalOrganisation.BillingAddress);
        }

        [Fact]
        public void ChangedTakenByGeneralCorrespondenceDeriveDerivedTakenByContactMechanism()
        {
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Session).WithTakenBy(internalOrganisation).Build();
            this.Session.Derive(false);

            internalOrganisation.GeneralCorrespondence = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedTakenByContactMechanism, internalOrganisation.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedBillToContactMechanismDeriveDerivedBillToContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedBillToContactMechanism = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, order.AssignedBillToContactMechanism);
        }

        [Fact]
        public void OnChangedRoleBillToCustomerDeriveDerivedBillToContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            order.BillToCustomer = order.TakenBy.ActiveCustomers.First;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, order.BillToCustomer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerBillingAddressDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();
            this.Session.Derive(false);

            customer.BillingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, customer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerShippingAddressDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();
            this.Session.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerGeneralCorrespondenceDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();
            this.Session.Derive(false);

            customer.GeneralCorrespondence = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, customer.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedBillToEndCustomerContactMechanismDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedBillToEndCustomerContactMechanism = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, order.AssignedBillToEndCustomerContactMechanism);
        }

        [Fact]
        public void OnChangedRoleBillToEndCustomerDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            order.BillToEndCustomer = order.TakenBy.ActiveCustomers.First;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, order.BillToEndCustomer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerBillingAddressDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithBillToEndCustomer(customer).Build();
            this.Session.Derive(false);

            customer.BillingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, customer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerShippingAddressDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithBillToEndCustomer(customer).Build();
            this.Session.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerGeneralCorrespondenceDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithBillToEndCustomer(customer).Build();
            this.Session.Derive(false);

            customer.GeneralCorrespondence = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, customer.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedShipToEndCustomerAddressDeriveDerivedShipToEndCustomerAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedShipToEndCustomerAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToEndCustomerAddress, order.AssignedShipToEndCustomerAddress);
        }

        [Fact]
        public void OnChangedRoleShipToEndCustomerDeriveDerivedShipToEndCustomerAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            order.ShipToEndCustomer = order.TakenBy.ActiveCustomers.First;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToEndCustomerAddress, order.ShipToEndCustomer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToEndCustomerCustomerShippingAddressDeriveDerivedShipToEndCustomerAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithShipToEndCustomer(customer).Build();
            this.Session.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToEndCustomerAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToEndCustomerCustomerGeneralCorrespondenceDeriveDerivedShipToEndCustomerAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithShipToEndCustomer(customer).Build();
            this.Session.Derive(false);

            customer.GeneralCorrespondence = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToEndCustomerAddress, customer.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedShipFromAddressDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedShipFromAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipFromAddress, order.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedTakenByShippingAddressDeriveDerivedShipFromAddress()
        {
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Session).WithTakenBy(internalOrganisation).Build();
            this.Session.Derive(false);

            internalOrganisation.ShippingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipFromAddress, internalOrganisation.ShippingAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedShipToAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToAddress, order.AssignedShipToAddress);
        }

        [Fact]
        public void OnChangedRoleShipToCustomerDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Session).Build();

            this.Session.Derive(false);

            order.ShipToCustomer = order.TakenBy.ActiveCustomers.First;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToAddress, order.ShipToCustomer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToCustomerShippingAddressDeriveDerivedShipToAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Session).WithShipToCustomer(customer).Build();
            this.Session.Derive(false);

            customer.ShippingAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedAssignedShipmentMethodDeriveDerivedShipmentMethod()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentMethod = new ShipmentMethods(this.Session).Boat;
            order.AssignedShipmentMethod = shipmentMethod;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipmentMethod, shipmentMethod);
        }

        [Fact]
        public void ChangedShipToCustomerDefaultShipmentMethodDeriveDerivedShipmentMethod()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveDefaultShipmentMethod();

            var order = new SalesOrderBuilder(this.Session).WithShipToCustomer(customer).Build();
            this.Session.Derive(false);

            var shipmentMethod = new ShipmentMethods(this.Session).Boat;
            customer.DefaultShipmentMethod = shipmentMethod;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipmentMethod, shipmentMethod);
        }

        [Fact]
        public void ChangedStoreDefaultShipmentMethodDeriveDerivedShipmentMethod()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            store.RemoveDefaultShipmentMethod();

            var order = new SalesOrderBuilder(this.Session).WithStore(store).Build();
            this.Session.Derive(false);

            var shipmentMethod = new ShipmentMethods(this.Session).Boat;
            store.DefaultShipmentMethod = shipmentMethod;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipmentMethod, shipmentMethod);
        }

        [Fact]
        public void ChangedAssignedPaymentMethodDeriveDerivedPaymentMethod()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var cash = new CashBuilder(this.Session).Build();
            order.AssignedPaymentMethod = cash;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedPaymentMethod, cash);
        }

        [Fact]
        public void ChangedTakenByDefaultPaymentMethodDeriveDerivedPaymentMethod()
        {
            this.InternalOrganisation.RemoveDefaultPaymentMethod();

            var order = new SalesOrderBuilder(this.Session).WithTakenBy(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var cash = new CashBuilder(this.Session).Build();
            this.InternalOrganisation.DefaultPaymentMethod = cash;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedPaymentMethod, cash);
        }

        [Fact]
        public void ChangedStoreDefaultPaymentMethodDeriveDerivedPaymentMethod()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            store.RemoveDefaultCollectionMethod();

            var order = new SalesOrderBuilder(this.Session).WithStore(store).Build();
            this.Session.Derive(false);

            var cash = new CashBuilder(this.Session).Build();
            store.DefaultCollectionMethod = cash;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedPaymentMethod, cash);
        }
    }

    public class SalesOrderDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTakenByFromValidationError()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.TakenBy = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();

            var expectedError = $"{order} {this.M.SalesOrder.TakenBy} {ErrorMessages.InternalOrganisationChanged}";
            Assert.Equal(expectedError, this.Session.Derive(false).Errors[0].Message);
        }

        [Fact]
        public void ChangedShipToCustomerDeriveBillToCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.ShipToCustomer = customer;
            this.Session.Derive(false);

            Assert.Equal(order.BillToCustomer, order.ShipToCustomer);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveShipToCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.BillToCustomer = customer;
            this.Session.Derive(false);

            Assert.Equal(order.ShipToCustomer, order.BillToCustomer);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveCustomers()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.BillToCustomer = customer;
            this.Session.Derive(false);

            Assert.Contains(order.BillToCustomer, order.Customers);
        }

        [Fact]
        public void ChangedShipToCustomerDeriveCustomers()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.ShipToCustomer = customer;
            this.Session.Derive(false);

            Assert.Contains(order.ShipToCustomer, order.Customers);
        }

        [Fact]
        public void ChangedPlacingCustomerDeriveCustomers()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.PlacingCustomer = customer;
            this.Session.Derive(false);

            Assert.Contains(order.PlacingCustomer, order.Customers);
        }

        [Fact]
        public void ChangedStoreDeriveOrderNumber()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            var store = new Stores(this.Session).Extent().First;
            store.RemoveSalesOrderNumberPrefix();
            var number = store.SalesOrderCounter.Value;

            this.Session.Derive(false);

            Assert.Equal(order.OrderNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableOrderNumber()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            var number = new Stores(this.Session).Extent().First.SalesOrderCounter.Value;

            this.Session.Derive(false);

            Assert.Equal(order.SortableOrderNumber.Value, number + 1);
        }

        [Fact]
        public void ValidateBillToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.CustomerRelationshipsWhereCustomer.First.ThroughDate = this.Session.Now().AddDays(-1);

            this.Session.Derive(false);

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer).Build();

            var expectedMessage = $"{order} {this.M.SalesOrder.BillToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ValidateShipToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.CustomerRelationshipsWhereCustomer.First.ThroughDate = this.Session.Now().AddDays(-1);

            this.Session.Derive(false);

            var order = new SalesOrderBuilder(this.Session).WithShipToCustomer(customer).Build();

            var expectedMessage = $"{order} {this.M.SalesOrder.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void ValidateExistDerivedShipToAddress()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.RemoveDerivedShipToAddress();

            var expectedMessage = $"{order} {this.M.SalesOrder.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertExists: ")));
        }

        [Fact]
        public void ValidateExistDerivedBillToContactMechanism()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            order.RemoveDerivedBillToContactMechanism();

            var expectedMessage = $"{order} {this.M.SalesOrder.ShipToCustomer} { ErrorMessages.PartyIsNotACustomer}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertExists: ")));
        }

        [Fact]
        public void ChangedCanShipCreateShipment()
        {
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

            Assert.True(item.ExistOrderShipmentsWhereOrderItem);
        }

        [Fact]
        public void ChangedStoreAutoGenerateCustomerShipmentCreateShipment()
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

            Assert.False(item.ExistOrderShipmentsWhereOrderItem);

            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = true;
            this.Session.Derive(false);

            Assert.True(item.ExistOrderShipmentsWhereOrderItem);
        }

        [Fact]
        public void OnChangedValidOrderItemsSyncSalesOrderItemSyncedOrder()
        {
            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(order, orderItem.SyncedOrder);
        }
    }

    public class SalesOrderCanInvoiceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderCanInvoiceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void InvoicableOrderDeriveCanInvoiceFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForShipmentItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.False(order.CanInvoice);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanInvoiceFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            Assert.False(order.CanInvoice);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanInvoiceTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanInvoice);
        }

        [Fact]
        public void ChangedSalesOrderItemStateDeriveCanInvoice()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanInvoice);

            foreach(SalesOrderItem salesOrderItem in order.SalesOrderItems)
            {
                salesOrderItem.Cancel();
            }

            this.Session.Derive(false);

            Assert.False(order.CanInvoice);
        }

        [Fact]
        public void ChangedOrderItemBillingAmountDeriveCanInvoiceTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanInvoice);

            var orderItem = order.SalesOrderItems[0];
            var fullAmount = orderItem.QuantityOrdered * orderItem.UnitPrice;

            new OrderItemBillingBuilder(this.Session)
                .WithOrderItem(order.SalesOrderItems[0])
                .WithAmount(fullAmount)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.CanInvoice);
        }

        [Fact]
        public void ChangedOrderItemBillingAmountDeriveCanInvoiceFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanInvoice);

            foreach (SalesOrderItem salesOrderItem in order.SalesOrderItems)
            {
                var fullAmount = salesOrderItem.QuantityOrdered * salesOrderItem.UnitPrice;

                new OrderItemBillingBuilder(this.Session)
                    .WithOrderItem(salesOrderItem)
                    .WithAmount(fullAmount)
                    .Build();
            }

            this.Session.Derive(false);

            Assert.False(order.CanInvoice);
        }
    }

    public class SalesOrderCanShipDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderCanShipDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanShipFalse()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());

            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanShipTrue()
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

            Assert.True(order.CanShip);
        }

        [Fact]
        public void ChangedPartiallyShipDeriveCanShipFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

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
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanShip);

            order.PartiallyShip = false;
            this.Session.Derive(false);

            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedPartiallyShipDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered - 1)
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

            Assert.False(order.CanShip);

            order.PartiallyShip = true;
            this.Session.Derive(false);

            Assert.True(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityOrderedDeriveCanShipFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

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
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanShip);

            item.QuantityOrdered += 1;
            this.Session.Derive(false);

            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityOrderedDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered - 1)
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

            Assert.False(order.CanShip);

            item.QuantityOrdered -= 1;
            this.Session.Derive(false);

            Assert.True(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityRequestsShippingDeriveCanShipFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

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
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(order.CanShip);
            var before = item.QuantityRequestsShipping;

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Session).Consumption)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(before - 1, item.QuantityRequestsShipping);
            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityRequestsShippingDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(item.QuantityOrdered - 1)
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

            Assert.False(order.CanShip);
            var before = item.QuantityRequestsShipping;

            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(before + 1, item.QuantityRequestsShipping);
            Assert.True(order.CanShip);
        }
    }

    public class SalesOrderPriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderPriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedValidOrderItemsCalculatePrice()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());
            this.Session.Derive();

            Assert.True(order.TotalIncVat > 0);
            var totalIncVatBefore = order.TotalIncVat;

            order.SalesOrderItems.First.Cancel();
            this.Session.Derive();

            Assert.Equal(order.TotalIncVat, totalIncVatBefore - order.SalesOrderItems.First.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemsCalculatePriceForProductFeature()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            var productFeature = new ColourBuilder(this.Session)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProductFeature(productFeature)
                .WithPrice(0.2M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            var orderFeatureItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(productFeature).WithQuantityOrdered(1).Build();
            orderItem.AddOrderedWithFeature(orderFeatureItem);
            this.Session.Derive(false);

            Assert.Equal(1.2M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByPriceComponentFromDateCalculatePrice()
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

            Assert.Equal(0, order.TotalIncVat);

            basePrice.FromDate = this.Session.Now().AddMinutes(-1);
            this.Session.Derive(false);

            Assert.Equal(basePrice.Price, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByDiscountComponentPercentageCalculatePrice()
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

            new DiscountComponentBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredBySurchargeComponentPercentageCalculatePrice()
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

            new SurchargeComponentBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemQuantityOrderedCalculatePrice()
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
        public void OnChangedSalesOrderItemAssignedUnitPriceCalculatePrice()
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
        public void OnChangedSalesOrderItemProductCalculatePrice()
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
        public void OnChangedSalesOrderItemProductFeatureCalculatePrice()
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
                .WithPrice(1.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            var orderFeatureItem = new SalesOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(productFeature).WithQuantityOrdered(1).Build();
            orderItem.AddOrderedWithFeature(orderFeatureItem);
            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedBillToCustomerCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Session).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Session).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Session.Derive(false);

            Assert.NotEqual(customer1, customer2);

            new BasePriceBuilder(this.Session)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithPartyClassification(theBad)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Session).WithBillToCustomer(customer1).WithOrderDate(this.Session.Now()).Build();
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
        public void OnChangedRoleSalesOrderItemDiscountAdjustmentsCalculatePrice()
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
        public void OnChangedSalesOrderItemDiscountAdjustmentPercentageCalculatePrice()
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
        public void OnChangedSalesOrderItemDiscountAdjustmentAmountCalculatePrice()
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
        public void OnChangedRoleSalesOrderItemSurchargeAdjustmentsCalculatePrice()
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
        public void OnChangedSalesOrderItemSurchargeAdjustmentPercentageCalculatePrice()
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
        public void OnChangedSalesOrderItemSurchargeAdjustmentAmountCalculatePrice()
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
        public void OnChangedDiscountAdjustmentsCalculatePrice()
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
            order.AddOrderAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentPercentageCalculatePrice()
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
            order.AddOrderAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, order.TotalIncVat);

            discount.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(0.8M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentAmountCalculatePrice()
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
            order.AddOrderAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.5M, order.TotalIncVat);

            discount.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(0.6M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentsCalculatePrice()
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
            order.AddOrderAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentPercentageCalculatePrice()
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
            order.AddOrderAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, order.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(1.2M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentAmountCalculatePrice()
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
            order.AddOrderAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.5M, order.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(1.4M, order.TotalIncVat);
        }
    }

    public class SalesOrderStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveValidOrderItems()
        {
            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();

            this.Session.Derive();

            var orderItem1 = new SalesOrderItemBuilder(this.Session).WithDefaults().Build();
            order.AddSalesOrderItem(orderItem1);
            this.Session.Derive();

            var orderItem2 = new SalesOrderItemBuilder(this.Session).WithDefaults().Build();
            order.AddSalesOrderItem(orderItem2);
            this.Session.Derive();

            Assert.Equal(2, order.ValidOrderItems.Count);

            orderItem2.Cancel();
            this.Session.Derive();

            Assert.Single(order.ValidOrderItems);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsNotShipped()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.True(order.SalesOrderShipmentState.IsNotShipped);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsInProgress()
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

            Assert.True(order.SalesOrderShipmentState.IsInProgress);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsPartiallyShipped()
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

            Assert.True(order.SalesOrderShipmentState.IsPartiallyShipped);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsShipped()
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

            Assert.True(order.SalesOrderShipmentState.IsShipped);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemPaymentStateDeriveSalesOrderPaymentStateIsNotPaid()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.True(order.SalesOrderPaymentState.IsNotPaid);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemPaymentStateDeriveSalesOrderPaymentStateIsPartiallyPaid()
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

            Assert.True(order.SalesOrderPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemPaymentStateDeriveSalesOrderPaymentStateIsPaid()
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

            Assert.True(order.SalesOrderPaymentState.IsPaid);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemInvoiceStateDeriveSalesOrderInvoiceStateIsNotInvoiced()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = false;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.True(order.SalesOrderInvoiceState.IsNotInvoiced);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemInvoiceStateDeriveSalesOrderInvoiceStateIsInvoiced()
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

            invoice.Send();
            this.Session.Derive();

            Assert.True(order.SalesOrderInvoiceState.IsInvoiced);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemAvailability()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Session).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session).InRent;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.Equal(salesOrderItem.NextSerialisedItemAvailability, salesOrderItem.SerialisedItem.SerialisedItemAvailability);
        }

        [Fact]
        public void ChangedSalesOrderStateNotDeriveSerialisedItemSerialisedItemAvailability()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Session).CustomerShipmentShip);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.SerialisedItem.SerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session).Available;
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session).InRent;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.NotEqual(salesOrderItem.NextSerialisedItemAvailability, salesOrderItem.SerialisedItem.SerialisedItemAvailability);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemOwnedBy()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Session).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session).Sold;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.Equal(order.ShipToCustomer, salesOrderItem.SerialisedItem.OwnedBy);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemOwnership()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Session).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session).Sold;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.True(salesOrderItem.SerialisedItem.Ownership.IsThirdParty);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemAvailableForSale()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Session).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Session).InRent;
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive(false);

            order.Post();
            this.Session.Derive(false);

            order.Accept();
            this.Session.Derive(false);

            Assert.False(salesOrderItem.SerialisedItem.AvailableForSale);
        }
    }

    [Trait("Category", "Security")]
    public class SalesOrderDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
        }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveShipPermissionIsTrue()
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

            var shipPermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Ship);
            Assert.DoesNotContain(shipPermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveShipPermissionIsFalse()
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

            var shipPermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Ship);
            Assert.Contains(shipPermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveInvoicePermissionIsTrue()
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

            var invoicePermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Invoice);
            Assert.DoesNotContain(invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveInvoicePermissionIsFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive();

            var invoicePermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Invoice);
            Assert.Contains(invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedSalesOrderStateProvisionalDeriveDeletePermission()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var deletePermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Delete);
            Assert.DoesNotContain(deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedSalesOrderStateCancelledDeriveDeletePermission()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.Cancel();
            this.Session.Derive(false);

            var deletePermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Delete);
            Assert.DoesNotContain(deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedSalesOrderStateRejectedDeriveDeletePermission()
        {
            var order = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.Reject();
            this.Session.Derive(false);

            var deletePermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Delete);
            Assert.DoesNotContain(deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedQuoteDeriveDeletePermission()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());

            quote.SetReadyForProcessing();
            this.Session.Derive(false);

            quote.Order();
            this.Session.Derive(false);

            var order = quote.SalesOrderWhereQuote;

            var deletePermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Delete);
            Assert.Contains(deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedSalesOrderShipmentStateDeriveCancelPermission()
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

            var cancelPermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Cancel);
            Assert.Contains(cancelPermission, order.DeniedPermissions);
        }

        [Fact]
        public void ChangedSalesOrderInvoiceStateDeriveCancelPermission()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Session).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Invoice();
            this.Session.Derive();

            var cancelPermission = new Permissions(this.Session).Get(this.M.SalesOrder.ObjectType, this.M.SalesOrder.Cancel);
            Assert.Contains(cancelPermission, order.DeniedPermissions);
        }
    }

    [Trait("Category", "Security")]
    public class SalesOrderSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenSalesOrder_WhenObjectStateIsProvisional_ThenCheckTransitions()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            Assert.Equal(new SalesOrderStates(this.Session).Provisional, order.SalesOrderState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.True(acl.CanExecute(this.M.SalesOrder.SetReadyForPosting));
            Assert.True(acl.CanExecute(this.M.SalesOrder.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Approve));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Reject));
            Assert.True(acl.CanExecute(this.M.SalesOrder.Hold));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Continue));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Accept));
            Assert.True(acl.CanExecute(this.M.SalesOrder.DoTransfer));
        }

        [Fact]
        public void GivenSalesOrder_WhenObjectStateIsInProcess_ThenCheckTransitions()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            order.SetReadyForPosting();

            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.True(acl.CanExecute(this.M.SalesOrder.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Approve));
            Assert.True(acl.CanExecute(this.M.SalesOrder.Hold));
            Assert.False(acl.CanExecute(this.M.SalesOrder.SetReadyForPosting));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Continue));
        }

        [Fact]
        public void GivenSalesOrder_WhenObjectStateIsCancelled_ThenCheckTransitions()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            this.Session.SetUser(customer);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            order.Cancel();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Cancelled, order.SalesOrderState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.False(acl.CanExecute(this.M.SalesOrder.SetReadyForPosting));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Approve));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Continue));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Hold));
        }

        [Fact]
        public void GivenSalesOrder_WhenObjectStateIsRejected_ThenCheckTransitions()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            this.Session.SetUser(customer);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            order.Reject();

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Rejected, order.SalesOrderState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.False(acl.CanExecute(this.M.SalesOrder.SetReadyForPosting));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Approve));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Continue));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Hold));
        }

        [Fact]
        public void GivenSalesOrder_WhenObjectStateIsFinished_ThenCheckTransitions()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            this.Session.SetUser(customer);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.SalesOrderState = new SalesOrderStates(this.Session).Finished;

            this.Session.Derive();

            Assert.Equal(new SalesOrderStates(this.Session).Finished, order.SalesOrderState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.False(acl.CanExecute(this.M.SalesOrder.SetReadyForPosting));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Cancel));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Approve));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Continue));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Hold));
        }

        [Fact]
        public void GivenSalesOrder_WhenObjectStateIsOnHold_ThenCheckTransitions()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            this.Session.Derive();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();
            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            order.Hold();

            this.Session.Derive();
            this.Session.Commit();

            Assert.Equal(new SalesOrderStates(this.Session).OnHold, order.SalesOrderState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.True(acl.CanExecute(this.M.SalesOrder.Cancel));
            Assert.True(acl.CanExecute(this.M.SalesOrder.Continue));
            Assert.False(acl.CanExecute(this.M.SalesOrder.SetReadyForPosting));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Reject));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Approve));
            Assert.False(acl.CanExecute(this.M.SalesOrder.Hold));
        }

        [Fact]
        public void GivenSalesOrder_WhenShipmentStateIsInProgress_ThenCancelIsNotAllowed()
        {
            var customer = new PersonBuilder(this.Session).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();

            var good = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good.Part).Build();

            this.Session.Derive();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var order = new SalesOrderBuilder(this.Session)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            var item = new SalesOrderItemBuilder(this.Session)
                .WithProduct(good)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Session.Derive();

            order.SetReadyForPosting();
            this.Session.Derive();

            order.Post();
            this.Session.Derive();

            order.Accept();
            this.Session.Derive();

            Assert.Equal(new SalesOrderShipmentStates(this.Session).InProgress, order.SalesOrderShipmentState);
            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[order];
            Assert.False(acl.CanExecute(this.M.SalesOrder.Cancel));
        }
    }
}
