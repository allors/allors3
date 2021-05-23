// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrderTermTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderTermTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrderTerm_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new OrderTermBuilder(this.Transaction);
            var salesTerm = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithTermType(new OrderTermTypes(this.Transaction).PercentageCancellationCharge);
            salesTerm = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
