// <copyright file="SingletonTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Xunit;

    public class SingletonTests : DomainTest, IClassFixture<Fixture>
    {
        public SingletonTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSingleton_WhenCreated_ThenLogoImageMustExist()
        {
            var singleton = this.Session.GetSingleton();

            Assert.True(singleton.ExistLogoImage);
        }

        [Fact]
        public void GivenNewAdditionaleLocale_WhenDeriving_ThenLocalesIsDerived()
        {
            var singleton = this.Session.GetSingleton();
            var localeCount = singleton.Locales.Count;

            singleton.AddAdditionalLocale(new Locales(this.Session).EnglishUnitedStates);

            this.Session.Derive();

            Assert.Contains(singleton.DefaultLocale, singleton.Locales);
            Assert.Contains(new Locales(this.Session).EnglishUnitedStates, singleton.Locales);
            Assert.Equal(localeCount + 1, singleton.Locales.Count);
        }
    }
}
