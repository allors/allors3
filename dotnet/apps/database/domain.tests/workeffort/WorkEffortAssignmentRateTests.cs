// <copyright file="WorkEffortAssignmentRateTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class WorkEffortAssignmentRateTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortAssignmentRateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkEffort_WhenAddingRates_ThenRateForPartyIsNotAllowed()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            // Calculating rates per party is not implemented yet
            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive(true);

            new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();

            Assert.False(this.Derive().HasErrors);

            new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Transaction).OvertimeRate)
                .Build();

            Assert.False(this.Derive().HasErrors);

            var assignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.WorkEffortRateError));
        }
    }

    public class WorkEffortAssignmentRateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortAssignmentRateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortPartyAssignmentDeriveWorkEffort()
        {
            var workEffortAssignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction).Build();
            this.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            workEffortAssignmentRate.WorkEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).WithAssignment(workTask).Build();
            this.Derive();

            Assert.Equal(workTask, workEffortAssignmentRate.WorkEffort);
        }

        [Fact]
        public void ChangedWorkEffortDeriveWorkEffort()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            var workEffortAssignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction).WithWorkEffort(workTask).Build();
            this.Derive();

            workEffortAssignmentRate.WorkEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).WithAssignment(workTask).Build();
            this.Derive();

            workEffortAssignmentRate.RemoveWorkEffort();
            this.Derive();

            Assert.Equal(workTask, workEffortAssignmentRate.WorkEffort);
        }

        [Fact]
        public void ChangedRateTypeThrowValidationError()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            var workEffortAssignmentRate1 = new WorkEffortAssignmentRateBuilder(this.Transaction).WithWorkEffort(workTask).WithRateType(new RateTypes(this.Transaction).StandardRate).Build();
            var workEffortAssignmentRate2 = new WorkEffortAssignmentRateBuilder(this.Transaction).WithWorkEffort(workTask).WithRateType(new RateTypes(this.Transaction).OvertimeRate).Build();
            this.Derive();

            var errors = this.Derive().Errors.ToList();
            Assert.DoesNotContain(errors, e => e.Message.Contains(ErrorMessages.WorkEffortRateError));

            workEffortAssignmentRate2.RateType = new RateTypes(this.Transaction).StandardRate;

            errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.WorkEffortRateError));
        }
    }
}
