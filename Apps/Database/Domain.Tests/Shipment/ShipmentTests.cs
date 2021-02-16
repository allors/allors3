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
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            Assert.Equal(shipment, shipmentItem.SyncedShipment);
        }
    }
}
