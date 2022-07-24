// <copyright file="CountryTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CountryTests : DomainTest, IClassFixture<Fixture>
    {
        public CountryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCountryWhenValidatingThenRequiredRelationsMustExist()
        {
            var builder = new CountryBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("name");
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithIsoCode("nm");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }

    public class CountryRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CountryRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIsoCodeDeriveEuMemberState()
        {
            var country = new CountryBuilder(this.Transaction).Build();
            this.Derive();

            country.IsoCode = "NL";
            this.Derive();

            Assert.True(country.EuMemberState);
        }

        [Fact]
        public void ChangedIsoCodeDeriveIbanLength()
        {
            var country = new CountryBuilder(this.Transaction).Build();
            this.Derive();

            country.IsoCode = "NL";
            this.Derive();

            Assert.Equal(18, country.IbanLength);
        }

        [Fact]
        public void ChangedIsoCodeDeriveIbanRegex()
        {
            var country = new CountryBuilder(this.Transaction).Build();
            this.Derive();

            country.IsoCode = "NL";
            this.Derive();

            Assert.Equal(@"[A-Z]{4}\d{10}", country.IbanRegex);
        }
    }

    public class CountryVatRegimesRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CountryVatRegimesRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedVatRegimeCountryDeriveDerivedVatRegimes()
        {
            var country = new CountryBuilder(this.Transaction).Build();
            this.Derive();

            var vatRegime = new VatRegimeBuilder(this.Transaction).Build();
            this.Derive();

            vatRegime.Country = country;
            this.Derive();

            Assert.Contains(vatRegime, country.DerivedVatRegimes);
        }
    }
}
