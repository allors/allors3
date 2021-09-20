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
                                                                                            .WithReferenceNumber("RNumber")
                                                                                            .WithReferenceCode("RCode")
                                                                                            .WithSortCode("SCode")
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
                                                            .WithReferenceNumber("RNumber")
                                                            .WithReferenceCode("RCode")
                                                            .WithSortCode("SCode")
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
                                                            .WithReferenceNumber("RNumber")
                                                            .WithReferenceCode("RCode")
                                                            .WithSortCode("SCode")
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
}
