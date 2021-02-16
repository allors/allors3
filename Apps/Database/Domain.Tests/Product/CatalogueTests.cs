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
            var catalogue = new CatalogueBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(new Scopes(this.Transaction).Public, catalogue.CatScope);
        }

        [Fact]
        public void DeriveCatalogueImage()
        {
            var catalogue = new CatalogueBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.Transaction.GetSingleton().Settings.NoImageAvailableImage, catalogue.CatalogueImage);
        }
    }

    public class CatalogueDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CatalogueDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedLocalisedNamesDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build();

            var catalogue = new CatalogueBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            catalogue.AddLocalisedName(localisedName);
            this.Transaction.Derive(false);

            Assert.Equal(catalogue.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build();

            var catalogue = new CatalogueBuilder(this.Transaction).WithLocalisedName(localisedName).Build();
            this.Transaction.Derive(false);

            localisedName.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Equal(catalogue.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedDescriptionsDeriveDescription()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedDesc = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultdesc").Build();

            var catalogue = new CatalogueBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            catalogue.AddLocalisedDescription(localisedDesc);
            this.Transaction.Derive(false);

            Assert.Equal(catalogue.Description, localisedDesc.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedDesc = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultdesc").Build();

            var catalogue = new CatalogueBuilder(this.Transaction).WithLocalisedDescription(localisedDesc).Build();
            this.Transaction.Derive(false);

            localisedDesc.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Equal(catalogue.Description, localisedDesc.Text);
        }

        [Fact]
        public void ChangedCatalogueImageDeriveCatalogueImage()
        {
            var noImageAvailableImage = this.Transaction.GetSingleton().Settings.NoImageAvailableImage;

            var catalogue = new CatalogueBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            catalogue.RemoveCatalogueImage();
            this.Transaction.Derive(false);

            Assert.Equal(noImageAvailableImage, catalogue.CatalogueImage);
        }
    }
}
