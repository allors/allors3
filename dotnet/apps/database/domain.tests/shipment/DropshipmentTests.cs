// <copyright file="DropShipmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class DropShipmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public DropShipmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault();
            store.RemoveDropShipmentNumberPrefix();
            var number = this.InternalOrganisation.StoresWhereInternalOrganisation.First().DropShipmentNumberCounter.Value;

            var shipment = new DropShipmentBuilder(this.Transaction).WithStore(store).Build();
            this.Derive();

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault();
            var number = store.DropShipmentNumberCounter.Value;

            var shipment = new DropShipmentBuilder(this.Transaction).WithStore(store).Build();
            this.Derive();

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.FirstOrDefault())
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First().ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.FirstOrDefault())
                .Build();
            this.Derive();

            shipment.RemoveShipToAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First().ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            shipment.RemoveShipFromAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }
    }
}
