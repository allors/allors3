// <copyright file="PurchaseShipmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Xunit;

    public class PurchaseShipmentTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseShipmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseShipmentBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Session).WithShipmentMethod(new ShipmentMethods(this.Session).Ground).WithShipFromParty(supplier).Build();

            this.Session.Derive();

            Assert.Equal(new ShipmentStates(this.Session).Created, shipment.ShipmentState);
            Assert.Equal(this.InternalOrganisation.GeneralCorrespondence, shipment.ShipToAddress);
            Assert.Equal(this.InternalOrganisation, shipment.ShipToParty);
        }

        [Fact]
        public void GivenPurchaseShipment_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();

            this.Session.Commit();

            var builder = new PurchaseShipmentBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithShipmentMethod(new ShipmentMethods(this.Session).Ground);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithShipFromParty(supplier);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseShipment_WhenGettingShipmentNumberWithoutFormat_ThenShipmentNumberShouldBeReturned()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.RemovePurchaseShipmentNumberPrefix();

            var shipment1 = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("1", shipment1.ShipmentNumber);

            var shipment2 = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("2", shipment2.ShipmentNumber);
        }

        [Fact]
        public void GivenPurchaseShipment_WhenGettingShipmentNumberWithFormat_ThenFormattedShipmentNumberShouldBeReturned()
        {
            this.InternalOrganisation.PurchaseShipmentSequence = new PurchaseShipmentSequences(this.Session).EnforcedSequence;
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var shipment1 = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("incoming shipmentno: 1", shipment1.ShipmentNumber);

            var shipment2 = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("incoming shipmentno: 2", shipment2.ShipmentNumber);
        }

        [Fact]
        public void GivenShipToWithoutShipmentNumberPrefix_WhenDeriving_ThenSortableShipmentNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseShipmentNumberPrefix();
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(shipment.ShipmentNumber), shipment.SortableShipmentNumber);
        }

        [Fact]
        public void GivenShipToWithShipmentNumberPrefix_WhenDeriving_ThenSortableShipmentNumberIsSet()
        {
            this.InternalOrganisation.PurchaseShipmentSequence = new PurchaseShipmentSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.PurchaseShipmentNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(shipment.ShipmentNumber.Split('-')[1]), shipment.SortableShipmentNumber);
        }

        [Fact]
        public void GivenPurchaseShipmentWithShipToCustomerWithshippingAddress_WhenDeriving_ThenDerivedShipToCustomerAndDerivedShipToAddressMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Session).WithAddress1("Haverwerf 15").WithPostalAddressBoundary(mechelen).Build();

            var shippingAddress = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            this.InternalOrganisation.AddPartyContactMechanism(shippingAddress);

            this.Session.Derive();

            var order = new PurchaseShipmentBuilder(this.Session).WithShipmentMethod(new ShipmentMethods(this.Session).Ground).WithShipFromParty(supplier).Build();

            this.Session.Derive();

            Assert.Equal(shippingAddress.ContactMechanism, order.ShipToAddress);
        }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            new PurchaseShipmentBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("PurchaseShipment.ShipFromParty is required"));
        }
    }

    public class PurchaseShipmentDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseShipmentDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipToPartyDeriveShipmentNumber()
        {
            this.InternalOrganisation.RemovePurchaseShipmentNumberPrefix();
            var number = this.InternalOrganisation.PurchaseShipmentNumberCounter.Value;

            var shipment = new PurchaseShipmentBuilder(this.Session).WithShipToParty(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedShipToPartyDeriveSortableShipmentNumber()
        {
            var number = this.InternalOrganisation.PurchaseShipmentNumberCounter.Value;
            var shipment = new PurchaseShipmentBuilder(this.Session).WithShipToParty(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session)
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
            var shipment = new PurchaseShipmentBuilder(this.Session)
                .WithShipFromParty(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session)
                .WithShipFromParty(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Session.Derive(false);

            shipment.RemoveShipFromAddress();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First.ShippingAddress, shipment.ShipFromAddress);
        }
    }

    public class PurchaseShipmentStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseShipmentStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDeriveShipmentState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithQuantityOrdered(10).Build();
            this.Session.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithShipmentItemState(new ShipmentItemStates(this.Session).Received).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            new ShipmentReceiptBuilder(this.Session).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).WithQuantityAccepted(10).Build();
            this.Session.Derive(false);

            Assert.True(shipment.ShipmentState.IsReceived);
        }

        [Fact]
        public void ChangedShipmentItemShipmentItemStateDeriveShipmentState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithQuantityOrdered(10).Build();
            this.Session.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            new ShipmentReceiptBuilder(this.Session).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).WithQuantityAccepted(10).Build();
            this.Session.Derive(false);

            shipmentItem.ShipmentItemState = new ShipmentItemStates(this.Session).Received;
            this.Session.Derive(false);

            Assert.True(shipment.ShipmentState.IsReceived);
        }
    }
}
