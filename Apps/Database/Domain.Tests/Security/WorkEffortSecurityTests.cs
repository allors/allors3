// <copyright file="WorkEffortSecurityTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

            var acl = new DatabaseAccessControlLists(this.Administrator)[workTask];
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

            var acl = new DatabaseAccessControlLists(this.Administrator)[workTask];
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

            var billToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").WithPartyContactMechanism(billToMechelen).Build();
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

            var acl = new DatabaseAccessControlLists(this.Administrator)[workTask];
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

            var acl = new DatabaseAccessControlLists(this.Administrator)[timeEntry];
            Assert.False(acl.CanWrite(this.M.TimeEntry.AmountOfTime));
        }
    }


    [Trait("Category", "Security")]
    public class WorkEffortDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.invoicePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Invoice);
            this.completePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Complete);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission invoicePermission;
        private readonly Permission completePermission;

        [Fact]
        public void OnChangedWorkTaskDeriveInvoicePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.invoicePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskStateCompletedDeriveInvoicePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffort.Complete();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.invoicePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.completePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskServiceEntriesWhereWorkEffortDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffort.Complete();
            this.Transaction.Derive(false);

            var serviceEntrie = new ExpenseEntryBuilder(this.Transaction).WithWorkEffort(workEffort).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.completePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskWorkEffortExistThroughDateNotInProgressStateDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serviceEntrie = new ExpenseEntryBuilder(this.Transaction).WithWorkEffort(workEffort).WithThroughDate(this.Transaction.Now().AddDays(1)).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.completePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskWorkEffortExistThroughDateInProgressStateDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).WithActualStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var serviceEntrie = new ExpenseEntryBuilder(this.Transaction).WithWorkEffort(workEffort).WithThroughDate(this.Transaction.Now().AddDays(1)).Build();
            this.Transaction.Derive(false);
            // HOW TO IN PROGRESS

            Assert.DoesNotContain(this.completePermission, workEffort.DeniedPermissions);
        }
    }
}
