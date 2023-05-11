// <copyright file="OrganisationGlAccountTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrganisationGlAccountTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationGlAccountTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrganisationGlAccount_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OrganisationGlAccountBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithGeneralLedgerAccount(new GeneralLedgerAccountBuilder(this.Transaction)
                                                .WithReferenceNumber("ReferenceNumber")
                                                .WithReferenceCode("ReferenceCode")
                                                .WithSortCode("SortCode")
                                                .WithName("GeneralLedgerAccount")
                                                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                                                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                                                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                                                .WithGeneralLedgerAccountClassification(new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                                                            .WithReferenceNumber("SCode")
                                                                                            .WithName("accountGroup")
                                                                                            .Build())
                                                .Build());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenOrganisationGlAccount_WhenBuild_ThenHasBankStatementTransactionsIsAlwaysFalse()
        {
            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                                                            .WithReferenceNumber("SCode")
                                                            .WithName("accountGroup")
                                                            .Build())
                .Build();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            this.Transaction.Derive();

            Assert.False(organisationGlAccount.HasBankStatementTransactions);
        }

        [Fact]
        public void GivenOrganisationGlAccount_WhenNotReferenced_ThenAccountIsNeutral()
        {
            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                                                            .WithReferenceNumber("SCode")
                                                            .WithName("accountGroup")
                                                            .Build())
                .Build();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            Assert.True(organisationGlAccount.IsNeutralAccount());
            Assert.False(organisationGlAccount.IsBankAccount());
            Assert.False(organisationGlAccount.IsCashAccount());
            Assert.False(organisationGlAccount.IsCostAccount());
            Assert.False(organisationGlAccount.IsCreditorAccount());
            Assert.False(organisationGlAccount.IsDebtorAccount());
            Assert.False(organisationGlAccount.IsInventoryAccount());
            Assert.False(organisationGlAccount.IsTurnOverAccount());
        }
    }

    [Trait("Category", "Security")]
    public class OrganisationGlAccountDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationGlAccountDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).OrganisationGlAccountDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedOrganisationGlAccountDeriveDeletePermission()
        {
            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, organisationGlAccount.Revocations);
        }

        [Fact]
        public void OnChangedOrganisationGlAccountGeneralLedgerAccountDeriveDeletePermission()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Derive();

            organisationGlAccount.GeneralLedgerAccount = glAccount;
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, organisationGlAccount.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionDetailOrganisationGlAccountDeriveDeletePermission()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).WithGeneralLedgerAccount(glAccount).Build();
            this.Derive();

            var transaction = new AccountingTransactionBuilder(this.Transaction)
                .WithAccountingTransactionDetail(new AccountingTransactionDetailBuilder(this.Transaction).WithOrganisationGlAccount(organisationGlAccount).Build())
                .Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, organisationGlAccount.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionDeleteDeriveDeletePermission()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).WithGeneralLedgerAccount(glAccount).Build();
            this.Derive();

            var transaction = new AccountingTransactionBuilder(this.Transaction)
                .WithAccountingTransactionDetail(new AccountingTransactionDetailBuilder(this.Transaction).WithOrganisationGlAccount(organisationGlAccount).Build())
                .Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, organisationGlAccount.Revocations);

            transaction.Delete();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, organisationGlAccount.Revocations);
        }
    }
}
