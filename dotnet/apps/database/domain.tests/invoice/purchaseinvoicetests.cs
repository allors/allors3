// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using TestPopulation;
    using Xunit;

    public class PurchaseInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseInvoice_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PurchaseInvoiceBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithBilledFrom(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
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

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASupplier));

            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier2).Build();

            Assert.False(this.Derive().HasErrors);
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
            this.Derive();

            Assert.True(invoice.ExistPurchaseInvoiceState);
        }

        [Fact]
        public void DeriveInvoiceDate()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(invoice.ExistInvoiceDate);
        }

        [Fact]
        public void DeriveEntryDate()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(invoice.ExistEntryDate);
        }
    }

    public class PurchaseInvoiceCreatedRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceCreatedRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateDeriveDerivedVatRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithPurchaseInvoiceState(new PurchaseInvoiceStates(this.Transaction).Cancelled).Build();
            this.Derive();

            invoice.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Derive();

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).Created;
            this.Derive();

            Assert.Equal(invoice.DerivedVatRegime, invoice.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Derive();

            Assert.Equal(invoice.DerivedVatRegime, invoice.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Derive();

            Assert.Equal(invoice.DerivedIrpfRegime, invoice.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            invoice.AssignedCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(invoice.DerivedCurrency, invoice.AssignedCurrency);
        }

        [Fact]
        public void ChangedBillToCustomerPreferredCurrencyDerivedCurrency()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            invoice.BilledTo.PreferredCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(invoice.DerivedCurrency, invoice.BilledTo.PreferredCurrency);
        }

        [Fact]
        public void ChangedOrderItemBillingInvoiceItemDerivePurchaseOrders()
        {
            var purchaseOrder = this.InternalOrganisation.CreatePurchaseOrderWithBothItems(this.Transaction.Faker());
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(purchaseOrder.PurchaseOrderItems.ElementAt(0)).WithInvoiceItem(invoiceItem).Build();
            this.Derive();

            Assert.Contains(purchaseOrder, purchaseInvoice.PurchaseOrders);
        }

        [Fact]
        public void ChangedInvoiceDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates.ElementAt(0).ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Derive();

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Derive();

            Assert.NotEqual(newVatRate, invoice.DerivedVatRate);

            invoice.InvoiceDate = this.Transaction.Now().AddDays(1).Date;
            this.Derive();

            Assert.Equal(newVatRate, invoice.DerivedVatRate);
        }
    }

    public class PurchaseInvoiceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBilledToDeriveValidationError()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.BilledTo = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InternalOrganisationChanged));
        }

        [Fact]
        public void ChangedBilledToDeriveInvoiceNumber()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(invoice.ExistInvoiceNumber);
        }

        [Fact]
        public void ChangedBilledToDeriveSortableInvoiceNumber()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(invoice.ExistSortableInvoiceNumber);
        }

        [Fact]
        public void ChangedBilledFromDeriveValidationError()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.BilledFrom = new OrganisationBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASupplier));
        }

        [Fact]
        public void ChangedBilledFromDeriveWorkItemDescription()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var expected = $"PurchaseInvoice: {invoice.InvoiceNumber} [{invoice.BilledFrom?.DisplayName}]";
            Assert.Equal(expected, invoice.WorkItemDescription);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateCreatedDeriveValidInvoiceItems()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Contains(invoiceItem, purchaseInvoice.ValidInvoiceItems);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateCancelledDeriveValidInvoiceItems()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoiceItem.CancelFromInvoice();
            this.Derive();

            Assert.DoesNotContain(invoiceItem, purchaseInvoice.ValidInvoiceItems);
        }

        [Fact]
        public void ChangedPaymentApplicationInvoiceDeriveAmountPaid()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            new PaymentApplicationBuilder(this.Transaction).WithInvoice(purchaseInvoice).WithAmountApplied(10).Build();
            this.Derive();

            Assert.Equal(10, purchaseInvoice.AmountPaid);
        }

        [Fact]
        public void ChangedPaymentApplicationInvoiceItemDeriveAmountPaid()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(10).Build();
            this.Derive();

            Assert.Equal(10, purchaseInvoice.AmountPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemDerivePurchaseInvoiceItemSyncedInvoice()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(purchaseInvoice, invoiceItem.SyncedInvoice);
        }
    }

    public class PurchaseInvoiceStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateDerivePurchaseInvoiceStateNotPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            Assert.True(invoice.PurchaseInvoiceState.IsNotPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateDerivePurchaseInvoiceStatePartiallyPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(10).Build();
            this.Derive();

            Assert.True(invoice.PurchaseInvoiceState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemPurchaseInvoiceItemStateDerivePurchaseInvoiceStatePaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(10).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(10).Build();
            this.Derive();

            Assert.True(invoice.PurchaseInvoiceState.IsPaid);
        }

        [Fact]
        public void ChangedAmountPaidDerivePurchaseInvoiceStateNotPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem1 = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(8).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem1);
            this.Derive();

            var invoiceItem2 = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(9).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem2);
            this.Derive();

            invoice.Confirm();
            this.Derive();

            invoice.Approve();
            this.Derive();

            new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(10).Build();
            this.Derive();

            Assert.True(invoice.PurchaseInvoiceState.IsPartiallyPaid);

        }
    }

    public class PurchaseInvoiceAwaitingApprovalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceAwaitingApprovalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateCreateApprovalTask()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.Confirm();
            this.Derive();

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
            this.Derive();

            purchaseInvoice.Confirm();
            this.Derive();

            purchaseInvoice.Approve();
            this.Derive();

            Assert.Equal(purchaseInvoice.BilledTo, purchaseInvoice.PurchaseInvoiceItems.ElementAt(0).SerialisedItem.Buyer);
        }

        [Fact]
        public void ChangedPurchaseInvoiceStateDeriveSerialisedItemPurchasePrice()
        {
            this.InternalOrganisation.AddSerialisedItemSoldOn(new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm);

            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            var serialisedItem = purchaseInvoice.PurchaseInvoiceItems.ElementAt(0).SerialisedItem;
            serialisedItem.RemoveAssignedPurchasePrice();
            this.Derive();

            purchaseInvoice.Confirm();
            this.Derive();

            purchaseInvoice.Approve();
            this.Derive();

            Assert.Equal(purchaseInvoice.PurchaseInvoiceItems.ElementAt(0).TotalExVat, serialisedItem.PurchasePrice);
        }
    }

    public class PurchaseInvoicePriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoicePriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceStateCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.ElementAt(0))
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceState(new PurchaseInvoiceStates(this.Transaction).Cancelled)
                .WithBilledFrom(this.InternalOrganisation.ActiveSuppliers.ElementAt(0))
                .WithInvoiceDate(this.Transaction.Now())
                .Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(0, invoice.TotalIncVat);

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).Created;
            this.Derive();

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedValidInvoiceItemsCalculatePrice()
        {
            var invoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            this.Transaction.Derive();

            Assert.True(invoice.TotalIncVat > 0);
            var totalIncVatBefore = invoice.TotalIncVat;

            invoice.PurchaseInvoiceItems.First().CancelFromInvoice();
            this.Transaction.Derive();

            Assert.Equal(invoice.TotalIncVat, totalIncVatBefore - invoice.PurchaseInvoiceItems.First().TotalIncVat);
        }

        [Fact]
        public void ChangedDerivationTriggerCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.ElementAt(0))
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation.ActiveSuppliers.ElementAt(0)).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);

            supplierOffering.Price = 2;
            this.Derive();

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemQuantityCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Quantity = 2;
            this.Derive();

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemAssignedUnitPriceCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.AssignedUnitPrice = 3;
            this.Derive();

            Assert.Equal(3, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceItemPartCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
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
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part1).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Part = part2;
            this.Derive();

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedBilledFromCalculatePrice()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Transaction.Faker());
            var part = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

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
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoice.BilledFrom = supplier2;
            this.Derive();

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedRoleSalesInvoiceItemDiscountAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemDiscountAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSurchargeAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseInvoiceInvoiceDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates.ElementAt(0).ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Derive();

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            Assert.NotEqual(newVatRate, invoiceItem.VatRate);

            invoice.InvoiceDate = this.Transaction.Now().AddDays(1).Date;
            this.Derive();

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

            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[invoice];

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
    public class PurchaseInvoiceDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.deleteRevocation = new Revocations(this.Transaction).PurchaseInvoiceDeleteRevocation;
            this.createSalesInvoiceRevocation = new Revocations(this.Transaction).PurchaseInvoiceCreateSalesInvoiceRevocation;
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;
        private readonly Revocation createSalesInvoiceRevocation;

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(purchaseInvoice.PurchaseInvoiceState.IsCreated);
            Assert.DoesNotContain(this.deleteRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCancelledDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.Cancel();
            this.Derive();

            Assert.True(purchaseInvoice.PurchaseInvoiceState.IsCancelled);
            Assert.DoesNotContain(this.deleteRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateRejectedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.Reject();
            this.Derive();

            Assert.True(purchaseInvoice.PurchaseInvoiceState.IsRejected);
            Assert.DoesNotContain(this.deleteRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateApprovedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            this.Derive();

            purchaseInvoice.Approve();

            this.Derive();

            Assert.Contains(this.deleteRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedWithSalesInvoiceDeriveDeletePermissionDenied()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            new SalesInvoiceBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Derive();

            Assert.Contains(this.deleteRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedWithSalesInvoiceDeriveDeletePermissionAllowed()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();
            this.Derive();

            salesInvoice.RemovePurchaseInvoice();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedDeriveCreateSalesInvoicePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();

            this.Derive();

            Assert.Contains(this.createSalesInvoiceRevocation, purchaseInvoice.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateNotPaidWithInternalOrganisationDeriveCreateSalesInvoicePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation).Build();
            this.Derive();

            purchaseInvoice.Approve();
            this.Derive();

            purchaseInvoice.SetPaid();
            this.Derive();

            Assert.DoesNotContain(this.createSalesInvoiceRevocation, purchaseInvoice.Revocations);
        }
    }
}
