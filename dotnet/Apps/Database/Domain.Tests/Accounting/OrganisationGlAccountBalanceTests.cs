// <copyright file="GeneralLedgerAccountTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
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
    }
}
