// <copyright file="OrderQuantityBreakTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class OrderQuantityBreakTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderQuantityBreakTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnBuildThrowValidationError()
        {
            var orderQuantityBreak = new OrderQuantityBreakBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors, e => e.Message == "OrderQuantityBreak.FromAmount, OrderQuantityBreak.ThroughAmount at least one");
        }
    }
}
