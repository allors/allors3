// <copyright file="BankTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
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

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCountry(netherlands);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithBic("RABONL2U");
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Rabo");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenBankWithBic_WhenDeriving_ThenFirstfourCharactersMustBeAlphabetic()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            Assert.False(this.Derive().HasErrors);

            bank.Bic = "RAB1NL2U";

            Assert.True(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenBankWithBic_WhenDeriving_ThenCharacters5And6MustBeValidCountryCode()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            Assert.False(this.Derive().HasErrors);

            bank.Bic = "RABONN2U";

            Assert.True(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenBankWithBic_WhenDeriving_ThenStringLengthMustBeEightOrEleven()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2").Build();

            Assert.True(this.Derive().HasErrors);

            bank.Bic = "RABONL2UAAAA";

            Assert.True(this.Derive().HasErrors);

            bank.Bic = "RABONL2UAAA";

            Assert.False(this.Derive().HasErrors);
        }
    }

    public class BankRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public BankRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBicThrowValidationError()
        {
            new BankBuilder(this.Transaction).WithBic("invalid").Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.NotAValidBic));
        }
    }
}
