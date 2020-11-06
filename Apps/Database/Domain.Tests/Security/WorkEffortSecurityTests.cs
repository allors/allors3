// <copyright file="WorkEffortSecurityTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
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
            var customer = new OrganisationBuilder(this.Session).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Session).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Session.Derive();

            Assert.Equal(new WorkEffortStates(this.Session).Created, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var acl = new DatabaseAccessControlLists(this.Administrator)[workTask];
            Assert.True(acl.CanExecute(this.M.WorkEffort.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Complete));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Invoice));
        }

        [Fact]
        public void WorkTask_StateCompleted()
        {
            var customer = new OrganisationBuilder(this.Session).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Session).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Session.Derive();

            workTask.Complete();

            this.Session.Derive();

            Assert.Equal(new WorkEffortStates(this.Session).Completed, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var acl = new DatabaseAccessControlLists(this.Administrator)[workTask];
            Assert.True(acl.CanExecute(this.M.WorkEffort.Invoice));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Complete));
        }

        [Fact]
        public void WorkTask_StateFinished()
        {
            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var billToMechelen = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new OrganisationBuilder(this.Session).WithName("Org1").WithPartyContactMechanism(billToMechelen).Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Session).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Session).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Session.Derive();

            var workTask = new WorkTaskBuilder(this.Session).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var timeEntry = new TimeEntryBuilder(this.Session)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .WithFromDate(DateTimeFactory.CreateDateTime(this.Session.Now()))
                .WithThroughDate(DateTimeFactory.CreateDateTime(this.Session.Now().AddHours(1)))
                .WithTimeFrequency(new TimeFrequencies(this.Session).Hour)
                .WithWorkEffort(workTask)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Session.Derive();

            workTask.Complete();

            this.Session.Derive();

            workTask.Invoice();

            this.Session.Derive();

            Assert.Equal(new WorkEffortStates(this.Session).Finished, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var acl = new DatabaseAccessControlLists(this.Administrator)[workTask];
            Assert.False(acl.CanExecute(this.M.WorkEffort.Invoice));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkEffort.Complete));
        }

        [Fact]
        public void WorkTask_StateCancelled_TimeEntry()
        {
            var customer = new OrganisationBuilder(this.Session).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Session).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Session).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Session.Derive();

            var employee = new PersonBuilder(this.Session).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Session).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Session.Derive();

            var timeEntry = new TimeEntryBuilder(this.Session)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .WithFromDate(DateTimeFactory.CreateDateTime(this.Session.Now()))
                .WithTimeFrequency(new TimeFrequencies(this.Session).Hour)
                .WithWorkEffort(workTask)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Session.Derive();

            workTask.Cancel();

            this.Session.Derive();

            Assert.Equal(new WorkEffortStates(this.Session).Cancelled, workTask.WorkEffortState);

            User user = this.Administrator;
            this.Session.SetUser(user);

            var acl = new DatabaseAccessControlLists(this.Administrator)[timeEntry];
            Assert.False(acl.CanWrite(this.M.TimeEntry.AmountOfTime));
        }
    }


    [Trait("Category", "Security")]
    public class WorkEffortDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
            this.invoicePermission = new Permissions(this.Session).Get(this.M.WorkTask.ObjectType, this.M.WorkTask.Invoice);
            this.completePermission = new Permissions(this.Session).Get(this.M.WorkTask.ObjectType, this.M.WorkTask.Complete);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission invoicePermission;
        private readonly Permission completePermission;

        [Fact]
        public void OnChangedWorkTaskDeriveInvoicePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.invoicePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskStateCompletedDeriveInvoicePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            workEffort.Complete();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.invoicePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.completePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskServiceEntriesWhereWorkEffortDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            workEffort.Complete();
            this.Session.Derive(false);

            var serviceEntrie = new ExpenseEntryBuilder(this.Session).WithWorkEffort(workEffort).Build();
            this.Session.Derive(false);

            Assert.Contains(this.completePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskWorkEffortExistThroughDateNotInProgressStateDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serviceEntrie = new ExpenseEntryBuilder(this.Session).WithWorkEffort(workEffort).WithThroughDate(this.Session.Now().AddDays(1)).Build();
            this.Session.Derive(false);

            Assert.Contains(this.completePermission, workEffort.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkTaskWorkEffortExistThroughDateInProgressStateDeriveCompletePermission()
        {
            var workEffort = new WorkTaskBuilder(this.Session).WithActualStart(this.Session.Now()).Build();
            this.Session.Derive(false);

            var serviceEntrie = new ExpenseEntryBuilder(this.Session).WithWorkEffort(workEffort).WithThroughDate(this.Session.Now().AddDays(1)).Build();
            this.Session.Derive(false);
            // HOW TO IN PROGRESS

            Assert.DoesNotContain(this.completePermission, workEffort.DeniedPermissions);
        }
    }
}
