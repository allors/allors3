// <copyright file="PassportTests.cs" company="Allors bvba">
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

    public class PassportTests : DomainTest, IClassFixture<Fixture>
    {
        public PassportTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPartyWithOpenOrders_WhenDeriving_ThenOpenOrderAmountIsUpdated()
        {
            new PassportBuilder(this.Transaction).WithNumber("1").Build();
            this.Derive();

            new PassportBuilder(this.Transaction).WithNumber("1").Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Equals("Passport.Number is not unique"));
        }
    }
}
