// <copyright file="PurchaseShipmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class PurchaseShipmentTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseShipmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseShipmentBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground).WithShipFromParty(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal(new ShipmentStates(this.Transaction).Created, shipment.ShipmentState);
            Assert.Equal(this.InternalOrganisation.GeneralCorrespondence, shipment.ShipToAddress);
            Assert.Equal(this.InternalOrganisation, shipment.ShipToParty);
        }

        [Fact]
        public void GivenPurchaseShipment_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            this.Transaction.Commit();

            var builder = new PurchaseShipmentBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithShipFromParty(supplier);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseShipment_WhenGettingShipmentNumberWithoutFormat_ThenShipmentNumberShouldBeReturned()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.RemovePurchaseShipmentNumberPrefix();

            var shipment1 = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("1", shipment1.ShipmentNumber);

            var shipment2 = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("2", shipment2.ShipmentNumber);
        }

        [Fact]
        public void GivenPurchaseShipment_WhenGettingShipmentNumberWithFormat_ThenFormattedShipmentNumberShouldBeReturned()
        {
            this.InternalOrganisation.PurchaseShipmentSequence = new PurchaseShipmentSequences(this.Transaction).EnforcedSequence;
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var shipment1 = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("incoming shipmentno: 1", shipment1.ShipmentNumber);

            var shipment2 = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("incoming shipmentno: 2", shipment2.ShipmentNumber);
        }

        [Fact]
        public void GivenShipToWithoutShipmentNumberPrefix_WhenDeriving_ThenSortableShipmentNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseShipmentNumberPrefix();
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(shipment.ShipmentNumber), shipment.SortableShipmentNumber);
        }

        [Fact]
        public void GivenShipToWithShipmentNumberPrefix_WhenDeriving_ThenSortableShipmentNumberIsSet()
        {
            this.InternalOrganisation.PurchaseShipmentSequence = new PurchaseShipmentSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.PurchaseShipmentNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(shipment.ShipmentNumber.Split('-')[1]), shipment.SortableShipmentNumber);
        }

        [Fact]
        public void GivenPurchaseShipmentWithShipToCustomerWithshippingAddress_WhenDeriving_ThenDerivedShipToCustomerAndDerivedShipToAddressMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithAddress1("Haverwerf 15").WithPostalAddressBoundary(mechelen).Build();

            var shippingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            this.InternalOrganisation.AddPartyContactMechanism(shippingAddress);

            this.Transaction.Derive();

            var order = new PurchaseShipmentBuilder(this.Transaction).WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground).WithShipFromParty(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal(shippingAddress.ContactMechanism, order.ShipToAddress);
        }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            new PurchaseShipmentBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.Equals("PurchaseShipment.ShipFromParty is required"));
        }
    }

    public class PurchaseShipmentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseShipmentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipToPartyDeriveShipmentNumber()
        {
            this.InternalOrganisation.RemovePurchaseShipmentNumberPrefix();
            var number = this.InternalOrganisation.PurchaseShipmentNumberCounter.Value;

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipToParty(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedShipToPartyDeriveSortableShipmentNumber()
        {
            var number = this.InternalOrganisation.PurchaseShipmentNumberCounter.Value;
            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipToParty(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipToAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Transaction.Derive(false);

            shipment.RemoveShipFromAddress();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First.ShippingAddress, shipment.ShipFromAddress);
        }
    }

    public class PurchaseShipmentStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseShipmentStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDeriveShipmentState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithQuantityOrdered(10).Build();
            this.Transaction.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithShipmentItemState(new ShipmentItemStates(this.Transaction).Received).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).WithQuantityAccepted(10).Build();
            this.Transaction.Derive(false);

            Assert.True(shipment.ShipmentState.IsReceived);
        }

        [Fact]
        public void ChangedShipmentItemShipmentItemStateDeriveShipmentState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithQuantityOrdered(10).Build();
            this.Transaction.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithOrderItem(orderItem).WithQuantityAccepted(10).Build();
            this.Transaction.Derive(false);

            shipmentItem.ShipmentItemState = new ShipmentItemStates(this.Transaction).Received;
            this.Transaction.Derive(false);

            Assert.True(shipment.ShipmentState.IsReceived);
        }
    }
}
