// <copyright file="OrderShipmentDerivationTests.cs" company="Allors bvba">
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

    public class OrderShipmentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderShipmentDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityDeriveshipmentItemQuantity()
        {
            var salesOrder = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var orderShipment = new OrderShipmentBuilder(this.Session).WithQuantity(10).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            orderShipment.Quantity = 9;
            this.Session.Derive(false);

            Assert.Equal(9, shipmentItem.Quantity);
        }

        [Fact]
        public void ChangedQuantityDeriveSalesOrderItemQuantityRequestsShipping()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithQuantityRequestsShipping(3).Build();

            new OrderShipmentBuilder(this.Session).WithQuantity(1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            Assert.Equal(2, orderItem.QuantityRequestsShipping);
        }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrderForSingleNonSerialisedItem(this.Session.Faker());
            order.PartiallyShip = true;
            this.Session.Derive(false);

            var orderItem = order.SalesOrderItems.First(v => v.QuantityOrdered > 1);
            new InventoryItemTransactionBuilder(this.Session)
                .WithQuantity(orderItem.QuantityOrdered)
                .WithReason(new InventoryTransactionReasons(this.Session).Unknown)
                .WithPart(orderItem.Part)
                .Build();
            this.Session.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var orderShipment = new OrderShipmentBuilder(this.Session).WithQuantity(orderItem.QuantityOrdered + 1).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();

            var expectedMessage = $"{orderShipment}, { this.M.OrderShipment.Quantity}, { ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
