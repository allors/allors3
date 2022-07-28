// <copyright file="OrderKindTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrderKindTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderKindTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrderKind_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OrderKindBuilder(this.Transaction);
            var orderKind = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("orderkind");
            orderKind = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenOrderKind_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var orderKind = new OrderKindBuilder(this.Transaction)
                .WithDescription("Pre order summer collections")
                .Build();

            this.Transaction.Derive();

            Assert.False(orderKind.ScheduleManually);
        }
    }
}
