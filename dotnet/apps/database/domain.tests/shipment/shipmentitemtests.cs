// <copyright file="ShipmentItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Resources;
    using Xunit;
    using ShipmentItem = Domain.ShipmentItem;
    using User = Domain.User;

    public class ShipmentItemTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseShipmentItemForNonSerialisedNotFromPurchaseOrder_WhenDerived_ThenUnitPurchasePriceIsRequired()
        {
            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithShipFromParty(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithCreationDate(this.Transaction.Now())
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithPart(good1.Part).WithQuantity(1).Build();
            shipment.AddShipmentItem(shipmentItem);

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Equal(new IRoleType[]
            {
                this.M.ShipmentItem.UnitPurchasePrice,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }

    public class ShipmentItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNextSerialisedItemAvailabilityThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .Build();
            shipment.AddShipmentItem(shipmentItem);

            this.Derive();

            shipmentItem.RemoveNextSerialisedItemAvailability();

            {
                var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
                Assert.Equal(new IRoleType[]
                {
                    this.M.ShipmentItem.NextSerialisedItemAvailability,
                }, errors.SelectMany(v => v.RoleTypes).Distinct());
            }
        }

        [Fact]
        public void ChangedSerialisedThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);

            this.Derive();

            shipmentItem.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            {
                var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
                Assert.Contains(this.M.ShipmentItem.NextSerialisedItemAvailability, errors.SelectMany(v => v.RoleTypes));
            }
        }

        [Fact]
        public void ChangedQuantityThrowValidationError()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipmentItem.Quantity = 2;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }

        [Fact]
        public void ChangedSerialisedThrowValidationError_2()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithQuantity(2).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipmentItem.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }

        [Fact]
        public void ChangedItemIssuanceQuantityDeriveQuantityPicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Derive();

            Assert.Equal(10, shipmentItem.QuantityPicked);
        }

        [Fact]
        public void ChangedPickListPickListStateDeriveQuantityPicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Derive();

            Assert.Equal(0, shipmentItem.QuantityPicked);

            pickList.PickListState = new PickListStates(this.Transaction).Picked;
            this.Derive();

            Assert.Equal(10, shipmentItem.QuantityPicked);
        }

        [Fact]
        public void ChangedItemIssuanceQuantityDeriveQuantityShipped()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipmentState(new ShipmentStates(this.Transaction).Shipped)
                .Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Derive();

            Assert.Equal(10, shipmentItem.QuantityShipped);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveQuantityShipped()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).WithQuantity(10).Build();
            this.Derive();

            Assert.Equal(0, shipmentItem.QuantityShipped);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Derive();

            Assert.Equal(10, shipmentItem.QuantityShipped);
        }

        [Fact]
        public void ChangedStoredInFacilityDeriveStoredInFacility()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipmentItem.RemoveStoredInFacility();
            this.Derive();

            Assert.Equal(shipment.ShipToFacility, shipmentItem.StoredInFacility);
        }

        [Fact]
        public void ChangedShipmentShipToFacilityDeriveStoredInFacility()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            shipment.RemoveShipToFacility();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipment.ShipToFacility = this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault();
            this.Derive();

            Assert.Equal(shipment.ShipToFacility, shipmentItem.StoredInFacility);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDeriveQuantity()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            shipmentReceipt.QuantityAccepted = 2;
            this.Derive();

            Assert.Equal(2, shipmentItem.Quantity);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityRejectedDeriveQuantity()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Transaction).WithShipmentItem(shipmentItem).Build();
            this.Derive();

            shipmentReceipt.QuantityRejected = 2;
            this.Derive();

            Assert.Equal(2, shipmentItem.Quantity);
        }

        [Fact]
        public void ChangedUnitPurchasePriceThrowValidationError()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithPart(new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .WithUnitPurchasePrice(1)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipmentItem.RemoveUnitPurchasePrice();

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Equal(new IRoleType[]
            {
                this.M.ShipmentItem.UnitPurchasePrice,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedPartThrowValidationError()
        {
            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipmentItem.Part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Contains(this.M.ShipmentItem.UnitPurchasePrice, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }

    public class ShipmentItemStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentItemStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveShipmentItemStatePicked()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Picked;
            this.Derive();

            Assert.True(shipmentItem.ShipmentItemState.IsPicked);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveShipmentItemStatePacked()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Packed;
            this.Derive();

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
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).PhysicalCount).WithPart(good1.Part).Build();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipToParty(customer)
                .WithShipToAddress(shipToAddress)
                .WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithGood(good1).WithQuantity(10).Build();
            shipment.AddShipmentItem(shipmentItem);

            this.Transaction.Derive();

            shipment.Pick();
            this.Transaction.Derive();

            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Transaction).Picking, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));

            var pickList = shipment.ShipmentItems.ElementAt(0).ItemIssuancesWhereShipmentItem.ElementAt(0).PickListItem.PickListWherePickListItem;
            pickList.Picker = this.OrderProcessor;

            pickList.SetPicked();
            this.Transaction.Derive();

            acl = new DatabaseAccessControl(this.Transaction.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Transaction).Picked, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);

            foreach (ShipmentItem item in shipment.ShipmentItems)
            {
                package.AddPackagingContent(new PackagingContentBuilder(this.Transaction).WithShipmentItem(item).WithQuantity(shipmentItem.Quantity).Build());
            }

            this.Transaction.Derive();

            shipment.SetPacked();
            this.Transaction.Derive();

            acl = new DatabaseAccessControl(this.Transaction.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Transaction).Packed, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));

            shipment.Ship();
            this.Transaction.Derive();

            acl = new DatabaseAccessControl(this.Transaction.GetUser())[shipmentItem];
            Assert.Equal(new ShipmentItemStates(this.Transaction).Shipped, shipmentItem.ShipmentItemState);
            Assert.False(acl.CanExecute(this.M.ShipmentItem.Delete));
        }
    }
}
