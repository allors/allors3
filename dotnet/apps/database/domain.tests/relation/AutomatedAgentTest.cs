// <copyright file="AutomatedAgentTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class AutomatedAgentTest : DomainTest, IClassFixture<Fixture>
    {
        public AutomatedAgentTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNameDerivePartyName()
        {
            var automatedAgent = new AutomatedAgentBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            automatedAgent.Name = "name";

            Assert.Equal("name", automatedAgent.Name);
        }
    }
}
