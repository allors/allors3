// <copyright file="SerialisedItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;
    using System.Linq;
    using TestPopulation;
    using Resources;
    using System.Collections.Generic;
    using Allors.Database.Derivations;

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

            var expectedMessage = $"{newItem} { this.M.SerialisedItem.SerialNumber} { ErrorMessages.SameSerialNumber}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
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

    public class SerialisedItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAcquisitionYearThrowValidation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).WithAcquiredDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            serialisedItem.AcquisitionYear = 2020;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SerialisedItem.AcquiredDate\nSerialisedItem.AcquisitionYear"));
        }

        [Fact]
        public void ChangedAcquiredDateThrowValidation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).WithAcquisitionYear(2020).Build();
            this.Session.Derive(false);

            serialisedItem.AcquiredDate = this.Session.Now();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: SerialisedItem.AcquiredDate\nSerialisedItem.AcquisitionYear"));
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveName()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            Assert.Equal("partname", serialisedItem.Name);
        }

        [Fact]
        public void ChangedAssignedSuppliedByDeriveSuppliedBy()
        {
            var supplier = new OrganisationBuilder(this.Session).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Session.Derive(false);

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveSuppliedBy()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var supplier = new OrganisationBuilder(this.Session).Build();
            new SupplierOfferingBuilder(this.Session).WithPart(part).WithSupplier(supplier).Build(); 

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSuppliedBy()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var supplier = new OrganisationBuilder(this.Session).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            new SupplierOfferingBuilder(this.Session).WithPart(part).WithSupplier(supplier).Build();
            this.Session.Derive(false);

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedAssignedSuppliedByDeriveSuppliedByPartyName()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Session.Derive(false);

            Assert.Equal("supplier", serialisedItem.SuppliedByPartyName);
        }

        [Fact]
        public void ChangedOwnedByDeriveOwnedByPartyName()
        {
            var owner = new OrganisationBuilder(this.Session).WithName("owner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.OwnedBy = owner;
            this.Session.Derive(false);

            Assert.Equal("owner", serialisedItem.OwnedByPartyName);
        }

        [Fact]
        public void ChangedRentedByDeriveRentedByPartyName()
        {
            var rentner = new OrganisationBuilder(this.Session).WithName("rentner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.RentedBy = rentner;
            this.Session.Derive(false);

            Assert.Equal("rentner", serialisedItem.RentedByPartyName);
        }

        [Fact]
        public void ChangedOwnershipDeriveOwnershipByOwnershipName()
        {
            var own = new Ownerships(this.Session).Own;
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.Ownership = own;
            this.Session.Derive(false);

            Assert.Equal(own.Name, serialisedItem.OwnershipByOwnershipName);
        }

        [Fact]
        public void ChangedOwnershipDeriveSerialisedItemAvailabilityName()
        {
            var availability = new SerialisedItemAvailabilities(this.Session).Available;
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.SerialisedItemAvailability = availability;
            this.Session.Derive(false);

            Assert.Equal(availability.Name, serialisedItem.SerialisedItemAvailabilityName);
        }

        [Fact]
        public void ChangedSerialNumberThrowValidationError()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).Build();
            var serialisedItem1 = new SerialisedItemBuilder(this.Session).WithSerialNumber("1").Build();
            var serialisedItem2 = new SerialisedItemBuilder(this.Session).WithSerialNumber("2").Build();
            part.AddSerialisedItem(serialisedItem1);
            part.AddSerialisedItem(serialisedItem2);
            this.Session.Derive(false);

            serialisedItem2.SerialNumber = "1";

            var expectedMessage = $"{serialisedItem2} { this.M.SerialisedItem.SerialNumber} { ErrorMessages.SameSerialNumber}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuoteItemSerialisedItemDeriveOnQuote()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            new QuoteItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.True(serialisedItem.OnQuote);
        }

        [Fact]
        public void ChangedQuoteItemQuoteItemStateDeriveOnQuote()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.True(serialisedItem.OnQuote);

            quoteItem.QuoteItemState = new QuoteItemStates(this.Session).Cancelled;
            this.Session.Derive(false);

            Assert.False(serialisedItem.OnQuote);
        }

        [Fact]
        public void ChangedSalesOrderItemSerialisedItemDeriveOnSalesOrder()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SalesOrderItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.True(serialisedItem.OnSalesOrder);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveOnSalesOrder()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Session).WithSerialisedItem(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.True(serialisedItem.OnSalesOrder);

            salesOrderItem.SalesOrderItemState = new SalesOrderItemStates(this.Session).Cancelled;
            this.Session.Derive(false);

            Assert.False(serialisedItem.OnSalesOrder);
        }

        [Fact]
        public void ChangedWorkEffortFixedAssetAssignmentsFixedAssetDeriveOnWorkEffort()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Session).WithAssignment(workEffort).WithFixedAsset(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.True(serialisedItem.OnWorkEffort);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveOnWorkEffort()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Session).WithAssignment(workEffort).WithFixedAsset(serialisedItem).Build();
            this.Session.Derive(false);

            Assert.True(serialisedItem.OnWorkEffort);

            workEffort.WorkEffortState = new WorkEffortStates(this.Session).Cancelled;
            this.Session.Derive(false);

            Assert.False(serialisedItem.OnWorkEffort);
        }

        [Fact]
        public void ChangedProductTypeDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Session).Build();
            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Session).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            Assert.Empty(serialisedItem.SerialisedItemCharacteristics);

            part.ProductType = productType;
            this.Session.Derive(false);

            Assert.Equal(characteristicType, serialisedItem.SerialisedItemCharacteristics.First.SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedProductTypeSerialisedItemCharacteristicTypesDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Session).Build();

            var part = new NonUnifiedPartBuilder(this.Session).WithProductType(productType).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            Assert.Empty(serialisedItem.SerialisedItemCharacteristics);

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Session).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Session.Derive(false);

            Assert.Equal(characteristicType, serialisedItem.SerialisedItemCharacteristics.First.SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedNameDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.Name = "name";
            this.Session.Derive(false);

            Assert.Contains("name", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedSerialNumberDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.SerialNumber = "number";
            this.Session.Derive(false);

            Assert.Contains("number", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedItemNumberDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(serialisedItem.ItemNumber, serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedOwnedByDeriveSearchString()
        {
            var owner = new OrganisationBuilder(this.Session).WithName("owner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.OwnedBy = owner;
            this.Session.Derive(false);

            Assert.Contains("owner", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedBuyerDeriveSearchString()
        {
            var buyer = new OrganisationBuilder(this.Session).WithName("buyer").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.Buyer = buyer;
            this.Session.Derive(false);

            Assert.Contains("buyer", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedSellerDeriveSearchString()
        {
            var seller = new OrganisationBuilder(this.Session).WithName("seller").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            serialisedItem.Seller = seller;
            this.Session.Derive(false);

            Assert.Contains("seller", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedPartBrandDeriveSearchString()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            part.Brand = new BrandBuilder(this.Session).WithName("brand").Build();
            this.Session.Derive(false);

            Assert.Contains("brand", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedBrandNameDeriveSearchString()
        {
            var brand = new BrandBuilder(this.Session).WithName("brand").Build();
            var part = new UnifiedGoodBuilder(this.Session).WithBrand(brand).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            brand.Name = "changedBrandName";
            this.Session.Derive(false);

            Assert.Contains("changedBrandName", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedPartModelDeriveSearchString()
        {
            var part = new UnifiedGoodBuilder(this.Session).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            part.Model = new ModelBuilder(this.Session).WithName("model").Build();
            this.Session.Derive(false);

            Assert.Contains("model", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedModelNameDeriveSearchString()
        {
            var model = new ModelBuilder(this.Session).WithName("model").Build();
            var part = new UnifiedGoodBuilder(this.Session).WithModel(model).WithInventoryItemKind(new InventoryItemKinds(this.Session).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            model.Name = "changedModelName";
            this.Session.Derive(false);

            Assert.Contains("changedModelName", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveDisplayProductCategories()
        {
            var part = new UnifiedGoodBuilder(this.Session).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            new ProductCategoryBuilder(this.Session).WithName("catname").WithProduct(part).Build();
            this.Session.Derive(false);

            Assert.Contains("catname", serialisedItem.DisplayProductCategories);
        }
    }

    public class SerialisedItemPurchaseInvoiceDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseInvoiceDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Session.Derive(false);

            invoice.Confirm();
            this.Session.Derive(false);

            invoice.Approve();
            this.Session.Derive(false);

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);
        }

        [Fact]
        public void ChangedPurchaseInvoiceValidInvoiceItemsDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Session.Derive(false);

            invoice.Confirm();
            this.Session.Derive(false);

            invoice.Approve();
            this.Session.Derive(false);

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);

            invoiceItem.CancelFromInvoice();
            this.Session.Derive(false);

            Assert.False(serialisedItem.ExistPurchaseInvoice);
        }
    }

    public class SerialisedItemPurchasePriceDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchasePriceDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceDerivePurchasePrice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .WithAssignedUnitPrice(1)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Session.Derive(false);

            invoice.Confirm();
            this.Session.Derive(false);

            invoice.Approve();
            this.Session.Derive(false);

            Assert.Equal(1, serialisedItem.PurchasePrice);
        }
    }

    public class SerialisedItemPurchaseOrderDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseOrderDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.SetReadyForProcessing();
            this.Session.Derive(false);

            order.Send();
            this.Session.Derive(false);

            Assert.Equal(order, serialisedItem.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderValidOrderItemsDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.SetReadyForProcessing();
            this.Session.Derive(false);

            order.Send();
            this.Session.Derive(false);

            Assert.Equal(order, serialisedItem.PurchaseOrder);

            orderItem.Cancel();
            this.Session.Derive(false);

            Assert.False(serialisedItem.ExistPurchaseOrder);
        }
    }

    [Trait("Category", "Security")]
    public class SerialisedItemDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.SerialisedItem.ObjectType, this.M.SerialisedItem.Delete);

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
