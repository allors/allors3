// <copyright file="PurchaseInvoiceItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PurchaseInvoiceItemTests : DomainTest, IClassFixture<Fixture>
    {

        public PurchaseInvoiceItemTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void OnChangedRolePaymentApplicationAmountAppliedDeriveAmountPaid()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Session).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Session.Derive(false);

            new PaymentApplicationBuilder(this.Session).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Session.Derive(false);

            Assert.Equal(1, invoiceItem.AmountPaid);
        }
    }
}
