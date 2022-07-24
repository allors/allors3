// <copyright file="CostCenterTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CostCenterTests : DomainTest, IClassFixture<Fixture>
    {
        public CostCenterTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCostCenter_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new CostCenterBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("CostCenter");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenCostCenter_WhenDeriving_ThenPostBuildRelationsMustExist()
        {
            var costCenter = new CostCenterBuilder(this.Transaction)
                .WithName("CostCenter")
                .Build();

            Assert.True(costCenter.ExistUniqueId);
        }
    }
}
