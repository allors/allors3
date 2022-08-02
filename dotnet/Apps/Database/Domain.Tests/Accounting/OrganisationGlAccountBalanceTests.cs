// <copyright file="GeneralLedgerAccountTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Allors.Database.Domain.TestPopulation;
    using Xunit;

    public class OrganisationGlAccountBalanceTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationGlAccountBalanceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OrganisationGlAccountBalanceAmountRuleTest()
        {
            var organisationGlAccount = new OrganisationGlAccounts(this.Transaction).Extent().ToArray().First();

            var accountingPeriod = new AccountingPeriodBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount.InternalOrganisation)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .Build();

            var accountingTransactionDetail1 = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(100)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithOrganisationGlAccount(organisationGlAccount)
                .Build();

            var accountingTransactionDetail2 = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(200)
                .WithBalanceSide(new BalanceSides(this.Transaction).Credit)
                .WithOrganisationGlAccount(organisationGlAccount)
                .Build();

            var accountingTransaction = new AccountingTransactionBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount.InternalOrganisation)
                .WithAccountingPeriod(accountingPeriod)
                .WithDescription("Test")
                .WithTransactionDate(DateTime.Now)
                .WithEntryDate(DateTime.Now.AddHours(1))
                .WithAccountingTransactionDetail(accountingTransactionDetail1)
                .WithAccountingTransactionDetail(accountingTransactionDetail2)
                .Build();

            var organisationGlAccountBalance = new OrganisationGlAccountBalanceBuilder(this.Transaction)
                .WithOrganisationGlAccount(organisationGlAccount)
                .WithAccountingPeriod(accountingPeriod)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            Assert.Equal(100, organisationGlAccountBalance.DebitAmount);
            Assert.Equal(200, organisationGlAccountBalance.CreditAmount);
            Assert.Equal(-100, organisationGlAccountBalance.Amount);
        }

        [Fact]
        public void OrganisationGlAccountBalanceAmountRuleDeletingAccountingTransactionDetailTest()
        {
            var organisationGlAccount1 = new OrganisationGlAccounts(this.Transaction).Extent().ToArray().First();

            var generalLedgerAccount = new GeneralLedgerAccounts(this.Transaction).Extent().ToArray().First();

            var organisationGlAccount2 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount1.InternalOrganisation)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var accountingPeriod = new AccountingPeriodBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount1.InternalOrganisation)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .Build();

            var accountingPeriod2 = new AccountingPeriodBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount1.InternalOrganisation)
                .WithFrequency(new TimeFrequencies(this.Transaction).Year)
                .Build();

            var accountingTransactionDetail1 = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(100)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithOrganisationGlAccount(organisationGlAccount1)
                .Build();

            var accountingTransactionDetail2 = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(200)
                .WithBalanceSide(new BalanceSides(this.Transaction).Credit)
                .WithOrganisationGlAccount(organisationGlAccount1)
                .Build();

            var accountingTransaction1 = new AccountingTransactionBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount1.InternalOrganisation)
                .WithAccountingPeriod(accountingPeriod)
                .WithDescription("Test")
                .WithTransactionDate(DateTime.Now)
                .WithEntryDate(DateTime.Now.AddHours(1))
                .WithAccountingTransactionDetail(accountingTransactionDetail1)
                .WithAccountingTransactionDetail(accountingTransactionDetail2)
                .Build();

            var accountingTransaction2 = new AccountingTransactionBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount1.InternalOrganisation)
                .WithAccountingPeriod(accountingPeriod2)
                .WithDescription("Test")
                .WithTransactionDate(DateTime.Now)
                .WithEntryDate(DateTime.Now.AddHours(1))
                .Build();

            var organisationGlAccountBalance = new OrganisationGlAccountBalanceBuilder(this.Transaction)
                .WithOrganisationGlAccount(organisationGlAccount1)
                .WithAccountingPeriod(accountingPeriod)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            Assert.Equal(100, organisationGlAccountBalance.DebitAmount);
            Assert.Equal(200, organisationGlAccountBalance.CreditAmount);
            Assert.Equal(-100, organisationGlAccountBalance.Amount);

            accountingTransactionDetail2.OrganisationGlAccount = organisationGlAccount2;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            Assert.Equal(100, organisationGlAccountBalance.DebitAmount);
            Assert.Equal(0, organisationGlAccountBalance.CreditAmount);
            Assert.Equal(100, organisationGlAccountBalance.Amount);
        }

        [Fact]
        public void OrganisationGlAccountWhenLevel5AccountAmountChangesThenLevel4AmountShouldReflectThis()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithInternalOrganisationDefaults().Build();

            var level4GeneralLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BLimBanRba")
                .WithName("Rekening-courant bank")
                .WithDescription("Rekening-courant bank tegoeden op bankgirorekeningen")
                .WithRgsLevel(4)
                .Build();

            var level5GeneralLedgerAccount1 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BLimBanRbaBg1")
                .WithName("Rekening-courant bank groep 1")
                .WithDescription("Rekening-courant bank groep 1")
                .WithRgsLevel(5)
                .WithParent(level4GeneralLedgerAccount)
                .Build();

            var level5GeneralLedgerAccount2 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BLimBanRbaBg2")
                .WithName("Rekening-courant bank groep 2")
                .WithDescription("Rekening-courant bank groep 2")
                .WithRgsLevel(5)
                .WithParent(level4GeneralLedgerAccount)
                .Build();

            var level4OrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithGeneralLedgerAccount(level4GeneralLedgerAccount)
                .Build();

            var level5OrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithGeneralLedgerAccount(level5GeneralLedgerAccount1)
                .WithSubsidiaryOf(level4OrganisationGlAccount)
                .Build();

            var level5OrganisationGlAccount2 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithGeneralLedgerAccount(level5GeneralLedgerAccount2)
                .WithSubsidiaryOf(level4OrganisationGlAccount)
                .Build();

            var accountingPeriod = new AccountingPeriodBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .Build();

            var accountingTransactionDetail1 = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(100)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithOrganisationGlAccount(level5OrganisationGlAccount)
                .Build();

            var accountingTransactionDetail2 = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(200)
                .WithBalanceSide(new BalanceSides(this.Transaction).Credit)
                .WithOrganisationGlAccount(level5OrganisationGlAccount2)
                .Build();

            var accountingTransaction1 = new AccountingTransactionBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithAccountingPeriod(accountingPeriod)
                .WithDescription("Level 5 Transaction 1")
                .WithTransactionDate(DateTime.Now)
                .WithEntryDate(DateTime.Now.AddHours(1))
                .WithAccountingTransactionDetail(accountingTransactionDetail1)
                .WithAccountingTransactionDetail(accountingTransactionDetail2)
                .Build();

            var level5OrganisationGlAccountBalance = new OrganisationGlAccountBalanceBuilder(this.Transaction)
                .WithOrganisationGlAccount(level5OrganisationGlAccount)
                .WithAccountingPeriod(accountingPeriod)
                .Build();

            var level5OrganisationGlAccountBalance2 = new OrganisationGlAccountBalanceBuilder(this.Transaction)
                .WithOrganisationGlAccount(level5OrganisationGlAccount2)
                .WithAccountingPeriod(accountingPeriod)
                .Build();

            var level4OrganisationGlAccountBalance = new OrganisationGlAccountBalanceBuilder(this.Transaction)
                .WithOrganisationGlAccount(level4OrganisationGlAccount)
                .WithAccountingPeriod(accountingPeriod)
                .Build();

            this.Derive();

            //Assert.Equal(100, level4OrganisationGlAccountBalance.DebitAmount);
            //Assert.Equal(200, level4OrganisationGlAccountBalance.CreditAmount);
            //Assert.Equal(-100, level4OrganisationGlAccountBalance.Amount);
        }
    }
}
