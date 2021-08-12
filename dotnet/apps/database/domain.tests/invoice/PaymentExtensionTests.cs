// <copyright file="PaymentExtensionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using TestPopulation;
    using Resources;
    using Xunit;
    using System.Linq;

    public class PaymentExtensionTests : DomainTest, IClassFixture<Fixture>
    {
        public PaymentExtensionTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveOnCreatePaymentAmountIsSmallerThanTheAppliedAmount()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            var fullAmount = salesInvoice.TotalIncVat;
            var extraAmount = salesInvoice.TotalIncVat + 1;
            var paymentApp = new PaymentApplicationBuilder(this.Transaction).WithInvoice(salesInvoice).WithAmountApplied(extraAmount).Build();

            var receipt = new ReceiptBuilder(this.Transaction)
                .WithAmount(fullAmount)
                .WithPaymentApplication(paymentApp)
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentAmountIsToSmall)));
        }
    }
}
