// <copyright file="LocalizedTextTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Domain;
    using Xunit;

    public class LocalisedTextTests : DomainTest, IClassFixture<Fixture>
    {
        public LocalisedTextTests(Fixture fixture) : base(fixture) { }

        [Fact(Skip = "TODO: Koen  Locale is required")]
        public void GivenLocalisedTextWhenValidatingThenRequiredRelationsMustExist()
        {
            var builder = new LocalisedTextBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithLocale(new Locales(this.Session).EnglishGreatBritain);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }
    }
}
