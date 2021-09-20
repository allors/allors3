// <copyright file="CostCenterCategoryTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CostCenterCategoryTests : DomainTest, IClassFixture<Fixture>
    {
        public CostCenterCategoryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCostCenterCategory_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new CostCenterCategoryBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("CostCenterCategory");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenCostCenterCategory_WhenDeriving_ThenPostBuildRelationsMustExist()
        {
            var costCenterCategory = new CostCenterCategoryBuilder(this.Transaction)
                .WithDescription("CostCenterCategory")
                .Build();

            Assert.True(costCenterCategory.ExistUniqueId);
        }
    }
}
