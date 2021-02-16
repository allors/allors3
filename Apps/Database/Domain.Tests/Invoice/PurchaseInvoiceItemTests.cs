// <copyright file="PurchaseInvoiceItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PurchaseInvoiceItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceItemDerivationTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedPaymentApplicationAmountAppliedDeriveAmountPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, invoiceItem.AmountPaid);
        }
    }

    public class PurchaseInvoiceItemStateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceItemStateDerivationTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedPurchaseInvoiceValidInvoiceItemsDerivePurchaseInvoiceItemStateCreated()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsCreated);
        }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoiceItemStateAwaitingApproval()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsAwaitingApproval);
        }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoiceItemStateRevising()
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

            invoice.Revise();
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsRevising);
        }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoiceItemStateNotPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsNotPaid);
        }

        [Fact]
        public void ChangedAmountPaidDerivePurchaseInvoiceItemStatePartiallyPaid()
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

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedAmountPaidDerivePurchaseInvoiceItemStatePaid()
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

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(100).Build();
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceAmountPaidDerivePurchaseInvoiceItemStatePartiallyPaid()
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

            new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedPurchaseInvoiceAmountPaidDerivePurchaseInvoiceItemStatePaid()
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

            new PaymentApplicationBuilder(this.Transaction).WithInvoice(invoice).WithAmountApplied(10).Build();
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.PurchaseInvoiceItemState.IsPaid);
        }
    }
}
