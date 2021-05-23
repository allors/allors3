// <copyright file="EngagementItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EngagementItemTests : DomainTest, IClassFixture<Fixture>
    {
        public EngagementItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCustomEngagementItem_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new CustomEngagementItemBuilder(this.Transaction);
            var customEngagementItem = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("CustomEngagementItem");
            customEngagementItem = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenDeliverableOrderItem_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new DeliverableOrderItemBuilder(this.Transaction);
            var deliverableOrderItem = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("DeliverableOrderItem");
            deliverableOrderItem = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenGoodOrderItem_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new GoodOrderItemBuilder(this.Transaction);
            var goodOrderItem = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("GoodOrderItem");
            goodOrderItem = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenProfessionalPlacement_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new ProfessionalPlacementBuilder(this.Transaction);
            var professionalPlacement = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("ProfessionalPlacement");
            professionalPlacement = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenStandardServiceOrderItem_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new StandardServiceOrderItemBuilder(this.Transaction);
            var standardServiceOrderItem = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("StandardServiceOrderItem");
            standardServiceOrderItem = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
