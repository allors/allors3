// <copyright file="SerialisedItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;
    using TestPopulation;
    using Resources;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Permission = Domain.Permission;

    public class SerialisedItemTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSerializedItem_WhenAddingWithSameSerialNumber_ThenError()
        {
            var good = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var serialNumber = good.SerialisedItems.First.SerialNumber;

            var newItem = new SerialisedItemBuilder(this.Transaction).WithSerialNumber(serialNumber).Build();
            good.AddSerialisedItem(newItem);

            var expectedMessage = $"{newItem} { this.M.SerialisedItem.SerialNumber} { ErrorMessages.SameSerialNumber}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
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
        public void GivenSerializedItem_WhenDerived_ThenSuppliedByPartyNameIsSet()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).WithAssignedSuppliedBy(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal(supplier.PartyName, newItem.SuppliedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenSuppliedByPartyNameIsSetFromSupplierOffering()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

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

            Assert.Equal(supplier.PartyName, newItem.SuppliedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenOwnedByPartyNameIsSet()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.OwnedBy = customer;

            this.Transaction.Derive();

            Assert.Equal(customer.PartyName, newItem.OwnedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenRentedByPartyNameIsSet()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.RentedBy = customer;

            this.Transaction.Derive();

            Assert.Equal(customer.PartyName, newItem.RentedByPartyName);
        }

        [Fact]
        public void GivenSerializedItem_WhenDerived_ThenOwnershipByOwnershipNameIsSet()
        {
            var newItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.InternalOrganisation).Build();
            newItem.Ownership = new Ownerships(this.Transaction).Own;

            this.Transaction.Derive();

            Assert.Equal(newItem.Ownership.Name, newItem.OwnershipByOwnershipName);
        }
    }

    public class SerialisedItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAcquisitionYearThrowValidation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAcquiredDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            serialisedItem.AcquisitionYear = 2020;

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SerialisedItem.AcquiredDate,
                this.M.SerialisedItem.AcquisitionYear,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedAcquiredDateThrowValidation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAcquisitionYear(2020).Build();
            this.Transaction.Derive(false);

            serialisedItem.AcquiredDate = this.Transaction.Now();

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SerialisedItem.AcquiredDate,
                this.M.SerialisedItem.AcquisitionYear,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveName()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            Assert.Equal("partname", serialisedItem.Name);
        }

        [Fact]
        public void ChangedAssignedSuppliedByDeriveSuppliedBy()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Transaction.Derive(false);

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveSuppliedBy()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(supplier).Build(); 

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSuppliedBy()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var supplier = new OrganisationBuilder(this.Transaction).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(supplier).Build();
            this.Transaction.Derive(false);

            Assert.Equal(supplier, serialisedItem.SuppliedBy);
        }

        [Fact]
        public void ChangedAssignedSuppliedByDeriveSuppliedByPartyName()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Transaction.Derive(false);

            Assert.Equal("supplier", serialisedItem.SuppliedByPartyName);
        }

        [Fact]
        public void ChangedOwnedByDeriveOwnedByPartyName()
        {
            var owner = new OrganisationBuilder(this.Transaction).WithName("owner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.OwnedBy = owner;
            this.Transaction.Derive(false);

            Assert.Equal("owner", serialisedItem.OwnedByPartyName);
        }

        [Fact]
        public void ChangedRentedByDeriveRentedByPartyName()
        {
            var rentner = new OrganisationBuilder(this.Transaction).WithName("rentner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.RentedBy = rentner;
            this.Transaction.Derive(false);

            Assert.Equal("rentner", serialisedItem.RentedByPartyName);
        }

        [Fact]
        public void ChangedOwnershipDeriveOwnershipByOwnershipName()
        {
            var own = new Ownerships(this.Transaction).Own;
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.Ownership = own;
            this.Transaction.Derive(false);

            Assert.Equal(own.Name, serialisedItem.OwnershipByOwnershipName);
        }

        [Fact]
        public void ChangedOwnershipDeriveSerialisedItemAvailabilityName()
        {
            var availability = new SerialisedItemAvailabilities(this.Transaction).Available;
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.SerialisedItemAvailability = availability;
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            serialisedItem2.SerialNumber = "1";

            var expectedMessage = $"{serialisedItem2} { this.M.SerialisedItem.SerialNumber} { ErrorMessages.SameSerialNumber}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuoteItemSerialisedItemDeriveOnQuote()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new QuoteItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Transaction.Derive(false);

            Assert.True(serialisedItem.OnQuote);
        }

        [Fact]
        public void ChangedQuoteItemQuoteItemStateDeriveOnQuote()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Transaction.Derive(false);

            Assert.True(serialisedItem.OnQuote);

            quoteItem.QuoteItemState = new QuoteItemStates(this.Transaction).Cancelled;
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.OnQuote);
        }

        [Fact]
        public void ChangedSalesOrderItemSerialisedItemDeriveOnSalesOrder()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SalesOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Transaction.Derive(false);

            Assert.True(serialisedItem.OnSalesOrder);
        }

        [Fact]
        public void ChangedSalesOrderItemSalesOrderItemStateDeriveOnSalesOrder()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Transaction.Derive(false);

            Assert.True(serialisedItem.OnSalesOrder);

            salesOrderItem.SalesOrderItemState = new SalesOrderItemStates(this.Transaction).Cancelled;
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.OnSalesOrder);
        }

        [Fact]
        public void ChangedWorkEffortFixedAssetAssignmentsFixedAssetDeriveOnWorkEffort()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithAssignment(workEffort).WithFixedAsset(serialisedItem).Build();
            this.Transaction.Derive(false);

            Assert.True(serialisedItem.OnWorkEffort);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveOnWorkEffort()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithAssignment(workEffort).WithFixedAsset(serialisedItem).Build();
            this.Transaction.Derive(false);

            Assert.True(serialisedItem.OnWorkEffort);

            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction).Cancelled;
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.OnWorkEffort);
        }

        [Fact]
        public void ChangedProductTypeDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();
            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Transaction.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            Assert.Empty(serialisedItem.SerialisedItemCharacteristics);

            part.ProductType = productType;
            this.Transaction.Derive(false);

            Assert.Equal(characteristicType, serialisedItem.SerialisedItemCharacteristics.First.SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedProductTypeSerialisedItemCharacteristicTypesDeriveSerialisedItemCharacteristics()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithProductType(productType).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            Assert.Empty(serialisedItem.SerialisedItemCharacteristics);

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            productType.AddSerialisedItemCharacteristicType(characteristicType);
            this.Transaction.Derive(false);

            Assert.Equal(characteristicType, serialisedItem.SerialisedItemCharacteristics.First.SerialisedItemCharacteristicType);
        }

        [Fact]
        public void ChangedNameDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.Name = "name";
            this.Transaction.Derive(false);

            Assert.Contains("name", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedSerialNumberDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.SerialNumber = "number";
            this.Transaction.Derive(false);

            Assert.Contains("number", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedItemNumberDeriveSearchString()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem.ItemNumber, serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedOwnedByDeriveSearchString()
        {
            var owner = new OrganisationBuilder(this.Transaction).WithName("owner").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.OwnedBy = owner;
            this.Transaction.Derive(false);

            Assert.Contains("owner", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedBuyerDeriveSearchString()
        {
            var buyer = new OrganisationBuilder(this.Transaction).WithName("buyer").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.Buyer = buyer;
            this.Transaction.Derive(false);

            Assert.Contains("buyer", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedSellerDeriveSearchString()
        {
            var seller = new OrganisationBuilder(this.Transaction).WithName("seller").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.Seller = seller;
            this.Transaction.Derive(false);

            Assert.Contains("seller", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedPartBrandDeriveSearchString()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            part.Brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            this.Transaction.Derive(false);

            Assert.Contains("brand", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedBrandNameDeriveSearchString()
        {
            var brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            brand.Name = "changedBrandName";
            this.Transaction.Derive(false);

            Assert.Contains("changedBrandName", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedPartModelDeriveSearchString()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            part.Model = new ModelBuilder(this.Transaction).WithName("model").Build();
            this.Transaction.Derive(false);

            Assert.Contains("model", serialisedItem.SearchString);
        }

        [Fact]
        public void ChangedModelNameDeriveSearchString()
        {
            var model = new ModelBuilder(this.Transaction).WithName("model").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithModel(model).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithName("partname").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            model.Name = "changedModelName";
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            new ProductCategoryBuilder(this.Transaction).WithName("catname").WithProduct(part).Build();
            this.Transaction.Derive(false);

            Assert.Contains("catname", serialisedItem.DisplayProductCategories);
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
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithSerialisedItem(serialisedItem)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Transaction.Derive(false);

            Assert.Equal(shipToParty, serialisedItem.OwnedBy);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveOwnership()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).CustomerShipmentShip);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipToParty = new PersonBuilder(this.Transaction).Build();
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(shipToParty).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithSerialisedItem(serialisedItem)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Transaction.Derive(false);

            Assert.Equal(new Ownerships(this.Transaction).ThirdParty, serialisedItem.Ownership);
        }

        [Fact]
        public void ChangedShipmentShipmentStateDeriveAvailableForSale()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).CustomerShipmentShip);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipToParty = new PersonBuilder(this.Transaction).Build();
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipToParty(shipToParty).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive(false);

            shipment.ShipmentState = new ShipmentStates(this.Transaction).Shipped;
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);
        }

        [Fact]
        public void ChangedPurchaseInvoiceValidInvoiceItemsDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);

            invoiceItem.CancelFromInvoice();
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.ExistPurchaseInvoice);
        }
    }

    public class SerialisedItemPurchasePriceDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchasePriceDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceDerivePurchasePrice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .WithAssignedUnitPrice(1)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(1, serialisedItem.PurchasePrice);
        }
    }

    public class SerialisedItemPurchaseOrderDervivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseOrderDervivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.SetReadyForProcessing();
            this.Transaction.Derive(false);

            order.Send();
            this.Transaction.Derive(false);

            Assert.Equal(order, serialisedItem.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderValidOrderItemsDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.SetReadyForProcessing();
            this.Transaction.Derive(false);

            order.Send();
            this.Transaction.Derive(false);

            Assert.Equal(order, serialisedItem.PurchaseOrder);

            orderItem.Cancel();
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.ExistPurchaseOrder);
        }
    }

    [Trait("Category", "Security")]
    public class SerialisedItemDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.SerialisedItem, this.M.SerialisedItem.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangeSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeInventoryItemTransactionSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItemTransaction.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoiceItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseOrderItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseOrderItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeQuoteItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quoteItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoiceItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrderItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedInventoryItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedInventoryItem = new SerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedInventoryItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangeShipmentItemSerialisedItemDeriveDeletePermission()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Transaction.Derive(false);

            shipmentItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, serialisedItem.DeniedPermissions);
        }
    }
}
