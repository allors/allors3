// <copyright file="OrderValueTests.cs" company="Allors bvba">
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

    public class OrderValueTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderValueTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnBuildThrowValidationError()
        {
            var orderValue = new OrderValueBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors, e => e.Message == "OrderValue.FromAmount, OrderValue.ThroughAmount at least one");
        }
    }
}
