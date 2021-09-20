// <copyright file="SurchargeAdjustmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SurchargeAdjustmentTests : DomainTest, IClassFixture<Fixture>
    {
        public SurchargeAdjustmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSurchargeAdjustment_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new SurchargeAdjustmentBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithAmount(1);
            builder.Build();

            Assert.False(this.Derive().HasErrors);

            builder.WithPercentage(1);
            builder.Build();

            Assert.True(this.Derive().HasErrors);
        }
    }
}
