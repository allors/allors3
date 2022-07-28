// <copyright file="EstimatedProductCostTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EstimatedProductCostTests : DomainTest, IClassFixture<Fixture>
    {
        public EstimatedProductCostTests(Fixture fixture) : base(fixture) { }


        [Fact]
        public void GivenEstimatedLaborCost_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new EstimatedLaborCostBuilder(this.Transaction);
            var laborCost = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCost(1);
            laborCost = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"));
            laborCost = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenEstimatedMaterialCost_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new EstimatedMaterialCostBuilder(this.Transaction);
            var materialCost = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCost(1);
            materialCost = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"));
            materialCost = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenEstimatedOtherCost_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new EstimatedOtherCostBuilder(this.Transaction);
            var otherCost = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCost(1);
            otherCost = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"));
            otherCost = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
