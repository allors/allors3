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
            var customer = new OrganisationBuilder(this.Session).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            // Calculating rates per party is not implemented yet
            var workOrder = new WorkTaskBuilder(this.Session).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Session).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Session).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Session.Derive(true);

            new WorkEffortAssignmentRateBuilder(this.Session)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            new WorkEffortAssignmentRateBuilder(this.Session)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Session).OvertimeRate)
                .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            var assignmentRate = new WorkEffortAssignmentRateBuilder(this.Session)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .Build();

            var expectedMessage = $"{assignmentRate}, { this.M.WorkEffortAssignmentRate.RateType}, { ErrorMessages.WorkEffortRateError}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }

    public class WorkEffortAssignmentRateDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortAssignmentRateDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortPartyAssignmentDeriveWorkEffort()
        {
            var workEffortAssignmentRate = new WorkEffortAssignmentRateBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workTask = new WorkTaskBuilder(this.Session).Build();
            workEffortAssignmentRate.WorkEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Session).WithAssignment(workTask).Build();
            this.Session.Derive(false);

            Assert.Equal(workTask, workEffortAssignmentRate.WorkEffort);
        }

        [Fact]
        public void ChangedWorkEffortDeriveWorkEffort()
        {
            var workTask = new WorkTaskBuilder(this.Session).Build();
            var workEffortAssignmentRate = new WorkEffortAssignmentRateBuilder(this.Session).WithWorkEffort(workTask).Build();
            this.Session.Derive(false);

            workEffortAssignmentRate.WorkEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Session).WithAssignment(workTask).Build();
            this.Session.Derive(false);

            workEffortAssignmentRate.RemoveWorkEffort();
            this.Session.Derive(false);

            Assert.Equal(workTask, workEffortAssignmentRate.WorkEffort);
        }

        [Fact]
        public void ChangedRateTypeThrowValidationError()
        {
            var workTask = new WorkTaskBuilder(this.Session).Build();
            var workEffortAssignmentRate1 = new WorkEffortAssignmentRateBuilder(this.Session).WithWorkEffort(workTask).WithRateType(new RateTypes(this.Session).StandardRate).Build();
            var workEffortAssignmentRate2 = new WorkEffortAssignmentRateBuilder(this.Session).WithWorkEffort(workTask).WithRateType(new RateTypes(this.Session).OvertimeRate).Build();
            this.Session.Derive(false);

            var expectedMessage = $"{workEffortAssignmentRate2}, { this.M.WorkEffortAssignmentRate.RateType}, { ErrorMessages.WorkEffortRateError}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals(expectedMessage));

            workEffortAssignmentRate2.RateType = new RateTypes(this.Session).StandardRate;

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
