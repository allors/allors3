// <copyright file="DiscountAdjustmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class DiscountAdjustmentTests : DomainTest, IClassFixture<Fixture>
    {
        public DiscountAdjustmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenDiscountAdjustment_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new DiscountAdjustmentBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithAmount(1);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            builder.WithPercentage(1);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }
    }
}
