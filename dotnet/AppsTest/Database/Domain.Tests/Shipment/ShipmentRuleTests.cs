// <copyright file="ShipmentRuleTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ShipmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentItemsDeriveShipmentItemDelegatedAccess()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(new PersonBuilder(this.Transaction).Build()).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            Assert.Equal(shipment, shipmentItem.DelegatedAccess);
        }
    }
}
