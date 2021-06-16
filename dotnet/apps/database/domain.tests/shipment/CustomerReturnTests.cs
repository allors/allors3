// <copyright file="CustomerReturnTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CustomerReturnRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerReturnRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipToPartyDeriveShipmentNumber()
        {
            this.InternalOrganisation.RemoveCustomerReturnNumberPrefix();
            var number = this.InternalOrganisation.CustomerReturnNumberCounter.Value;

            var shipment = new CustomerReturnBuilder(this.Transaction).WithShipToParty(this.InternalOrganisation).Build();
            this.Derive();

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedShipToPartyDeriveSortableShipmentNumber()
        {
            var number = this.InternalOrganisation.CustomerReturnNumberCounter.Value;
            var shipment = new CustomerReturnBuilder(this.Transaction).WithShipToParty(this.InternalOrganisation).Build();
            this.Derive();

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            shipment.RemoveShipToAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Derive();

            shipment.RemoveShipFromAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipmentItemsSyncShipmentItem()
        {
            var shipment = new CustomerReturnBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            Assert.Equal(shipment, shipmentItem.SyncedShipment);
        }
    }
}
