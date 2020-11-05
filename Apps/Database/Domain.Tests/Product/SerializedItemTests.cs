// <copyright file="SerialisedItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;
    using System.Linq;
    using Allors.Domain.TestPopulation;
    using Resources;
    using System.Collections.Generic;

    public class SerialisedItemTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSerializedItem_WhenAddingWithSameSerialNumber_ThenError()
        {
            var good = new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var serialNumber = good.SerialisedItems.First.SerialNumber;

            var newItem = new SerialisedItemBuilder(this.Session).WithSerialNumber(serialNumber).Build();
            good.AddSerialisedItem(newItem);

            var expectedErrorMessage = ErrorMessages.SameSerialNumber;

            var errors = new List<string>(this.Session.Derive(false).Errors.Select(v => v.Message));
            Assert.Contains(expectedErrorMessage, errors);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenAvailabilityIsSet()
        {
            var available = new SerialisedItemAvailabilities(this.Session).Available;
            var notAvailable = new SerialisedItemAvailabilities(this.Session).NotAvailable;

            var newItem = new SerialisedItemBuilder(this.Session).WithForSaleDefaults(this.InternalOrganisation).Build();

            this.Session.Derive();

            Assert.Equal(available, newItem.SerialisedItemAvailability);

            newItem.SerialisedItemAvailability = notAvailable;

            this.Session.Derive();

            Assert.Equal(notAvailable, newItem.SerialisedItemAvailability);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenSuppliedByPartyNameIsSet()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var newItem = new SerialisedItemBuilder(this.Session).WithForSaleDefaults(this.InternalOrganisation).WithAssignedSuppliedBy(supplier).Build();

            this.Session.Derive();

            Assert.Equal(supplier.PartyName, newItem.SuppliedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenSuppliedByPartyNameIsSetFromSupplierOffering()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var unifiedGood = new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Session.Derive();


            new SupplierOfferingBuilder(this.Session)
                .WithSupplier(supplier)
                .WithPart(unifiedGood)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithPrice(1)
                .Build();

            this.Session.Derive();

            var newItem = new SerialisedItemBuilder(this.Session).WithForSaleDefaults(this.InternalOrganisation).Build();
            unifiedGood.AddSerialisedItem(newItem);

            this.Session.Derive();

            Assert.Equal(supplier.PartyName, newItem.SuppliedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenOwnedByPartyNameIsSet()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var newItem = new SerialisedItemBuilder(this.Session).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.OwnedBy = customer;

            this.Session.Derive();

            Assert.Equal(customer.PartyName, newItem.OwnedByPartyName);
        }


        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenRentedByPartyNameIsSet()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var newItem = new SerialisedItemBuilder(this.Session).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.RentedBy = customer;

            this.Session.Derive();

            Assert.Equal(customer.PartyName, newItem.RentedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenOwnershipByOwnershipNameIsSet()
        {
            var newItem = new SerialisedItemBuilder(this.Session).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.Ownership = new Ownerships(this.Session).Own;

            this.Session.Derive();

            Assert.Equal(newItem.Ownership.Name, newItem.OwnershipByOwnershipName);
        }
    }

    [Trait("Category", "Security")]
    public class SerialisedItemDeniedPermissionTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDeniedPermissionTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.SerialisedItem.ObjectType, this.M.SerialisedItem.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;


        [Fact]
        public void OnChangeSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithInventoryItemTransactionDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session)
                .WithReason(new InventoryTransactionReasonBuilder(this.Session).Build())
                .WithPart(new NonUnifiedPartBuilder(this.Session).Build())
                .WithSerialisedItem(serialisedItem).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithPurchaseInvoiceItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseInvoiceItem = new PurchaseInvoiceItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithPurchaseOrderItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithAssignedUnitPrice(1)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithSerialisedItem(serialisedItem).Build();

            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithQuoteItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quote = new ProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithSerialisedItem(serialisedItem).Build();

            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithSalesInvoiceItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();

            salesInvoice.AddSalesInvoiceItem(salesInvoiceItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithSalesOrderItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();

            salesOrder.AddSalesOrderItem(salesOrderItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithSerialisedInventoryItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Session)
                .WithPart(new NonUnifiedPartBuilder(this.Session).Build())
                .WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemWithShipmentItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new TransferBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();

            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }
    }
}
