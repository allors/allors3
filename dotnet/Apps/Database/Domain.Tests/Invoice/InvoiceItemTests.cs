// <copyright file="InvoiceItemTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using TestPopulation;
    using Xunit;

    public class InvoiceItemTests : DomainTest, IClassFixture<Fixture>
    {

        public InvoiceItemTests(Fixture fixture) : base(fixture)
        { }

        [Fact]
        public void ChangedPaymentApplicationAmountAppliedThrowValidationError()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Derive().HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Transaction)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            Assert.False(this.Derive().HasErrors);

            var fullAmount = 1;
            new PaymentApplicationBuilder(this.Transaction)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(fullAmount)
                                        .Build();

            Assert.False(this.Derive().HasErrors);

            var extraAmount = 1;
            new PaymentApplicationBuilder(this.Transaction)
                                    .WithInvoiceItem(invoiceItem)
                                    .WithAmountApplied(extraAmount)
                                    .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount)));
        }

        [Fact]
        public void ChangedSalesInvoiceItemTotalIncVatThrowValidationError()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Derive().HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Transaction)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            Assert.False(this.Derive().HasErrors);

            invoiceItem.AssignedUnitPrice = 0;

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount)));
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemTotalIncVatThrowValidationError()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            var invoiceItem = purchaseInvoice.InvoiceItems.First();

            Assert.False(this.Derive().HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Transaction)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            Assert.False(this.Derive().HasErrors);

            invoiceItem.AssignedUnitPrice = 0;

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount)));
        }
    }
}
