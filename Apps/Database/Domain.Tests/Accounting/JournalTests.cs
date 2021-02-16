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
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
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

            builder.WithDescription("description");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenJournal_WhenDeriving_ThenSingletonMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;
            this.Transaction.Derive(false);

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(glAccount)
                .Build();

            this.Transaction.Commit();

            var builder = new JournalBuilder(this.Transaction);
            builder.WithDescription("description");
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
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(glAccount)
                .Build();

            this.Transaction.Commit();

            var builder = new JournalBuilder(this.Transaction);
            builder.WithDescription("description");
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
            builder.WithDescription("description");
            builder.WithJournalType(new JournalTypes(this.Transaction).Bank);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            var glAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
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
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithContraAccount(internalOrganisationGlAccount)
                .WithDescription("journal")
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
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithContraAccount(internalOrganisationGlAccount)
                .WithDescription("journal")
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
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithContraAccount(internalOrganisationGlAccount)
                .WithDescription("journal")
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
                .WithAccountNumber("0001")
                .WithName("bankAccount 1")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount1 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var generalLedgerAccount2 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithAccountNumber("0002")
                .WithName("bankAccount 2")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount2 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount2)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithDescription("description")
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
                .WithAccountNumber("0001")
                .WithName("bankAccount 1")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount1 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var generalLedgerAccount2 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithAccountNumber("0002")
                .WithName("bankAccount 2")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount2 = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount2)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithDescription("description")
                .WithContraAccount(internalOrganisationGlAccount1)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .Build();

            this.Transaction.Derive();

            journal.AddJournalEntry(new JournalEntryBuilder(this.Transaction)
                                        .WithJournalEntryDetail(new JournalEntryDetailBuilder(this.Transaction)
                                                                    .WithAmount(1)
                                                                    .WithDebit(true)
                                                                    .WithGeneralLedgerAccount(internalOrganisationGlAccount1)
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
                .WithAccountNumber("0001")
                .WithName("bankAccount 1")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithDescription("description")
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
                .WithAccountNumber("0001")
                .WithName("bankAccount 1")
                .WithBalanceSheetAccount(true)
                .WithSide(new DebitCreditConstants(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountGroup(new GeneralLedgerAccountGroupBuilder(this.Transaction).WithDescription("accountGroup").Build())
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithFromDate(this.Transaction.Now())
                .WithGeneralLedgerAccount(generalLedgerAccount1)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithDescription("description")
                .WithContraAccount(internalOrganisationGlAccount)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .Build();

            this.Transaction.Derive();

            journal.AddJournalEntry(new JournalEntryBuilder(this.Transaction)
                                        .WithJournalEntryDetail(new JournalEntryDetailBuilder(this.Transaction)
                                                                    .WithAmount(1)
                                                                    .WithDebit(true)
                                                                    .WithGeneralLedgerAccount(internalOrganisationGlAccount)
                                                                    .Build())
                                        .Build());

            journal.JournalType = new JournalTypes(this.Transaction).Cash;

            var expectedMessage = $"{journal} {this.M.Journal.JournalType} {ErrorMessages.JournalTypeChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }

    public class JournalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public JournalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedContraAccountThrowValidationError()
        {
            var contraAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            var journal = new JournalBuilder(this.Transaction)
                .WithContraAccount(contraAccount)
                .Build();
            this.Transaction.Derive(false);

            var detail = new JournalEntryDetailBuilder(this.Transaction).WithGeneralLedgerAccount(contraAccount).Build();
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

            var detail = new JournalEntryDetailBuilder(this.Transaction).WithGeneralLedgerAccount(journal.ContraAccount).Build();
            this.Transaction.Derive(false);

            journal.JournalType = new JournalTypeBuilder(this.Transaction).Build();

            var expectedMessage = $"{journal} {this.M.Journal.JournalType} {ErrorMessages.JournalTypeChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
