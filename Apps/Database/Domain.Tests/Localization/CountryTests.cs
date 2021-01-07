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
            var builder = new CountryBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("name");
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithIsoCode("nm");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }
    }

    public class CountryDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CountryDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIsoCodeDeriveEuMemberState()
        {
            var country = new CountryBuilder(this.Session).Build();
            this.Session.Derive(false);

            country.IsoCode = "NL";
            this.Session.Derive(false);

            Assert.True(country.EuMemberState);
        }

        [Fact]
        public void ChangedIsoCodeDeriveIbanLength()
        {
            var country = new CountryBuilder(this.Session).Build();
            this.Session.Derive(false);

            country.IsoCode = "NL";
            this.Session.Derive(false);

            Assert.Equal(18, country.IbanLength);
        }

        [Fact]
        public void ChangedIsoCodeDeriveIbanRegex()
        {
            var country = new CountryBuilder(this.Session).Build();
            this.Session.Derive(false);

            country.IsoCode = "NL";
            this.Session.Derive(false);

            Assert.Equal(@"[A-Z]{4}\d{10}", country.IbanRegex);
        }
    }
}
