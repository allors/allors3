// <copyright file="PurchaseReturnRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PurchaseReturnRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseReturnRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault();
            store.RemovePurchaseReturnNumberPrefix();
            var number = this.InternalOrganisation.StoresWhereInternalOrganisation.First().PurchaseReturnNumberCounter.Value;

            var shipment = new PurchaseReturnBuilder(this.Transaction).WithStore(store).Build();
            this.Derive();

            Assert.Equal(shipment.ShipmentNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableShipmentNumber()
        {
            var store = this.InternalOrganisation.StoresWhereInternalOrganisation.FirstOrDefault();
            var number = store.PurchaseReturnNumberCounter.Value;

            var shipment = new PurchaseReturnBuilder(this.Transaction).WithStore(store).Build();
            this.Derive();

            Assert.Equal(shipment.SortableShipmentNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedShipToPartyDeriveShipToAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First().ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipToAddressDeriveShipToAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .Build();
            this.Derive();

            shipment.RemoveShipToAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ActiveSuppliers.First().ShippingAddress, shipment.ShipToAddress);
        }

        [Fact]
        public void ChangedShipFromPartyDeriveShipFromAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }

        [Fact]
        public void ChangedShipFromAddressDeriveShipFromAddress()
        {
            var shipment = new PurchaseReturnBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();
            this.Derive();

            shipment.RemoveShipFromAddress();
            this.Derive();

            Assert.Equal(this.InternalOrganisation.ShippingAddress, shipment.ShipFromAddress);
        }
    }

    public class PurchaseReturnCanShipRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseReturnCanShipRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNonSerialisInventoryItemQuantityOnHandThrowValidationError()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;
            var supplier = this.InternalOrganisation.ActiveSuppliers.First();

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(part)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();

            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            order.Return();
            this.Transaction.Derive();

            var purchaseReturn = this.Transaction.Extent<PurchaseReturn>()[0];

            Assert.True(purchaseReturn.CanShip);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Theft).WithPart(part).Build();
            this.Transaction.Derive();

            Assert.False(purchaseReturn.CanShip);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseReturnDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseReturnDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.shipRevocation = new Revocations(this.Transaction).PurchaseReturnShipRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation shipRevocation;

        [Fact]
        public void ChangedPurchaseReturnTransitionalDeniedPermissionsDeriveShipPermissionAllowed()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;
            var supplier = this.InternalOrganisation.ActiveSuppliers.First();

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(part)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();

            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            order.Return();
            this.Transaction.Derive();

            var purchaseReturn = this.Transaction.Extent<PurchaseReturn>()[0];

            Assert.DoesNotContain(this.shipRevocation, purchaseReturn.Revocations);
        }

        [Fact]
        public void ChangedPurchaseReturnCanShipDeriveShipPermissionDenied()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;
            var supplier = this.InternalOrganisation.ActiveSuppliers.First();

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(part)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();

            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            order.Return();
            this.Transaction.Derive();

            var purchaseReturn = this.Transaction.Extent<PurchaseReturn>()[0];

            Assert.True(purchaseReturn.CanShip);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(1).WithReason(new InventoryTransactionReasons(this.Transaction).Theft).WithPart(part).Build();
            this.Transaction.Derive();

            Assert.Contains(this.shipRevocation, purchaseReturn.Revocations);
        }
    }
}
