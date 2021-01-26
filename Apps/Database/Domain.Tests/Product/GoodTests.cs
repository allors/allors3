
// <copyright file="GoodTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using TestPopulation;
    using Xunit;

    public class GoodTests : DomainTest, IClassFixture<Fixture>
    {
        public GoodTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnInitAddProductIdentification()
        {
            this.Session.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Single(nonUnifiedGood.ProductIdentifications);
        }
    }

    public class GoodDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public GoodDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Session.GetSingleton().Settings;
            settings.UseProductNumberCounter = false;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.False(nonUnifiedGood.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Session)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build();

            nonUnifiedGood.AddProductIdentification(goodIdentification);
            this.Session.Derive(false);

            Assert.True(nonUnifiedGood.ExistProductNumber);
        }

        [Fact]
        public void ChangedLocalisedNamesDeriveName()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddLocalisedName(localisedName);
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithLocalisedName(localisedName).Build();
            this.Session.Derive(false);

            localisedName.Text = "changed";
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedDescriptionsDeriveDescription()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedDescription = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddLocalisedDescription(localisedDescription);
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Description, localisedDescription.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedDescription = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithLocalisedDescription(localisedDescription).Build();
            this.Session.Derive(false);

            localisedDescription.Text = "changed";
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Description, localisedDescription.Text);
        }
    }
}
