// <copyright file="PackagingContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class PackagingContentTests : DomainTest, IClassFixture<Fixture>
    {
        public PackagingContentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPackingingContent_WhenDeriving_ThenAssertQuantityPackedIsNotGreaterThanQuantityShipped()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good2.Part).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

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

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);
            package.AddPackagingContent(new PackagingContentBuilder(this.Session)
                                            .WithShipmentItem(shipment.ShipmentItems[0])
                                            .WithQuantity(shipment.ShipmentItems[0].Quantity + 1)
                                            .Build());

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPackingingContent_WhenDerived_ThenShipmentItemsQuantityPackedIsSet()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var shipToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Session).WithLastName("customer").WithPartyContactMechanism(shipToMechelen).Build();
            new CustomerRelationshipBuilder(this.Session).WithFromDate(this.Session.Now()).WithCustomer(customer).Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            var good2 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good2");

            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).Unknown).WithPart(good1.Part).Build();

            this.Session.Derive();

            var order = new SalesOrderBuilder(this.Session)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .Build();

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
            pickList.SetPicked();

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);
            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.Ship();
            this.Session.Derive();

            foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
            {
                Assert.Equal(shipmentItem.QuantityShipped, shipmentItem.Quantity);
            }
        }
    }

    public class PackagingContentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PackagingContentDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPackagingContentShipmentItemThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var packagingContent = new PackagingContentBuilder(this.Session).WithQuantity(11).Build();
            this.Session.Derive(false);

            packagingContent.ShipmentItem = shipmentItem;

            var expectedMessage = $"{packagingContent}, { this.M.PackagingContent.Quantity}, { ErrorMessages.PackagingContentMaximum}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedShipmentItemQuantityThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var packagingContent = new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(10).Build();
            this.Session.Derive(false);

            shipmentItem.Quantity = 9;

            var expectedMessage = $"{packagingContent}, { this.M.PackagingContent.Quantity}, { ErrorMessages.PackagingContentMaximum}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedShipmentItemQuantityShippedThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var packagingContent = new PackagingContentBuilder(this.Session).WithShipmentItem(shipmentItem).WithQuantity(10).Build();
            this.Session.Derive(false);

            shipmentItem.QuantityShipped = 1;

            var expectedMessage = $"{packagingContent}, { this.M.PackagingContent.Quantity}, { ErrorMessages.PackagingContentMaximum}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
