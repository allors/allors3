// <copyright file="GeneralLedgerAccountTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class GeneralLedgerAccountTests : DomainTest, IClassFixture<Fixture>
    {
        public GeneralLedgerAccountTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenGeneralLedgerAccount_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            Assert.True(generalLedgerAccount.ExistUniqueId);
            Assert.False(generalLedgerAccount.CashAccount);
            Assert.False(generalLedgerAccount.CostCenterAccount);
            Assert.False(generalLedgerAccount.CostCenterRequired);
            Assert.False(generalLedgerAccount.CostUnitAccount);
            Assert.False(generalLedgerAccount.CostUnitRequired);
            Assert.False(generalLedgerAccount.Blocked);
            Assert.False(generalLedgerAccount.ReconciliationAccount);
        }

        [Fact]
        public void GivenGeneralLedgerAccount_WhenAddedToChartOfAccounts_ThenAccountNumberMustBeUnique()
        {
            var glAccount0001 = new GeneralLedgerAccountBuilder(this.Transaction)
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

            var glAccount0001Dup = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount duplicate number")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                                                            .WithReferenceNumber("SCode")
                                                            .WithName("accountGroup")
                                                            .Build())
                .Build();

            var chart = new ChartOfAccountsBuilder(this.Transaction).WithName("name").WithGeneralLedgerAccount(glAccount0001).Build();

            Assert.False(this.Derive().HasErrors);

            chart.AddGeneralLedgerAccount(glAccount0001Dup);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.AccountNumberUniqueWithinChartOfAccounts));

            new ChartOfAccountsBuilder(this.Transaction).WithName("another Chart").WithGeneralLedgerAccount(glAccount0001Dup).Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenGeneralLedgerAccount_WhenSettingCostCenterRequired_ThenAccountMustBeMarkedAsCostCenterAccount()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithCostCenterRequired(true)
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                                                            .WithReferenceNumber("SCode")
                                                            .WithName("accountGroup")
                                                            .Build())
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotACostCenterAccount));
        }

        [Fact]
        public void GivenGeneralLedgerAccount_WhenSettingCostUnitRequired_ThenAccountMustBeMarkedAsCostUnitAccount()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithCostUnitRequired(true)
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                                                            .WithReferenceNumber("SCode")
                                                            .WithName("accountGroup")
                                                            .Build())
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotACostUnitAccount));
        }
    }

    public class GeneralLedgerAccountRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public GeneralLedgerAccountRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedDefaultCostCenterDeriveDerivedCostCentersAllowed()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            glAccount.DefaultCostCenter = new CostCenterBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(glAccount.DefaultCostCenter, glAccount.DerivedCostCentersAllowed);
        }

        [Fact]
        public void ChangedAssignedCostCentersAllowedDeriveDerivedCostCentersAllowed()
        {
            var defaultCostCenter = new CostCenterBuilder(this.Transaction).Build();
            var allowedCostCenter = new CostCenterBuilder(this.Transaction).Build();

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).WithDefaultCostCenter(defaultCostCenter).Build();
            this.Derive();

            glAccount.AddAssignedCostCentersAllowed(allowedCostCenter);
            this.Derive();

            Assert.Contains(defaultCostCenter, glAccount.DerivedCostCentersAllowed);
            Assert.Contains(allowedCostCenter, glAccount.DerivedCostCentersAllowed);
        }

        [Fact]
        public void ChangedDefaultCostUnitDeriveDerivedCostUnitsAllowed()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            glAccount.DefaultCostUnit = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(glAccount.DefaultCostUnit, glAccount.DerivedCostUnitsAllowed);
        }

        [Fact]
        public void ChangedAssignedCostUnitsAllowedDeriveDerivedCostUnitsAllowed()
        {
            var defaultCostUnit = new UnifiedGoodBuilder(this.Transaction).Build();
            var allowedCostUnit = new UnifiedGoodBuilder(this.Transaction).Build();

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).WithDefaultCostUnit(defaultCostUnit).Build();
            this.Derive();

            glAccount.AddAssignedCostUnitsAllowed(allowedCostUnit);
            this.Derive();

            Assert.Contains(defaultCostUnit, glAccount.DerivedCostUnitsAllowed);
            Assert.Contains(allowedCostUnit, glAccount.DerivedCostUnitsAllowed);
        }

        [Fact]
        public void ChangedChartOfAccountsGeneralLedgerAccountsThrowValidationError()
        {
            var chartOfAccounts = new ChartOfAccountsBuilder(this.Transaction)
                .WithGeneralLedgerAccount(new GeneralLedgerAccountBuilder(this.Transaction).WithReferenceNumber("1").Build())
                .Build();
            this.Derive();

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).WithReferenceNumber("1").Build();
            chartOfAccounts.AddGeneralLedgerAccount(glAccount);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.AccountNumberUniqueWithinChartOfAccounts));
        }

        [Fact]
        public void ChangedReferenceNumberThrowValidationError()
        {
            var chartOfAccounts = new ChartOfAccountsBuilder(this.Transaction)
                .WithGeneralLedgerAccount(new GeneralLedgerAccountBuilder(this.Transaction).WithReferenceNumber("1").Build())
                .Build();
            this.Derive();

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).WithReferenceNumber("2").Build();
            chartOfAccounts.AddGeneralLedgerAccount(glAccount);

            var errors = this.Derive().Errors.ToList();
            Assert.DoesNotContain(errors, e => e.Message.Contains(ErrorMessages.AccountNumberUniqueWithinChartOfAccounts));

            glAccount.ReferenceNumber = "1";

            errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.AccountNumberUniqueWithinChartOfAccounts));
        }

        [Fact]
        public void ChangedCostCenterRequiredThrowValidationError()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            glAccount.CostCenterRequired = true;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotACostCenterAccount));
        }

        [Fact]
        public void ChangedCostCenterAccountThrowValidationError()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).WithCostCenterAccount(true).WithCostCenterRequired(true).Build();
            this.Derive();

            glAccount.CostCenterAccount = false;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotACostCenterAccount));
        }

        [Fact]
        public void ChangedCostUnitRequiredThrowValidationError()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            glAccount.CostUnitRequired = true;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotACostUnitAccount));
        }

        [Fact]
        public void ChangedCostUnitAccountThrowValidationError()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).WithCostUnitAccount(true).WithCostUnitRequired(true).Build();
            this.Derive();

            glAccount.CostUnitAccount = false;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotACostUnitAccount));
        }
    }

    [Trait("Category", "Security")]
    public class GeneralLedgerAccountDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public GeneralLedgerAccountDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).GeneralLedgerAccountDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedGeneralLedgerAccountDeriveDeletePermission()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, glAccount.Revocations);
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

            Assert.DoesNotContain(this.deleteRevocation, glAccount.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionDetailOrganisationGlAccountDeriveDeletePermission()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).WithGeneralLedgerAccount(glAccount).Build();
            this.Derive();

            var transaction = new AccountingTransactionBuilder(this.Transaction)
                .WithAccountingTransactionDetail(new AccountingTransactionDetailBuilder(this.Transaction).WithGeneralLedgerAccount(glAccount).Build())
                .Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, glAccount.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionDeleteDeriveDeletePermission()
        {
            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).WithGeneralLedgerAccount(glAccount).Build();
            this.Derive();

            var transaction = new AccountingTransactionBuilder(this.Transaction)
                .WithAccountingTransactionDetail(new AccountingTransactionDetailBuilder(this.Transaction).WithGeneralLedgerAccount(glAccount).Build())
                .Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, glAccount.Revocations);

            transaction.Delete();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, glAccount.Revocations);
        }
    }
}
