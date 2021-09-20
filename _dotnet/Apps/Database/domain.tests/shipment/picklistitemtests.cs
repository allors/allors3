// <copyright file="PickListItemDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class PickListItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var pickListItem = new PickListItemBuilder(this.Transaction).WithQuantityPicked(10).Build();
            this.Derive();

            pickListItem.Quantity = 1;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PickListItemQuantityMoreThanAllowed));
        }

        [Fact]
        public void ChangedQuantityPickedThrowValidationError()
        {
            var pickListItem = new PickListItemBuilder(this.Transaction).WithQuantity(10).Build();
            this.Derive();

            pickListItem.QuantityPicked = 11;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PickListItemQuantityMoreThanAllowed));
        }

        [Fact]
        public void ChangedPickListPickListStateCreateOrderShipment()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Derive();

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var orderShipment1 = new OrderShipmentBuilder(this.Transaction).WithQuantity(3).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            var orderShipment2 = new OrderShipmentBuilder(this.Transaction).WithQuantity(2).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).WithQuantityPicked(1).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithPickListItem(pickListItem).WithShipmentItem(shipmentItem).WithQuantity(5).Build();
            this.Derive();

            pickList.PickListState = new PickListStates(this.Transaction).Picked;
            this.Derive();

            Assert.Equal(0, orderShipment1.Quantity);
            Assert.Equal(1, orderShipment2.Quantity);
        }
    }

    public class PickListItemQuantityPickedRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListItemQuantityPickedRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPickListPickListStateDeriveQuantityPicked()
        {
            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).WithQuantity(1).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            pickList.PickListState = new PickListStates(this.Transaction).Picked;
            this.Derive();

            Assert.Equal(pickListItem.Quantity, pickListItem.QuantityPicked);
        }
    }

    public class PickListStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentShipmentStateDerivePickListStateOnHold()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithPickListItem(pickListItem).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).OnHold;
            this.Derive();

            Assert.True(pickList.PickListState.IsOnHold);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDerivePickListStateCreated()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithPickListItem(pickListItem).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).OnHold;
            this.Derive();

            Assert.True(pickList.PickListState.IsOnHold);

            shipment.Continue();
            this.Derive();

            Assert.True(pickList.PickListState.IsCreated);
        }
    }
}
