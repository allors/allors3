// <copyright file="BankAccountTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Database.Derivations;
    using Resources;

    using Xunit;

    public class BankAccountTests : DomainTest, IClassFixture<Fixture>
    {
        public BankAccountTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenBankAccount_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new BankAccountBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithIban("NL50RABO0109546784");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankAccount_WhenOwnBankAccount_ThenRequiredRelationsMustExist()
        {
            var netherlands = new Countries(this.Session).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;
            var bank = new BankBuilder(this.Session).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            this.Session.Commit();

            var builder = new BankAccountBuilder(this.Session).WithIban("NL50RABO0109546784");
            var bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Session).WithBankAccount(bankAccount).Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithBank(bank);
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Session).WithBankAccount(bankAccount).Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithCurrency(euro);
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Session).WithBankAccount(bankAccount).Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithNameOnAccount("name");
            bankAccount = builder.Build();

            new OwnBankAccountBuilder(this.Session).WithBankAccount(bankAccount).WithDescription("description").Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankAccount_WhenDeriving_ThenIbanMustBeUnique()
        {
            var netherlands = new Countries(this.Session).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Session).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();
            new BankAccountBuilder(this.Session).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            new BankAccountBuilder(this.Session).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenIllegalCharactersResultInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("-=jw").Build();

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanIllegalCharacters}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));

            this.Session.Rollback();

            new BankAccountBuilder(this.Session).WithIban("TR33000610+51978645,841326").Build();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWrongStructureResultsInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("D497888").Build();

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanStructuralFailure}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWrongCheckDigitsResultInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("TR000006100519786457841326").Build();

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanCheckDigitsError}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));

            this.Session.Rollback();

            new BankAccountBuilder(this.Session).WithIban("TR010006100519786457841326").Build();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));

            this.Session.Rollback();

            new BankAccountBuilder(this.Session).WithIban("TR990006100519786457841326").Build();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenCountryWithoutIbanRulesResultsInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("XX330006100519786457841326").Build();
            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanValidationUnavailable}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWronglengthResultsInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("TR3300061005196457841326").Build();

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanLengthFailure}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));

            this.Session.Rollback();

            new BankAccountBuilder(this.Session).WithIban("TR3300061005197864578413268").Build();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenWrongStuctureForCountryResultsInValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("LV80B12K0000435195001").Build();

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanStructuralFailure}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenBankAccount_WhenValidatingIban_ThenInvalidIbanResultsInValidationError()
        {
            var bankAccount=  new BankAccountBuilder(this.Session).WithIban("TR330006100519716457841326").Build();

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanIncorrect}";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void GivenValidIbanNumber_WhenValidatingIban_ThenValidationNoError()
        {
            new BankAccountBuilder(this.Session).WithIban("TR330006100519786457841326").Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }
    }

    public class BankAccountDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public BankAccountDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIbanThrowValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).Build();
            this.Session.Derive(false);

            bankAccount.Iban = "TR330006100519716457841326";

            var expectedErrorMessage = $"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanIncorrect}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedErrorMessage)));
        }

        [Fact]
        public void ChangedOwnBankAccountBankAccountThrowValidationError()
        {
            var bankAccount = new BankAccountBuilder(this.Session).WithIban("TR330006100519786457841326").Build();
            this.Session.Derive();

            new OwnBankAccountBuilder(this.Session).WithBankAccount(bankAccount).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: BankAccount.Bank"));
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: BankAccount.Currency"));
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: BankAccount.NameOnAccount"));
        }
    }
}
