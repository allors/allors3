// <copyright file="PackagingContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class PackagingContentTests : DomainTest, IClassFixture<Fixture>
    {
        public PackagingContentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPackingingContent_WhenDeriving_ThenAssertQuantityPackedIsNotGreaterThanQuantityShipped()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good2.Part).Build();

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

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);
            package.AddPackagingContent(new PackagingContentBuilder(this.Transaction)
                                            .WithShipmentItem(shipment.ShipmentItems[0])
                                            .WithQuantity(shipment.ShipmentItems[0].Quantity + 1)
                                            .Build());

            Assert.True(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenPackingingContent_WhenDerived_ThenShipmentItemsQuantityPackedIsSet()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

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

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                Assert.Equal(shipmentItem.QuantityShipped, shipmentItem.Quantity);
            }
        }
    }

    public class PackagingContentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PackagingContentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPackagingContentShipmentItemThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var packagingContent = new PackagingContentBuilder(this.Transaction).WithQuantity(11).Build();
            this.Derive();

            packagingContent.ShipmentItem = shipmentItem;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PackagingContentMaximum));
        }

        [Fact]
        public void ChangedShipmentItemQuantityThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var packagingContent = new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(10).Build();
            this.Derive();

            shipmentItem.Quantity = 9;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PackagingContentMaximum));
        }

        [Fact]
        public void ChangedShipmentItemQuantityShippedThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var packagingContent = new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(10).Build();
            this.Derive();

            shipmentItem.QuantityShipped = 1;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PackagingContentMaximum));
        }
    }
}
