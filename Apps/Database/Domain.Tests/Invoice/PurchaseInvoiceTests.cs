// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Resources;
    using Xunit;
    using System.Collections.Generic;
    using Allors.Database.Derivations;

    public class PurchaseInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseInvoice_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PurchaseInvoiceBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithBilledFrom(this.InternalOrganisation.ActiveSuppliers.First);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseInvoice_WhenDeriving_ThenBilledFromPartyMustBeInSupplierRelationship()
        {
            var supplier2 = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier2)
                .Build();

            var expectedMessage = $"{invoice} { this.M.PurchaseInvoice.BilledFrom} { ErrorMessages.PartyIsNotASupplier}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));

            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier2).Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseInvoice_WhenGettingInvoiceNumberWithoutFormat_ThenInvoiceNumberShouldBeReturned()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var invoice1 = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("incoming invoiceno: 1", invoice1.InvoiceNumber);

            var invoice2 = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("incoming invoiceno: 2", invoice2.InvoiceNumber);
        }

        [Fact]
        public void GivenBilledToWithoutInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseInvoiceNumberPrefix();
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            invoice.Confirm();
            this.Transaction.Derive();

            Assert.Equal(int.Parse(invoice.InvoiceNumber), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenBilledToWithInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.PurchaseInvoiceNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            invoice.Confirm();
            this.Transaction.Derive();

            Assert.Equal(int.Parse(invoice.InvoiceNumber.Split('-')[1]), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenBilledToWithParametrizedInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.PurchaseInvoiceNumberPrefix = "prefix-{year}-";
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            invoice.Confirm();
            this.Transaction.Derive();

            var number = int.Parse(invoice.InvoiceNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Transaction.Now().Date.Year.ToString(), number)), invoice.SortableInvoiceNumber);
        }
    }

    public class PurchaseInvoiceOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DerivePurchaseInvoiceState()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.ExistPurchaseInvoiceState);
        }

        [Fact]
        public void DeriveInvoiceDate()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.ExistInvoiceDate);
        }

        [Fact]
        public void DeriveEntryDate()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.ExistEntryDate);
        }
    }

    public class PurchaseInvoiceCreatedDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceCreatedDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateDeriveDerivedVatRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithPurchaseInvoiceState(new PurchaseInvoiceStates(this.Transaction).Cancelled).Build();
            this.Transaction.Derive(false);

            invoice.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Transaction.Derive(false);

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedVatRegime, invoice.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedVatRegime, invoice.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedIrpfRegime, invoice.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            invoice.AssignedCurrency = swedishKrona;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, invoice.AssignedCurrency);
        }

        [Fact]
        public void ChangedBillToCustomerPreferredCurrencyDerivedCurrency()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            invoice.BilledTo.PreferredCurrency = swedishKrona;
            this.Transaction.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, invoice.BilledTo.PreferredCurrency);
        }

        [Fact]
        public void ChangedOrderItemBillingInvoiceItemDerivePurchaseOrders()
        {
            var purchaseOrder = this.InternalOrganisation.CreatePurchaseOrderWithBothItems(this.Transaction.Faker());
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(purchaseOrder.PurchaseOrderItems[0]).WithInvoiceItem(invoiceItem).Build();
            this.Transaction.Derive(false);

            Assert.Contains(purchaseOrder, purchaseInvoice.PurchaseOrders);
        }

        [Fact]
        public void ChangedInvoiceDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates[0].ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Transaction.Derive(false);

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Transaction.Derive(false);

            Assert.NotEqual(newVatRate, invoice.DerivedVatRate);

            invoice.InvoiceDate = this.Transaction.Now().AddDays(1).Date;
            this.Transaction.Derive(false);

            Assert.Equal(newVatRate, invoice.DerivedVatRate);
        }
    }

    public class PurchaseInvoiceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBilledToDeriveValidationError()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.BilledTo = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var expectedMessage = $"{invoice} { this.M.PurchaseInvoice.BilledTo} { ErrorMessages.InternalOrganisationChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedBilledToDeriveInvoiceNumber()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.ExistInvoiceNumber);
        }

        [Fact]
        public void ChangedBilledToDeriveSortableInvoiceNumber()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.ExistSortableInvoiceNumber);
        }

        [Fact]
        public void ChangedBilledFromDeriveValidationError()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoice.BilledFrom = new OrganisationBuilder(this.Transaction).Build();

            var expectedMessage = $"{invoice} { this.M.PurchaseInvoice.BilledFrom} { ErrorMessages.PartyIsNotASupplier}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedBilledFromDeriveWorkItemDescription()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var expected = $"PurchaseInvoice: {invoice.InvoiceNumber} [{invoice.BilledFrom?.PartyName}]";
            Assert.Equal(expected, invoice.WorkItemDescription);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateCreatedDeriveValidInvoiceItems()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Contains(invoiceItem, purchaseInvoice.ValidInvoiceItems);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateCancelledDeriveValidInvoiceItems()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.CancelFromInvoice();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(invoiceItem, purchaseInvoice.ValidInvoiceItems);
        }

        [Fact]
        public void ChangedPaymentApplicationInvoiceDeriveAmountPaid()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoice(purchaseInvoice).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.Equal(10, purchaseInvoice.AmountPaid);
        }

        [Fact]
        public void ChangedPaymentApplicationInvoiceItemDeriveAmountPaid()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.Equal(10, purchaseInvoice.AmountPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemDerivePurchaseInvoiceItemSyncedInvoice()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(purchaseInvoice, invoiceItem.SyncedInvoice);
        }
    }

    public class PurchaseInvoiceStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceStateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateDerivePurchaseInvoiceStateNotPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.True(invoice.PurchaseInvoiceState.IsNotPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateDerivePurchaseInvoiceStatePartiallyPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.PurchaseInvoiceState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateDerivePurchaseInvoiceStatePaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(10).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.PurchaseInvoiceState.IsPaid);
        }

        [Fact]
        public void ChangedAmountPaidDerivePurchaseInvoiceStateNotPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem1 = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(8).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem1);
            this.Transaction.Derive(false);

            var invoiceItem2 = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(9).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem2);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.True(invoice.PurchaseInvoiceState.IsPartiallyPaid);

        }
    }

    public class PurchaseInvoiceAwaitingApprovalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceAwaitingApprovalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateCreateApprovalTask()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            Assert.True(invoice.ExistPurchaseInvoiceApprovalsWherePurchaseInvoice);
        }
    }

    public class PurchaseInvoiceSerialisedItemDerivation : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceSerialisedItemDerivation(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateDeriveSerialisedItemBuyer()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm);

            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            this.Transaction.Derive(false);

            purchaseInvoice.Confirm();
            this.Transaction.Derive(false);

            purchaseInvoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(purchaseInvoice.BilledTo, purchaseInvoice.PurchaseInvoiceItems[0].SerialisedItem.Buyer);
        }

        [Fact]
        public void ChangedPurchaseInvoiceStateDeriveSerialisedItemPurchasePrice()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm);

            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            this.Transaction.Derive(false);

            purchaseInvoice.Confirm();
            this.Transaction.Derive(false);

            purchaseInvoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(purchaseInvoice.PurchaseInvoiceItems[0].TotalExVat, purchaseInvoice.PurchaseInvoiceItems[0].SerialisedItem.PurchasePrice);
        }
    }

    public class PurchaseInvoicePriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoicePriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers[0])
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceState(new PurchaseInvoiceStates(this.Transaction).Cancelled)
                .WithBilledFrom(this.InternalOrganisation.ActiveSuppliers[0])
                .WithInvoiceDate(this.Transaction.Now())
                .Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(0, invoice.TotalIncVat);

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedValidInvoiceItemsCalculatePrice()
        {
            var invoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            this.Transaction.Derive();

            Assert.True(invoice.TotalIncVat > 0);
            var totalIncVatBefore = invoice.TotalIncVat;

            invoice.PurchaseInvoiceItems.First.CancelFromInvoice();
            this.Transaction.Derive();

            Assert.Equal(invoice.TotalIncVat, totalIncVatBefore - invoice.PurchaseInvoiceItems.First.TotalIncVat);
        }

        [Fact]
        public void ChangedDerivationTriggerCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers[0])
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation.ActiveSuppliers[0]).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);

            supplierOffering.Price = 2;
            this.Transaction.Derive(false);

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemQuantityCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Quantity = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemAssignedUnitPriceCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.AssignedUnitPrice = 3;
            this.Transaction.Derive(false);

            Assert.Equal(3, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemPartCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part1 = new UnifiedGoodBuilder(this.Transaction).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part1)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part2)
                .WithSupplier(supplier)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part1).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Part = part2;
            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedBilledFromCalculatePrice()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers[0];
            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Transaction.Faker());
            var part = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier1)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier1).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            invoice.BilledFrom = supplier2;
            this.Transaction.Derive(false);

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedRoleSalesInvoiceItemDiscountAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemDiscountAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSurchargeAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers[0];
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceInvoiceDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates[0].ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Transaction.Derive(false);

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(newVatRate, invoiceItem.VatRate);

            invoice.InvoiceDate = this.Transaction.Now().AddDays(1).Date;
            this.Transaction.Derive(false);

            Assert.Equal(newVatRate, invoiceItem.VatRate);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseInvoiceSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenPurchaseInvoice_WhenObjectStateIsCreated_ThenCheckTransitions()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[invoice];

            Assert.Equal(new PurchaseInvoiceStates(this.Transaction).Created, invoice.PurchaseInvoiceState);
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Approve));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Reject));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Reopen));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.SetPaid));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Revise));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.FinishRevising));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.CreateSalesInvoice));
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseInvoiceDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Transaction).Get(this.M.PurchaseInvoice, this.M.PurchaseInvoice.Delete);
            this.createSalesInvoicePermission = new Permissions(this.Transaction).Get(this.M.PurchaseInvoice, this.M.PurchaseInvoice.CreateSalesInvoice);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission createSalesInvoicePermission;

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(purchaseInvoice.PurchaseInvoiceState.IsCreated);
            Assert.DoesNotContain(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCancelledDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.Cancel();
            this.Transaction.Derive(false);

            Assert.True(purchaseInvoice.PurchaseInvoiceState.IsCancelled);
            Assert.DoesNotContain(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateRejectedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.Reject();
            this.Transaction.Derive(false);

            Assert.True(purchaseInvoice.PurchaseInvoiceState.IsRejected);
            Assert.DoesNotContain(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateApprovedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            purchaseInvoice.Approve();

            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedWithSalesInvoiceDeriveDeletePermissionDenied()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            new SalesInvoiceBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedWithSalesInvoiceDeriveDeletePermissionAllowed()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();
            this.Transaction.Derive(false);

            salesInvoice.RemovePurchaseInvoice();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedDeriveCreateSalesInvoicePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Contains(this.createSalesInvoicePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateNotPaidWithInternalOrganisationDeriveCreateSalesInvoicePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.Approve();
            this.Transaction.Derive(false);

            purchaseInvoice.SetPaid();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.createSalesInvoicePermission, purchaseInvoice.DeniedPermissions);
        }
    }
}
