// <copyright file="TrnasferTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class TransferRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public TransferRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new TransferBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveCustomers.FirstOrDefault())
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveCustomers.First().ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new TransferBuilder(this.Transaction)
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
            var shipment = new TransferBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new TransferBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            shipment.RemoveShipFromAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }
    }
}
