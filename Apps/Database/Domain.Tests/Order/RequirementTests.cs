// <copyright file="RequirementTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class RequirementTests : DomainTest, IClassFixture<Fixture>
    {
        public RequirementTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCustomerRequirement_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var requirement = new RequirementBuilder(this.Transaction).WithDescription("CustomerRequirement").Build();

            this.Transaction.Derive();

            Assert.Equal(new RequirementStates(this.Transaction).Active, requirement.RequirementState);
            Assert.Equal(requirement.LastRequirementState, requirement.RequirementState);
        }

        [Fact]
        public void GivenCustomerRequirement_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var requirement = new RequirementBuilder(this.Transaction).WithDescription("CustomerRequirement").Build();

            this.Transaction.Derive();

            Assert.Null(requirement.PreviousRequirementState);
        }

        [Fact]
        public void GivenCustomerRequirement_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new RequirementBuilder(this.Transaction);
            var customerRequirement = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("CustomerRequirement");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
