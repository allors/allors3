// <copyright file="SalesOrderTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Resources;
    using TestPopulation;
    using Xunit;

    public class SalesOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSalesOrderBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Provisional, order.SalesOrderState);
            Assert.True(order.PartiallyShip);
            Assert.Equal(this.Transaction.Now().Date, order.OrderDate.Date);
            Assert.Equal(this.Transaction.Now().Date, order.EntryDate.Date);
            Assert.Equal(order.PreviousBillToCustomer, order.BillToCustomer);
            Assert.Equal(order.PreviousShipToCustomer, order.ShipToCustomer);
            Assert.Equal(new Stores(this.Transaction).FindBy(this.M.Store.Name, "store"), order.Store);
            Assert.Equal(order.Store.DefaultCollectionMethod, order.DerivedPaymentMethod);
            Assert.Equal(order.Store.DefaultShipmentMethod, order.DerivedShipmentMethod);
        }

        [Fact]
        public void GivenSalesOrder_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Provisional, order.SalesOrderState);
            Assert.Equal(order.LastSalesOrderState, order.SalesOrderState);
        }

        [Fact]
        public void GivenSalesOrder_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Null(order.PreviousSalesOrderState);
        }

        [Fact]
        public void GivenSalesOrderBuilder_WhenBuild_ThenOrderMustBeValid()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            new SalesOrderBuilder(this.Transaction).WithBillToCustomer(billToCustomer).WithTakenBy(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSalesOrderForItemsThatAreAvailable_WhenShipped_ThenOrderIsCompleted()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithPart(good1.Part).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithPart(good2.Part).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(mechelenAddress)
                .Build();

            this.Transaction.Derive();

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

            var shipment = (CustomerShipment)mechelenAddress.ShipmentsWhereShipToAddress.ElementAt(0);

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(0).PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            // var derivation = new Allors.Domain.Logging.Derivation(this.Transaction, new DerivationConfig { DerivationLogFunc = () => new DerivationLog() });
            // derivation.Derive();

            // var list = ((DerivationLog)derivation.DerivationLog).List;
            ////list.RemoveAll(v => !v.StartsWith("Dependency"));

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

            Assert.Equal(new SalesOrderStates(this.Transaction).Completed, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Completed, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Completed, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Completed, item3.SalesOrderItemState);
        }

        [Fact]
        public void GivenSalesOrderShippedInMultipleParts_WhenPaymentsAreReceived_ThenObjectStateCorrespondingSalesOrderIsUpdated()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).WithComment("item1").Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).WithComment("item2").Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).WithComment("item3").Build();
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

            var shipment = (CustomerShipment)item1.OrderShipmentsWhereOrderItem.ElementAt(0).ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Transaction.Derive();

            var pickList = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(0).PickListItem.PickListWherePickListItem;
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

            shipment.SetPacked();
            this.Transaction.Derive();

            shipment.Ship();
            this.Transaction.Derive();

            var salesInvoiceitem = (SalesInvoiceItem)shipment.ShipmentItems.ElementAt(0).ShipmentItemBillingsWhereShipmentItem.ElementAt(0).InvoiceItem;
            var invoice1 = salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;

            invoice1.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(15)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice1.SalesInvoiceItems.ElementAt(0)).WithAmountApplied(15).Build())
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item3.SalesOrderItemState);

            Assert.Equal(1, item1.QuantityShipped);
            Assert.Equal(0, item1.QuantityCommittedOut);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(0, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            shipment = (CustomerShipment)item2.OrderShipmentsWhereOrderItem.ElementAt(0).ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Transaction.Derive();

            pickList = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(0).PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            this.Transaction.Derive();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();

            this.Transaction.Derive();

            salesInvoiceitem = (SalesInvoiceItem)shipment.ShipmentItems.ElementAt(0).ShipmentItemBillingsWhereShipmentItem.ElementAt(0).InvoiceItem;
            var invoice2 = salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;

            invoice2.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(30)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice2.SalesInvoiceItems.ElementAt(0)).WithAmountApplied(30).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item3.SalesOrderItemState);

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

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            shipment = (CustomerShipment)item3.OrderShipmentsWhereOrderItem.ElementAt(0).ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Pick();
            this.Transaction.Derive();

            pickList = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(0).PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();

            this.Transaction.Derive();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.Ship();

            this.Transaction.Derive();

            salesInvoiceitem =
                (SalesInvoiceItem)shipment.ShipmentItems.ElementAt(0).ShipmentItemBillingsWhereShipmentItem.ElementAt(0).InvoiceItem;
            var invoice3 = salesInvoiceitem.SalesInvoiceWhereSalesInvoiceItem;

            invoice3.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(75)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice3.SalesInvoiceItems.ElementAt(0)).WithAmountApplied(75).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Finished, order.SalesOrderState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Finished, item3.SalesOrderItemState);
        }

        [Fact]
        public void GivenPendingShipmentAndAssignedPickList_WhenNewOrderIsConfirmed_ThenNewPickListIsCreatedAndSingleOrderShipmentIsUpdated()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(mechelenAddress)
                .WithAssignedVatRegime(assessable)
                .Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            order1.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order1.SetReadyForPosting();
            this.Transaction.Derive();

            order1.Post();
            this.Transaction.Derive();

            order1.Accept();
            this.Transaction.Derive();

            var shipment = (CustomerShipment)item.OrderShipmentsWhereOrderItem.Single().ShipmentItem.ShipmentWhereShipmentItem;
            Assert.Equal(3, shipment.ShipmentItems.ElementAt(0).Quantity);

            shipment.Pick();
            this.Transaction.Derive();

            var pickList1 = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(0).PickListItem.PickListWherePickListItem;
            Assert.Equal(3, pickList1.PickListItems.ElementAt(0).Quantity);

            pickList1.Picker = this.OrderProcessor;

            this.Transaction.Derive();

            var order2 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(mechelenAddress)
                .WithAssignedVatRegime(assessable)
                .Build();

            this.Transaction.Derive();

            item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(5)
                .Build();

            order2.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order2.SetReadyForPosting();
            this.Transaction.Derive();

            order2.Post();
            this.Transaction.Derive();

            order2.Accept();
            this.Transaction.Derive();

            shipment.Pick();
            this.Transaction.Derive();

            Assert.Equal(5, shipment.ShipmentItems.First().Quantity);

            var pickList2 = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(1).PickListItem.PickListWherePickListItem;
            Assert.Equal(2, pickList2.PickListItems.ElementAt(0).Quantity);
        }

        [Fact]
        public void GivenSalesOrderOnHold_WhenInventoryBecomesAvailable_ThenOrderIsNotSelectedForShipment()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Hold();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityCommittedOut);
            Assert.Equal(10, item.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityCommittedOut);
            Assert.Equal(10, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderOnHold_WhenOrderIsContinued_ThenOrderIsSelectedForShipment()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Hold();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(10, item.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).OnHold, order.SalesOrderState);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(10, item.QuantityShortFalled);

            order.Continue();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
            Assert.Equal(10, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderNotPartiallyShipped_WhenInComplete_ThenOrderIsNotSelectedForShipment()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithPartiallyShip(false)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good1)
                .WithQuantityOrdered(20)
                .WithAssignedUnitPrice(5)
                .Build();

            var item2 = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good2)
                .WithQuantityOrdered(20)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.False(customer.ExistShipmentsWhereShipToParty);

            Assert.Equal(10, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(10, item1.QuantityShortFalled);

            Assert.Equal(10, item2.QuantityRequestsShipping);
            Assert.Equal(0, item2.QuantityPendingShipment);
            Assert.Equal(10, item2.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            Assert.False(customer.ExistShipmentsWhereShipToParty);

            Assert.Equal(20, item1.QuantityRequestsShipping);
            Assert.Equal(0, item1.QuantityPendingShipment);
            Assert.Equal(0, item1.QuantityShortFalled);

            Assert.Equal(10, item2.QuantityRequestsShipping);
            Assert.Equal(0, item2.QuantityPendingShipment);
            Assert.Equal(10, item2.QuantityShortFalled);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

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
            var store = this.Transaction.Extent<Store>().FirstOrDefault();
            store.IsImmediatelyPicked = false;

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;
            var contactMechanism = new ContactMechanisms(this.Transaction).Extent().FirstOrDefault();

            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(this.Transaction.Now().AddYears(-2))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithInvoiceDate(this.Transaction.Now().AddYears(-1))
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(10).WithAssignedUnitPrice(100).WithInvoiceItemType(productItem).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).RequestsApproval, order.SalesOrderState);
            Assert.Equal(0, item.QuantityReserved);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);

            order.Approve();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
            Assert.Equal(10, item.QuantityReserved);
            Assert.Equal(10, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderForCustomerExceedingCreditLimit_WhenOrderIsSetReadyForPosting_ThenOrderRequestsApproval()
        {
            var store = this.Transaction.Extent<Store>().FirstOrDefault();
            store.IsImmediatelyPicked = false;

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;
            var contactMechanism = new ContactMechanisms(this.Transaction).Extent().FirstOrDefault();

            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(this.Transaction.Now().AddYears(-2))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var partyFinancial = customer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, customerRelationship.InternalOrganisation));
            partyFinancial.CreditLimit = 100M;

            this.Transaction.Derive();
            this.Transaction.Commit();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithInvoiceDate(this.Transaction.Now().AddYears(-1))
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(10).WithAssignedUnitPrice(11).WithInvoiceItemType(productItem).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).RequestsApproval, order.SalesOrderState);
            Assert.Equal(0, item.QuantityReserved);
            Assert.Equal(0, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);

            order.Approve();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
            Assert.Equal(10, item.QuantityReserved);
            Assert.Equal(10, item.QuantityPendingShipment);
            Assert.Equal(0, item.QuantityRequestsShipping);
            Assert.Equal(0, item.QuantityShortFalled);
        }

        [Fact]
        public void GivenSalesOrderBelowOrderThreshold_WhenOrderIsConfirmed_ThenOrderIsNotShipped()
        {
            new Stores(this.Transaction).Extent().First().OrderThreshold = 1;

            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)
                .WithInternalOrganisation(this.InternalOrganisation)
                .Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(0.1M)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).RequestsApproval, order.SalesOrderState);
        }

        [Fact]
        public void GivenSalesOrderWithManualShipmentSchedule_WhenOrderIsConfirmed_ThenInventoryIsNotReservedAndOrderIsNotShipped()
        {
            var assessable = new VatRegimes(this.Transaction).ZeroRated;

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            var inventoryItem = (NonSerialisedInventoryItem)good.Part.InventoryItemsWherePart.FirstOrDefault();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var manual = new OrderKindBuilder(this.Transaction).WithDescription("manual").WithScheduleManually(true).Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedVatRegime(assessable)
                .WithOrderKind(manual)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(50)
                .WithAssignedUnitPrice(50)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
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
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .WithAssignedBillToContactMechanism(shipToContactMechanism)
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(3, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(97, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            order.Reject();

            this.Transaction.Derive();

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
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .WithAssignedBillToContactMechanism(shipToContactMechanism)
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(3, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(97, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(100, item1.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(0, item3.ReservedFromNonSerialisedInventoryItem.QuantityOnHand);

            order.Cancel();

            this.Transaction.Derive();

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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Provisional, order.SalesOrderState);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).InProcess, order.SalesOrderState);
        }

        [Fact]
        public void GivenSalesOrderWithCancelledItem_WhenDeriving_ThenCancelledItemIsNotInValidOrderItems()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            item4.Cancel();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count());
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
        }

        [Fact]
        public void GivenSalesOrder_WhenGettingOrderNumberWithoutFormat_ThenOrderNumberShouldBeReturned()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var store = new Stores(this.Transaction).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.RemoveSalesOrderNumberPrefix();

            this.Transaction.Derive();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("1", order1.OrderNumber);

            var order2 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("2", order2.OrderNumber);
        }

        [Fact]
        public void GivenSalesOrder_WhenGettingOrderNumberWithFormat_ThenFormattedOrderNumberShouldBeReturned()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            var store = new Stores(this.Transaction).Extent().First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));
            store.SalesOrderNumberPrefix = "the format is ";

            this.Transaction.Derive();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("the format is 1", order1.OrderNumber);

            var order2 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithStore(store)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("the format is 2", order2.OrderNumber);
        }

        [Fact]
        public void GivenIssuerWithoutOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().RemoveSalesOrderNumberPrefix();
            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Transaction.Derive();

            Assert.Equal(int.Parse(order.OrderNumber), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenIssuerWithOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().SalesOrderNumberPrefix = "prefix-";
            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Transaction.Derive();

            Assert.Equal(int.Parse(order.OrderNumber.Split('-')[1]), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().SalesOrderNumberPrefix = "prefix-{year}-";
            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Transaction.Derive();

            var number = int.Parse(order.OrderNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Transaction.Now().Date.Year.ToString(), number)), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenSalesOrder_WhenDeriving_ThenTakenByContactMechanismMustExist()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var orderContact = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("orders@acme.com").Build();

            var orderFrom = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(internalOrganisation)
                .WithUseAsDefault(true)
                .WithContactMechanism(orderContact)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).OrderAddress)
                .Build();

            this.Transaction.Derive();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(internalOrganisation.OrderAddress, order1.DerivedTakenByContactMechanism);
        }

        [Fact]
        public void GivenSalesOrder_WhenDeriving_ThenBillFromContactMechanismMustExist()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var billingContact = new EmailAddressBuilder(this.Transaction)
                .WithElectronicAddressString("orders@acme.com")
                .Build();

            var billingFrom = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(internalOrganisation)
                .WithUseAsDefault(true)
                .WithContactMechanism(billingContact)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .Build();

            this.Transaction.Derive();

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(internalOrganisation.BillingAddress, order1.DerivedTakenByContactMechanism);
        }

        [Fact]
        public void GivenSalesOrderWithBillToCustomerWithPreferredCurrency_WhenBuild_ThenCurrencyIsFromCustomer()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var englischLocale = new Locales(this.Transaction).LocaleByName["en"];

            var euro = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");
            var poundSterling = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "GBP");

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(this.Transaction.Now())
                .WithFromCurrency(euro)
                .WithToCurrency(poundSterling)
                .WithRate(0.8553M)
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").WithLocale(englischLocale).WithPreferredCurrency(poundSterling).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(poundSterling, order.DerivedCurrency);

            customer.PreferredCurrency = euro;

            this.Transaction.Derive();

            Assert.Equal(customer.PreferredCurrency, order.DerivedCurrency);
        }

        [Fact]
        public void GivenSalesOrder_WhenDeriving_ThenLocaleMustExist()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(order.TakenBy.Locale, order.DerivedLocale);
        }

        [Fact]
        public void GivenSalesOrderWithShippingAndHandlingAmount_WhenDeriving_ThenOrderTotalsMustIncludeShippingAndHandlingAmount()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var adjustment = new ShippingAndHandlingChargeBuilder(this.Transaction).WithAmount(7.5M).Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

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
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var adjustment = new ShippingAndHandlingChargeBuilder(this.Transaction).WithPercentage(5).Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

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
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var adjustment = new FeeBuilder(this.Transaction).WithAmount(7.5M).Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

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
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var vatRate21 = new VatRateBuilder(this.Transaction).WithRate(21).Build();
            var adjustment = new FeeBuilder(this.Transaction).WithAmount(7.5M).Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithOrderAdjustment(adjustment)
                .Build();

            this.Transaction.Derive();

            Assert.NotNull(this.InternalOrganisation.ShippingAddress);
            Assert.Equal(order.DerivedShipFromAddress, this.InternalOrganisation.ShippingAddress);
        }

        [Fact]
        public void GivenSalesOrderWithShipFromAddress_WhenDeriving_ThenUseOrderShipFromAddress()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipFrom = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var adjustment = new FeeBuilder(this.Transaction).WithAmount(7.5M).Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithAssignedShipFromAddress(shipFrom)
                .WithBillToCustomer(billToCustomer)
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(order.DerivedShipFromAddress, shipFrom);
        }

        [Fact]
        public void GivenSalesOrderWithFeePercentage_WhenDeriving_ThenOrderTotalsMustIncludeFeeAmount()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var adjustment = new FeeBuilder(this.Transaction).WithPercentage(5).Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good.Part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderAdjustment(adjustment)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            const decimal quantityOrdered = 3;
            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good).WithQuantityOrdered(quantityOrdered).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();

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
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var vatRate21 = new VatRateBuilder(this.Transaction).WithRate(21).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(10).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
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

            Assert.Equal(6, item1.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(4, item1.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
            Assert.Equal(3, item3.ReservedFromNonSerialisedInventoryItem.QuantityCommittedOut);
            Assert.Equal(7, item3.ReservedFromNonSerialisedInventoryItem.AvailableToPromise);
        }

        [Fact]
        public void GivenSalesOrder_WhenChangingItemQuantityToZero_ThenItemIsInvalid()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Transaction.Derive();
            Assert.Equal(4, order.ValidOrderItems.Count());

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            item4.QuantityOrdered = 0;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void GivenSalesOrder_WhenOrderItemIsWithoutBasePrice_ThenExceptionIsThrown()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            item4.RemoveAssignedUnitPrice();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("No BasePrice with a Price"));

            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
        }

        [Fact]
        public void GivenSalesOrder_WhenConfirming_ThenAllValidItemsAreInConfirmedState()
        {
            var billToCustomer = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            var shipToCustomer = new PersonBuilder(this.Transaction).WithLastName("person2").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(billToCustomer)
                .WithShipToCustomer(shipToCustomer)
                .WithAssignedShipToAddress(shipToContactMechanism)
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(4).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);
            order.AddSalesOrderItem(item2);
            order.AddSalesOrderItem(item3);
            order.AddSalesOrderItem(item4);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            item4.Cancel();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count());
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item1.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item2.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).InProcess, item3.SalesOrderItemState);
            Assert.Equal(new SalesOrderItemStates(this.Transaction).Cancelled, item4.SalesOrderItemState);
        }

        [Fact]
        public void GivenSalesOrder_WhenConfirmed_ThenShipmentItemsAreCreated()
        {
            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithOrderKind(new OrderKindBuilder(this.Transaction).WithDescription("auto").WithScheduleManually(false).Build())
                .Build();

            this.Transaction.Derive();

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

            var shipment = customer.ShipmentsWhereShipToParty.FirstOrDefault();

            Assert.Equal(2, shipment.ShipmentItems.Count());
            Assert.Equal(1, item1.OrderShipmentsWhereOrderItem.ElementAt(0).Quantity);
            Assert.Equal(2, item2.OrderShipmentsWhereOrderItem.ElementAt(0).Quantity);
            Assert.Equal(5, item3.OrderShipmentsWhereOrderItem.ElementAt(0).Quantity);
        }

        [Fact]
        public void GivenSalesOrderForSerialisedItem_WhenConfirmed_ThenShipmentItemIsCreated()
        {
            var good1 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                    .Build())
                .Build();

            var serialisedItem1 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").WithAvailableForSale(true).Build();
            good1.Part.AddSerialisedItem(serialisedItem1);

            new SerialisedInventoryItemBuilder(this.Transaction).WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault()).WithPart(good1.Part).WithSerialisedItem(serialisedItem1).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            order.AddSalesOrderItem(item1);

            this.Transaction.Derive();
            order.SetReadyForPosting();

            //var derivation = new Logging.Derivation(this.Transaction, new DerivationConfig
            //    {
            //        DerivationLogFunc = () => new CustomListDerivationLog(),
            //    }
            //);

            //derivation.Derive();

            //var list = ((CustomListDerivationLog)derivation.DerivationLog).List;
            //list.RemoveAll(v => !v.StartsWith("Dependency"));

            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var shipment = customer.ShipmentsWhereShipToParty.FirstOrDefault();

            Assert.Single(shipment.ShipmentItems);
            Assert.Equal(1, item1.OrderShipmentsWhereOrderItem.ElementAt(0).Quantity);
        }

        [Fact]
        public void GivenSalesOrderWithMultipleRecipients_WhenConfirmed_ThenShipmentIsCreatedForEachRecipientAndPickListIsCreated()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var baal = new CityBuilder(this.Transaction).WithName("Baal").Build();
            var baalAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(baal).WithAddress1("Haverwerf 15").Build();

            var person1 = new PersonBuilder(this.Transaction).WithLastName("person1").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(person1)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var person2 = new PersonBuilder(this.Transaction).WithLastName("person2").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(person2)
                .WithContactMechanism(baalAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(person1).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(person2).WithInternalOrganisation(this.InternalOrganisation).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

            this.Transaction.Derive();

            var colorBlack = new ColourBuilder(this.Transaction)
                .WithName("Black")
                .Build();

            var extraLarge = new SizeBuilder(this.Transaction)
                .WithName("Extra large")
                .Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(person1)
                .WithShipToCustomer(person1)
                .WithAssignedShipToAddress(mechelenAddress)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            this.Transaction.Derive();

            var item1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item2 = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(colorBlack).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            var item3 = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(extraLarge).WithQuantityOrdered(1).WithAssignedUnitPrice(15).Build();
            item1.AddOrderedWithFeature(item2);
            item1.AddOrderedWithFeature(item3);
            var item4 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good1).WithQuantityOrdered(2).WithAssignedUnitPrice(15).Build();
            var item5 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good2).WithQuantityOrdered(5).WithAssignedUnitPrice(15).WithAssignedShipToParty(person2).Build();
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

            var shipmentToMechelen = mechelenAddress.ShipmentsWhereShipToAddress.ElementAt(0);

            var shipmentToBaal = baalAddress.ShipmentsWhereShipToAddress.ElementAt(0);

            this.Transaction.Derive();

            Assert.Equal(mechelenAddress, shipmentToMechelen.ShipToAddress);
            Assert.Single(shipmentToMechelen.ShipmentItems);
            Assert.Equal(3, shipmentToMechelen.ShipmentItems.ElementAt(0).Quantity);

            Assert.Equal(baalAddress, shipmentToBaal.ShipToAddress);
            Assert.Single(shipmentToBaal.ShipmentItems);
            Assert.Equal(good2, shipmentToBaal.ShipmentItems.ElementAt(0).Good);
            Assert.Equal(5, shipmentToBaal.ShipmentItems.ElementAt(0).Quantity);
        }

        [Fact]
        public void GivenSalesOrder_WhenShipToAndBillToAreSameCustomer_ThenDerivedCustomersIsSingleCustomer()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(customer)
                .WithBillToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Single(order.Customers);
            Assert.Equal(customer, order.Customers.FirstOrDefault());
        }

        [Fact]
        public void GivenSalesOrder_WhenShipToAndBillToAreDifferentCustomers_ThenDerivedCustomersHoldsBothCustomers()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(shipToCustomer)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, order.Customers.Count());
            Assert.Contains(billToCustomer, order.Customers);
            Assert.Contains(shipToCustomer, order.Customers);
        }

        [Fact]
        public void GivenSettingSerialisedItemSoldOnSalesOrderAccept_WhenAcceptingSalesOrder_ThenSerialisedItemStateIsChanged()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).SalesOrderAccept);

            this.Transaction.Derive();

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                    .Build())
                .Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").WithAvailableForSale(true).Build();
            good.Part.AddSerialisedItem(serialisedItem);

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(billToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(shipToCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var address = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(shipToCustomer)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedBillToContactMechanism(address)
                .WithAssignedShipToAddress(address)
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithSerialisedItem(serialisedItem)
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(1)
                .Build();

            salesOrder.AddSalesOrderItem(orderItem);

            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            Assert.NotEqual(new SerialisedItemAvailabilities(this.Transaction).Sold, serialisedItem.SerialisedItemAvailability);

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SerialisedItemAvailabilities(this.Transaction).Sold, serialisedItem.SerialisedItemAvailability);
        }
    }

    public class SalesOrderOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderState()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(order.ExistSalesOrderState);
        }

        [Fact]
        public void DeriveSalesOrderShipmentState()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(order.ExistSalesOrderShipmentState);
        }

        [Fact]
        public void DeriveSalesOrderPaymentState()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(order.ExistSalesOrderPaymentState);
        }

        [Fact]
        public void DeriveOrderDate()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(order.ExistOrderDate);
        }

        [Fact]
        public void DeriveEntryDate()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(order.ExistEntryDate);
        }

        [Fact]
        public void DeriveTakenBy()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.Equal(order.TakenBy, this.InternalOrganisation);
        }

        [Fact]
        public void DeriveStore()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.Equal(order.Store, this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault());
        }

        [Fact]
        public void DeriveOriginFacility()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            Assert.Equal(order.OriginFacility, this.InternalOrganisation.StoresWhereInternalOrganisation.First().DefaultFacility);
        }
    }

    public class SalesOrderProvisionalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderProvisionalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderStateDeriveDerivedLocaleFromBillToCustomerLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.Locale = swedishLocale;

            var order = new SalesOrderBuilder(this.Transaction).WithSalesOrderState(new SalesOrderStates(this.Transaction).Cancelled).Build();
            this.Derive();

            order.BillToCustomer = customer;
            this.Derive();

            Assert.False(order.ExistDerivedLocale);

            order.SalesOrderState = new SalesOrderStates(this.Transaction).Provisional;
            this.Derive();

            Assert.Equal(order.DerivedLocale, customer.Locale);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedLocaleFromBillToCustomerLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.Locale = swedishLocale;

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.BillToCustomer = customer;
            this.Derive();

            Assert.Equal(order.DerivedLocale, customer.Locale);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedLocaleFromTakenByLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            this.InternalOrganisation.Locale = swedishLocale;

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveLocale();

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.BillToCustomer = customer;
            this.Derive();

            Assert.False(customer.ExistLocale);
            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedLocaleDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.Locale = swedishLocale;
            this.Derive();

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Derive();

            Assert.Equal(order.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Derive();

            Assert.Equal(order.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            order.AssignedCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(order.DerivedCurrency, order.AssignedCurrency);
        }

        [Fact]
        public void ChangedTakenByPreferredCurrencyDeriveDerivedCurrency()
        {
            Assert.True(this.InternalOrganisation.ExistPreferredCurrency);

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedCurrency, this.InternalOrganisation.PreferredCurrency);
        }

        [Fact]
        public void ChangedBillToCustomerLocaleDeriveDerivedCurrency()
        {
            var se = new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE");
            var newLocale = new LocaleBuilder(this.Transaction)
                .WithCountry(se)
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Derive();

            customer.RemovePreferredCurrency();
            customer.Locale = newLocale;
            this.Derive();

            Assert.Equal(order.DerivedCurrency, se.Currency);
        }

        [Fact]
        public void ChangedBillToCustomerPreferredCurrencyDeriveDerivedCurrency()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveLocale();
            customer.RemovePreferredCurrency();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            customer.PreferredCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(order.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveCurrencyFromBillToCustomerLocale()
        {
            var newLocale = new LocaleBuilder(this.Transaction)
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.Locale = newLocale;
            customer.RemovePreferredCurrency();

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.BillToCustomer = customer;
            this.Derive();

            Assert.Equal(order.DerivedCurrency, newLocale.Country.Currency);
        }

        [Fact]
        public void ChangedAssignedTakenByContactMechanismDeriveDerivedTakenByContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedTakenByContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedTakenByContactMechanism, order.AssignedTakenByContactMechanism);
        }

        [Fact]
        public void ChangedTakenByOrderAddressDeriveDerivedTakenByContactMechanism()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(internalOrganisation).Build();
            this.Derive();

            internalOrganisation.OrderAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedTakenByContactMechanism, internalOrganisation.OrderAddress);
        }

        [Fact]
        public void ChangedTakenByBillingAddressDeriveDerivedTakenByContactMechanism()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(internalOrganisation).Build();
            this.Derive();

            internalOrganisation.BillingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedTakenByContactMechanism, internalOrganisation.BillingAddress);
        }

        [Fact]
        public void ChangedTakenByGeneralCorrespondenceDeriveDerivedTakenByContactMechanism()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(internalOrganisation).Build();
            this.Derive();

            internalOrganisation.GeneralCorrespondence = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedTakenByContactMechanism, internalOrganisation.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedBillToContactMechanismDeriveDerivedBillToContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedBillToContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, order.AssignedBillToContactMechanism);
        }

        [Fact]
        public void OnChangedRoleBillToCustomerDeriveDerivedBillToContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            order.BillToCustomer = order.TakenBy.ActiveCustomers.FirstOrDefault();
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, order.BillToCustomer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerBillingAddressDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Derive();

            customer.BillingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, customer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerShippingAddressDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Derive();

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedBillToCustomerGeneralCorrespondenceDeriveDerivedBillToContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer).Build();
            this.Derive();

            customer.GeneralCorrespondence = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, customer.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedBillToEndCustomerContactMechanismDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedBillToEndCustomerContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, order.AssignedBillToEndCustomerContactMechanism);
        }

        [Fact]
        public void OnChangedRoleBillToEndCustomerDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            order.BillToEndCustomer = order.TakenBy.ActiveCustomers.FirstOrDefault();
            this.Derive();

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, order.BillToEndCustomer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerBillingAddressDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToEndCustomer(customer).Build();
            this.Derive();

            customer.BillingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, customer.BillingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerShippingAddressDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToEndCustomer(customer).Build();
            this.Derive();

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedBillToEndCustomerGeneralCorrespondenceDeriveDerivedBillToEndCustomerContactMechanism()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToEndCustomer(customer).Build();
            this.Derive();

            customer.GeneralCorrespondence = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToEndCustomerContactMechanism, customer.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedShipToEndCustomerAddressDeriveDerivedShipToEndCustomerAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedShipToEndCustomerAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipToEndCustomerAddress, order.AssignedShipToEndCustomerAddress);
        }

        [Fact]
        public void OnChangedRoleShipToEndCustomerDeriveDerivedShipToEndCustomerAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            order.ShipToEndCustomer = order.TakenBy.ActiveCustomers.FirstOrDefault();
            this.Derive();

            Assert.Equal(order.DerivedShipToEndCustomerAddress, order.ShipToEndCustomer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToEndCustomerCustomerShippingAddressDeriveDerivedShipToEndCustomerAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithShipToEndCustomer(customer).Build();
            this.Derive();

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipToEndCustomerAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToEndCustomerCustomerGeneralCorrespondenceDeriveDerivedShipToEndCustomerAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithShipToEndCustomer(customer).Build();
            this.Derive();

            customer.GeneralCorrespondence = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipToEndCustomerAddress, customer.GeneralCorrespondence);
        }

        [Fact]
        public void ChangedAssignedShipFromAddressDeriveDerivedShipFromAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedShipFromAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipFromAddress, order.AssignedShipFromAddress);
        }

        [Fact]
        public void ChangedTakenByShippingAddressDeriveDerivedShipFromAddress()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(internalOrganisation).Build();
            this.Derive();

            internalOrganisation.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipFromAddress, internalOrganisation.ShippingAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedShipToAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipToAddress, order.AssignedShipToAddress);
        }

        [Fact]
        public void OnChangedRoleShipToCustomerDeriveDerivedShipToAddress()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();

            this.Derive();

            order.ShipToCustomer = order.TakenBy.ActiveCustomers.FirstOrDefault();
            this.Derive();

            Assert.Equal(order.DerivedShipToAddress, order.ShipToCustomer.ShippingAddress);
        }

        [Fact]
        public void ChangedShipToCustomerShippingAddressDeriveDerivedShipToAddress()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveBillingAddress();
            customer.RemoveShippingAddress();
            customer.RemoveGeneralCorrespondence();

            var order = new SalesOrderBuilder(this.Transaction).WithShipToCustomer(customer).Build();
            this.Derive();

            customer.ShippingAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedShipToAddress, customer.ShippingAddress);
        }

        [Fact]
        public void ChangedAssignedShipmentMethodDeriveDerivedShipmentMethod()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentMethod = new ShipmentMethods(this.Transaction).Boat;
            order.AssignedShipmentMethod = shipmentMethod;
            this.Derive();

            Assert.Equal(order.DerivedShipmentMethod, shipmentMethod);
        }

        [Fact]
        public void ChangedShipToCustomerDefaultShipmentMethodDeriveDerivedShipmentMethod()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveDefaultShipmentMethod();

            var order = new SalesOrderBuilder(this.Transaction).WithShipToCustomer(customer).Build();
            this.Derive();

            var shipmentMethod = new ShipmentMethods(this.Transaction).Boat;
            customer.DefaultShipmentMethod = shipmentMethod;
            this.Derive();

            Assert.Equal(order.DerivedShipmentMethod, shipmentMethod);
        }

        [Fact]
        public void ChangedStoreDefaultShipmentMethodDeriveDerivedShipmentMethod()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault();
            store.RemoveDefaultShipmentMethod();

            var order = new SalesOrderBuilder(this.Transaction).WithStore(store).Build();
            this.Derive();

            var shipmentMethod = new ShipmentMethods(this.Transaction).Boat;
            store.DefaultShipmentMethod = shipmentMethod;
            this.Derive();

            Assert.Equal(order.DerivedShipmentMethod, shipmentMethod);
        }

        [Fact]
        public void ChangedAssignedPaymentMethodDeriveDerivedPaymentMethod()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var cash = new CashBuilder(this.Transaction).Build();
            order.AssignedPaymentMethod = cash;
            this.Derive();

            Assert.Equal(order.DerivedPaymentMethod, cash);
        }

        [Fact]
        public void ChangedTakenByDefaultPaymentMethodDeriveDerivedPaymentMethod()
        {
            this.InternalOrganisation.RemoveDefaultPaymentMethod();

            var order = new SalesOrderBuilder(this.Transaction).WithTakenBy(this.InternalOrganisation).Build();
            this.Derive();

            var cash = new CashBuilder(this.Transaction).Build();
            this.InternalOrganisation.DefaultPaymentMethod = cash;
            this.Derive();

            Assert.Equal(order.DerivedPaymentMethod, cash);
        }

        [Fact]
        public void ChangedStoreDefaultPaymentMethodDeriveDerivedPaymentMethod()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault();
            store.RemoveDefaultCollectionMethod();

            var order = new SalesOrderBuilder(this.Transaction).WithStore(store).Build();
            this.Derive();

            var cash = new CashBuilder(this.Transaction).Build();
            store.DefaultCollectionMethod = cash;
            this.Derive();

            Assert.Equal(order.DerivedPaymentMethod, cash);
        }

        [Fact]
        public void ChangedOrderDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates.ElementAt(0).ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Derive();

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Derive();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithOrderDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Derive();

            Assert.NotEqual(newVatRate, order.DerivedVatRate);

            order.OrderDate = this.Transaction.Now().AddDays(1).Date;
            this.Derive();

            Assert.Equal(newVatRate, order.DerivedVatRate);
        }
    }

    public class SalesOrderRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTakenByFromValidationError()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.TakenBy = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            Assert.Contains(ErrorMessages.InternalOrganisationChanged, this.Derive().Errors.Select(v => v.Message));
        }

        [Fact]
        public void ChangedShipToCustomerDeriveBillToCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.ShipToCustomer = customer;
            this.Derive();

            Assert.Equal(order.BillToCustomer, order.ShipToCustomer);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveShipToCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.BillToCustomer = customer;
            this.Derive();

            Assert.Equal(order.ShipToCustomer, order.BillToCustomer);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveCustomers()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.BillToCustomer = customer;
            this.Derive();

            Assert.Contains(order.BillToCustomer, order.Customers);
        }

        [Fact]
        public void ChangedShipToCustomerDeriveCustomers()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.ShipToCustomer = customer;
            this.Derive();

            Assert.Contains(order.ShipToCustomer, order.Customers);
        }

        [Fact]
        public void ChangedPlacingCustomerDeriveCustomers()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.PlacingCustomer = customer;
            this.Derive();

            Assert.Contains(order.PlacingCustomer, order.Customers);
        }

        [Fact]
        public void ChangedStoreDeriveOrderNumber()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            var store = new Stores(this.Transaction).Extent().FirstOrDefault();
            store.RemoveSalesOrderNumberPrefix();
            var number = store.SalesOrderNumberCounter.Value;

            this.Derive();

            Assert.Equal(order.OrderNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableOrderNumber()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            var number = new Stores(this.Transaction).Extent().First().SalesOrderNumberCounter.Value;

            this.Derive();

            Assert.Equal(order.SortableOrderNumber.Value, number + 1);
        }

        [Fact]
        public void ValidateBillToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.CustomerRelationshipsWhereCustomer.First().ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Derive();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotACustomer));
        }

        [Fact]
        public void ValidateShipToCustomerIsActiveCustomer()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.CustomerRelationshipsWhereCustomer.First().ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Derive();

            var order = new SalesOrderBuilder(this.Transaction).WithShipToCustomer(customer).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotACustomer));
        }

        [Fact]
        public void ValidateExistDerivedShipToAddress()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            order.RemoveDerivedShipToAddress();

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Contains(this.M.SalesOrder.DerivedShipToAddress, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ValidateExistDerivedBillToContactMechanism()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            order.RemoveDerivedBillToContactMechanism();

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Contains(this.M.SalesOrder.DerivedBillToContactMechanism, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedCanShipCreateShipment()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(item.ExistOrderShipmentsWhereOrderItem);
        }

        [Fact]
        public void ChangedStoreAutoGenerateCustomerShipmentCreateShipment()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.False(item.ExistOrderShipmentsWhereOrderItem);

            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = true;
            this.Derive();

            Assert.True(item.ExistOrderShipmentsWhereOrderItem);
        }

        [Fact]
        public void OnChangedValidOrderItemsDeriveSalesOrderItemDelegatedAccess()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(order, orderItem.DelegatedAccess);
        }
    }

    public class SalesOrderCanInvoiceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderCanInvoiceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void InvoicableOrderDeriveCanInvoiceFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForShipmentItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.False(order.CanInvoice);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanInvoiceFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            Assert.False(order.CanInvoice);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanInvoiceTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanInvoice);
        }

        [Fact]
        public void ChangedSalesOrderItemStateDeriveCanInvoice()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanInvoice);

            foreach (SalesOrderItem salesOrderItem in order.SalesOrderItems)
            {
                salesOrderItem.Cancel();
            }

            this.Derive();

            Assert.False(order.CanInvoice);
        }

        [Fact]
        public void ChangedOrderItemBillingAmountDeriveCanInvoiceTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanInvoice);

            var orderItem = order.SalesOrderItems.ElementAt(0);
            var fullAmount = orderItem.QuantityOrdered * orderItem.UnitPrice;

            new OrderItemBillingBuilder(this.Transaction)
                .WithOrderItem(order.SalesOrderItems.ElementAt(0))
                .WithAmount(fullAmount)
                .Build();
            this.Derive();

            Assert.True(order.CanInvoice);
        }

        [Fact]
        public void ChangedOrderItemBillingAmountDeriveCanInvoiceFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanInvoice);

            foreach (SalesOrderItem salesOrderItem in order.SalesOrderItems)
            {
                var fullAmount = salesOrderItem.QuantityOrdered * salesOrderItem.UnitPrice;

                new OrderItemBillingBuilder(this.Transaction)
                    .WithOrderItem(salesOrderItem)
                    .WithAmount(fullAmount)
                    .Build();
            }

            this.Derive();

            Assert.False(order.CanInvoice);
        }
    }

    public class SalesOrderCanShipRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderCanShipRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanShipFalse()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());

            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanShip);
        }

        [Fact]
        public void ChangedPartiallyShipDeriveCanShipFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanShip);

            order.PartiallyShip = false;
            this.Derive();

            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedPartiallyShipDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.False(order.CanShip);

            order.PartiallyShip = true;
            this.Derive();

            Assert.True(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityOrderedDeriveCanShipFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanShip);

            item.QuantityOrdered += 1;
            this.Derive();

            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityOrderedDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.False(order.CanShip);

            item.QuantityOrdered -= 1;
            this.Derive();

            Assert.True(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityRequestsShippingDeriveCanShipFalse()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(order.CanShip);
            var before = item.QuantityRequestsShipping;

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Consumption)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            Assert.Equal(before - 1, item.QuantityRequestsShipping);
            Assert.False(order.CanShip);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityRequestsShippingDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.False(order.CanShip);
            var before = item.QuantityRequestsShipping;

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            Assert.Equal(before + 1, item.QuantityRequestsShipping);
            Assert.True(order.CanShip);
        }

        [Fact]
        public void DeleteShipmentItemDeriveDeriveCanShipTrue()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            order.Ship();
            this.Derive();

            var shipment = item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;

            shipment.Delete();
            this.Derive();

            Assert.True(order.CanShip);
        }
    }

    public class SalesOrderPriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderPriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedValidOrderItemsCalculatePrice()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Transaction.Derive();

            Assert.True(order.TotalIncVat > 0);
            var totalIncVatBefore = order.TotalIncVat;

            order.SalesOrderItems.First().Cancel();
            this.Transaction.Derive();

            Assert.Equal(order.TotalIncVat, totalIncVatBefore - order.SalesOrderItems.First().TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderItemsCalculatePriceForProductFeature()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProductFeature(productFeature)
                .WithPrice(0.2M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Derive();

            var orderFeatureItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantityOrdered(1).Build();
            orderItem.AddOrderedWithFeature(orderFeatureItem);
            this.Derive();

            Assert.Equal(1.2M, order.TotalExVat);
        }

        [Fact]
        public void ChangedDerivationTriggerTriggeredByPriceComponentFromDateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var basePrice = new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("No BasePrice with a Price"));

            Assert.Equal(0, order.TotalExVat);

            basePrice.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Derive();

            Assert.Equal(basePrice.Price, order.TotalExVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByDiscountComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            new DiscountComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Derive();

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredBySurchargeComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            new SurchargeComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Derive();

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemQuantityOrderedCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            orderItem.QuantityOrdered = 2;
            this.Derive();

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            orderItem.AssignedUnitPrice = 3;
            this.Derive();

            Assert.Equal(3, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemProductCalculatePrice()
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
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product1).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            orderItem.Product = product2;
            this.Derive();

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemProductFeatureCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

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

            this.Derive();

            var orderFeatureItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantityOrdered(1).Build();
            orderItem.AddOrderedWithFeature(orderFeatureItem);
            this.Derive();

            Assert.Equal(1.1M, order.TotalExVat);
        }

        [Fact]
        public void OnChangedBillToCustomerCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Derive();

            Assert.NotEqual(customer1, customer2);

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theBad)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(customer1).WithOrderDate(this.Transaction.Now()).WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            order.BillToCustomer = customer2;
            this.Derive();

            Assert.Equal(2, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedRoleSalesOrderItemDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, order.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

            Assert.Equal(0.8M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            orderItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, order.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

            Assert.Equal(0.6M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedRoleSalesOrderItemSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, order.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

            Assert.Equal(1.2M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSalesOrderItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            orderItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, order.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            order.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            order.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, order.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

            Assert.Equal(0.8M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            order.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, order.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

            Assert.Equal(0.6M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            order.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            order.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, order.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

            Assert.Equal(1.2M, order.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantityOrdered(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            order.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, order.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, order.TotalIncVat);
        }
    }

    public class SalesOrderStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveValidOrderItems()
        {
            var order = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var orderItem1 = new SalesOrderItemBuilder(this.Transaction).WithDefaults().Build();
            order.AddSalesOrderItem(orderItem1);
            this.Transaction.Derive();

            var orderItem2 = new SalesOrderItemBuilder(this.Transaction).WithDefaults().Build();
            order.AddSalesOrderItem(orderItem2);
            this.Transaction.Derive();

            Assert.Equal(2, order.ValidOrderItems.Count());

            orderItem2.Cancel();
            this.Transaction.Derive();

            Assert.Single(order.ValidOrderItems);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsNotShipped()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderShipmentState.IsNotShipped);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsInProgress()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Ship();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderShipmentState.IsInProgress);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsPartiallyShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered - 1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

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

            Assert.True(order.SalesOrderShipmentState.IsPartiallyShipped);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemShipmentStateDeriveSalesOrderShipmentStateIsShipped()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateCustomerShipment = false;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPicked = true;
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().IsImmediatelyPacked = true;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(item.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(item.Part)
                .Build();
            this.Derive();

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

            Assert.True(order.SalesOrderShipmentState.IsShipped);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemPaymentStateDeriveSalesOrderPaymentStateIsNotPaid()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderPaymentState.IsNotPaid);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemPaymentStateDeriveSalesOrderPaymentStateIsPartiallyPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.FirstOrDefault();
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

            Assert.True(order.SalesOrderPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemPaymentStateDeriveSalesOrderPaymentStateIsPaid()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.FirstOrDefault();
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

            Assert.True(order.SalesOrderPaymentState.IsPaid);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemInvoiceStateDeriveSalesOrderInvoiceStateIsNotInvoiced()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderInvoiceState.IsNotInvoiced);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemInvoiceStateDeriveSalesOrderInvoiceStateIsInvoiced()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().BillingProcess = new BillingProcesses(this.Transaction).BillingForOrderItems;

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = false;
            this.Derive();

            var item = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Invoice();
            this.Transaction.Derive();

            var invoice = order.SalesInvoicesWhereSalesOrder.FirstOrDefault();

            invoice.Send();
            this.Transaction.Derive();

            Assert.True(order.SalesOrderInvoiceState.IsInvoiced);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemAvailability()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Transaction.Faker());
            this.Derive();

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).InRent;
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.Equal(salesOrderItem.NextSerialisedItemAvailability, salesOrderItem.SerialisedItem.SerialisedItemAvailability);
        }

        [Fact]
        public void ChangedSalesOrderStateNotDeriveSerialisedItemSerialisedItemAvailability()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).CustomerShipmentShip);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Transaction.Faker());
            this.Derive();

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.SerialisedItem.SerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).Available;
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).InRent;
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.NotEqual(salesOrderItem.NextSerialisedItemAvailability, salesOrderItem.SerialisedItem.SerialisedItemAvailability);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemOwnedBy()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Transaction.Faker());
            this.Derive();

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).Sold;
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.Equal(order.ShipToCustomer, salesOrderItem.SerialisedItem.OwnedBy);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemOwnership()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Transaction.Faker());
            this.Derive();

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).Sold;
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.True(salesOrderItem.SerialisedItem.Ownership.IsThirdParty);
        }

        [Fact]
        public void ChangedSalesOrderStateDeriveSerialisedItemSerialisedItemAvailableForSale()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).SalesOrderAccept);

            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleSerialisedItem(this.Transaction.Faker());
            this.Derive();

            var salesOrderItem = order.SalesOrderItems.First();
            salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).InRent;
            this.Derive();

            order.SetReadyForPosting();
            this.Derive();

            order.Post();
            this.Derive();

            order.Accept();
            this.Derive();

            Assert.False(salesOrderItem.SerialisedItem.AvailableForSale);
        }
    }

    [Trait("Category", "Security")]
    public class SalesOrderTransferRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderTransferRuleTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void SalesOrderTransfer()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Derive();

            order.AddOrderAdjustment(new ShippingAndHandlingChargeBuilder(this.Transaction).WithAmount(1).Build());
            this.Derive();

            var firstItem = order.SalesOrderItems.FirstOrDefault();
            firstItem.AddDiscountAdjustment(new DiscountAdjustmentBuilder(this.Transaction).WithAmount(1).Build());
            firstItem.AddSurchargeAdjustment(new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(1).Build());
            firstItem.AddSalesTerm(new OrderTermBuilder(this.Transaction).WithDefaults().Build());
            this.Derive();

            var salesOrderItemsBefore = order.SalesOrderItems.Count();
            var salesTermsBefore = order.SalesTerms.Count();
            var localisedCommentsBefore = order.LocalisedComments.Count();
            var orderAdjustmentsBefore = order.OrderAdjustments.Count();
            var electronicDocumentsBefore = order.ElectronicDocuments.Count();
            var itemDiscountsBefore = firstItem.DiscountAdjustments.Count();
            var itemSurchargesBefore = firstItem.SurchargeAdjustments.Count();
            var itemSalesTermsBefore = firstItem.SalesTerms.Count();

            var toInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(order.BillToCustomer)
                .WithInternalOrganisation(toInternalOrganisation)
                .WithFromDate(order.OrderDate)
                .Build();
            this.Derive();

            var transfer = new SalesOrderTransferBuilder(this.Transaction).WithFrom(order).WithToInternalOrganisation(toInternalOrganisation).Build();
            this.Derive();

            var newOrder = transfer.ToSalesOrder;

            Assert.True(transfer.ExistToSalesOrder);
            Assert.True(order.SalesOrderState.IsTransferred);
            Assert.Equal(salesOrderItemsBefore, order.SalesOrderItems.Count());
            Assert.Equal(salesTermsBefore, order.SalesTerms.Count());
            Assert.Equal(localisedCommentsBefore, order.LocalisedComments.Count());
            Assert.Equal(orderAdjustmentsBefore, order.OrderAdjustments.Count());
            Assert.Equal(electronicDocumentsBefore, order.ElectronicDocuments.Count());
            Assert.Equal(itemDiscountsBefore, firstItem.DiscountAdjustments.Count());
            Assert.Equal(itemSurchargesBefore, firstItem.SurchargeAdjustments.Count());
            Assert.Equal(itemSalesTermsBefore, firstItem.SalesTerms.Count());

            Assert.NotEqual(order.OrderNumber, newOrder.OrderNumber);
            Assert.NotEmpty(newOrder.OrderNumber);
            Assert.Equal(salesOrderItemsBefore, newOrder.SalesOrderItems.Count());
            Assert.Equal(salesTermsBefore, newOrder.SalesTerms.Count());
            Assert.Equal(localisedCommentsBefore, newOrder.LocalisedComments.Count());
            Assert.Equal(orderAdjustmentsBefore, newOrder.OrderAdjustments.Count());
            Assert.Equal(electronicDocumentsBefore, newOrder.ElectronicDocuments.Count());
            Assert.Equal(itemDiscountsBefore, newOrder.SalesOrderItems.First().DiscountAdjustments.Count());
            Assert.Equal(itemSurchargesBefore, newOrder.SalesOrderItems.First().SurchargeAdjustments.Count());
            Assert.Equal(itemSalesTermsBefore, newOrder.SalesOrderItems.First().SalesTerms.Count());
        }
    }

    [Trait("Category", "Security")]
    public class SalesOrderDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
        }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveTransFerAllowed()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var transferPermission = new Permissions(this.Transaction).Get(this.M.SalesOrder, this.M.SalesOrder.DoTransfer);
            Assert.DoesNotContain(transferPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveTransFerDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.SalesOrderState = new SalesOrderStates(this.Transaction).ReadyForPosting;
            this.Derive();

            var transferPermission = new Permissions(this.Transaction).Get(this.M.SalesOrder, this.M.SalesOrder.DoTransfer);
            Assert.Contains(transferPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveShipDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var shipRevocation = new Revocations(this.Transaction).SalesOrderShipRevocation;
            Assert.Contains(shipRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedCanShipDeriveShipAllowed()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.CanShip = true;
            this.Derive();

            var shipRevocation = new Revocations(this.Transaction).SalesOrderShipRevocation;
            Assert.DoesNotContain(shipRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionsDeriveInvoiceDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceRevocation = new Revocations(this.Transaction).SalesOrderInvoiceRevocation;
            Assert.Contains(invoiceRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedCanInvoiceDeriveInvoiceAllowed()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.CanShip = true;
            this.Derive();

            var invoiceRevocation = new Revocations(this.Transaction).SalesOrderInvoiceRevocation;
            Assert.Contains(invoiceRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionslDeriveDeletePermissionAllowed()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedTransitionalDeniedPermissionslDeriveDeletePermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.SalesOrderState = new SalesOrderStates(this.Transaction).OnHold;
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedQuoteDeriveDeletePermission()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.Quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveDeletePermissionAllowed()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.DoesNotContain(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveDeletePermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);

            orderItem.SalesOrderItemState = new SalesOrderItemStates(this.Transaction).InProcess;
            this.Derive();

            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveDeletePermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            var orderItemBilling = new OrderItemBillingBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);

            orderItemBilling.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedOrderShipmentOrderItemDeriveDeletePermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            var orderShipment = new OrderShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);

            orderShipment.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedOrderRequirementCommitmentOrderItemDeriveDeletePermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            var orderRequirementCommitment = new OrderRequirementCommitmentBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);

            orderRequirementCommitment.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedWorkEffortOrderItemFulfillmentDeriveDeletePermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var deleteRevocation = new Revocations(this.Transaction).SalesOrderDeleteRevocation;
            Assert.DoesNotContain(deleteRevocation, order.Revocations);

            workTask.OrderItemFulfillment = orderItem;
            this.Derive();

            Assert.Contains(deleteRevocation, order.Revocations);
        }

        [Fact]
        public void ChangedSalesOrderShipmentStateDeriveCancelPermission()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var cancelPermission = new Permissions(this.Transaction).Get(this.M.SalesOrder, this.M.SalesOrder.Cancel);
            Assert.DoesNotContain(cancelPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));

            order.SalesOrderShipmentState = new SalesOrderShipmentStates(this.Transaction).Shipped;
            this.Derive();

            Assert.Contains(cancelPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void ChangedSalesOrderInvoiceStateDeriveCancelPermissionDenied()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var cancelPermission = new Permissions(this.Transaction).Get(this.M.SalesOrder, this.M.SalesOrder.Cancel);
            Assert.DoesNotContain(cancelPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));

            order.SalesOrderInvoiceState = new SalesOrderInvoiceStates(this.Transaction).Invoiced;
            this.Derive();

            Assert.Contains(cancelPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void ChangedSalesOrderInvoiceStateDeriveCancelPermissionAllowed()
        {
            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.SalesOrderInvoiceState = new SalesOrderInvoiceStates(this.Transaction).Invoiced;
            this.Derive();

            var cancelPermission = new Permissions(this.Transaction).Get(this.M.SalesOrder, this.M.SalesOrder.Cancel);
            Assert.Contains(cancelPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));

            order.SalesOrderInvoiceState = new SalesOrderInvoiceStates(this.Transaction).NotInvoiced;
            this.Derive();

            Assert.DoesNotContain(cancelPermission, order.Revocations.SelectMany(v => v.DeniedPermissions));
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(new SalesOrderStates(this.Transaction).Provisional, order.SalesOrderState);
            var acl= this.CreateDatabaseAccessControl()[order];
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            order.SetReadyForPosting();

            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            var acl= this.CreateDatabaseAccessControl()[order];
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(customer);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            order.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Cancelled, order.SalesOrderState);
            var acl= this.CreateDatabaseAccessControl()[order];
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(customer);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            order.Reject();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Rejected, order.SalesOrderState);
            var acl= this.CreateDatabaseAccessControl()[order];
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(customer);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.SalesOrderState = new SalesOrderStates(this.Transaction).Finished;

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Finished, order.SalesOrderState);
            var acl= this.CreateDatabaseAccessControl()[order];
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();
            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            order.Hold();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(new SalesOrderStates(this.Transaction).OnHold, order.SalesOrderState);
            var acl= this.CreateDatabaseAccessControl()[order];
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
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            var good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good.Part).Build();

            this.Transaction.Derive();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            var item = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddSalesOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForPosting();
            this.Transaction.Derive();

            order.Post();
            this.Transaction.Derive();

            order.Accept();
            this.Transaction.Derive();

            Assert.Equal(new SalesOrderShipmentStates(this.Transaction).InProgress, order.SalesOrderShipmentState);
            var acl= this.CreateDatabaseAccessControl()[order];
            Assert.False(acl.CanExecute(this.M.SalesOrder.Cancel));
        }
    }
}
