// <copyright file="PartCategoryTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PartCategoryRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartCategoryRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;

            var partCategory = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Null(partCategory.Name);

            partCategory.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("name").Build());
            this.Derive();

            Assert.Equal("name", partCategory.Name);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;

            var partCategory = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Null(partCategory.Description);

            partCategory.AddLocalisedDescription(new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("description").Build());
            this.Derive();

            Assert.Equal("description", partCategory.Description);
        }

        [Fact]
        public void ChangedCategoryImageDeriveCategoryImage()
        {
            var noImageAvailableImage = this.Transaction.GetSingleton().Settings.NoImageAvailableImage;

            var partCategory = new PartCategoryBuilder(this.Transaction).WithName("name").Build();
            this.Derive();

            partCategory.RemoveCategoryImage();
            this.Derive();

            Assert.Equal(noImageAvailableImage, partCategory.CategoryImage);
        }

        [Fact]
        public void ChangedPrimaryParentValidationError()
        {
            var partCategory1 = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var partCategory2 = new PartCategoryBuilder(this.Transaction).WithPrimaryParent(partCategory1).Build();
            this.Derive();

            partCategory1.PrimaryParent = partCategory2;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("Cycle detected in"));
        }

        [Fact]
        public void ChangedPrimaryParentDeriveChildren()
        {
            var partCategory1 = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var partCategory11 = new PartCategoryBuilder(this.Transaction).WithPrimaryParent(partCategory1).Build();
            this.Derive();

            Assert.Single(partCategory1.Children);
            Assert.Contains(partCategory11, partCategory1.Children);
        }

        [Fact]
        public void ChangedSecondaryParentDeriveChildren()
        {
            var partCategory1 = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var partCategory11 = new PartCategoryBuilder(this.Transaction).WithPrimaryParent(partCategory1).Build();
            this.Derive();

            var partCategory12 = new PartCategoryBuilder(this.Transaction).WithSecondaryParent(partCategory1).Build();
            this.Derive();

            Assert.Equal(2, partCategory1.Children.Count());
            Assert.Contains(partCategory11, partCategory1.Children);
            Assert.Contains(partCategory12, partCategory1.Children);
        }

        [Fact]
        public void ChangedChildrenDeriveDescendants()
        {
            var partCategory111 = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var partCategory11 = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var partCategory1 = new PartCategoryBuilder(this.Transaction).Build();
            this.Derive();

            partCategory11.PrimaryParent = partCategory1;
            partCategory111.PrimaryParent = partCategory11;
            this.Derive();

            Assert.Equal(2, partCategory1.Descendants.Count());
            Assert.Contains(partCategory11, partCategory1.Descendants);
            Assert.Contains(partCategory111, partCategory1.Descendants);
        }
    }
}
