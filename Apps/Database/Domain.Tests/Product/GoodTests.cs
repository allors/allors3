
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
            this.Transaction.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Single(nonUnifiedGood.ProductIdentifications);
        }
    }

    public class GoodRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public GoodRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Transaction.GetSingleton().Settings;
            settings.UseProductNumberCounter = false;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.False(nonUnifiedGood.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Transaction)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build();

            nonUnifiedGood.AddProductIdentification(goodIdentification);
            this.Transaction.Derive(false);

            Assert.True(nonUnifiedGood.ExistProductNumber);
        }

        [Fact]
        public void ChangedLocalisedNamesDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.AddLocalisedName(localisedName);
            this.Transaction.Derive(false);

            Assert.Equal(nonUnifiedGood.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithLocalisedName(localisedName).Build();
            this.Transaction.Derive(false);

            localisedName.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Equal(nonUnifiedGood.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedDescriptionsDeriveDescription()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedDescription = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.AddLocalisedDescription(localisedDescription);
            this.Transaction.Derive(false);

            Assert.Equal(nonUnifiedGood.Description, localisedDescription.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;
            var localisedDescription = new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithLocalisedDescription(localisedDescription).Build();
            this.Transaction.Derive(false);

            localisedDescription.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Equal(nonUnifiedGood.Description, localisedDescription.Text);
        }
    }
}
