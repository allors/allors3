// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    public class CatalogueDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CatalogueDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedCatalogueDeriveCatScope()
        {
            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(new Scopes(this.Session).Public, catalogue.CatScope);
        }

        [Fact]
        public void OnCreatedCatalogueWithNameAndLocalisedNameDeriveName()
        {
            var locale = new Locales(this.Session).EnglishGreatBritain;
            var text = new LocalisedTextBuilder(this.Session).WithLocale(locale).WithText("Catalogue One").Build();

            var catalogue = new CatalogueBuilder(this.Session)
                .WithName("Catalogus één")
                .WithLocalisedName(text)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(catalogue.Name, text.Text);
        }

        [Fact]
        public void OnCreatedCatalogueWithDescriptionAndLocalisedDescriptionDeriveDescription()
        {
            var locale = new Locales(this.Session).EnglishGreatBritain;
            var text = new LocalisedTextBuilder(this.Session).WithLocale(locale).WithText("Description One").Build();

            var catalogue = new CatalogueBuilder(this.Session)
                .WithDescription("Beschrijving één")
                .WithLocalisedDescription(text)
                .Build();
            this.Session.Derive(false);

            Assert.Equal(catalogue.Description, text.Text);
        }

        [Fact]
        public void OnCreatedCatalogueWithoutImageDeriveImage()
        {
            var catalogue = new CatalogueBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(this.Session.GetSingleton().Settings.NoImageAvailableImage, catalogue.CatalogueImage);
        }

        // TODO: Test CatalogueImage
    }
}
