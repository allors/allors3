// <copyright file="OwnBankAccountTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Xunit;

    public class OwnBankAccountTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnBankAccountTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOwnBankAccount_WhenDeriving_ThenBankAccountRelationMustExist()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();
            var bankAccount = new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien").Build();

            this.Transaction.Commit();

            var builder = new OwnBankAccountBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithBankAccount(bankAccount);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnBankAccount_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();
            var bankAccount = new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien").Build();

            var paymentMethod = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("own account")
                .WithBankAccount(bankAccount)
                .Build();

            this.Transaction.Derive();

            Assert.True(paymentMethod.IsActive);
            Assert.True(paymentMethod.ExistDescription);
        }

        [Fact]
        public void GivenOwnBankAccount_WhenDeriving_ThenBankAccountMustBeValidated()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var builder = new BankAccountBuilder(this.Transaction).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien");
            var bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("own account")
                .WithBankAccount(bankAccount).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            builder.WithBank(bank);
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("own account")
                .WithBankAccount(bankAccount).Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnBankAccount_WhenDeriving_ThenGeneralLedgerAccountAndJournalAtMostOne()
        {
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

            var journal = new JournalBuilder(this.Transaction).WithName("journal").Build();

            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();
            var bankAccount = new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien").Build();

            var paymentMethod = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("own account")
                .WithBankAccount(bankAccount)
                .WithGeneralLedgerAccount(internalOrganisationGlAccount)
                .Build();

            this.Transaction.Commit();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.DoAccounting = true;
            internalOrganisation.DefaultCollectionMethod = paymentMethod;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            paymentMethod.Journal = journal;

            Assert.True(this.Transaction.Derive(false).HasErrors);

            paymentMethod.RemoveGeneralLedgerAccount();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnBankAccountForSingletonThatDoesAccounting_WhenDeriving_ThenEitherGeneralLedgerAccountOrJournalMustExist()
        {
            var internalOrganisation = this.InternalOrganisation;

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

            var journal = new JournalBuilder(this.Transaction).WithName("journal").Build();

            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();
            var bankAccount = new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien").Build();

            var collectionMethod = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("own account")
                .WithBankAccount(bankAccount)
                .Build();

            this.Transaction.Commit();

            internalOrganisation.DoAccounting = true;

            internalOrganisation.AddAssignedActiveCollectionMethod(collectionMethod);

            Assert.True(this.Transaction.Derive(false).HasErrors);

            collectionMethod.Journal = journal;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            collectionMethod.RemoveJournal();
            collectionMethod.GeneralLedgerAccount = internalOrganisationGlAccount;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }

    public class OwnBankAccountRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnBankAccountRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInternalOrganisationDerivedCollectionMethodsThrowValidation()
        {
            this.InternalOrganisation.DoAccounting = true;

            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.DefaultCollectionMethod = ownBankAccount;

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtLeastOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.OwnBankAccount.GeneralLedgerAccount,
                this.M.OwnBankAccount.Journal,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedGeneralLedgerAccountThrowValidation()
        {
            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction).WithJournal(new JournalBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            ownBankAccount.GeneralLedgerAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.Cash.GeneralLedgerAccount,
                this.M.Cash.Journal,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedJournalThrowValidation()
        {
            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            ownBankAccount.Journal = new JournalBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.Cash.GeneralLedgerAccount,
                this.M.Cash.Journal,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }

}
