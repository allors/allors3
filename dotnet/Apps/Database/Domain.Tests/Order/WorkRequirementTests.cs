// <copyright file="WorkRequirementTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;
    using TestPopulation;

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

        [Fact]
        public void GivenWorkRequirement_WhenBuild_ThenWorkEffortPurposeDefaultsToRepair()
        {
            var requirement = new WorkRequirementBuilder(this.Transaction).WithDescription("WorkRequirement").Build();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortPurposes(this.Transaction).Repair, requirement.WorkEffortPurpose);
        }

        [Fact]
        public void GivenWorkRequirementWithPurpose_WhenCreateWorkTask_ThenWorkEffortPurposeIsCopied()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            var maintenance = new WorkEffortPurposes(this.Transaction).Maintenance; // non-default, proves copy

            var requirement = new WorkRequirementBuilder(this.Transaction)
                .WithDescription("desc")
                .WithFixedAsset(serialisedItem)
                .WithWorkEffortPurpose(maintenance)
                .Build();

            this.Transaction.Derive();

            requirement.CreateWorkTask();

            this.Transaction.Derive(false);

            var workTask = (WorkTask)requirement.WorkRequirementFulfillmentWhereFullfilledBy.FullfillmentOf;

            Assert.Equal(maintenance, workTask.WorkEffortPurpose);
        }

        [Fact]
        public void ChangedFullfilledByDescriptionDeriveWorkRequirementDescription()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            var requirement = new WorkRequirementBuilder(this.Transaction)
                .WithDescription("origdesc")
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive();

            requirement.CreateWorkTask();
            this.Transaction.Derive(false);

            var fulfillment = requirement.WorkRequirementFulfillmentWhereFullfilledBy;
            Assert.Equal("origdesc", fulfillment.WorkRequirementDescription);

            requirement.Description = "newdesc";
            this.Transaction.Derive();

            Assert.Equal("newdesc", fulfillment.WorkRequirementDescription);
        }
    }
}
