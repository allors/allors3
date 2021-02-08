// <copyright file="ShipmentItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class ShipmentItemTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseShipmentItemForNonSerialisedNotFromPurchaseOrder_WhenDerived_ThenUnitPurchasePriceIsRequired()
        {
            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");

            User user = this.Administrator;
            this.Session.SetUser(user);

            var shipment = new PurchaseShipmentBuilder(this.Session)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .WithShipFromParty(this.InternalOrganisation.ActiveSuppliers.First)
                .WithCreationDate(this.Session.Now())
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithPart(good1.Part).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: ShipmentItem.UnitPurchasePrice"));
        }
    }

    public class ShipmentItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNextSerialisedItemAvailabilityThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Session).Build())
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Session).Sold)
                .Build();
            shipment.AddShipmentItem(shipmentItem);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals("AssertExists: ShipmentItem.NextSerialisedItemAvailability"));

            shipmentItem.RemoveNextSerialisedItemAvailability();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: ShipmentItem.NextSerialisedItemAvailability"));
        }

        [Fact]
        public void ChangedSerialisedThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals("AssertExists: ShipmentItem.NextSerialisedItemAvailability"));

            shipmentItem.SerialisedItem = new SerialisedItemBuilder(this.Session).Build();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: ShipmentItem.NextSerialisedItemAvailability"));
        }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Session).Build())
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipmentItem.Quantity = 2;

            var expectedMessage = $"{shipmentItem}, { this.M.ShipmentItem.Quantity}, { ErrorMessages.SerializedItemQuantity}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedSerialisedThrowValidationError_2()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithQuantity(2).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipmentItem.SerialisedItem = new SerialisedItemBuilder(this.Session).Build();

            var expectedMessage = $"{shipmentItem}, { this.M.ShipmentItem.Quantity}, { ErrorMessages.SerializedItemQuantity}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedItemIssuanceQuantityDeriveQuantityPicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session)
                .WithPickListState(new PickListStates(this.Session).Picked)
                .Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Session.Derive(false);

            Assert.Equal(10, shipmentItem.QuantityPicked);
        }

        [Fact]
        public void ChangedPickListPickListStateDeriveQuantityPicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Session.Derive(false);

            Assert.Equal(0, shipmentItem.QuantityPicked);

            pickList.PickListState = new PickListStates(this.Session).Picked;
            this.Session.Derive(false);

            Assert.Equal(10, shipmentItem.QuantityPicked);
        }

        [Fact]
        public void ChangedItemIssuanceQuantityDeriveQuantityShipped()
        {
            var shipment = new CustomerShipmentBuilder(this.Session)
                .WithShipmentState(new ShipmentStates(this.Session).Shipped)
                .Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Session.Derive(false);

            Assert.Equal(10, shipmentItem.QuantityShipped);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveQuantityShipped()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Session.Derive(false);

            Assert.Equal(0, shipmentItem.QuantityShipped);

            shipment.ShipmentState = new ShipmentStates(this.Session).Shipped;
            this.Session.Derive(false);

            Assert.Equal(10, shipmentItem.QuantityShipped);
        }

        [Fact]
        public void ChangedStoredInFacilityDeriveStoredInFacility()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipmentItem.RemoveStoredInFacility();
            this.Session.Derive(false);

            Assert.Equal(shipment.ShipToFacility, shipmentItem.StoredInFacility);
        }

        [Fact]
        public void ChangedShipmentShipToFacilityDeriveStoredInFacility()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            shipment.RemoveShipToFacility();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipment.ShipToFacility = this.InternalOrganisation.FacilitiesWhereOwner.First;
            this.Session.Derive(false);

            Assert.Equal(shipment.ShipToFacility, shipmentItem.StoredInFacility);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDeriveQuantity()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Session).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            shipmentReceipt.QuantityAccepted = 2;
            this.Session.Derive(false);

            Assert.Equal(2, shipmentItem.Quantity);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityRejectedDeriveQuantity()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Session).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            shipmentReceipt.QuantityRejected = 2;
            this.Session.Derive(false);

            Assert.Equal(2, shipmentItem.Quantity);
        }

        [Fact]
        public void ChangedUnitPurchasePriceThrowValidationError()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session)
                .WithPart(new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build())
                .WithUnitPurchasePrice(1)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipmentItem.RemoveUnitPurchasePrice();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: ShipmentItem.UnitPurchasePrice"));
        }

        [Fact]
        public void ChangedPartThrowValidationError()
        {
            var shipment = new PurchaseShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipmentItem.Part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: ShipmentItem.UnitPurchasePrice"));
        }
    }

    public class ShipmentItemStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveShipmentItemStatePicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Session).Picked;
            this.Session.Derive(false);

            Assert.True(shipmentItem.ShipmentItemState.IsPicked);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveShipmentItemStatePacked()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Session).Packed;
            this.Session.Derive(false);

            Assert.True(shipmentItem.ShipmentItemState.IsPacked);
        }
    }

    [Trait("Category", "Security")]
    public class ShipmentItemSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenShipmentItem_WhenProcessed_ThenDeleteIsNotAllowed()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Session).FindBy(this.M.Good.Name, "good1");
            new InventoryItemTransactionBuilder(this.Session).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Session).PhysicalCount).WithPart(good1.Part).Build();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var customer = new PersonBuilder(this.Session).WithLastName("customer").Build();

            var shipment = new CustomerShipmentBuilder(this.Session)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Session).Ground)
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithGood(good1).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);

            this.Session.Derive();

            shipment.Pick();
            this.Session.Derive();

            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Session).Picking, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));

            var pickList = shipment.ShipmentItems[0].ItemIssuancesWhereShipmentItem[0].PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Session.Derive();

            acl = new DatabaseAccessControlLists(this.Session.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Session).Picked, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem item in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Session).WithShipmentItem(item).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Session.Derive();

            shipment.SetPacked();
            this.Session.Derive();

            acl = new DatabaseAccessControlLists(this.Session.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Session).Packed, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));

            shipment.Ship();
            this.Session.Derive();

            acl = new DatabaseAccessControlLists(this.Session.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Session).Shipped, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));
        }
    }
}
