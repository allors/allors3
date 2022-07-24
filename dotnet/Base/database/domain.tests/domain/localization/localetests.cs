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
            var builder = new LocaleBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            var language = new Languages(this.Session).FindBy(this.M.Language.IsoCode, "en");

            builder.WithLanguage(language);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            var country = new Countries(this.Session).FindBy(this.M.Country.IsoCode, "BE");

            builder.WithCountry(country);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenLocale_WhenDeriving_ThenNameIsSet()
        {
            var locale = new LocaleBuilder(this.Session)
                .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "en"))
                .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            this.Session.Derive();

            Assert.Equal("en-BE", locale.Name);
        }

        [Fact]
        public void GivenLocaleWhenValidatingThenRequiredRelationsMustExist()
        {
            var dutch = new Languages(this.Session).LanguageByCode["nl"];
            var netherlands = new Countries(this.Session).CountryByIsoCode["NL"];

            var builder = new LocaleBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            builder.WithLanguage(dutch).Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            builder.WithCountry(netherlands).Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenLocaleWhenValidatingThenNameIsSet()
        {
            var locale = new Locales(this.Session).FindBy(this.M.Locale.Name, Locales.DutchNetherlandsName);

            Assert.Equal("nl-NL", locale.Name);
        }
    }
}
