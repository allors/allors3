// <copyright file="DerivationNodesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CustomPatternTest : DomainTest, IClassFixture<Fixture>
    {
        public CustomPatternTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void UnitRoles()
        {
            var person = new PersonBuilder(this.Transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .Build();

            this.Transaction.Derive();

            Assert.Equal("Jane Doe", person.CustomFullName);
        }
    }
}
