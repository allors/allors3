// <copyright file="AutomatedAgentTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
        public void ChangedNameDeriveDisplayName()
        {
            var automatedAgent = new AutomatedAgentBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            automatedAgent.Name = "name";
            this.Transaction.Derive();

            Assert.Equal("name", automatedAgent.DisplayName);
        }

        [Fact]
        public void ChangedUserNameDeriveDisplayName()
        {
            var automatedAgent = new AutomatedAgentBuilder(this.Transaction)
                .WithName("name")
                .WithUserName("username")
                .Build();
            this.Transaction.Derive();

            Assert.Equal("name", automatedAgent.DisplayName);

            automatedAgent.UserName = null;
            this.Transaction.Derive();

            Assert.Equal("name", automatedAgent.DisplayName);
        }
    }
}
