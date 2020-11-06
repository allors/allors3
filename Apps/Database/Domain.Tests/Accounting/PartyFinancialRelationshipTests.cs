// <copyright file="PartyFinancialRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using System.Linq;
    using Allors.Domain.TestPopulation;
    using Xunit;

    public class PartyFinancialRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyFinancialRelationshipTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveOpenOrderAmount()
        {
            var order = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());

            this.Session.Derive();

            var partyFinancial = order.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, order.TakenBy));

            Assert.True(partyFinancial.OpenOrderAmount == order.TotalIncVat);
        }

        [Fact]
        public void DeriveAmountDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            Assert.True(partyFinancial.AmountDue == invoice.TotalIncVat - invoice.AmountPaid);
        }

        [Fact]
        public void DeriveAmountDueWithoutPayment()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            invoice.AdvancePayment = 0;

            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            Assert.True(partyFinancial.AmountDue == invoice.TotalIncVat);
        }

        [Fact]
        public void DeriveAmountOverDueWithoutGracePeriod()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            // we know payment must be made within 30 days
            invoice.InvoiceDate = this.Session.Now().AddDays(-31); //due becomes yesterday

            this.Session.Derive();

            invoice.Store.PaymentGracePeriod = 0;

            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            Assert.True(partyFinancial.AmountOverDue == invoice.TotalIncVat - invoice.AdvancePayment);
        }

        [Fact]
        public void DeriveAmountOverDueWithGracePeriod()
        {
            var invoice = new SalesInvoiceBuilder(this.Session).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();

            // we know payment must be made within 30 days
            invoice.InvoiceDate = this.Session.Now().AddDays(-31); //due becomes yesterday

            this.Session.Derive();

            invoice.Store.PaymentGracePeriod = 10;

            this.Session.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            Assert.True(partyFinancial.AmountOverDue == 0);
        }

        [Fact]
        public void DerivePartyFinancialRelationCreated()
        {
            var customer = this.InternalOrganisation.CreateB2BCustomer(this.Session.Faker());

            this.Session.Derive();

            Assert.Equal(this.InternalOrganisation, customer.PartyFinancialRelationshipsWhereFinancialParty.First().InternalOrganisation);
        }
    }
}
