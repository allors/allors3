// <copyright file="DropShipmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class DropShipmentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public DropShipmentDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            store.RemoveDropShipmentNumberPrefix();
            var number = this.InternalOrganisation.StoresWhereInternalOrganisation.First.DropShipmentNumberCounter.Value;

            var shipment = new DropShipmentBuilder(this.Transaction).WithStore(store).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            var number = store.DropShipmentNumberCounter.Value;

            var shipment = new DropShipmentBuilder(this.Transaction).WithStore(store).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.First)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipToAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new DropShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipFromAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }
    }
}