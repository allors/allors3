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
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive(true);

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var hour = new TimeFrequencies(this.Transaction).Hour;

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(tomorrow)
                .WithTimeFrequency(hour)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            this.Transaction.Derive(true);

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
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            internalOrganisation.RequireExistingWorkEffortPartyAssignment = true;
            this.Transaction.Derive(true);

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var hour = new TimeFrequencies(this.Transaction).Hour;

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(tomorrow)
                .WithTimeFrequency(hour)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("No Work Effort Party Assignment matches Worker"));

            // Re-Arrange
            employee.TimeSheetWhereWorker.RemoveTimeEntries();

            var assignment = new WorkEffortPartyAssignmentBuilder(this.Transaction)
                .WithAssignment(workOrder)
                .WithParty(employee)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            errors = this.Transaction.Derive(false).Errors.ToList();

            // Assert
            Assert.Empty(errors);
        }

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

            var workEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).WithAssignment(workOrder).WithParty(employee).Build();

            this.Transaction.Derive(true);

            var assignedRate = new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workOrder)
                .WithRate(1)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            workEffortPartyAssignment.AddAssignmentRate(assignedRate);

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.WorkEffortRateError));
        }
    }
}
