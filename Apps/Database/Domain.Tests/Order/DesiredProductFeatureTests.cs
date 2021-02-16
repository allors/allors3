// <copyright file="DesiredProductFeatureTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class DesiredProductFeatureTests : DomainTest, IClassFixture<Fixture>
    {
        public DesiredProductFeatureTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenDesiredProductFeature_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Transaction).WithRate(21).Build();
            var softwareFeature = new SoftwareFeatureBuilder(this.Transaction)
                .WithVatRate(vatRate21)
                .WithName("Tutorial")
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new DesiredProductFeatureBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithRequired(false);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithProductFeature(softwareFeature);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
