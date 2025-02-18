// <copyright file="WorkRequirementTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class WorkRequirementTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkRequirementTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkRequirement_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var requirement = new WorkRequirementBuilder(this.Transaction).WithDescription("WorkRequirement").Build();

            this.Transaction.Derive();

            Assert.Equal(new RequirementStates(this.Transaction).Created, requirement.RequirementState);
            Assert.Equal(requirement.LastRequirementState, requirement.RequirementState);
        }

        [Fact]
        public void GivenWorkRequirement_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var requirement = new WorkRequirementBuilder(this.Transaction).WithDescription("WorkRequirement").Build();

            this.Transaction.Derive();

            Assert.Null(requirement.PreviousRequirementState);
        }

        [Fact]
        public void GivenWorkRequirement_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new WorkRequirementBuilder(this.Transaction);
            var WorkRequirement = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("WorkRequirement");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
