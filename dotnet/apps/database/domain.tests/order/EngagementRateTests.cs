// <copyright file="EngagementRateTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EngagementRateTests : DomainTest, IClassFixture<Fixture>
    {
        public EngagementRateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenEngagementRate_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new EngagementRateBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithBillingRate(10M);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithRateType(new RateTypes(this.Transaction).StandardRate);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
