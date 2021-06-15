// <copyright file="OrderShipmentRuleTests.cs" company="Allors bvba">
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

    public class OrderShipmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderShipmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityDeriveshipmentItemQuantity()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var orderShipment = new OrderShipmentBuilder(this.Transaction).WithQuantity(10).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            orderShipment.Quantity = 9;
            this.Transaction.Derive(false);

            Assert.Equal(9, shipmentItem.Quantity);
        }

        [Fact]
        public void ChangedQuantityDeriveSalesOrderItemQuantityRequestsShipping()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithQuantityRequestsShipping(3).Build();

            new OrderShipmentBuilder(this.Transaction).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            Assert.Equal(2, orderItem.QuantityRequestsShipping);
        }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Transaction.Faker());
            order.PartiallyShip = true;
            this.Transaction.Derive(false);

            var orderItem = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithQuantity(orderItem.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .WithPart(orderItem.Part)
                .Build();
            this.Transaction.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new OrderShipmentBuilder(this.Transaction).WithQuantity(orderItem.QuantityOrdered + 1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable));
        }
    }
}
