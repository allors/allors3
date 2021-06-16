// <copyright file="ShipmentReceiptTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class ShipmentReceiptTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentReceiptTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenShipmentReceiptBuilderWhenBuildThenPostBuildRelationsMustExist()
        {
            var receipt = new ShipmentReceiptBuilder(this.Transaction).Build();

            Assert.True(receipt.ExistReceivedDateTime);
            Assert.Equal(0, receipt.QuantityAccepted);
            Assert.Equal(0, receipt.QuantityRejected);
        }

        [Fact]
        public void GivenShipmentReceiptWhenValidatingThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var inventoryItem = good1.Part.InventoryItemsWherePart.First;
            var builder = new ShipmentReceiptBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithInventoryItem(inventoryItem);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground).WithShipFromParty(supplier).Build();
            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithGood(good1).Build();
            shipment.AddShipmentItem(shipmentItem);

            builder.WithShipmentItem(shipmentItem);
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenShipmentReceiptForPartWithoutSelectedInventoryItemWhenDerivingThenInventoryItemIsFromDefaultFacility()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            var shipment = (PurchaseShipment)item1.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            var receipt = item1.ShipmentReceiptsWhereOrderItem.Single();

            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), receipt.InventoryItem.Facility);
            Assert.Equal(part.InventoryItemsWherePart[0], receipt.InventoryItem);
        }

        [Fact]
        public void GivenShipmentReceiptForGoodWithoutSelectedInventoryItemWhenDerivingThenInventoryItemIsFromDefaultFacility()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(good1.Part).WithQuantityOrdered(1).Build();
            order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();
            this.Transaction.Commit();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            var shipment = (PurchaseShipment)item1.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            var receipt = item1.ShipmentReceiptsWhereOrderItem.Single();

            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), receipt.InventoryItem.Facility);
            Assert.Equal(good1.Part.InventoryItemsWherePart[0], receipt.InventoryItem);

            this.Transaction.Rollback();
        }

        [Fact]
        public void GivenShipmentReceiptWhenDerivingThenInventoryItemQuantityOnHandIsUpdated()
        {
            var store = this.Transaction.Extent<Store>().First;
            store.IsImmediatelyPicked = false;

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

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(20).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(good1.Part).Build();

            this.Transaction.Derive();

            var inventoryItem = good1.Part.InventoryItemsWherePart.First;

            var order1 = new SalesOrderBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            var salesItem = new SalesOrderItemBuilder(this.Transaction).WithDescription("item1").WithProduct(good1).WithQuantityOrdered(30).WithAssignedUnitPrice(15).Build();
            order1.AddSalesOrderItem(salesItem);

            this.Transaction.Derive();
            this.Transaction.Commit();

            order1.SetReadyForPosting();

            this.Transaction.Derive();

            order1.Post();
            this.Transaction.Derive();

            order1.Accept();
            this.Transaction.Derive();

            var transactionInventoryItem = (NonSerialisedInventoryItem)this.Transaction.Instantiate(inventoryItem);
            var transactionSalesItem = (SalesOrderItem)this.Transaction.Instantiate(salesItem);

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            Assert.Equal(20, transactionSalesItem.QuantityPendingShipment);
            Assert.Equal(30, transactionSalesItem.QuantityReserved);
            Assert.Equal(10, transactionSalesItem.QuantityShortFalled);

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(good1.Part).WithQuantityOrdered(10).Build();
            order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();
            this.Transaction.Commit();

            order.SetReadyForProcessing();

            order.OrderedBy.IsAutomaticallyReceived = true;
            order.QuickReceive();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(30, transactionInventoryItem.QuantityOnHand);

            Assert.Equal(30, transactionSalesItem.QuantityPendingShipment);
            Assert.Equal(30, transactionSalesItem.QuantityReserved);
            Assert.Equal(0, transactionSalesItem.QuantityShortFalled);
        }

        [Fact]
        public void GivenShipmentReceiptWhenDerivingThenOrderItemQuantityReceivedIsUpdated()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(good1.Part).WithQuantityOrdered(10).Build();
            order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();
            this.Transaction.Commit();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            var shipment = (PurchaseShipment)item1.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(10, item1.QuantityReceived);

            this.Transaction.Rollback();
        }
    }

    public class ShipmentReceiptRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentReceiptRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentReceiptShipmentItemCreateOrderShipment()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var receipt = new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).Build();
            this.Derive();

            receipt.ShipmentItem = shipmentItem;
            this.Derive();

            Assert.True(shipmentItem.ExistOrderShipmentsWhereShipmentItem);
        }

        [Fact]
        public void ChangedShipmentReceiptOrderItemCreateOrderShipment()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var receipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            receipt.OrderItem = orderItem;
            this.Derive();

            Assert.True(shipmentItem.ExistOrderShipmentsWhereShipmentItem);
        }

        [Fact]
        public void ChangedQuantityAcceptedDeriveOrderShipmentQuantity()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).Build();
            this.Derive();

            shipmentReceipt.QuantityAccepted = 2;
            this.Derive();

            var orderShipment = shipmentItem.OrderShipmentsWhereShipmentItem.First;
            Assert.Equal(2, orderShipment.Quantity);
        }

        [Fact]
        public void ChangedFacilityDeriveInventoryItem()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece).Build();
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithPart(part).WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build()).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).Build();
            this.Derive();

            shipmentReceipt.Facility = this.InternalOrganisation.FacilitiesWhereOwner.First;
            this.Derive();

            Assert.True(shipmentReceipt.ExistInventoryItem);
        }

        [Fact]
        public void ChangedSerialisedItemDeriveInventoryItem()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece).Build();
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithPart(part).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First).Build();
            this.Derive();

            shipmentItem.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(shipmentReceipt.ExistInventoryItem);
        }

        [Fact]
        public void ChangedPartDeriveInventoryItem()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece).Build();
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First).Build();
            this.Derive();

            shipmentItem.Part = part;
            this.Derive();

            Assert.True(shipmentReceipt.ExistInventoryItem);
        }
    }
}
