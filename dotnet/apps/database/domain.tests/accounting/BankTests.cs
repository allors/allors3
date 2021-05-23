// <copyright file="BankTests.cs" company="Allors bvba">
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

    public class BankTests : DomainTest, IClassFixture<Fixture>
    {
        public BankTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenBank_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var builder = new BankBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithCountry(netherlands);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithBic("RABONL2U");
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Rabo");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankWithBic_WhenDeriving_ThenFirstfourCharactersMustBeAlphabetic()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            bank.Bic = "RAB1NL2U";

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankWithBic_WhenDeriving_ThenCharacters5And6MustBeValidCountryCode()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            bank.Bic = "RABONN2U";

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenBankWithBic_WhenDeriving_ThenStringLengthMustBeEightOrEleven()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2").Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            bank.Bic = "RABONL2UAAAA";

            Assert.True(this.Transaction.Derive(false).HasErrors);

            bank.Bic = "RABONL2UAAA";

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }

    public class BankRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public BankRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBicThrowValidationError()
        {
            var bank = new BankBuilder(this.Transaction).WithBic("invalid").Build();

            var expectedMessage = $"{bank}, {bank.Bic}, {ErrorMessages.NotAValidBic}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
