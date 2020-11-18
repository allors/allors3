// <copyright file="PaymentApplicationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.TestPopulation;
    using Database.Derivations;
    using Resources;
    using Xunit;

    public class PaymentApplicationTests : DomainTest, IClassFixture<Fixture>
    {
        public PaymentApplicationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPaymentApplication_WhenDeriving_ThenAmountAppliedCannotBeLargerThenAmountReceived()
        {
            var contactMechanism = new ContactMechanisms(this.Session).Extent().First;
            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            var customer = new PersonBuilder(this.Session).WithLastName("customer").Build();
            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(customer)

                .Build();

            var invoice = new SalesInvoiceBuilder(this.Session)
                .WithBillToCustomer(customer)
                .WithBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Session).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000M).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).Build())
                .Build();

            this.Session.Derive();

            var receipt = new ReceiptBuilder(this.Session)
                .WithAmount(100)
                .WithEffectiveDate(this.Session.Now())
                .Build();

            var paymentApplication = new PaymentApplicationBuilder(this.Session)
                .WithAmountApplied(200)
                .WithInvoiceItem(invoice.InvoiceItems[0])
                .Build();

            this.Session.Derive();

            receipt.AddPaymentApplication(paymentApplication);

            var expectedMessage = $"{paymentApplication} {this.M.Payment.Amount} {ErrorMessages.PaymentAmountIsToSmall}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Contains(expectedMessage)));
        }

        [Fact]
        public void DeriveOnCreateAtMostOneInvalidPaymentApplication()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Session.Derive(false).HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Session)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithInvoice(salesInvoice)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            var expectedMessage = $"{invoiceItem} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertExistsAtMostOne")));
        }

        [Fact]
        public void DeriveOnCreateAtLeastOneInvalidPaymentApplication()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Session.Derive(false).HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Session)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            var expectedMessage = $"{invoiceItem} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertAtLeastOne")));
        }

        [Fact]
        public void DeriveOnCreatePaymentApplicationNotLargerThanPaymentAmount()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            var fullAmount = salesInvoice.TotalIncVat - 1;
            var extraAmount = salesInvoice.TotalIncVat + 1;
            var paymentApp = new PaymentApplicationBuilder(this.Session).WithInvoice(salesInvoice).WithAmountApplied(extraAmount).Build();

            new ReceiptBuilder(this.Session)
                .WithAmount(fullAmount)
                .WithPaymentApplication(paymentApp)
                .WithEffectiveDate(this.Session.Now())
                .Build();

            var expectedMessage = $"{paymentApp} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanPaymentAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Contains(expectedMessage)));
        }

        [Fact]
        public void DeriveOnCreatePaymentApplicationNotLargerThanInvoiceAmount()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            var fullAmount = salesInvoice.TotalIncVat;
            var extraAmount = salesInvoice.TotalIncVat + 1;
            var paymentApp = new PaymentApplicationBuilder(this.Session).WithInvoice(salesInvoice).WithAmountApplied(extraAmount).Build();

            new ReceiptBuilder(this.Session)
                .WithAmount(fullAmount)
                .WithPaymentApplication(paymentApp)
                .WithEffectiveDate(this.Session.Now())
                .Build();

            var expectedMessage = $"{paymentApp} { this.M.PaymentApplication.AmountApplied} { ErrorMessages.PaymentApplicationNotLargerThanInvoiceAmount}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Contains(expectedMessage)));
        }
    }
}
