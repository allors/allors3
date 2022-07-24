// <copyright file="SingletonTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class SingletonTests : DomainTest, IClassFixture<Fixture>
    {
        public SingletonTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSingleton_WhenBuild_ThenLogoImageMustExist()
        {
            var singleton = this.Transaction.GetSingleton();

            Assert.True(singleton.ExistLogoImage);
        }

        [Fact]
        public void GivenNewAdditionaleLocale_WhenDeriving_ThenLocalesIsDerived()
        {
            var singleton = this.Transaction.GetSingleton();
            var localeCount = singleton.Locales.Count();

            singleton.AddAdditionalLocale(new Locales(this.Transaction).EnglishUnitedStates);

            this.Transaction.Derive();

            Assert.Contains(singleton.DefaultLocale, singleton.Locales);
            Assert.Contains(new Locales(this.Transaction).EnglishUnitedStates, singleton.Locales);
            Assert.Equal(localeCount + 1, singleton.Locales.Count());
        }
    }
}
