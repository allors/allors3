// <copyright file="WorkEffortSecurityTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    [Trait("Category", "Security")]
    public class WorkEffortSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WorkTask_StateCreated()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workTask];
            Assert.True(acl.CanExecute(this.M.WorkEffort.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Complete));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Invoice));
        }

        [Fact]
        public void WorkTask_StateCompleted()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();

            workTask.Complete();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Completed, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workTask];
            Assert.True(acl.CanExecute(this.M.WorkEffort.Invoice));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Complete));
        }

        [Fact]
        public void WorkTask_StateFinished()
        {
            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(DateTimeFactory.CreateDateTime(this.Transaction.Now()))
                .WithThroughDate(DateTimeFactory.CreateDateTime(this.Transaction.Now().AddHours(1)))
                .WithTimeFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithWorkEffort(workTask)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();

            workTask.Complete();

            this.Transaction.Derive();

            workTask.Invoice();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Finished, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workTask];
            Assert.False(acl.CanExecute(this.M.WorkEffort.Invoice));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Complete));
        }

        [Fact]
        public void WorkTask_StateCancelled_TimeEntry()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(DateTimeFactory.CreateDateTime(this.Transaction.Now()))
                .WithTimeFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithWorkEffort(workTask)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();

            workTask.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Cancelled, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[timeEntry];
            Assert.False(acl.CanWrite(this.M.TimeEntry.AmountOfTime));
        }
    }


    [Trait("Category", "Security")]
    public class WorkEffortDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.invoiceRevocation = new Revocations(this.Transaction).WorkTaskInvoiceRevocation;
            this.completeRevocation = new Revocations(this.Transaction).WorkTaskCompleteRevocation;
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation invoiceRevocation;
        private readonly Revocation completeRevocation;

        [Fact]
        public void OnChangedWorkTaskDeriveInvoicePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.invoiceRevocation, workEffort.Revocations);
        }

        [Fact]
        public void OnChangedWorkTaskStateCompletedDeriveInvoicePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workEffort.Complete();
            this.Derive();

            Assert.DoesNotContain(this.invoiceRevocation, workEffort.Revocations);
        }

        [Fact]
        public void OnChangedWorkTaskDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var completePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Complete);
            Assert.Contains(completePermission, workEffort.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void OnChangedWorkTaskServiceEntriesWhereWorkEffortDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workEffort.Complete();
            this.Derive();

            var serviceEntrie = new ExpenseEntryBuilder(this.Transaction).WithWorkEffort(workEffort).Build();
            this.Derive();

            Assert.Contains(this.completeRevocation, workEffort.Revocations);
        }

        [Fact]
        public void OnChangedWorkTaskWorkEffortExistThroughDateNotInProgressStateDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var serviceEntrie = new ExpenseEntryBuilder(this.Transaction).WithWorkEffort(workEffort).WithThroughDate(this.Transaction.Now().AddDays(1)).Build();
            this.Derive();

            var completePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Complete);
            Assert.Contains(completePermission, workEffort.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void OnChangedWorkTaskWorkEffortExistThroughDateInProgressStateDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).WithActualStart(this.Transaction.Now()).Build();
            this.Derive();

            var serviceEntrie = new ExpenseEntryBuilder(this.Transaction).WithWorkEffort(workEffort).WithThroughDate(this.Transaction.Now().AddDays(1)).Build();
            this.Derive();
            // HOW TO IN PROGRESS

            Assert.DoesNotContain(this.completeRevocation, workEffort.Revocations);
        }
    }
}
