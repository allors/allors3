// <copyright file="PaymentApplicationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using TestPopulation;
    using Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Resources;
    using Xunit;

    public class PaymentApplicationTests : DomainTest, IClassFixture<Fixture>
    {
        public PaymentApplicationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPaymentApplication_WhenDeriving_ThenAmountAppliedCannotBeLargerThenAmountReceived()
        {
            var contactMechanism = new ContactMechanisms(this.Transaction).Extent().First;
            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)

                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000M).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).Build())
                .Build();

            this.Transaction.Derive();

            var receipt = new ReceiptBuilder(this.Transaction)
                .WithAmount(100)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            var paymentApplication = new PaymentApplicationBuilder(this.Transaction)
                .WithAmountApplied(200)
                .WithInvoiceItem(invoice.InvoiceItems[0])
                .Build();

            this.Transaction.Derive();

            receipt.AddPaymentApplication(paymentApplication);

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentAmountIsToSmall)));
        }

        [Fact]
        public void DeriveOnCreateAtMostOneInvalidPaymentApplication()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Derive().HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Transaction)
                                        .WithInvoiceItem(invoiceItem)
                                        .WithInvoice(salesInvoice)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            var errors = this.Derive().Errors.OfType<DerivationErrorAtMostOne>().ToList();
            Assert.Contains(this.M.PaymentApplication.Invoice, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.PaymentApplication.InvoiceItem, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.PaymentApplication.BillingAccount, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void DeriveOnCreateAtLeastOneInvalidPaymentApplication()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            var invoiceItem = salesInvoice.InvoiceItems.First();

            Assert.False(this.Derive().HasErrors);

            var partialAmount = invoiceItem.TotalIncVat - 1;
            new PaymentApplicationBuilder(this.Transaction)
                                        .WithAmountApplied(partialAmount)
                                        .Build();

            // TODO: Shouldn't this be Required?
            var errors = this.Derive().Errors.OfType<DerivationErrorAtLeastOne>();
            Assert.Contains(this.M.PaymentApplication.Invoice, errors.SelectMany(v => v.RoleTypes));
            Assert.Contains(this.M.PaymentApplication.InvoiceItem, errors.SelectMany(v => v.RoleTypes));
            Assert.Contains(this.M.PaymentApplication.BillingAccount, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void DeriveOnCreatePaymentApplicationNotLargerThanPaymentAmount()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            var fullAmount = salesInvoice.TotalIncVat - 1;
            var extraAmount = salesInvoice.TotalIncVat + 1;
            var paymentApp = new PaymentApplicationBuilder(this.Transaction).WithInvoice(salesInvoice).WithAmountApplied(extraAmount).Build();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(fullAmount)
                .WithPaymentApplication(paymentApp)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentApplicationNotLargerThanPaymentAmount)));
        }

        [Fact]
        public void DeriveOnCreatePaymentApplicationNotLargerThanInvoiceAmount()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            var fullAmount = salesInvoice.TotalIncVat;
            var extraAmount = salesInvoice.TotalIncVat + 1;
            var paymentApp = new PaymentApplicationBuilder(this.Transaction).WithInvoice(salesInvoice).WithAmountApplied(extraAmount).Build();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(fullAmount)
                .WithPaymentApplication(paymentApp)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentApplicationNotLargerThanInvoiceAmount)));
        }
    }
}
