// <copyright file="PurchaseReturnDerivationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PurchaseReturnDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseReturnDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            store.RemovePurchaseReturnNumberPrefix();
            var number = this.InternalOrganisation.StoresWhereInternalOrganisation.First.PurchaseReturnNumberCounter.Value;

            var shipment = new PurchaseReturnBuilder(this.Transaction).WithStore(store).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.First;
            var number = store.PurchaseReturnNumberCounter.Value;

            var shipment = new PurchaseReturnBuilder(this.Transaction).WithStore(store).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipToAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipFromAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }
    }
}
