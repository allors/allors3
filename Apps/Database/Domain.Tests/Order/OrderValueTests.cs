// <copyright file="OrderValueTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Xunit;

    public class OrderValueTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderValueTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnBuildThrowValidationError()
        {
            var orderValue = new OrderValueBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors, e => e.Message == "OrderValue.FromAmount, OrderValue.ThroughAmount at least one");
        }
    }
}
