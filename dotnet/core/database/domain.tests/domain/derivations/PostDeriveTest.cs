// <copyright file="RequiredTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PostDeriveTest : DomainTest, IClassFixture<Fixture>
    {
        public PostDeriveTest(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void CycleAgain()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();

            Assert.False(organisation.PostDeriveTrigger);
            Assert.False(organisation.PostDeriveTriggered);

            this.Transaction.Derive(false);

            Assert.True(organisation.PostDeriveTrigger);
            Assert.True(organisation.PostDeriveTriggered);
        }
    }
}
