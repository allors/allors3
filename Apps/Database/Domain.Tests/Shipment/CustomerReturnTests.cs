// <copyright file="CustomerReturnTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CustomerReturnDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerReturnDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Session)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Session)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Session.Derive(false);

            shipment.RemoveShipToAddress();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Session)
                .WithShipFromParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new CustomerReturnBuilder(this.Session)
                .WithShipFromParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Session.Derive(false);

            shipment.RemoveShipFromAddress();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipmentItemsSyncShipmentItem()
        {
            var shipment = new CustomerReturnBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            Assert.Equal(shipment, shipmentItem.SyncedShipment);
        }
    }
}
