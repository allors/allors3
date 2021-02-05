// <copyright file="PickListItemDerivation.cs" company="Allors bvba">
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

    public class PickListItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListItemDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var pickListItem = new PickListItemBuilder(this.Session).WithQuantityPicked(10).Build();
            this.Session.Derive(false);

            pickListItem.Quantity = 1;

            var expectedMessage = $"{pickListItem}, { this.M.PickListItem.QuantityPicked}, { ErrorMessages.PickListItemQuantityMoreThanAllowed}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityPickedThrowValidationError()
        {
            var pickListItem = new PickListItemBuilder(this.Session).WithQuantity(10).Build();
            this.Session.Derive(false);

            pickListItem.QuantityPicked = 11;

            var expectedMessage = $"{pickListItem}, { this.M.PickListItem.QuantityPicked}, { ErrorMessages.PickListItemQuantityMoreThanAllowed}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedPickListPickListStateCreateOrderShipment()
        {
            var salesOrder = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).Build();
            salesOrder.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            var shipment = new CustomerShipmentBuilder(this.Session).WithShipToParty(new PersonBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var orderShipment1 = new OrderShipmentBuilder(this.Session).WithQuantity(3).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            var orderShipment2 = new OrderShipmentBuilder(this.Session).WithQuantity(2).WithOrderItem(orderItem).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).WithQuantityPicked(1).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithPickListItem(pickListItem).WithShipmentItem(shipmentItem).WithQuantity(5).Build();
            this.Session.Derive(false);

            pickList.PickListState = new PickListStates(this.Session).Picked;
            this.Session.Derive(false);

            Assert.Equal(0, orderShipment1.Quantity);
            Assert.Equal(1, orderShipment2.Quantity);
        }
    }

    public class PickListItemQuantityPickedDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListItemQuantityPickedDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPickListPickListStateDeriveQuantityPicked()
        {
            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).WithQuantity(1).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            pickList.PickListState = new PickListStates(this.Session).Picked;
            this.Session.Derive(false);

            Assert.Equal(pickListItem.Quantity, pickListItem.QuantityPicked);
        }
    }

    public class PickListStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PickListStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentShipmentStateDerivePickListStateOnHold()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithPickListItem(pickListItem).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Session).OnHold;
            this.Session.Derive(false);

            Assert.True(pickList.PickListState.IsOnHold);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDerivePickListStateCreated()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithPickListItem(pickListItem).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Session).OnHold;
            this.Session.Derive(false);

            Assert.True(pickList.PickListState.IsOnHold);

            shipment.Continue();
            this.Session.Derive(false);

            Assert.True(pickList.PickListState.IsCreated);
        }
    }
}
