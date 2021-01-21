// <copyright file="PartyFinancialRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class PartyFinancialRelationshipAmountDueDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyFinancialRelationshipAmountDueDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesInvoiceTotalIncVatDeriveAmountDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            Assert.True(partyFinancial.AmountDue == invoice.TotalIncVat - invoice.AmountPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceAmountPaidDeriveAmountDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.AdvancePayment = 0;
            this.Session.Derive();

            Assert.True(partyFinancial.AmountDue == invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedStorePaymentGracePeriodWithoutGraceDeriveAmountOverDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            // we know payment must be made within 30 days
            invoice.InvoiceDate = this.Session.Now().AddDays(-31); //due becomes yesterday
            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.Store.PaymentGracePeriod = 0;
            this.Session.Derive();

            Assert.True(partyFinancial.AmountOverDue == invoice.TotalIncVat - invoice.AdvancePayment);
        }

        [Fact]
        public void ChangedStorePaymentGracePeriodWitGraceDeriveAmountOverDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.DueDate = this.Session.Now().AddDays(-1);
            invoice.Store.PaymentGracePeriod = 10;
            this.Session.Derive(false);

            Assert.True(partyFinancial.AmountOverDue == 0);
        }
    }

    public class PartyFinancialRelationshipOpenOrderAmountDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyFinancialRelationshipOpenOrderAmountDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveOpenOrderAmount()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());
            this.Session.Derive();

            var partyFinancial = order.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, order.TakenBy));

            Assert.True(partyFinancial.OpenOrderAmount == order.TotalIncVat);
        }
    }
}
