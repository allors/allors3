// <copyright file="InvoiceItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using TestPopulation;
    using Database.Derivations;
    using Resources;
    using Xunit;

    public class InvoiceItemTests : DomainTest, IClassFixture<Fixture>
    {

        public InvoiceItemTests(Fixture fixture) : base(fixture)
        { }

        [Fact]
        public void DeriveOnChangedRolePaymentApplicationAmountApplied()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Session.Derive(false).HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Session)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            var fullAmount = 1;
            new PaymentApplicationBuilder(this.Session)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(fullAmount)
                                        .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            var extraAmount = 1;
            new PaymentApplicationBuilder(this.Session)
                                    .WithInvoiceItem(invoiceItem)
                                    .WithAmountApplied(extraAmount)
                                    .Build();

            var expectedMessage = $"{invoiceItem} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }

        [Fact]
        public void DeriveOnChangedRoleSalesTotalIncVat()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Session.Derive(false).HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Session)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            invoiceItem.AssignedUnitPrice = 0;

            var expectedMessage = $"{invoiceItem} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }

        [Fact]
        public void DeriveOnChangedRolePurchaseTotalIncVat()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();
            var invoiceItem = purchaseInvoice.InvoiceItems.First();

            Assert.False(this.Session.Derive(false).HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Session)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            invoiceItem.AssignedUnitPrice = 0;

            var expectedMessage = $"{invoiceItem} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }
    }
}
