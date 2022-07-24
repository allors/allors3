// <copyright file="VersioningTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Domain;
    using Xunit;

    public class VersioningTests : DomainTest, IClassFixture<Fixture>
    {
        public VersioningTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void InitialNothing()
        {
            var order = new OrderBuilder(this.Session).Build();

            this.Session.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.Equal(order.Amount, version.Amount);
        }

        [Fact]
        public void VersionedUnitRole()
        {
            var order = new OrderBuilder(this.Session)
                .WithAmount(10m)
                .Build();

            this.Session.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.Equal(10m, version.Amount);
            Assert.False(version.ExistOrderState);
            Assert.False(version.ExistOrderLines);
        }

        [Fact]
        public void NonVersionedUnitRole()
        {
            var order = new OrderBuilder(this.Session)
                .WithAmount(10m)
                .Build();

            this.Session.Derive();

            var currentVersion = order.CurrentVersion;

            order.NonVersionedAmount = 20m;

            this.Session.Derive();

            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        [Fact]
        public void InitialCompositeRole()
        {
            var initialObjectState = new OrderStates(this.Session).Initial;

            var order = new OrderBuilder(this.Session)
                .WithOrderState(initialObjectState)
                .Build();

            this.Session.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.False(version.ExistAmount);
            Assert.Equal(initialObjectState, version.OrderState);
            Assert.False(version.ExistOrderLines);
        }

        [Fact]
        public void InitialCompositeRoles()
        {
            var orderLine = new OrderLineBuilder(this.Session).Build();

            var order = new OrderBuilder(this.Session)
                .WithOrderLine(orderLine)
                .Build();

            this.Session.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.False(version.ExistAmount);
            Assert.False(version.ExistOrderState);
            Assert.Single(version.OrderLines);
            Assert.Equal(orderLine, version.OrderLines.ElementAt(0));
        }
    }
}
