// <copyright file="WorkEffortPartyAssignmentTests.cs" company="Allors bvba">
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

    public class WorkEffortPartyAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortPartyAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkEffortAndTimeEntry_WhenDeriving_ThenWorkEffortPartyAssignmentSynced()
        {
            // Arrange
            var customer = new OrganisationBuilder(this.Session).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Session).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Session).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Session).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Session.Derive(true);

            var today = DateTimeFactory.CreateDateTime(this.Session.Now());
            var tomorrow = DateTimeFactory.CreateDateTime(this.Session.Now().AddDays(1));
            var hour = new TimeFrequencies(this.Session).Hour;

            var timeEntry = new TimeEntryBuilder(this.Session)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(tomorrow)
                .WithTimeFrequency(hour)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            this.Session.Derive(true);

            // Assert
            var partyAssignment = workOrder.WorkEffortPartyAssignmentsWhereAssignment.First;

            Assert.Equal(workOrder, partyAssignment.Assignment);
            Assert.Equal(employee, partyAssignment.Party);
            Assert.False(partyAssignment.ExistFromDate);
            Assert.False(partyAssignment.ExistThroughDate);
        }

        [Fact]
        public void GivenTimeEntryWithRequiredAssignmentOrganisation_WhenDeriving_ThenWorkEffortPartyAssignmentSynced()
        {
            // Arrange
            var customer = new OrganisationBuilder(this.Session).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Session).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Session).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Session).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            internalOrganisation.RequireExistingWorkEffortPartyAssignment = true;
            this.Session.Derive(true);

            var today = DateTimeFactory.CreateDateTime(this.Session.Now());
            var tomorrow = DateTimeFactory.CreateDateTime(this.Session.Now().AddDays(1));
            var hour = new TimeFrequencies(this.Session).Hour;

            var timeEntry = new TimeEntryBuilder(this.Session)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(tomorrow)
                .WithTimeFrequency(hour)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains("No Work Effort Party Assignment matches Worker"));

            // Re-Arrange
            employee.TimeSheetWhereWorker.RemoveTimeEntries();

            var assignment = new WorkEffortPartyAssignmentBuilder(this.Session)
                .WithAssignment(workOrder)
                .WithParty(employee)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);

            // Assert
            Assert.Empty(errors);
        }

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

            var workEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Session).WithAssignment(workOrder).WithParty(employee).Build();

            this.Session.Derive(true);

            var assignedRate = new WorkEffortAssignmentRateBuilder(this.Session)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            workEffortPartyAssignment.AddAssignmentRate(assignedRate);

            var expectedMessage = $"{workEffortPartyAssignment}, { this.M.WorkEffortPartyAssignment.AssignmentRates}, { ErrorMessages.WorkEffortRateError}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
