// <copyright file="PassportTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class PassportTests : DomainTest, IClassFixture<Fixture>
    {
        public PassportTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPartyWithOpenOrders_WhenDeriving_ThenOpenOrderAmountIsUpdated()
        {
            new PassportBuilder(this.Session).WithNumber("1").Build();
            this.Session.Derive(false);

            new PassportBuilder(this.Session).WithNumber("1").Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("Passport.Number is not unique"));
        }
    }
}
