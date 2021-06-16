// <copyright file="ProductFeatureTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ProductFeatureTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductFeatureTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenDimension_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new DimensionBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("name");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenModel_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ModelBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Mt");

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenServiceFeature_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ServiceFeatureBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Mt");

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSizeConstant_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new SizeBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Mt");

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSoftwareFeature_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new SoftwareFeatureBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Mt");

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenProductQualityConstant_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ProductQualityBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Mt");

            Assert.False(this.Derive().HasErrors);
        }
    }
}
