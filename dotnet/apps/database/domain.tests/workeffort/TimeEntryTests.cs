// <copyright file="TimeEntryTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Xunit;

    public class TimeEntryTests : DomainTest, IClassFixture<Fixture>
    {
        public TimeEntryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenTimeEntry_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            // Arrange
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();

            // Act
            var derivation = this.Derive();
            var originalCount = derivation.Errors.Count();

            // Assert
            Assert.True(derivation.HasErrors);

            //// Re-arrange
            var tomorrow = this.Transaction.Now().AddDays(1);
            timeEntry.ThroughDate = tomorrow;

            // Act
            derivation = this.Derive();

            // Assert
            Assert.True(derivation.HasErrors);
            Assert.Equal(originalCount, derivation.Errors.Count());

            //// Re-arrange
            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Work").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            timeEntry.WorkEffort = workOrder;

            // Act
            derivation = this.Derive();

            // Assert
            Assert.True(derivation.HasErrors);
            Assert.Equal(originalCount - 1, derivation.Errors.Count());

            //// Re-arrange
            var worker = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(worker).WithEmployer(internalOrganisation).Build();

            derivation = this.Derive();

            worker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            derivation = this.Derive();

            // Assert
            Assert.False(derivation.HasErrors);
        }

        [Fact]
        public void GivenTimeEntryWithFromAndThroughDates_WhenDeriving_ThenAmountOfTimeDerived()
        {
            // Arrange
            var frequencies = new TimeFrequencies(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive(true);

            var now = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var later = DateTimeFactory.CreateDateTime(now.AddHours(4));

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(now)
                .WithThroughDate(later)
                .WithTimeFrequency(frequencies.Hour)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(4.00M, timeEntry.AmountOfTime);
            Assert.Equal(4.00M, timeEntry.ActualHours);

            //// Re-arrange
            timeEntry.RemoveAmountOfTime();
            timeEntry.TimeFrequency = frequencies.Day;

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(Math.Round(4.0M / 24.0M, this.M.TimeEntry.AmountOfTime.Scale ?? 2), timeEntry.AmountOfTime);
            Assert.Equal(4.00M, timeEntry.ActualHours);

            //// Re-arrange
            timeEntry.RemoveAmountOfTime();
            timeEntry.TimeFrequency = frequencies.Minute;

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(4.0M * 60.0M, timeEntry.AmountOfTime);
            Assert.Equal(4.00M, timeEntry.ActualHours);
        }

        [Fact]
        public void GivenTimeEntryWithFromDateAndAmountOfTime_WhenDeriving_ThenThroughDateDerived()
        {
            // Arrange
            var frequencies = new TimeFrequencies(this.Transaction);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive(true);

            var now = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var hour = frequencies.Hour;

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(now)
                .WithAssignedAmountOfTime(4.0M)
                .WithTimeFrequency(hour)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            // Act
            this.Transaction.Derive(true);

            // Assert
            var timeSpan = timeEntry.ThroughDate - timeEntry.FromDate;
            Assert.Equal(4.00, timeSpan.Value.TotalHours);
            Assert.Equal(4.0M, timeEntry.ActualHours);

            //// Re-arrange
            timeEntry.RemoveThroughDate();
            timeEntry.TimeFrequency = frequencies.Minute;

            // Act
            this.Transaction.Derive(true);

            // Assert
            timeSpan = timeEntry.ThroughDate - timeEntry.FromDate;
            Assert.Equal(4.00, timeSpan.Value.TotalMinutes);
            Assert.Equal(Math.Round(4.0M / 60.0M, this.M.TimeEntry.AmountOfTime.Scale ?? 2), timeEntry.ActualHours);

            //// Re-arrange
            timeEntry.RemoveThroughDate();
            timeEntry.TimeFrequency = frequencies.Day;

            // Act
            this.Transaction.Derive(true);

            // Assert
            timeSpan = timeEntry.ThroughDate - timeEntry.FromDate;
            Assert.Equal(4.00, timeSpan.Value.TotalDays);
            Assert.Equal(4.00M * 24.00M, timeEntry.ActualHours);
        }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new TimeEntryBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("TimeEntry.WorkEffort, TimeEntry.EngagementItem at least one"));
        }

        [Fact]
        public void OnCreatedThrowValidationError_2()
        {
            var basePrice = new TimeEntryBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("TimeEntry.TimeSheetWhereTimeEntry is required"));
        }

        //[Fact]
        //public void GivenActiveTimeEntry_WhenCreatingNewEntryForSamePerson_ThenDerivationError()
        //{
        //    // Arrange
        //    var frequencies = new TimeFrequencies(this.Transaction);

        //    var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
        //    var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
        //    new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

        //    var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();
        //    var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
        //    new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

        //    this.Transaction.Derive(true);

        //    var now = DateTimeFactory.CreateDateTime(this.Transaction.Now());
        //    var later = DateTimeFactory.CreateDateTime(now.AddHours(4));

        //    var timeEntry = new TimeEntryBuilder(this.Transaction)
        //        .WithRateType(new RateTypes(this.Transaction).StandardRate)
        //        .WithFromDate(now.AddSeconds(-1))
        //        .WithTimeFrequency(frequencies.Hour)
        //        .WithWorkEffort(workOrder)
        //        .Build();

        //    employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

        //    this.Transaction.Derive(true);

        //    var secondTimeEntry = new TimeEntryBuilder(this.Transaction)
        //        .WithRateType(new RateTypes(this.Transaction).StandardRate)
        //        .WithFromDate(now)
        //        .WithTimeFrequency(frequencies.Hour)
        //        .WithWorkEffort(workOrder)
        //        .Build();

        //    employee.TimeSheetWhereWorker.AddTimeEntry(secondTimeEntry);

        //    var errors = this.Derive().Errors.ToList();
        //    var expectedMessage = ErrorMessages.WorkerActiveTimeEntry.Replace("{0}", secondTimeEntry.WorkEffort?.WorkEffortNumber);
        //    Assert.NotNull(errors.Find(e => e.Message.Contains(expectedMessage)));
        //}
    }

    public class TimeEntryRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public TimeEntryRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTimeSheetWhereTimeEntryDeriveWorker()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction).Build();
            this.Derive();

            var worker = new PersonBuilder(this.Transaction).Build();
            var timesheet = new TimeSheetBuilder(this.Transaction).WithWorker(worker).Build();
            this.Derive();

            timesheet.AddTimeEntry(timeEntry);
            this.Derive();

            Assert.Equal(worker, timeEntry.Worker);
        }

        [Fact]
        public void ChangedAssignedBillingRateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build())
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction).WithWorkEffort(workTask).Build();
            this.Derive();

            timeEntry.AssignedBillingRate = 10;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortAssignmentRateWorkEffortDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            var assignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            assignmentRate.WorkEffort = workTask;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortAssignmentRateRateTypeDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            var assignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRate(10)
                .Build();
            this.Derive();

            assignmentRate.RateType = new RateTypes(this.Transaction).StandardRate;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortAssignmentRateFrequencyDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            var assignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            assignmentRate.Frequency = new TimeFrequencies(this.Transaction).Day;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortAssignmentRateRateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            var assignmentRate = new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();
            this.Derive();

            assignmentRate.Rate = 10;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();
            this.Derive();

            timeEntry.WorkEffort = workTask;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedRateTypeDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            timeEntry.RateType = new RateTypes(this.Transaction).StandardRate;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedBillingFrequencyDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            new WorkEffortAssignmentRateBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            timeEntry.BillingFrequency = new TimeFrequencies(this.Transaction).Day;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyPartyRateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateRateTypeDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRate(10)
                .Build();
            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.RateType = new RateTypes(this.Transaction).StandardRate;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateFrequencyDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.Frequency = new TimeFrequencies(this.Transaction).Day;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateRateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();
            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.Rate = 10;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateFromDateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .WithRate(10)
                .Build();
            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.FromDate = this.Transaction.Now().AddDays(-1);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateThroughDateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithRate(10)
                .Build();
            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.ThroughDate = this.Transaction.Now().AddDays(1);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedFromDateDeriveBillingRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation.ActiveCustomers.First).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.InternalOrganisation.ActiveCustomers.First.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();
            this.Derive();

            timeEntry.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyPartyRateDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            worker.AddPartyRate(partyRate);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateRateTypeDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRate(10)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            partyRate.RateType = new RateTypes(this.Transaction).StandardRate;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateFrequencyDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            partyRate.Frequency = new TimeFrequencies(this.Transaction).Day;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateRateDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            partyRate.Rate = 10;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateFromDateDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .WithRate(10)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            partyRate.FromDate = this.Transaction.Now().AddDays(-1);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateThroughDateDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithRate(10)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            partyRate.ThroughDate = this.Transaction.Now().AddDays(1);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkerDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            timeEntry.Worker = worker;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedRateTypeDeriveBillingRateFromWorkerRate()
        {
            var worker = new PersonBuilder(this.Transaction).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).OvertimeRate)
                .WithRate(10)
                .Build();
            worker.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            timeEntry.RateType = new RateTypes(this.Transaction).OvertimeRate;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyPartyRateDeriveBillingRateFromExecutedByRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            this.InternalOrganisation.AddPartyRate(partyRate);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateRateTypeDeriveBillingRateFromExecutedByRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRate(10)
                .Build();
            this.InternalOrganisation.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.RateType = new RateTypes(this.Transaction).StandardRate;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateFrequencyDeriveBillingRateFromExecutedByRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithRate(10)
                .Build();
            this.InternalOrganisation.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.Frequency = new TimeFrequencies(this.Transaction).Day;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateRateDeriveBillingRateFromExecutedByRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .Build();
            this.InternalOrganisation.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.Rate = 10;
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateFromDateDeriveBillingRateFromExecutedByRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .WithRate(10)
                .Build();
            this.InternalOrganisation.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.FromDate = this.Transaction.Now().AddDays(-1);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedPartyRateThroughDateDeriveBillingRateFromExecutedByRate()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var partyRate = new PartyRateBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithRate(10)
                .Build();
            this.InternalOrganisation.AddPartyRate(partyRate);
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Day)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            partyRate.ThroughDate = this.Transaction.Now().AddDays(1);
            this.Derive();

            Assert.Equal(10, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortCustomerDeriveBillingRateForInternalLabour()
        {
            this.Transaction.GetSingleton().Settings.InternalLabourSurchargePercentage = 10M;

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction).WithWorkEffort(workTask).WithAssignedBillingRate(10).Build();
            this.Derive();

            workTask.Customer = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            var expectedBillingRate = Math.Round(10 * (1 + 10M / 100), 2);

            Assert.Equal(expectedBillingRate, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedAssignedBillingRateDeriveBillingRateForInternalLabour()
        {
            this.Transaction.GetSingleton().Settings.InternalLabourSurchargePercentage = 10M;

            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build())
                .Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction).WithWorkEffort(workTask).Build();
            this.Derive();

            timeEntry.AssignedBillingRate = 10;
            this.Derive();

            var expectedBillingRate = Math.Round(10 * (1 + 10M / 100), 2);

            Assert.Equal(expectedBillingRate, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedWorkEffortExecutedByDeriveBillingRateForInternalLabour()
        {
            this.Transaction.GetSingleton().Settings.InternalLabourSurchargePercentage = 10M;

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction).WithWorkEffort(workTask).WithAssignedBillingRate(10).Build();
            this.Derive();

            workTask.ExecutedBy = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            var expectedBillingRate = Math.Round(10 * (1 + 10M / 100), 2);

            Assert.Equal(expectedBillingRate, timeEntry.BillingRate);
        }

        [Fact]
        public void ChangedBillingFrequencyThrowValidationError()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction).WithAssignedBillingRate(10).Build();
            this.Derive();

            timeEntry.RemoveBillingFrequency();

            var errors = this.Derive().Errors.OfType<DerivationErrorRequired>();
            Assert.Contains(this.M.TimeEntry.BillingFrequency, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedFromDateDeriveAmountOfTime()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction).Build();
            this.Derive();

            timeEntry.FromDate = this.Transaction.Now().AddHours(-1);
            this.Derive();

            Assert.Equal(1, timeEntry.AmountOfTime);
        }

        [Fact]
        public void ChangedAssignedAmountOfTimeDeriveAmountOfTime()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction).Build();
            this.Derive();

            timeEntry.AssignedAmountOfTime = 2;
            this.Derive();

            Assert.Equal(2, timeEntry.AmountOfTime);
        }

        [Fact]
        public void ChangedThroughDateDeriveAmountOfTime()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction).Build();
            this.Derive();

            timeEntry.ThroughDate = this.Transaction.Now().AddHours(3);
            this.Derive();

            Assert.Equal(3, timeEntry.AmountOfTime);
        }

        [Fact]
        public void ChangedBillableAmountOfTimeDeriveBillableAmountOfTimeInMinutes()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            Assert.Equal(60, timeEntry.BillableAmountOfTimeInMinutes);

            timeEntry.BillableAmountOfTime = 2;
            this.Derive();

            Assert.Equal(120, timeEntry.BillableAmountOfTimeInMinutes);
        }

        [Fact]
        public void ChangedBillableAmountOfTimeDeriveBillingAmount()
        {
            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            Assert.Equal(10, timeEntry.BillingAmount);

            timeEntry.BillableAmountOfTime = 2;
            this.Derive();

            Assert.Equal(20, timeEntry.BillingAmount);
        }
    }
}
