// <copyright file="JournalTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class JournalTests : DomainTest, IClassFixture<Fixture>
    {
        public JournalTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenDescriptionMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
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

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(glAccount)
                .Build();

            this.Transaction.Commit();

            var builder = new JournalBuilder(this.Transaction);
            builder.WithJournalType(new JournalTypes(this.Transaction).Bank);
            builder.WithContraAccount(internalOrganisationGlAccount);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithName("description");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenSingletonMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
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

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(glAccount)
                .Build();

            this.Transaction.Commit();

            var builder = new JournalBuilder(this.Transaction);
            builder.WithName("description");
            builder.WithJournalType(new JournalTypes(this.Transaction).Bank);
            builder.WithContraAccount(internalOrganisationGlAccount);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenJournalTypeMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
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

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(glAccount)
                .Build();

            this.Transaction.Commit();

            var builder = new JournalBuilder(this.Transaction);
            builder.WithName("description");
            builder.WithContraAccount(internalOrganisationGlAccount);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithJournalType(new JournalTypes(this.Transaction).Bank);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenContraAccountMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var builder = new JournalBuilder(this.Transaction);
            builder.WithName("description");
            builder.WithJournalType(new JournalTypes(this.Transaction).Bank);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
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

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(glAccount)
                .Build();

            builder.WithContraAccount(internalOrganisationGlAccount);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenJournal_WhenBuildWithout_ThenBlockUnpaidTransactionsIsFalse()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithContraAccount(internalOrganisationGlAccount)
                .WithName("journal")
                .Build();

            this.Transaction.Derive(false);

            Assert.False(journal.BlockUnpaidTransactions);
        }

        [Fact]
        public void GivenJournal_WhenBuildWithout_ThenCloseWhenInBalanceIsFalse()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithContraAccount(internalOrganisationGlAccount)
                .WithName("journal")
                .Build();

            this.Transaction.Derive(false);

            Assert.False(journal.CloseWhenInBalance);
        }

        [Fact]
        public void GivenJournal_WhenBuildWithout_ThenUseAsDefaultIsFalse()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithContraAccount(internalOrganisationGlAccount)
                .WithName("journal")
                .Build();

            this.Transaction.Derive(false);

            Assert.False(journal.UseAsDefault);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenContraAccountCanBeChangedWhenNotUsedYet()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount1 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("bankAccount 1")
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

            var internalOrganisationGlAccount1 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var generalLedgerAccount2 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber2")
                .WithReferenceCode("ReferenceCode2")
                .WithSortCode("SortCode2")
                .WithName("bankAccount 2")
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

            var internalOrganisationGlAccount2 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount2)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithName("description")
                .WithContraAccount(internalOrganisationGlAccount1)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .Build();

            this.Transaction.Derive();

            journal.ContraAccount = internalOrganisationGlAccount2;

            this.Transaction.Derive();

            Assert.Equal(generalLedgerAccount2, journal.ContraAccount.GeneralLedgerAccount);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenContraAccountCanNotBeChangedWhenJournalEntriesArePresent()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount1 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("bankAccount 1")
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

            var internalOrganisationGlAccount1 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var generalLedgerAccount2 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber2")
                .WithReferenceCode("ReferenceCode2")
                .WithSortCode("SortCode2")
                .WithName("bankAccount 2")
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

            var internalOrganisationGlAccount2 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount2)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithName("description")
                .WithContraAccount(internalOrganisationGlAccount1)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .Build();

            this.Transaction.Derive();

            journal.AddAccountingTransaction(new DepreciationBuilder(this.Transaction)
                                        .WithAccountingTransactionDetail(new AccountingTransactionDetailBuilder(this.Transaction)
                                                                    .WithAmount(1)
                                                                    .WithDebit(true)
                                                                    .WithOrganisationGlAccount(internalOrganisationGlAccount1)
                                                                    .Build())
                                        .Build());

            journal.ContraAccount = internalOrganisationGlAccount2;

            var expectedMessage = $"{journal} {this.M.Journal.ContraAccount} {ErrorMessages.ContraAccountChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenJournalTypeCanBeChangedWhenJournalIsNotUsedYet()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount1 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("bankAccount 1")
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

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithName("description")
                .WithContraAccount(internalOrganisationGlAccount)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .Build();

            this.Transaction.Derive();

            journal.JournalType = new JournalTypes(this.Transaction).Cash;

            this.Transaction.Derive();

            Assert.Equal(new JournalTypes(this.Transaction).Cash, journal.JournalType);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenJournalTypeCanNotBeChangedWhenJournalEntriesArePresent()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var generalLedgerAccount1 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("bankAccount 1")
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

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithName("description")
                .WithContraAccount(internalOrganisationGlAccount)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .Build();

            this.Transaction.Derive();

            journal.AddAccountingTransaction(new DepreciationBuilder(this.Transaction)
                                        .WithAccountingTransactionDetail(new AccountingTransactionDetailBuilder(this.Transaction)
                                                                    .WithAmount(1)
                                                                    .WithDebit(true)
                                                                    .WithOrganisationGlAccount(internalOrganisationGlAccount)
                                                                    .Build())
                                        .Build());

            journal.JournalType = new JournalTypes(this.Transaction).Cash;

            var expectedMessage = $"{journal} {this.M.Journal.JournalType} {ErrorMessages.JournalTypeChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }

    public class JournalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public JournalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedContraAccountThrowValidationError()
        {
            var contraAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            var journal = new JournalBuilder(this.Transaction)
                .WithContraAccount(contraAccount)
                .Build();
            this.Transaction.Derive(false);

            var detail = new AccountingTransactionDetailBuilder(this.Transaction).WithOrganisationGlAccount(contraAccount).Build();
            this.Transaction.Derive(false);

            journal.ContraAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();

            var expectedMessage = $"{journal} {this.M.Journal.ContraAccount} {ErrorMessages.ContraAccountChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedJournalTypeThrowValidationError()
        {
            var journalType = new JournalTypeBuilder(this.Transaction).Build();
            var journal = new JournalBuilder(this.Transaction)
                .WithContraAccount(new OrganisationGlAccountBuilder(this.Transaction).Build())
                .WithJournalType(journalType)
                .Build();
            this.Transaction.Derive(false);

            var detail = new AccountingTransactionDetailBuilder(this.Transaction).WithOrganisationGlAccount(journal.ContraAccount).Build();
            this.Transaction.Derive(false);

            journal.JournalType = new JournalTypeBuilder(this.Transaction).Build();

            var expectedMessage = $"{journal} {this.M.Journal.JournalType} {ErrorMessages.JournalTypeChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
