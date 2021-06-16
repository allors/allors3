// <copyright file="ContactMechanismTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ContactMechanismTests : DomainTest, IClassFixture<Fixture>
    {
        public ContactMechanismTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenTelecommunicationsNumber_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new TelecommunicationsNumberBuilder(this.Transaction);
            var contactMechanism = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithAreaCode("area");
            contactMechanism = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithContactNumber("number");
            contactMechanism = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
