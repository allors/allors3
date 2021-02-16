// <copyright file="PartWeightedAverageTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class PartWeightedAverageTests : DomainTest, IClassFixture<Fixture>
    {
        public PartWeightedAverageTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenNonSerialisedUnifiedGood_WhenPurchased_ThenAverageCostIsCalculated()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;
            var defaultFacility = this.InternalOrganisation.StoresWhereInternalOrganisation.Single().DefaultFacility;

            var secondFacility = new FacilityBuilder(this.Transaction)
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithName("second facility")
                .WithOwner(this.InternalOrganisation)
                .Build();

            var supplier = this.InternalOrganisation.ActiveSuppliers.First;
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var part = new UnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var purchaseOrder1 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(defaultFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            // Beginning inventory: 150 items at 8 euro received in 2 facilities
            var purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(100).WithAssignedUnitPrice(8M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            var purchaseOrder2 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(secondFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            // Beginning inventory: 150 items at 8 euro
            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(50).WithAssignedUnitPrice(8M).Build();
            purchaseOrder2.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder2.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder2.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(150, part.QuantityOnHand);
            Assert.Equal(8, part.PartWeightedAverage.AverageCost);

            purchaseOrder1.Revise();
            this.Transaction.Derive();

            // Purchase: 75 items at 8.1 euro
            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(75).WithAssignedUnitPrice(8.1M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(225, part.QuantityOnHand);
            Assert.Equal(8.03M, part.PartWeightedAverage.AverageCost);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .Build();

            this.Transaction.Derive();

            // Sell 50 items for 20 euro
            var salesItem1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(part).WithQuantityOrdered(50).WithAssignedUnitPrice(20M).Build();
            salesOrder.AddSalesOrderItem(salesItem1);

            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            salesOrder.Ship();
            this.Transaction.Derive();

            var customerShipment = salesItem1.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem as CustomerShipment;

            customerShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in customerShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            customerShipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(175, part.QuantityOnHand);
            Assert.Equal(8.03M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(401.5M, salesItem1.CostOfGoodsSold);

            // Again Sell 50 items for 20 euro
            salesOrder.Revise();
            this.Transaction.Derive();

            var salesItem2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(part).WithQuantityOrdered(50).WithAssignedUnitPrice(20M).Build();
            salesOrder.AddSalesOrderItem(salesItem2);

            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            salesOrder.Ship();
            this.Transaction.Derive();

            var customerShipment2 = salesItem2.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem as CustomerShipment;

            customerShipment2.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            var package2 = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package2);

            foreach (ShipmentItem shipmentItem in customerShipment2.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            customerShipment2.Ship();
            this.Transaction.Derive();

            Assert.Equal(125, part.QuantityOnHand);
            Assert.Equal(8.03M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(401.5M, salesItem1.CostOfGoodsSold);

            // Purchase: 50 items at 8.25 euro
            purchaseOrder1.Revise();
            this.Transaction.Derive();

            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(50).WithAssignedUnitPrice(8.25M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(175, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);

            // Use 65 items in a workorder
            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(65)
                .Build();

            this.Transaction.Derive(true);

            Assert.Equal(110, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(525.85M, inventoryAssignment.CostOfGoodsSold);

            // Cancel workeffort inventory assignment
            inventoryAssignment.Delete();

            this.Transaction.Derive(true);

            Assert.Equal(175, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);

            // Use 35 items in a workorder
            inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(35)
                .Build();

            this.Transaction.Derive(true);

            this.Transaction.Derive(true);

            Assert.Equal(140, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(283.15M, inventoryAssignment.CostOfGoodsSold);

            // Use 30 items in a workorder form second facility
            inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First(v => v.Facility.Equals(secondFacility)))
                .WithQuantity(30)
                .Build();

            this.Transaction.Derive(true);

            Assert.Equal(110, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(242.7M, inventoryAssignment.CostOfGoodsSold);

            // Purchase: 90 items at 8.35 euro
            var purchaseOrder3 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(defaultFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(90).WithAssignedUnitPrice(8.35M).Build();
            purchaseOrder3.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder3.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder3.Send();
            this.Transaction.Derive();

            purchaseOrder3.QuickReceive();
            this.Transaction.Derive();

            // Purchase: 50 items at 8.45 euro
            var purchaseOrder4 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(defaultFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(50).WithAssignedUnitPrice(8.45M).Build();
            purchaseOrder4.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder4.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder4.Send();
            this.Transaction.Derive();

            purchaseOrder4.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(250, part.QuantityOnHand);
            Assert.Equal(8.26M, part.PartWeightedAverage.AverageCost);

            // Ship 10 items to customer (without sales order)
            var outgoingShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(customer.ShippingAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive(true);

            var outgoingItem = new ShipmentItemBuilder(this.Transaction).WithGood(part).WithQuantity(10).Build();
            outgoingShipment.AddShipmentItem(outgoingItem);

            this.Transaction.Derive(true);

            outgoingShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in outgoingShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            outgoingShipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(240, part.QuantityOnHand);
            Assert.Equal(8.26M, part.PartWeightedAverage.AverageCost);

            // Receive 10 items at 8.55 from supplier (without purchase order)
            var incomingShipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            var incomingItem = new ShipmentItemBuilder(this.Transaction).WithPart(part).WithQuantity(10).WithUnitPurchasePrice(8.55M).Build();
            incomingShipment.AddShipmentItem(incomingItem);

            this.Transaction.Derive();

            incomingShipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(250, part.QuantityOnHand);
            Assert.Equal(8.27M, part.PartWeightedAverage.AverageCost);

            // Receive 100 items at 7.9 from supplier (without purchase order)
            incomingShipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            incomingItem = new ShipmentItemBuilder(this.Transaction).WithPart(part).WithQuantity(100).WithUnitPurchasePrice(7.9M).Build();
            incomingShipment.AddShipmentItem(incomingItem);

            this.Transaction.Derive();

            incomingShipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(350, part.QuantityOnHand);
            Assert.Equal(8.17M, part.PartWeightedAverage.AverageCost);

            // Ship all items to customer (without sales order)
            outgoingShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipFromFacility(part.DefaultFacility)
                .WithShipToAddress(customer.ShippingAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive(true);

            outgoingItem = new ShipmentItemBuilder(this.Transaction).WithGood(part).WithQuantity(330).Build();
            outgoingShipment.AddShipmentItem(outgoingItem);

            this.Transaction.Derive(true);

            outgoingShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in outgoingShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            outgoingShipment.Ship();
            this.Transaction.Derive();

            // Ship all items to customer (without sales order)
            outgoingShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipFromFacility(secondFacility)
                .WithShipToAddress(customer.ShippingAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive(true);

            outgoingItem = new ShipmentItemBuilder(this.Transaction).WithGood(part).WithQuantity(20).Build();
            outgoingShipment.AddShipmentItem(outgoingItem);

            this.Transaction.Derive(true);

            outgoingShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in outgoingShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            outgoingShipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(0, part.QuantityOnHand);
            Assert.Equal(8.17M, part.PartWeightedAverage.AverageCost);

            purchaseOrder1.Revise();
            this.Transaction.Derive();

            // Purchase 150 items at 8 euro
            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(150).WithAssignedUnitPrice(8M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(150, part.QuantityOnHand);
            Assert.Equal(8, part.PartWeightedAverage.AverageCost);
        }

        [Fact]
        public void GivenNonSerialisedNonUnifiedPart_WhenPurchased_ThenAverageCostIsCalculated()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;
            var defaultFacility = this.InternalOrganisation.StoresWhereInternalOrganisation.Single().DefaultFacility;

            var secondFacility = new FacilityBuilder(this.Transaction)
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithName("second facility")
                .WithOwner(this.InternalOrganisation)
                .Build();

            var supplier = this.InternalOrganisation.ActiveSuppliers.First;
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var part = new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName(part.Name)
                .WithPart(part)
                .WithVatRate(new VatRates(this.Transaction).Zero)
                .Build();

            this.Transaction.Derive();

            var purchaseOrder1 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithDeliveryDate(this.Transaction.Now())
                .WithStoredInFacility(defaultFacility)
                .Build();

            this.Transaction.Derive();

            // Beginning inventory: 150 items at 8 euro received in 2 facilities
            var purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(100).WithAssignedUnitPrice(8M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            var purchaseOrder2 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(secondFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            // Beginning inventory: 150 items at 8 euro
            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(50).WithAssignedUnitPrice(8M).Build();
            purchaseOrder2.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder2.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder2.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(150, part.QuantityOnHand);
            Assert.Equal(8, part.PartWeightedAverage.AverageCost);

            purchaseOrder1.Revise();
            this.Transaction.Derive();

            // Purchase: 75 items at 8.1 euro
            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(75).WithAssignedUnitPrice(8.1M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(225, part.QuantityOnHand);
            Assert.Equal(8.03M, part.PartWeightedAverage.AverageCost);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .Build();

            this.Transaction.Derive();

            // Sell 50 items for 20 euro
            var salesItem1 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good).WithQuantityOrdered(50).WithAssignedUnitPrice(20M).Build();
            salesOrder.AddSalesOrderItem(salesItem1);

            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            salesOrder.Ship();
            this.Transaction.Derive();

            var customerShipment = salesItem1.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem as CustomerShipment;

            customerShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in customerShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            customerShipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(175, part.QuantityOnHand);
            Assert.Equal(8.03M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(401.5M, salesItem1.CostOfGoodsSold);

            // Again Sell 50 items for 20 euro
            salesOrder.Revise();
            this.Transaction.Derive();

            var salesItem2 = new SalesOrderItemBuilder(this.Transaction).WithProduct(good).WithQuantityOrdered(50).WithAssignedUnitPrice(20M).Build();
            salesOrder.AddSalesOrderItem(salesItem2);

            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            salesOrder.Ship();
            this.Transaction.Derive();

            var customerShipment2 = salesItem2.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem as CustomerShipment;

            customerShipment2.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            var package2 = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package2);

            foreach (ShipmentItem shipmentItem in customerShipment2.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            customerShipment2.Ship();
            this.Transaction.Derive();

            Assert.Equal(125, part.QuantityOnHand);
            Assert.Equal(8.03M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(401.5M, salesItem1.CostOfGoodsSold);

            // Purchase: 50 items at 8.25 euro
            purchaseOrder1.Revise();
            this.Transaction.Derive();

            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(50).WithAssignedUnitPrice(8.25M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(175, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);

            // Use 65 items in a workorder
            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(65)
                .Build();

            this.Transaction.Derive(true);

            Assert.Equal(110, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(525.85M, inventoryAssignment.CostOfGoodsSold);

            // Cancel workeffort inventory assignment
            inventoryAssignment.Delete();

            this.Transaction.Derive(true);

            Assert.Equal(175, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);

            // Use 35 items in a workorder
            inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(35)
                .Build();

            this.Transaction.Derive(true);

            this.Transaction.Derive(true);

            Assert.Equal(140, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(283.15M, inventoryAssignment.CostOfGoodsSold);

            // Use 30 items in a workorder form second facility
            inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First(v => v.Facility.Equals(secondFacility)))
                .WithQuantity(30)
                .Build();

            this.Transaction.Derive(true);

            Assert.Equal(110, part.QuantityOnHand);
            Assert.Equal(8.09M, part.PartWeightedAverage.AverageCost);
            Assert.Equal(242.7M, inventoryAssignment.CostOfGoodsSold);

            // Purchase: 90 items at 8.35 euro
            var purchaseOrder3 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(defaultFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(90).WithAssignedUnitPrice(8.35M).Build();
            purchaseOrder3.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder3.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder3.Send();
            this.Transaction.Derive();

            purchaseOrder3.QuickReceive();
            this.Transaction.Derive();

            // Purchase: 50 items at 8.45 euro
            var purchaseOrder4 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(defaultFacility)
                .WithDeliveryDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(50).WithAssignedUnitPrice(8.45M).Build();
            purchaseOrder4.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder4.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder4.Send();
            this.Transaction.Derive();

            purchaseOrder4.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(250, part.QuantityOnHand);
            Assert.Equal(8.26M, part.PartWeightedAverage.AverageCost);

            // Ship 10 items to customer (without sales order)
            var outgoingShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(customer.ShippingAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive(true);

            var outgoingItem = new ShipmentItemBuilder(this.Transaction).WithGood(good).WithQuantity(10).Build();
            outgoingShipment.AddShipmentItem(outgoingItem);

            this.Transaction.Derive(true);

            outgoingShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in outgoingShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            outgoingShipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(240, part.QuantityOnHand);
            Assert.Equal(8.26M, part.PartWeightedAverage.AverageCost);

            // Receive 10 items at 8.55 from supplier (without purchase order)
            var incomingShipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            var incomingItem = new ShipmentItemBuilder(this.Transaction).WithPart(part).WithQuantity(10).WithUnitPurchasePrice(8.55M).Build();
            incomingShipment.AddShipmentItem(incomingItem);

            this.Transaction.Derive();

            incomingShipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(250, part.QuantityOnHand);
            Assert.Equal(8.27M, part.PartWeightedAverage.AverageCost);

            // Receive 100 items at 7.9 from supplier (without purchase order)
            incomingShipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            incomingItem = new ShipmentItemBuilder(this.Transaction).WithPart(part).WithQuantity(100).WithUnitPurchasePrice(7.9M).Build();
            incomingShipment.AddShipmentItem(incomingItem);

            this.Transaction.Derive();

            incomingShipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(350, part.QuantityOnHand);
            Assert.Equal(8.17M, part.PartWeightedAverage.AverageCost);

            // Ship all items to customer (without sales order)
            outgoingShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipFromFacility(part.DefaultFacility)
                .WithShipToAddress(customer.ShippingAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive(true);

            outgoingItem = new ShipmentItemBuilder(this.Transaction).WithGood(good).WithQuantity(330).Build();
            outgoingShipment.AddShipmentItem(outgoingItem);

            this.Transaction.Derive(true);

            outgoingShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in outgoingShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            outgoingShipment.Ship();
            this.Transaction.Derive();

            // Ship all items to customer (without sales order)
            outgoingShipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipFromFacility(secondFacility)
                .WithShipToAddress(customer.ShippingAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive(true);

            outgoingItem = new ShipmentItemBuilder(this.Transaction).WithGood(good).WithQuantity(20).Build();
            outgoingShipment.AddShipmentItem(outgoingItem);

            this.Transaction.Derive(true);

            outgoingShipment.Pick();
            this.Transaction.Derive();

            customer.PickListsWhereShipToParty.First(v => v.PickListState.Equals(new PickListStates(this.Transaction).Created)).SetPicked();

            package = new ShipmentPackageBuilder(this.Transaction).Build();
            customerShipment2.AddShipmentPackage(package);

            foreach (ShipmentItem shipmentItem in outgoingShipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            outgoingShipment.Ship();
            this.Transaction.Derive();

            Assert.Equal(0, part.QuantityOnHand);
            Assert.Equal(8.17M, part.PartWeightedAverage.AverageCost);

            purchaseOrder1.Revise();
            this.Transaction.Derive();

            // Purchase 150 items at 8 euro
            purchaseItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(150).WithAssignedUnitPrice(8M).Build();
            purchaseOrder1.AddPurchaseOrderItem(purchaseItem);

            this.Transaction.Derive();

            purchaseOrder1.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder1.Send();
            this.Transaction.Derive();

            purchaseItem.QuickReceive();
            this.Transaction.Derive();

            Assert.Equal(150, part.QuantityOnHand);
            Assert.Equal(8, part.PartWeightedAverage.AverageCost);
        }
    }
}
