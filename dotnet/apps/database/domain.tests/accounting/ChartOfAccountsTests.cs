// <copyright file="ChartOfAccountsTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ChartOfAccountsTests : DomainTest, IClassFixture<Fixture>
    {
        public ChartOfAccountsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenChartOfAccounts_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ChartOfAccountsBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithName("ChartOfAccounts");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenChartOfAccounts_WhenDeriving_ThenPostBuildRelationsMustExist()
        {
            var chartOfAccounts = new ChartOfAccountsBuilder(this.Transaction)
                .WithName("ChartOfAccounts")
                .Build();

            Assert.True(chartOfAccounts.ExistUniqueId);
        }
    }
}
