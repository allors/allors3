// <copyright file="BankAccountTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Resources;

    using Xunit;

    public class BankAccountTests : DomainTest, IClassFixture<Fixture>
    {
        public BankAccountTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenBankAccount_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new BankAccountBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithIban("NL50RABO0109546784");
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithNameOnAccount("name");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankAccount_WhenOwnBankAccount_ThenRequiredRelationsMustExist()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;
            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            this.Transaction.Commit();

            var builder = new BankAccountBuilder(this.Transaction).WithIban("NL50RABO0109546784");
            var bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Transaction).WithBankAccount(bankAccount).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithBank(bank);
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Transaction).WithBankAccount(bankAccount).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithCurrency(euro);
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Transaction).WithBankAccount(bankAccount).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithNameOnAccount("name");
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Transaction).WithBankAccount(bankAccount).WithDescription("description").Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankAccount_WhenDeriving_ThenIbanMustBeUnique()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();
            new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("name").Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenIllegalCharactersResultInValidationError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("-=jw").Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanIllegalCharacters)));

            this.Transaction.Rollback();

            new BankAccountBuilder(this.Transaction).WithIban("TR33000610+51978645,841326").Build();

            errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanIllegalCharacters)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWrongStructureResultsInValidationError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("D497888").Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanStructuralFailure)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWrongCheckDigitsResultInValidationError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("TR000006100519786457841326").Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanCheckDigitsError)));

            this.Transaction.Rollback();

            new BankAccountBuilder(this.Transaction).WithIban("TR010006100519786457841326").Build();

            errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanCheckDigitsError)));

            this.Transaction.Rollback();

            new BankAccountBuilder(this.Transaction).WithIban("TR990006100519786457841326").Build();

            errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanCheckDigitsError)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenCountryWithoutIbanRulesResultsInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Transaction).WithIban("XX330006100519786457841326").Build();
            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanValidationUnavailable}";

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWronglengthResultsInValidationError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("TR3300061005196457841326").Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanLengthFailure)));

            this.Transaction.Rollback();

            new BankAccountBuilder(this.Transaction).WithIban("TR3300061005197864578413268").Build();

            errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanLengthFailure)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWrongStuctureForCountryResultsInValidationError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("LV80B12K0000435195001").Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanStructuralFailure)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenInvalidIbanResultsInValidationError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("TR330006100519716457841326").Build();

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.IbanIncorrect)));
        }

        [Fact]
        public void GivenValidIbanNumber_WhenValidatingIban_ThenValidationNoError()
        {
            new BankAccountBuilder(this.Transaction).WithIban("TR330006100519786457841326").WithNameOnAccount("name").Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }

    public class BankAccountRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public BankAccountRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIbanThrowValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            bankAccount.Iban = "TR330006100519716457841326";

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanIncorrect}";
            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(expectedErrorMessage)));
        }

        [Fact]
        public void ChangedOwnBankAccountBankAccountThrowValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new OwnBankAccountBuilder(this.Transaction).WithBankAccount(bankAccount).Build();

            var validation = this.Transaction.Derive(false);

            var errors = validation.Errors.OfType<DerivationErrorRequired>().ToArray();
            Assert.Equal(new IRoleType[]
            {
                this.M.BankAccount.Bank,
                this.M.BankAccount.Currency,
                this.M.BankAccount.NameOnAccount
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }
}
