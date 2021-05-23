// <copyright file="OrganisationExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.TestPopulation
{
    using Domain;
    using Bogus;
    using Person = Person;

    public static class OrganisationExtensions
    {
        public static Person CreateEmployee(this Organisation @this, string password, Faker faker)
        {
            var person = new PersonBuilder(@this.Transaction()).WithDefaults().Build();

            new EmploymentBuilder(@this.Transaction())
                .WithEmployee(person)
                .WithEmployer(@this)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            new OrganisationContactRelationshipBuilder(@this.Transaction())
                .WithContact(person)
                .WithOrganisation(@this)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            new UserGroups(@this.Transaction()).Creators.AddMember(person);
            new UserGroups(@this.Transaction()).Employees.AddMember(person);

            person.SetPassword(password);

            return person;
        }

        public static Person CreateAdministrator(this Organisation @this, string password, Faker faker)
        {
            var person = @this.CreateEmployee(password, faker);
            new UserGroups(@this.Transaction()).Administrators.AddMember(person);

            return person;
        }

        public static Organisation CreateB2BCustomer(this Organisation @this, Faker faker)
        {
            var customer = new OrganisationBuilder(@this.Transaction()).WithDefaults().Build();

            new CustomerRelationshipBuilder(@this.Transaction())
                .WithCustomer(customer)
                .WithInternalOrganisation(@this)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .WithAgreement(new SalesAgreementBuilder(@this.Transaction())
                                .WithDescription("PaymentNetDays")
                                .WithAgreementTerm(new InvoiceTermBuilder(@this.Transaction())
                                .WithTermType(new InvoiceTermTypes(@this.Transaction()).PaymentNetDays).WithTermValue("30").Build())
                                .Build())
                .Build();


            new OrganisationContactRelationshipBuilder(@this.Transaction())
                .WithContact(new PersonBuilder(@this.Transaction()).WithDefaults().Build())
                .WithOrganisation(customer)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            return customer;
        }

        public static Person CreateB2CCustomer(this Organisation @this, Faker faker)
        {
            var customer = new PersonBuilder(@this.Transaction()).WithDefaults().Build();

            new CustomerRelationshipBuilder(@this.Transaction())
                .WithCustomer(customer)
                .WithInternalOrganisation(@this)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .WithAgreement(new SalesAgreementBuilder(@this.Transaction())
                                .WithDescription("PaymentNetDays")
                                .WithAgreementTerm(new InvoiceTermBuilder(@this.Transaction())
                                .WithTermType(new InvoiceTermTypes(@this.Transaction()).PaymentNetDays).WithTermValue("30").Build())
                                .Build())
                .Build();

            return customer;
        }

        public static Organisation CreateSupplier(this Organisation @this, Faker faker)
        {
            var supplier = new OrganisationBuilder(@this.Transaction()).WithDefaults().Build();

            new SupplierRelationshipBuilder(@this.Transaction())
                .WithSupplier(supplier)
                .WithInternalOrganisation(@this)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            new OrganisationContactRelationshipBuilder(@this.Transaction())
                .WithContact(new PersonBuilder(@this.Transaction()).WithDefaults().Build())
                .WithOrganisation(supplier)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            return supplier;
        }

        public static Organisation CreateSubContractor(this Organisation @this, Faker faker)
        {
            var subContractor = new OrganisationBuilder(@this.Transaction()).WithDefaults().Build();

            new SubContractorRelationshipBuilder(@this.Transaction())
                .WithSubContractor(subContractor)
                .WithContractor(@this)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            new OrganisationContactRelationshipBuilder(@this.Transaction())
                .WithContact(new PersonBuilder(@this.Transaction()).WithDefaults().Build())
                .WithOrganisation(subContractor)
                .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                .Build();

            return subContractor;
        }

        public static NonUnifiedPart CreateNonSerialisedNonUnifiedPart(this Organisation @this, Faker faker)
        {
            var part = new NonUnifiedPartBuilder(@this.Transaction()).WithNonSerialisedDefaults(@this).Build();

            foreach (Organisation supplier in @this.ActiveSuppliers)
            {
                new SupplierOfferingBuilder(@this.Transaction())
                    .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                    .WithSupplier(supplier)
                    .WithPart(part)
                    .WithPrice(faker.Random.Decimal(0, 10))
                    .WithUnitOfMeasure(part.UnitOfMeasure)
                    .Build();
            }

            new InventoryItemTransactionBuilder(@this.Transaction())
                .WithPart(part)
                .WithFacility(@this.FacilitiesWhereOwner.First)
                .WithQuantity(faker.Random.Number(1000))
                .WithReason(new InventoryTransactionReasons(@this.Transaction()).Unknown)
                .Build();

            return part;
        }

        public static NonUnifiedPart CreateSerialisedNonUnifiedPart(this Organisation @this, Faker faker)
        {
            var part = new NonUnifiedPartBuilder(@this.Transaction()).WithSerialisedDefaults(@this, faker).Build();

            foreach (Organisation supplier in @this.ActiveSuppliers)
            {
                new SupplierOfferingBuilder(@this.Transaction())
                    .WithFromDate(faker.Date.Past(refDate: @this.Transaction().Now()))
                    .WithSupplier(supplier)
                    .WithPart(part)
                    .WithPrice(faker.Random.Decimal(0, 10))
                    .WithUnitOfMeasure(part.UnitOfMeasure)
                    .Build();
            }

            return part;
        }

        public static UnifiedGood CreateUnifiedWithGoodInventoryAvailableForSale(this Organisation @this, Faker faker)
        {
            var unifiedGood = new UnifiedGoodBuilder(@this.Transaction()).WithSerialisedDefaults(@this).Build();
            var serialisedItem = new SerialisedItemBuilder(@this.Transaction()).WithForSaleDefaults(@this).Build();

            unifiedGood.AddSerialisedItem(serialisedItem);

            new InventoryItemTransactionBuilder(@this.Transaction())
                .WithSerialisedItem(serialisedItem)
                .WithFacility(@this.FacilitiesWhereOwner.First)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(@this.Transaction()).IncomingShipment)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(@this.Transaction()).Good)
                .Build();

            return unifiedGood;
        }

        /*
         * Create a PurchaseOrder without any PurchaseOrder Items
         */
        public static PurchaseOrder CreatePurchaseOrderWithoutItems(this Organisation @this)
        {
            var purchaseOrder = new PurchaseOrderBuilder(@this.Transaction()).WithDefaults(@this).Build();

            return purchaseOrder;
        }

        /*
         * Create PurchaseOrder with both Serialized & NonSerialized PurchaseOrder Items
         */
        public static PurchaseOrder CreatePurchaseOrderWithBothItems(this Organisation @this, Faker faker)
        {
            var serializedPart = new UnifiedGoodBuilder(@this.Transaction()).WithSerialisedDefaults(@this).Build();
            var serializedItem = new SerialisedItemBuilder(@this.Transaction()).WithDefaults(@this).Build();
            serializedPart.AddSerialisedItem(serializedItem);

            var nonSerializedPart = new NonUnifiedPartBuilder(@this.Transaction()).WithNonSerialisedDefaults(@this).Build();

            var purchaseOrder = new PurchaseOrderBuilder(@this.Transaction()).WithDefaults(@this).Build();

            new SupplierOfferingBuilder(@this.Transaction())
                .WithPart(nonSerializedPart)
                .WithSupplier(purchaseOrder.TakenViaSupplier)
                .WithFromDate(@this.Transaction().Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction()).Piece)
                .WithPrice(faker.Random.Decimal(0, 10))
                .Build();

            var nonSerializedPartItem = new PurchaseOrderItemBuilder(@this.Transaction()).WithNonSerializedPartDefaults(nonSerializedPart, @this).Build();
            var serializedPartItem = new PurchaseOrderItemBuilder(@this.Transaction()).WithSerializedPartDefaults(serializedPart, serializedItem, @this).Build();

            purchaseOrder.AddPurchaseOrderItem(nonSerializedPartItem);
            purchaseOrder.AddPurchaseOrderItem(serializedPartItem);

            return purchaseOrder;
        }

        /*
         * Create PurchaseOrder with Serialized PurchaseOrderItem
         */
        public static PurchaseOrder CreatePurchaseOrderWithSerializedItem(this Organisation @this)
        {
            var serializedPart = new UnifiedGoodBuilder(@this.Transaction()).WithSerialisedDefaults(@this).Build();
            var serializedItem = new SerialisedItemBuilder(@this.Transaction()).WithDefaults(@this).Build();
            serializedPart.AddSerialisedItem(serializedItem);

            var purchaseOrder = new PurchaseOrderBuilder(@this.Transaction()).WithDefaults(@this).Build();

            var item = new PurchaseOrderItemBuilder(@this.Transaction()).WithSerializedPartDefaults(serializedPart, serializedItem, @this).Build();

            purchaseOrder.AddPurchaseOrderItem(item);

            return purchaseOrder;
        }

        /*
         * Create PurchaseOrder with NonSerialized PurchaseOrderItem
         */
        public static PurchaseOrder CreatePurchaseOrderWithNonSerializedItem(this Organisation @this, Faker faker)
        {
            var nonSerializedPart = new NonUnifiedPartBuilder(@this.Transaction()).WithNonSerialisedDefaults(@this).Build();

            var purchaseOrder = new PurchaseOrderBuilder(@this.Transaction()).WithDefaults(@this).Build();

            new SupplierOfferingBuilder(@this.Transaction())
                .WithPart(nonSerializedPart)
                .WithSupplier(purchaseOrder.TakenViaSupplier)
                .WithFromDate(@this.Transaction().Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction()).Piece)
                .WithPrice(faker.Random.Decimal(0, 10))
                .Build();

            var item = new PurchaseOrderItemBuilder(@this.Transaction()).WithNonSerializedPartDefaults(nonSerializedPart, @this).Build();

            purchaseOrder.AddPurchaseOrderItem(item);

            return purchaseOrder;
        }

        /*
         * Create Purchase Invoice with Serialized Purchase Invoice Item
         */
        public static PurchaseInvoice CreatePurchaseInvoiceWithSerializedItem(this Organisation @this)
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(@this.Transaction()).WithExternalB2BInvoiceDefaults(@this).Build();

            var item = new PurchaseInvoiceItemBuilder(@this.Transaction()).WithSerialisedProductItemDefaults().Build();
            purchaseInvoice.AddPurchaseInvoiceItem(item);

            return purchaseInvoice;
        }

        public static SalesOrder CreateInternalSalesOrder(this Organisation @this, Faker faker)
        {
            var salesOrder = new SalesOrderBuilder(@this.Transaction()).WithOrganisationInternalDefaults(@this).Build();
            @this.Transaction().Derive();

            var productItem = new SalesOrderItemBuilder(@this.Transaction()).WithSerialisedProductDefaults().Build();
            salesOrder.AddSalesOrderItem(productItem);
            @this.Transaction().Derive();

            var partItem = new SalesOrderItemBuilder(@this.Transaction()).WithNonSerialisedPartItemDefaults().Build();
            salesOrder.AddSalesOrderItem(partItem);
            @this.Transaction().Derive();

            var otherItem = new SalesOrderItemBuilder(@this.Transaction()).WithDefaults().Build();
            salesOrder.AddSalesOrderItem(otherItem);
            @this.Transaction().Derive();

            return salesOrder;
        }

        public static SalesOrder CreateB2BSalesOrder(this Organisation @this, Faker faker)
        {
            var salesOrder = new SalesOrderBuilder(@this.Transaction()).WithOrganisationExternalDefaults(@this).Build();
            @this.Transaction().Derive();

            var productItem = new SalesOrderItemBuilder(@this.Transaction()).WithSerialisedProductDefaults().Build();
            salesOrder.AddSalesOrderItem(productItem);
            @this.Transaction().Derive();

            var partItem = new SalesOrderItemBuilder(@this.Transaction()).WithNonSerialisedPartItemDefaults().Build();
            salesOrder.AddSalesOrderItem(partItem);
            @this.Transaction().Derive();

            var otherItem = new SalesOrderItemBuilder(@this.Transaction()).WithDefaults().Build();
            salesOrder.AddSalesOrderItem(otherItem);
            @this.Transaction().Derive();

            return salesOrder;
        }

        public static SalesOrder CreateB2BSalesOrderForSingleNonSerialisedItem(this Organisation @this, Faker faker)
        {
            var salesOrder = new SalesOrderBuilder(@this.Transaction()).WithOrganisationExternalDefaults(@this).Build();
            @this.Transaction().Derive();

            var salesOrderItem = new SalesOrderItemBuilder(@this.Transaction()).WithNonSerialisedPartItemDefaults().Build();
            salesOrder.AddSalesOrderItem(salesOrderItem);
            @this.Transaction().Derive();

            return salesOrder;
        }

        public static SalesOrder CreateB2BSalesOrderForSingleSerialisedItem(this Organisation @this, Faker faker)
        {
            var salesOrder = new SalesOrderBuilder(@this.Transaction()).WithOrganisationExternalDefaults(@this).Build();
            @this.Transaction().Derive();

            var salesOrderItem = new SalesOrderItemBuilder(@this.Transaction()).WithSerialisedProductDefaults().Build();
            salesOrder.AddSalesOrderItem(salesOrderItem);
            @this.Transaction().Derive();

            return salesOrder;
        }

        public static SalesOrder CreateB2CSalesOrder(this Organisation @this, Faker faker)
        {
            var salesOrder = new SalesOrderBuilder(@this.Transaction()).WithPersonExternalDefaults(@this).Build();
            @this.Transaction().Derive();

            var productItem = new SalesOrderItemBuilder(@this.Transaction()).WithSerialisedProductDefaults().Build();
            salesOrder.AddSalesOrderItem(productItem);
            @this.Transaction().Derive();

            var partItem = new SalesOrderItemBuilder(@this.Transaction()).WithNonSerialisedPartItemDefaults().Build();
            salesOrder.AddSalesOrderItem(partItem);
            @this.Transaction().Derive();

            var otherItem = new SalesOrderItemBuilder(@this.Transaction()).WithDefaults().Build();
            salesOrder.AddSalesOrderItem(otherItem);
            @this.Transaction().Derive();

            return salesOrder;
        }

        public static ProductQuote CreateB2BProductQuoteWithSerialisedItem(this Organisation @this, Faker faker)
        {
            var quote = new ProductQuoteBuilder(@this.Transaction()).WithDefaults(@this).Build();
            @this.Transaction().Derive();

            var quoteItem = new QuoteItemBuilder(@this.Transaction()).WithSerializedDefaults(@this).Build();
            quote.AddQuoteItem(quoteItem);
            @this.Transaction().Derive();

            return quote;
        }
    }
}
