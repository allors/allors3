// <copyright file="AccountingTransactionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    [Trait("Category", "Security")]
    public class AccountingTransactionDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public AccountingTransactionDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).AccountingTransactionDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnInternalOrganisationExportAccountingTrueDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnInternalOrganisationExportAccountingFalseDeriveDeletePermission()
        {
            this.InternalOrganisation.ExportAccounting = false;

            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionExportedIsFalseDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            transaction.Exported = false;
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionExportedIsTrueDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            transaction.Exported = true;
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedInternalOrganisationExportAccountingDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);

            this.InternalOrganisation.ExportAccounting = false;
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }
    }

    public class AccountingTransactionDerivedTotalAmountRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public AccountingTransactionDerivedTotalAmountRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAccountingTransactionDetailAmountAndBalanceSideDeriveDerivedTotalAmount()
        {
            var organisationGlAccount = new OrganisationGlAccounts(this.Transaction).Extent().ToArray().First();

            var accountingPeriod = new AccountingPeriodBuilder(this.Transaction)
                .WithInternalOrganisation(organisationGlAccount.InternalOrganisation)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .Build();

            var detail = new AccountingTransactionDetailBuilder(this.Transaction)
                .WithAmount(100)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccount(organisationGlAccount.GeneralLedgerAccount)
                .Build();

            var accountingTransaction = new AccountingTransactionBuilder(this.Transaction)
                .WithAccountingTransactionType(new AccountingTransactionTypes(this.Transaction).Internal)
                .WithInternalOrganisation(organisationGlAccount.InternalOrganisation)
                .WithAccountingPeriod(accountingPeriod)
                .WithDescription("Test")
                .WithTransactionDate(DateTime.Now)
                .WithEntryDate(DateTime.Now.AddHours(1))
                .WithAccountingTransactionDetail(detail)
                .Build();
            this.Derive();

            Assert.Equal(100, accountingTransaction.DerivedTotalAmount);

            detail.Amount = 150;
            this.Derive();

            Assert.Equal(150, accountingTransaction.DerivedTotalAmount);

            detail.BalanceSide = new BalanceSides(this.Transaction).Credit;
            this.Derive();

            Assert.Equal(0, accountingTransaction.DerivedTotalAmount);
        }
    }
}
