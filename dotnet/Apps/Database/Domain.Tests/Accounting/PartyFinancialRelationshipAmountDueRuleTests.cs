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

    public class PartyFinancialRelationshipAmountDueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyFinancialRelationshipAmountDueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesInvoiceTotalIncVatDeriveAmountDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            Assert.True(partyFinancial.AmountDue == invoice.TotalIncVat - invoice.AmountPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceAmountPaidDeriveAmountDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.AdvancePayment = 0;
            this.Transaction.Derive();

            Assert.True(partyFinancial.AmountDue == invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesInvoiceDueDateDeriveAmountOverDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive();

            invoice.Store.PaymentGracePeriod = 0;
            this.Transaction.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.DueDate = this.Transaction.Now().AddDays(-31);
            this.Transaction.Derive();

            Assert.True(partyFinancial.AmountOverDue == invoice.TotalIncVat - invoice.AdvancePayment);
        }

        [Fact]
        public void ChangedStorePaymentGracePeriodWithoutGraceDeriveAmountOverDue()
        {
            var customer = new OrganisationBuilder(this.Transaction).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).WithFromDate(this.Transaction.Now().AddDays(-32)).Build();
            this.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).WithBilledFrom(this.InternalOrganisation).WithAdvancePayment(10).Build();
            this.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithQuantity(1).WithAssignedUnitPrice(100).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Derive();

            // we know payment must be made within 30 days
            invoice.InvoiceDate = this.Transaction.Now().AddDays(-31); //due becomes yesterday
            this.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.Store.PaymentGracePeriod = 0;
            this.Derive();

            Assert.Equal(90, partyFinancial.AmountOverDue); // invoice.TotalIncVat - invoice.AdvancePayment;
        }

        [Fact]
        public void ChangedStorePaymentGracePeriodWitGraceDeriveAmountOverDue()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(this.InternalOrganisation).Build();
            this.Derive();

            var partyFinancial = invoice.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, invoice.BilledFrom));

            invoice.DueDate = this.Transaction.Now().AddDays(-1);
            invoice.Store.PaymentGracePeriod = 10;
            this.Derive();

            Assert.True(partyFinancial.AmountOverDue == 0);
        }

        [Fact]
        public void OnSetOfInternalOrganisationSettingsForAccountingCreatePartyFinancialRelationships()
        {
            var interalOrganisation = new OrganisationBuilder(this.Transaction).WithInternalOrganisationDefaults().WithExportAccounting(false).Build();
            var organisation = new Organisations(this.Transaction).Extent().First(v => !Equals(v, interalOrganisation));

            new SupplierRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(interalOrganisation)
                .WithSupplier(organisation)
                .Build();

            this.Derive();

            Assert.Contains(organisation, interalOrganisation.ActiveSuppliers);
            Assert.False(interalOrganisation.PartyFinancialRelationshipsWhereInternalOrganisation.Any());

            interalOrganisation.ExportAccounting = true;

            this.Derive();

            Assert.True(interalOrganisation.PartyFinancialRelationshipsWhereInternalOrganisation.Any());
        }

        [Fact]
        public void OnCreateOfNewPartyRelationShipWithInternalOrganisationThatUsesAccounting_PartyFinancialRealtionShipShouldBeCreated()
        {
            var interalOrganisation = new OrganisationBuilder(this.Transaction).WithInternalOrganisationDefaults().WithExportAccounting(true).Build();
            interalOrganisation.SettingsForAccounting = new InternalOrganisationAccountingSettingsBuilder(this.Transaction).Build();

            var customer = new Parties(this.Transaction).Extent().First(v => !Equals(v, interalOrganisation));

            this.Derive();

            Assert.DoesNotContain(interalOrganisation.PartyFinancialRelationshipsWhereInternalOrganisation, v => v.FinancialParty == customer);

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)
                .WithInternalOrganisation(interalOrganisation)
                .Build();

            this.Derive();

            Assert.Contains(interalOrganisation.PartyFinancialRelationshipsWhereInternalOrganisation, v => v.FinancialParty == customer);
        }
    }
}
