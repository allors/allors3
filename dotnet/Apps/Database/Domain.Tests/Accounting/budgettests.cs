// <copyright file="BudgetTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class BudgetTests : DomainTest, IClassFixture<Fixture>
    {
        public BudgetTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOperatingBudget_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var budget = new OperatingBudgetBuilder(this.Transaction)
                .WithDescription("Budget")
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new BudgetStates(this.Transaction).Opened, budget.BudgetState);
            Assert.Equal(budget.LastBudgetState, budget.BudgetState);
        }

        [Fact]
        public void GivenOperatingBudget_WhenBuild_ThenPreviousObjectStateIsNUll()
        {
            var budget = new OperatingBudgetBuilder(this.Transaction)
                .WithDescription("Budget")
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.Transaction.Derive();

            Assert.Null(budget.PreviousBudgetState);
        }

        [Fact]
        public void GivenOperatingBudget_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OperatingBudgetBuilder(this.Transaction);
            var budget = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("Budget");
            budget = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
