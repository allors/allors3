// <copyright file="SerialisedItemTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Resources;
    using TestPopulation;
    using Xunit;
    using Revocation = Domain.Revocation;

    public class SerialisedItemTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSerializedItem_WhenAddingWithSameSerialNumber_ThenError()
        {
            var good = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var serialNumber = good.SerialisedItems.First().SerialNumber;

            var newItem = new SerialisedItemBuilder(this.Transaction).WithSerialNumber(serialNumber).Build();
            good.AddSerialisedItem(newItem);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SameSerialNumber));
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenAvailabilityIsSet()
        {
            var available = new SerialisedItemAvailabilities(this.Transaction).Available;
            var notAvailable = new SerialisedItemAvailabilities(this.Transaction).NotAvailable;

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            Assert.Equal(available, newItem.SerialisedItemAvailability);

            newItem.SerialisedItemAvailability = notAvailable;

            this.Transaction.Derive();

            Assert.Equal(notAvailable, newItem.SerialisedItemAvailability);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenSuppliedByDisplayNameIsSet()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).WithAssignedSuppliedBy(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal(supplier.DisplayName, newItem.SuppliedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenSuppliedByDisplayNameIsSetFromSupplierOffering()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();


            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithPart(unifiedGood)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(1)
                .Build();

            this.Transaction.Derive();

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            unifiedGood.AddSerialisedItem(newItem);

            this.Transaction.Derive();

            Assert.Equal(supplier.DisplayName, newItem.SuppliedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenOwnedByDisplayNameIsSet()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.OwnedBy = customer;

            this.Transaction.Derive();

            Assert.Equal(customer.DisplayName, newItem.OwnedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenRentedByDisplayNameIsSet()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.RentedBy = customer;

            this.Transaction.Derive();

            Assert.Equal(customer.DisplayName, newItem.RentedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenOwnershipByOwnershipNameIsSet()
        {
            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.Ownership = new Ownerships(this.Transaction).Own;

            this.Transaction.Derive();

            Assert.Equal(newItem.Ownership.Name, newItem.OwnershipName);
        }
    }

    public class SerialisedItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAcquisitionYearThrowValidation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAcquiredDate(this.Transaction.Now()).Build();
            this.Derive();

            serialisedItem.AcquisitionYear = 2020;

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SerialisedItem.AcquiredDate,
                this.M.SerialisedItem.AcquisitionYear,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedAcquiredDateThrowValidation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAcquisitionYear(2020).Build();
            this.Derive();

            serialisedItem.AcquiredDate = this.Transaction.Now();

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SerialisedItem.AcquiredDate,
                this.M.SerialisedItem.AcquisitionYear,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveDisplayName()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Contains("partname", serialisedItem.DisplayName);
        }

        [Fact]
        public void ChangedAssignedSuppliedByDeriveSuppliedBy()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Derive();

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveSuppliedBy()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(supplier).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSuppliedBy()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var supplier = new OrganisationBuilder(this.Transaction).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(supplier).Build();
            this.Derive();

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedAssignedSuppliedByDeriveSuppliedByPartyName()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Derive();

            Assert.Equal("supplier", serialisedItem.SuppliedByPartyName);
        }

        [Fact]
        public void ChangedOwnedByDeriveOwnedByPartyName()
        {
            var owner = new OrganisationBuilder(this.Transaction).WithName("owner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.OwnedBy = owner;
            this.Derive();

            Assert.Equal("owner", serialisedItem.OwnedByPartyName);
        }

        [Fact]
        public void ChangedRentedByDeriveRentedByPartyName()
        {
            var rentner = new OrganisationBuilder(this.Transaction).WithName("rentner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.RentedBy = rentner;
            this.Derive();

            Assert.Equal("rentner", serialisedItem.RentedByPartyName);
        }

        [Fact]
        public void ChangedOwnershipDeriveOwnershipByOwnershipName()
        {
            var own = new Ownerships(this.Transaction).Own;
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Ownership = own;
            this.Derive();

            Assert.Equal(own.Name, serialisedItem.OwnershipName);
        }

        [Fact]
        public void ChangedOwnershipDeriveSerialisedItemAvailabilityName()
        {
            var availability = new SerialisedItemAvailabilities(this.Transaction).Available;
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.SerialisedItemAvailability = availability;
            this.Derive();

            Assert.Equal(availability.Name, serialisedItem.SerialisedItemAvailabilityName);
        }

        [Fact]
        public void ChangedSerialNumberThrowValidationError()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem1 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").Build();
            var serialisedItem2 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("2").Build();
            part.AddSerialisedItem(serialisedItem1);
            part.AddSerialisedItem(serialisedItem2);
            this.Derive();

            serialisedItem2.SerialNumber = "1";

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SameSerialNumber));
        }

        [Fact]
        public void ChangedQuoteItemSerialisedItemDeriveOnQuote()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            new QuoteItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            Assert.True(serialisedItem.OnQuote);
        }

        [Fact]
        public void ChangedQuoteItemQuoteItemStateDeriveOnQuote()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            Assert.True(serialisedItem.OnQuote);

            quoteItem.QuoteItemState = new QuoteItemStates(this.Transaction).Cancelled;
            this.Derive();

            Assert.False(serialisedItem.OnQuote);
        }

        [Fact]
        public void ChangedSalesOrderItemSerialisedItemDeriveOnSalesOrder()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            new SalesOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            Assert.True(serialisedItem.OnSalesOrder);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveOnSalesOrder()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            Assert.True(serialisedItem.OnSalesOrder);

            salesOrderItem.SalesOrderItemState = new SalesOrderItemStates(this.Transaction).Cancelled;
            this.Derive();

            Assert.False(serialisedItem.OnSalesOrder);
        }

        [Fact]
        public void ChangedWorkEffortFixedAssetAssignmentsFixedAssetDeriveOnWorkEffort()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithAssignment(workEffort).WithFixedAsset(serialisedItem).Build();
            this.Derive();

            Assert.True(serialisedItem.OnWorkEffort);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveOnWorkEffort()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithAssignment(workEffort).WithFixedAsset(serialisedItem).Build();
            this.Derive();

            Assert.True(serialisedItem.OnWorkEffort);

            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction).Cancelled;
            this.Derive();

            Assert.False(serialisedItem.OnWorkEffort);
        }

        [Fact]
        public void ChangedProductTypeDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();
            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Empty(serialisedItem.SerialisedItemCharacteristics);

            part.ProductType = productType;
            this.Derive();

            Assert.Equal(characteristicType, serialisedItem.SerialisedItemCharacteristics.First().SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedProductTypeSerialisedItemCharacteristicTypesDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithProductType(productType).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Empty(serialisedItem.SerialisedItemCharacteristics);

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Derive();

            Assert.Equal(characteristicType, serialisedItem.SerialisedItemCharacteristics.First().SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedDisplayNameDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("partName").WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            Assert.Contains("partName", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedSerialNumberDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.SerialNumber = "number";
            this.Derive();

            Assert.Contains("number", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedItemNumberDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(serialisedItem.ItemNumber, serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedOwnedByDeriveSearchString()
        {
            var owner = new OrganisationBuilder(this.Transaction).WithName("owner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.OwnedBy = owner;
            this.Derive();

            Assert.Contains("owner", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedBuyerDeriveSearchString()
        {
            var buyer = new OrganisationBuilder(this.Transaction).WithName("buyer").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Buyer = buyer;
            this.Derive();

            Assert.Contains("buyer", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedSellerDeriveSearchString()
        {
            var seller = new OrganisationBuilder(this.Transaction).WithName("seller").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Seller = seller;
            this.Derive();

            Assert.Contains("seller", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedPartBrandDeriveSearchString()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.Brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            this.Derive();

            Assert.Contains("brand", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedBrandNameDeriveSearchString()
        {
            var brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            brand.Name = "changedBrandName";
            this.Derive();

            Assert.Contains("changedBrandName", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedPartModelDeriveSearchString()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.Model = new ModelBuilder(this.Transaction).WithName("model").Build();
            this.Derive();

            Assert.Contains("model", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedModelNameDeriveSearchString()
        {
            var model = new ModelBuilder(this.Transaction).WithName("model").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithModel(model).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            model.Name = "changedModelName";
            this.Derive();

            Assert.Contains("changedModelName", serialisedItem.SearchString);
        }
    }

    public class SerialisedItemDisplayProductCategoriesRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDisplayProductCategoriesRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveDisplayProductCategories()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            new ProductCategoryBuilder(this.Transaction).WithName("catname").WithProduct(part).Build();
            this.Derive();

            Assert.Contains("catname", serialisedItem.ProductCategoriesDisplayName);
        }
    }

    public class SerialisedItemOwnerDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemOwnerDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveOwnedBy()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).CustomerShipmentShip);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipToParty = new PersonBuilder(this.Transaction).Build();
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(shipToParty).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithSerialisedItem(serialisedItem)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Derive();

            Assert.Equal(shipToParty, serialisedItem.OwnedBy);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveOwnership()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).CustomerShipmentShip);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipToParty = new PersonBuilder(this.Transaction).Build();
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(shipToParty).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithSerialisedItem(serialisedItem)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Derive();

            Assert.Equal(new Ownerships(this.Transaction).ThirdParty, serialisedItem.Ownership);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveAvailableForSale()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).CustomerShipmentShip);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipToParty = new PersonBuilder(this.Transaction).Build();
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(shipToParty).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Derive();

            Assert.False(serialisedItem.AvailableForSale);
        }
    }

    public class SerialisedItemPurchaseInvoiceDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseInvoiceDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);
        }

        [Fact]
        public void ChangedPurchaseInvoiceValidInvoiceItemsDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);

            invoiceItem.CancelFromInvoice();
            this.Derive();

            Assert.False(serialisedItem.ExistPurchaseInvoice);
        }
    }

    public class SerialisedItemPurchasePriceDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchasePriceDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceDeriveDerivedPurchasePrice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .WithAssignedUnitPrice(1)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            Assert.Equal(1, serialisedItem.DerivedPurchasePrice);
        }
    }

    public class SerialisedItemPurchaseOrderDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseOrderDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            order.Send();
            this.Derive();

            Assert.Equal(order, serialisedItem.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderValidOrderItemsDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            order.Send();
            this.Derive();

            Assert.Equal(order, serialisedItem.PurchaseOrder);

            orderItem.Cancel();
            this.Derive();

            Assert.False(serialisedItem.ExistPurchaseOrder);
        }
    }

    public class SerialisedItemBuyerNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemBuyerNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBuyerDeriveBuyerName()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Buyer = this.InternalOrganisation;
            this.Derive();

            Assert.Equal(this.InternalOrganisation.DisplayName, serialisedItem.BuyerName);
        }

        [Fact]
        public void ChangedBuyerDisplayNameDeriveBuyerName()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithBuyer(this.InternalOrganisation).Build();
            this.Derive();

            this.InternalOrganisation.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.BuyerName);
        }
    }

    public class SerialisedItemSellerNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemSellerNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSellerDeriveSellerName()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Seller = this.InternalOrganisation;
            this.Derive();

            Assert.Equal(this.InternalOrganisation.DisplayName, serialisedItem.SellerName);
        }

        [Fact]
        public void ChangedSellerDisplayNameDeriveSellerName()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSeller(this.InternalOrganisation).Build();
            this.Derive();

            this.InternalOrganisation.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.SellerName);
        }
    }

    public class SerialisedItemPurchaseInvoiceNumberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseInvoiceNumberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoiceNumber()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            Assert.Equal(invoice.InvoiceNumber, serialisedItem.PurchaseInvoiceNumber);
        }

        [Fact]
        public void ChangedPurchaseInvoiceValidInvoiceItemsDerivePurchaseInvoiceNumber()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);

            invoiceItem.CancelFromInvoice();
            this.Derive();

            Assert.False(serialisedItem.ExistPurchaseInvoiceNumber);
        }
    }

    public class SerialisedItemPurchaseOrderNumberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseOrderNumberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrderNumber()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            order.Send();
            this.Derive();

            Assert.Equal(order.OrderNumber, serialisedItem.PurchaseOrderNumber);
        }

        [Fact]
        public void ChangedPurchaseOrderValidOrderItemsDerivePurchaseOrderNumber()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            order.Send();
            this.Derive();

            Assert.Equal(order, serialisedItem.PurchaseOrder);

            orderItem.Cancel();
            this.Derive();

            Assert.False(serialisedItem.ExistPurchaseOrderNumber);
        }
    }

    public class SerialisedItemPartNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPartNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartSerialisedItemDerivePartName()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Equal("partname", serialisedItem.PartName);
        }

        [Fact]
        public void ChangedPartNameDerivePartName()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.PartName);
        }
    }

    public class SerialisedItemProductTypeNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemProductTypeNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartSerialisedItemDeriveProductTypeName()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Equal(productType.Name, serialisedItem.ProductTypeName);
        }

        [Fact]
        public void ChangedPartProductTypeDeriveProductTypeName()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.ProductType = new ProductTypeBuilder(this.Transaction).WithName("changed").Build();
            this.Derive();

            Assert.Equal("changed", serialisedItem.ProductTypeName);
        }

        [Fact]
        public void ChangedProductTypeNameDeriveProductTypeName()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            productType.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.ProductTypeName);
        }
    }

    public class SerialisedItemBrandNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemBrandNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartSerialisedItemDeriveBrandName()
        {
            var brand = new BrandBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Equal(brand.Name, serialisedItem.BrandName);
        }

        [Fact]
        public void ChangedPartBrandDeriveBrandName()
        {
            var brand = new BrandBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.Brand = new BrandBuilder(this.Transaction).WithName("changed").Build();
            this.Derive();

            Assert.Equal("changed", serialisedItem.BrandName);
        }

        [Fact]
        public void ChangedBrandNameDeriveBrandName()
        {
            var brand = new BrandBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            brand.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.BrandName);
        }
    }

    public class SerialisedItemModelNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemModelNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartSerialisedItemDeriveModelName()
        {
            var model = new ModelBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithModel(model).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Equal(model.Name, serialisedItem.ModelName);
        }

        [Fact]
        public void ChangedPartModelDeriveModelName()
        {
            var model = new ModelBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithModel(model).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.Model = new ModelBuilder(this.Transaction).WithName("changed").Build();
            this.Derive();

            Assert.Equal("changed", serialisedItem.ModelName);
        }

        [Fact]
        public void ChangedModelNameDeriveModelName()
        {
            var model = new ModelBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithModel(model).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            model.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.ModelName);
        }
    }

    public class SerialisedItemManufacturedByNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemManufacturedByNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartSerialisedItemDeriveManufacturedByName()
        {
            var manufacturer = new OrganisationBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithManufacturedBy(manufacturer).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Equal(manufacturer.Name, serialisedItem.ManufacturedByName);
        }

        [Fact]
        public void ChangedPartManufacturedByDeriveManufacturedByName()
        {
            var manufacturer = new OrganisationBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithManufacturedBy(manufacturer).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            part.ManufacturedBy = new OrganisationBuilder(this.Transaction).WithName("changed").Build();
            this.Derive();

            Assert.Equal("changed", serialisedItem.ManufacturedByName);
        }

        [Fact]
        public void ChangedManufacturedByNameDeriveManufacturedByName()
        {
            var manufacturer = new OrganisationBuilder(this.Transaction).WithName("name").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithManufacturedBy(manufacturer).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Derive();

            manufacturer.Name = "changed";
            this.Derive();

            Assert.Equal("changed", serialisedItem.ManufacturedByName);
        }
    }

    [Trait("Category", "Security")]
    public class SerialisedItemDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).SerialisedItemDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangeSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangeInventoryItemTransactionSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).Build();
            this.Derive();

            inventoryItemTransaction.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangePurchaseInvoiceItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoiceItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangePurchaseOrderItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            purchaseOrderItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangeQuoteItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            this.Derive();

            quoteItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangeSalesInvoiceItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoiceItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangeSalesOrderItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            salesOrderItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangeSerialisedInventoryItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedInventoryItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }

        [Fact]
        public void OnChangeShipmentItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Derive();

            shipmentItem.SerialisedItem = serialisedItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, serialisedItem.Revocations);
        }
    }
}
