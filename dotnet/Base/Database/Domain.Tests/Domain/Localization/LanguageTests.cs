// <copyright file="LanguageTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Domain;
    using Xunit;

    public class LanguageTests : DomainTest, IClassFixture<Fixture>
    {
        public LanguageTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenLanguageWhenValidatingThenRequiredRelationsMustExist()
        {
            var builder = new LanguageBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            builder.WithIsoCode("XX").Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            builder.WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithLocale(new Locales(this.Transaction).LocaleByName["en"]).WithText("XXX)").Build());

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
