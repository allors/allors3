// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CatalogueOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public CatalogueOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveCatScope()
        {
            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(new Scopes(this.Session).Public, catalogue.CatScope);
        }

        [Fact]
        public void DeriveCatalogueImage()
        {
            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(this.Session.GetSingleton().Settings.NoImageAvailableImage, catalogue.CatalogueImage);
        }
    }

    public class CatalogueDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CatalogueDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedLocalisedNamesDeriveName()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            catalogue.AddLocalisedName(localisedName);
            this.Session.Derive(false);

            Assert.Equal(catalogue.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var catalogue = new CatalogueBuilder(this.Session).WithLocalisedName(localisedName).Build();
            this.Session.Derive(false);

            localisedName.Text = "changed";
            this.Session.Derive(false);

            Assert.Equal(catalogue.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedDescriptionsDeriveDescription()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedDesc = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultdesc").Build();

            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            catalogue.AddLocalisedDescription(localisedDesc);
            this.Session.Derive(false);

            Assert.Equal(catalogue.Description, localisedDesc.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedDesc = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultdesc").Build();

            var catalogue = new CatalogueBuilder(this.Session).WithLocalisedDescription(localisedDesc).Build();
            this.Session.Derive(false);

            localisedDesc.Text = "changed";
            this.Session.Derive(false);

            Assert.Equal(catalogue.Description, localisedDesc.Text);
        }

        [Fact]
        public void ChangedCatalogueImageDeriveCatalogueImage()
        {
            var noImageAvailableImage = this.Session.GetSingleton().Settings.NoImageAvailableImage;

            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            catalogue.RemoveCatalogueImage();
            this.Session.Derive(false);

            Assert.Equal(noImageAvailableImage, catalogue.CatalogueImage);
        }
    }
}
