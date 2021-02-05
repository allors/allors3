// <copyright file="ShipmentDerivationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ShipmentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentItemsSyncshipmentItemSyncedShipment()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).WithShipToParty(new PersonBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            Assert.Equal(shipment, shipmentItem.SyncedShipment);
        }
    }
}
