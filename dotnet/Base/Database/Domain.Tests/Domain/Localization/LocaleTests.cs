// <copyright file="LocaleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Domain;
    using Xunit;

    public class LocaleTests : DomainTest, IClassFixture<Fixture>
    {
        public LocaleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenLocale_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new LocaleBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            var country = new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE");

            builder.WithCountry(country);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            var language = new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "en");

            builder.WithLanguage(language);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenLocale_WhenDeriving_ThenNameIsSet()
        {
            var locale = new LocaleBuilder(this.Transaction)
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "en"))
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            this.Transaction.Derive();

            Assert.Equal("en-BE", locale.Name);
        }

        [Fact]
        public void GivenLocaleWhenValidatingThenRequiredRelationsMustExist()
        {
            var dutch = new Languages(this.Transaction).LanguageByCode["nl"];
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];

            var builder = new LocaleBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            builder.WithCountry(netherlands).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            builder.WithLanguage(dutch).Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenLocaleWhenValidatingThenNameIsSet()
        {
            var locale = new Locales(this.Transaction).FindBy(this.M.Locale.Name, "nl");

            Assert.Equal("nl", locale.Name);
        }
    }
}
