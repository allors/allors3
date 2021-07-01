// <copyright file="WorkTaskTests.cs" company="Allors bvba">
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
    using Resources;
    using Xunit;

    public class WorkTaskTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkTask_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workEffort.WorkEffortState);
            Assert.Equal(workEffort.LastWorkEffortState, workEffort.WorkEffortState);
        }

        [Fact]
        public void GivenWorkTask_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workEffort = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();

            Assert.Null(workEffort.PreviousWorkEffortState);
        }

        [Fact]
        public void GivenWorkEffortAndTimeEntries_WhenDeriving_ThenActualHoursDerived()
        {
            // Arrange
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(internalOrganisation).Build();

            this.Transaction.Derive(true);

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntry1 = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithWorkEffort(workOrder)
                .Build();

            var timeEntry2 = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(laterToday)
                .WithWorkEffort(workOrder)
                .Build();

            var timeEntry3 = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(tomorrow)
                .WithThroughDate(laterTomorrow)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry1);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry2);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry3);

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(13.0M, workOrder.ActualHours);
        }

        [Fact]
        public void GivenWorkEffortAndTimeEntries_WhenDeriving_ThenActualStartAndCompletionDerived()
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

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntryToday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(laterToday)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(today, workOrder.ActualStart);
            Assert.Equal(laterToday, workOrder.ActualCompletion);

            //// Re-arrange
            var timeEntryYesterday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(yesterday, workOrder.ActualStart);
            Assert.Equal(laterToday, workOrder.ActualCompletion);

            //// Re-arrange

            var timeEntryTomorrow = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(tomorrow)
                .WithThroughDate(laterTomorrow)
                .WithTimeFrequency(frequencies.Minute)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.Equal(yesterday, workOrder.ActualStart);
            Assert.Equal(laterTomorrow, workOrder.ActualCompletion);
        }

        [Fact]
        public void GivenWorkEffort_WhenDeriving_ThenPrintDocumentCreated()
        {
            // Arrange
            var frequencies = new TimeFrequencies(this.Transaction);
            var purposes = new ContactMechanismPurposes(this.Transaction);

            //// Customer Contact and Address Data
            var customer = new OrganisationBuilder(this.Transaction).WithName("Customer").Build();
            var customerContact = new PersonBuilder(this.Transaction).WithFirstName("Customer").WithLastName("Contact").Build();
            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var customerRelation = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();

            var usa = new Countries(this.Transaction).Extent().First(c => c.IsoCode.Equals("US"));
            var michigan = new StateBuilder(this.Transaction).WithName("Michigan").WithCountry(usa).Build();
            var northville = new CityBuilder(this.Transaction).WithName("Northville").WithState(michigan).Build();
            var postalCode = new PostalCodeBuilder(this.Transaction).WithCode("48167").Build();
            var billingAddress = this.CreatePostalAddress("Billing Address", "123 Street", "Suite S1", northville, postalCode);
            var shippingAddress = this.CreatePostalAddress("Shipping Address", "123 Street", "Dock D1", northville, postalCode);
            var phone = new TelecommunicationsNumberBuilder(this.Transaction).WithCountryCode("1").WithAreaCode("616").WithContactNumber("774-2000").Build();

            customer.AddPartyContactMechanism(this.CreatePartyContactMechanism(purposes.BillingAddress, billingAddress));
            customer.AddPartyContactMechanism(this.CreatePartyContactMechanism(purposes.ShippingAddress, shippingAddress));
            customerContact.AddPartyContactMechanism(this.CreatePartyContactMechanism(purposes.GeneralPhoneNumber, phone));

            //// Work Effort Data
            var salesPerson = new PersonBuilder(this.Transaction).WithFirstName("Sales").WithLastName("Person").Build();
            var salesOrder = this.CreateSalesOrder(customer, organisation);
            var workOrder = this.CreateWorkEffort(organisation, customer, customerContact, salesOrder.SalesOrderItems.FirstOrDefault());
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            var salesOrderItem = salesOrder.SalesOrderItems.FirstOrDefault();
            salesOrder.AddValidOrderItem(salesOrderItem);

            //// Work Effort Inventory Assignmets
            var part1 = this.CreatePart("P1");
            var part2 = this.CreatePart("P2");
            var part3 = this.CreatePart("P3");

            this.Transaction.Derive(true);

            var inventoryAssignment1 = this.CreateInventoryAssignment(workOrder, part1, 11);
            var inventoryAssignment2 = this.CreateInventoryAssignment(workOrder, part2, 12);
            var inventoryAssignment3 = this.CreateInventoryAssignment(workOrder, part3, 13);

            //// Work Effort Time Entries
            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntryYesterday = this.CreateTimeEntry(yesterday, laterYesterday, frequencies.Day, workOrder);
            var timeEntryToday = this.CreateTimeEntry(today, laterToday, frequencies.Hour, workOrder);
            var timeEntryTomorrow = this.CreateTimeEntry(tomorrow, laterTomorrow, frequencies.Minute, workOrder);

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            // Act
            this.Transaction.Derive(true);

            // Assert
            Assert.True(workOrder.ExistPrintDocument);
        }

        [Fact]
        public void GivenWorkEffortPrintDocument_WhenPrinting_ThenMediaCreated()
        {
            // Arrange
            var frequencies = new TimeFrequencies(this.Transaction);
            var purposes = new ContactMechanismPurposes(this.Transaction);

            //// Customer Contact and Address Data
            var customer = new OrganisationBuilder(this.Transaction).WithName("Customer").Build();
            var customerContact = new PersonBuilder(this.Transaction).WithFirstName("Customer").WithLastName("Contact").Build();
            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var customerRelation = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();

            var usa = new Countries(this.Transaction).Extent().First(c => c.IsoCode.Equals("US"));
            var michigan = new StateBuilder(this.Transaction).WithName("Michigan").WithCountry(usa).Build();
            var northville = new CityBuilder(this.Transaction).WithName("Northville").WithState(michigan).Build();
            var postalCode = new PostalCodeBuilder(this.Transaction).WithCode("48167").Build();
            var billingAddress = this.CreatePostalAddress("Billing Address", "123 Street", "Suite S1", northville, postalCode);
            var shippingAddress = this.CreatePostalAddress("Shipping Address", "123 Street", "Dock D1", northville, postalCode);
            var phone = new TelecommunicationsNumberBuilder(this.Transaction).WithCountryCode("1").WithAreaCode("616").WithContactNumber("774-2000").Build();

            customer.AddPartyContactMechanism(this.CreatePartyContactMechanism(purposes.BillingAddress, billingAddress));
            customer.AddPartyContactMechanism(this.CreatePartyContactMechanism(purposes.ShippingAddress, shippingAddress));
            customerContact.AddPartyContactMechanism(this.CreatePartyContactMechanism(purposes.GeneralPhoneNumber, phone));

            //// Work Effort Data
            var salesPerson = new PersonBuilder(this.Transaction).WithFirstName("Sales").WithLastName("Person").Build();
            var salesOrder = this.CreateSalesOrder(customer, organisation);
            var workOrder = this.CreateWorkEffort(organisation, customer, customerContact, salesOrder.SalesOrderItems.FirstOrDefault());
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            this.Transaction.Derive(true);

            var salesOrderItem = salesOrder.SalesOrderItems.FirstOrDefault();
            salesOrder.AddValidOrderItem(salesOrderItem);

            //// Work Effort Inventory Assignmets
            var part1 = this.CreatePart("P1");
            var part2 = this.CreatePart("P2");
            var part3 = this.CreatePart("P3");

            this.Transaction.Derive(true);

            var inventoryAssignment1 = this.CreateInventoryAssignment(workOrder, part1, 11);
            var inventoryAssignment2 = this.CreateInventoryAssignment(workOrder, part2, 12);
            var inventoryAssignment3 = this.CreateInventoryAssignment(workOrder, part3, 13);

            this.Transaction.Derive(true);

            //// Work Effort Time Entries
            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntryYesterday = this.CreateTimeEntry(yesterday, laterYesterday, frequencies.Day, workOrder);
            var timeEntryToday = this.CreateTimeEntry(today, laterToday, frequencies.Hour, workOrder);
            var timeEntryTomorrow = this.CreateTimeEntry(tomorrow, laterTomorrow, frequencies.Minute, workOrder);

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            this.Transaction.Derive(true);

            // Act
            workOrder.Print();

            this.Transaction.Derive();
            this.Transaction.Commit();

            // Assert
            Assert.True(workOrder.PrintDocument.ExistMedia);

            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var outputFile = System.IO.File.Create(System.IO.Path.Combine(desktopDir, "workTask.odt"));
            var stream = new System.IO.MemoryStream(workOrder.PrintDocument.Media.MediaContent.Data);

            stream.CopyTo(outputFile);
            stream.Close();
        }

        [Fact]
        public void GivenWorkEffortAndTimeEntriesWithBillingRate_WhenInvoiced_ThenTimeEntryBillingRateIsUsed()
        {
            var frequencies = new TimeFrequencies(this.Transaction);

            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var customer = new PersonBuilder(this.Transaction).WithLastName("Customer").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).Build();

            this.Transaction.Derive(true);

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntryYesterday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithWorkEffort(workOrder)
                .WithAssignedBillingRate(10)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);

            var timeEntryToday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(laterToday)
                .WithWorkEffort(workOrder)
                .WithAssignedBillingRate(12)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);

            var timeEntryTomorrow = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(tomorrow)
                .WithThroughDate(laterTomorrow)
                .WithWorkEffort(workOrder)
                .WithAssignedBillingRate(12)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            workOrder.Complete();

            this.Transaction.Derive(true);

            workOrder.Invoice();

            var salesInvoice = customer.SalesInvoicesWhereBillToCustomer.First();

            Assert.Equal(2, salesInvoice.InvoiceItems.Length);
            Assert.Equal(10, timeEntryYesterday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(3, timeEntryYesterday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.Quantity);
            Assert.Equal(12, timeEntryToday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(10, timeEntryToday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.Quantity);
        }

        [Fact]
        public void GivenParentWorkEffortAndTimeEntriesWithBillingRate_WhenInvoiced_ThenTimeEntryBillingRateIsUsed()
        {
            var frequencies = new TimeFrequencies(this.Transaction);

            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            var mechelenAddress = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var billToMechelen = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(mechelenAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").WithPartyContactMechanism(billToMechelen).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            var parentWorkOrder = new WorkTaskBuilder(this.Transaction).WithName("Parent Task").WithCustomer(customer).Build();

            this.Transaction.Derive(true);

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntryYesterday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithWorkEffort(parentWorkOrder)
                .WithAssignedBillingRate(10)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);

            var timeEntryToday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(laterToday)
                .WithWorkEffort(parentWorkOrder)
                .WithAssignedBillingRate(12)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);

            var childWorkOrder = new WorkTaskBuilder(this.Transaction).WithName("Child Task").WithCustomer(customer).Build();
            parentWorkOrder.AddChild(childWorkOrder);

            var timeEntryTomorrow = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(tomorrow)
                .WithThroughDate(laterTomorrow)
                .WithWorkEffort(childWorkOrder)
                .WithAssignedBillingRate(14)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            childWorkOrder.Complete();

            this.Transaction.Derive(true);

            parentWorkOrder.Complete();

            this.Transaction.Derive(true);

            parentWorkOrder.Invoice();

            this.Transaction.Derive(true);

            var salesInvoice = customer.SalesInvoicesWhereBillToCustomer.First();

            Assert.Equal(3, salesInvoice.InvoiceItems.Length);
            Assert.Equal(10, timeEntryYesterday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(3, timeEntryYesterday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.Quantity);
            Assert.Equal(12, timeEntryToday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(4, timeEntryToday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.Quantity);
            Assert.Equal(14, timeEntryTomorrow.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(6, timeEntryTomorrow.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.Quantity);
        }

        [Fact]
        public void GivenWorkEffortAndTimeEntriesWithoutBillingRate_WhenInvoiced_ThenWorkEffortRateIsUsed()
        {
            var frequencies = new TimeFrequencies(this.Transaction);

            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var customer = new PersonBuilder(this.Transaction).WithLastName("Customer").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).Build();

            new WorkEffortAssignmentRateBuilder(this.Transaction).WithWorkEffort(workOrder).WithRate(10).WithRateType(new RateTypes(this.Transaction).StandardRate).Build();

            this.Transaction.Derive(true);

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var timeEntryYesterday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);

            var timeEntryToday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(laterToday)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);

            var timeEntryTomorrow = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(tomorrow)
                .WithThroughDate(laterTomorrow)
                .WithWorkEffort(workOrder)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            workOrder.Complete();

            this.Transaction.Derive(true);

            workOrder.Invoice();

            var salesInvoice = customer.SalesInvoicesWhereBillToCustomer.First();

            Assert.Single(salesInvoice.InvoiceItems);
            Assert.Equal(10, timeEntryYesterday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(13, timeEntryYesterday.TimeEntryBillingsWhereTimeEntry.First().InvoiceItem.Quantity);
        }

        [Fact]
        public void GivenWorkEffortAndPartsUsed_WhenInvoiced_ThenPartsAreInvoiced()
        {
            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var customerEmail = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).WithElectronicAddressString($"customer@acme.com").Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("Customer").WithPartyContactMechanism(customerEmail).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));

            var part1 = this.CreatePart("P1");

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(11)
                .Build();

            this.Transaction.Derive(true);

            var part1BasePriceYesterday = new BasePriceBuilder(this.Transaction)
                .WithDescription("baseprice part1")
                .WithPrice(9)
                .WithPart(part1)
                .WithFromDate(yesterday)
                .WithThroughDate(today)
                .Build();

            var part1BasePriceToday = new BasePriceBuilder(this.Transaction)
                .WithDescription("baseprice part1")
                .WithPrice(10)
                .WithPart(part1)
                .WithFromDate(today)
                .WithThroughDate(tomorrow)
                .Build();

            var part1BasePriceTomorrow = new BasePriceBuilder(this.Transaction)
                .WithDescription("baseprice part1")
                .WithPrice(11)
                .WithPart(part1)
                .WithFromDate(tomorrow)
                .Build();

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).Build();

            this.Transaction.Derive(true);

            var timeEntryToday = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(today)
                .WithThroughDate(laterToday)
                .WithWorkEffort(workOrder)
                .WithAssignedBillingRate(12)
                .Build();

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);

            new WorkEffortInventoryAssignmentBuilder(this.Transaction).WithAssignment(workOrder).WithInventoryItem(part1.InventoryItemsWherePart.FirstOrDefault()).WithQuantity(3).Build();

            this.Transaction.Derive(true);

            workOrder.Complete();

            this.Transaction.Derive(true);

            workOrder.Invoice();

            this.Transaction.Derive(true);

            var salesInvoice = customer.SalesInvoicesWhereBillToCustomer.First();

            Assert.Equal(2, salesInvoice.InvoiceItems.Length);
            Assert.Equal(10, workOrder.WorkEffortBillingsWhereWorkEffort.First().InvoiceItem.AssignedUnitPrice);
            Assert.Equal(30, workOrder.WorkEffortBillingsWhereWorkEffort.First().InvoiceItem.TotalExVat);
        }

        [Fact]
        public void GivenWorkEffortWithInvoiceItems_WhenInvoiced_ThenInvoiceItemsAreInvoiced()
        {
            var organisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var customerEmail = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).WithElectronicAddressString($"customer@acme.com").Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var customer = new PersonBuilder(this.Transaction).WithLastName("Customer").WithPartyContactMechanism(customerEmail).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(organisation).Build();

            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));

            var today = DateTimeFactory.CreateDateTime(this.Transaction.Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(1));

            this.Transaction.Derive(true);

            var workOrder = new WorkTaskBuilder(this.Transaction).WithName("Task").WithCustomer(customer).Build();

            this.Transaction.Derive(true);

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                                        .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                                        .WithQuantity(1)
                                        .WithDescription("desc")
                                        .WithAssignedUnitPrice(1)
                                        .Build();

            new WorkEffortSalesInvoiceItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workOrder)
                .WithSalesInvoiceItem(salesInvoiceItem)
                .Build();

            this.Transaction.Derive(true);

            workOrder.Complete();
            this.Transaction.Derive(true);

            workOrder.Invoice();
            this.Transaction.Derive(true);

            var salesInvoice = customer.SalesInvoicesWhereBillToCustomer.First();

            Assert.Single(salesInvoice.InvoiceItems);
            Assert.NotEqual(salesInvoiceItem, salesInvoice.SalesInvoiceItems.FirstOrDefault());
            Assert.Equal(1, salesInvoice.GrandTotal);
        }

        private Part CreatePart(string id) =>
            new NonUnifiedPartBuilder(this.Transaction)
            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
            .WithIdentification(id)
            .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
            .Build();

        private WorkEffortInventoryAssignment CreateInventoryAssignment(WorkEffort workOrder, Part part, int quantity)
        {
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(quantity)
                .Build();

            this.Transaction.Derive();

            return new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workOrder)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(quantity)
                .Build();
        }

        private TimeEntry CreateTimeEntry(DateTime fromDate, DateTime throughDate, TimeFrequency frequency, WorkEffort workEffort) =>
            new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(fromDate)
                .WithThroughDate(throughDate)
                .WithTimeFrequency(frequency)
                .WithWorkEffort(workEffort)
                .Build();

        private PostalAddress CreatePostalAddress(string addressLine1,
            string addressLine2,
            string addressLine3,
            City city,
            PostalCode postalCode) =>
                new PostalAddressBuilder(this.Transaction)
                .WithAddress1(addressLine1)
                .WithAddress2(addressLine2)
                .WithAddress3(addressLine3)
                .WithPostalAddressBoundary(postalCode)
                .WithPostalAddressBoundary(city)
                .WithPostalAddressBoundary(city.State.Country)
                .Build();

        private PartyContactMechanism CreatePartyContactMechanism(ContactMechanismPurpose purpose, ContactMechanism mechanism) =>
            new PartyContactMechanismBuilder(this.Transaction)
            .WithContactPurpose(purpose)
            .WithContactMechanism(mechanism)
            .WithUseAsDefault(true)
            .Build();

        private SalesOrder CreateSalesOrder(Party customer, InternalOrganisation takenBy) =>
            new SalesOrderBuilder(this.Transaction)
            .WithShipToCustomer(customer)
            .WithTakenBy(takenBy)
            .WithSalesOrderItem(new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .WithAssignedUnitPrice(1)
                .Build())
            .WithSalesTerm(new OrderTermBuilder(this.Transaction)
                .WithDescription("Net 30")
                .WithTermType(new InvoiceTermTypes(this.Transaction).PaymentNetDays)
                .Build())
            .Build();

        private WorkEffort CreateWorkEffort(Organisation takenBy, Party customer, Person contact, SalesOrderItem salesOrderItem) =>
            new WorkTaskBuilder(this.Transaction)
            .WithName("Task")
            .WithTakenBy(takenBy)
            .WithFacility(new Facilities(this.Transaction).Extent().FirstOrDefault())
            .WithCustomer(customer)
            .WithContactPerson(contact)
            .WithWorkEffortPurpose(new WorkEffortPurposes(this.Transaction).Maintenance)
            .WithOrderItemFulfillment(salesOrderItem)
            .WithSpecialTerms("Net 45 Days")
            .Build();
    }

    public class WorkTaskRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTakenByDeriveValidationError()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.TakenBy = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InternalOrganisationChanged));
        }

        [Fact]
        public void ChangedTakenByDeriveInvoiceNumber()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(workTask.ExistWorkEffortNumber);
        }

        [Fact]
        public void ChangedTakenByDeriveSortableInvoiceNumber()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(workTask.ExistSortableWorkEffortNumber);
        }

        [Fact]
        public void ChangedTakenByDeriveExecutedBy()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(workTask.TakenBy, workTask.ExecutedBy);
        }

        [Fact]
        public void ChangedExecutedByDeriveExecutedBy()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.RemoveExecutedBy();
            this.Derive();

            Assert.Equal(workTask.TakenBy, workTask.ExecutedBy);
        }

        [Fact]
        public void ChangedTimeEntryWorkEffortCreateWorkEffortPartyAssignment()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var worker = this.InternalOrganisation.ActiveEmployees.FirstOrDefault();
            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            worker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);
            this.Derive();

            Assert.True(workTask.ExistWorkEffortPartyAssignmentsWhereAssignment);
        }

        [Fact]
        public void ChangedTimesheetTimeEntryCreateWorkEffortPartyAssignment()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var worker = this.InternalOrganisation.ActiveEmployees.FirstOrDefault();
            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            worker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);
            this.Derive();

            Assert.True(workTask.ExistWorkEffortPartyAssignmentsWhereAssignment);
        }

        [Fact]
        public void ChangedTimesheetTimeEntryThrowValidationError()
        {
            this.InternalOrganisation.RequireExistingWorkEffortPartyAssignment = true;
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var worker = this.InternalOrganisation.ActiveEmployees.FirstOrDefault();
            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithWorker(worker)
                .Build();
            this.Derive();

            worker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("No Work Effort Party Assignment matches Worker"));
        }

        [Fact]
        public void ChangedTimeEntryFromDateDeriveActualHours()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            timeEntry.FromDate = this.Transaction.Now().AddHours(-1);
            this.Derive();

            Assert.Equal(1, workTask.ActualHours);
        }

        [Fact]
        public void ChangedTimeEntryThroughDateDeriveActualHours()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            timeEntry.ThroughDate = this.Transaction.Now().AddHours(2);
            this.Derive();

            Assert.Equal(2, workTask.ActualHours);
        }

        [Fact]
        public void ChangedTimeEntryDeriveActualStart()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            Assert.Equal(timeEntry.FromDate, workTask.ActualStart);
        }

        [Fact]
        public void ChangedTimeEntryFromDateDeriveActualStart()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .Build();
            this.Derive();

            var fromDate = this.Transaction.Now().AddDays(-1);
            timeEntry.FromDate = fromDate;
            this.Derive();

            Assert.Equal(fromDate.Date, workTask.ActualStart.Value.Date);
        }

        [Fact]
        public void ChangedTimeEntryDeriveActualCompletion()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithThroughDate(this.Transaction.Now().AddHours(1))
                .Build();
            this.Derive();

            Assert.Equal(timeEntry.ThroughDate, workTask.ActualCompletion);
        }

        [Fact]
        public void ChangedTimeEntryFromDateDeriveActualCompletion()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithThroughDate(this.Transaction.Now().AddSeconds(1))
                .Build();
            this.Derive();

            var throughDate = this.Transaction.Now().AddDays(1);
            timeEntry.ThroughDate = throughDate;
            this.Derive();

            Assert.Equal(throughDate.Date, workTask.ActualCompletion.Value.Date);
        }

        [Fact]
        public void ChangedActualStartDeriveWorkEffortState()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.ActualStart = this.Transaction.Now();
            this.Derive();

            Assert.True(workTask.WorkEffortState.IsInProgress);
        }

        [Fact]
        public void ChangedCanInvoiceDeriveWorkEffortState()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithWorkEffortState(new WorkEffortStates(this.Transaction).Finished).Build();
            this.Derive();

            workTask.CanInvoice = true;
            this.Derive();

            Assert.True(workTask.WorkEffortState.IsCompleted);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentAssignmentCreateWorkEffortInventoryAssignmentInventoryItemTransaction()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).WithPart(part).Build();
            this.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction).WithInventoryItem(inventoryItem).WithQuantity(10).Build();
            this.Derive();

            inventoryAssignment.Assignment = workTask;
            this.Derive();

            Assert.Equal(10, inventoryAssignment.InventoryItemTransactions.First().Quantity);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentQuantityCreateWorkEffortInventoryAssignmentInventoryItemTransaction()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).WithPart(part).Build();
            this.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction).WithAssignment(workTask).WithInventoryItem(inventoryItem).Build();
            this.Derive();

            inventoryAssignment.Quantity = 10;
            this.Derive();

            Assert.Equal(10, inventoryAssignment.InventoryItemTransactions.First().Quantity);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentInventoryItemCreateInventoryItemTransaction()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part1 = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            var part2 = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part1)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part2)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part1.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(2, part1.QuantityOnHand);
            Assert.Equal(3, part2.QuantityOnHand);

            inventoryAssignment.InventoryItem = part2.InventoryItemsWherePart.FirstOrDefault();
            this.Derive();

            Assert.Equal(3, part1.QuantityOnHand);
            Assert.Equal(2, part2.QuantityOnHand);
        }

        [Fact]
        public void ChangedWorkEffortStateCreateInventoryItemTransaction()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .Build();
            this.Derive();

            Assert.Equal(2, part.QuantityOnHand);

            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction).Cancelled;
            this.Derive();

            Assert.Equal(3, part.QuantityOnHand);
        }
    }

    public class WorkTaskCanInvoiceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskCanInvoiceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortStateDeriveCanInvoice()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.WorkEffortState = new WorkEffortStates(this.Transaction).Completed;
            this.Derive();

            Assert.True(workTask.CanInvoice);
        }
    }

    public class WorkEffortTotalLabourRevenueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalLabourRevenueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTimeEntryWorkEffortDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            timeEntry.WorkEffort = workEffort;
            this.Derive();

            Assert.Equal(10, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryBillingAmountDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            timeEntry.AssignedBillingRate = 11;
            this.Derive();

            Assert.Equal(11, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryIsBillableDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(false)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            timeEntry.IsBillable = true;
            this.Derive();

            Assert.Equal(10, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryAmountOfTimeDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            timeEntry.RemoveThroughDate();
            timeEntry.AssignedAmountOfTime = 2;
            this.Derive();

            Assert.Equal(20, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryBillableAmountOfTimeDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            timeEntry.BillableAmountOfTime = 2;
            this.Derive();

            Assert.Equal(20, workEffort.TotalLabourRevenue);
        }
    }

    public class WorkEffortTotalMaterialRevenueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalMaterialRevenueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentAssignmentDeriveTotalMaterialRevenue()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithPart(part)
                .Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffortInventoryAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(10, workEffort.TotalMaterialRevenue);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentAssignedBillableQuantityDeriveTotalMaterialRevenue()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithPart(part)
                .Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffortInventoryAssignment.AssignedBillableQuantity = 2;
            this.Derive();

            Assert.Equal(20, workEffort.TotalMaterialRevenue);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentQuantityDeriveTotalMaterialRevenue()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithPart(part)
                .Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffortInventoryAssignment.Quantity = 2;
            this.Derive();

            Assert.Equal(20, workEffort.TotalMaterialRevenue);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentUnitSellingPriceDeriveTotalMaterialRevenue()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithPart(part)
                .Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffortInventoryAssignment.AssignedUnitSellingPrice = 11;
            this.Derive();

            Assert.Equal(11, workEffort.TotalMaterialRevenue);
        }
    }

    public class WorkEffortTotalOtherRevenueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalOtherRevenueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortSalesInvoiceItemAssignmentAssignmentDeriveTotalOtherRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                .WithQuantity(1)
                .WithDescription("desc")
                .WithAssignedUnitPrice(1)
                .Build();

            var workEffortSalesInvoiceItemAssignment = new WorkEffortSalesInvoiceItemAssignmentBuilder(this.Transaction)
                .WithSalesInvoiceItem(salesInvoiceItem)
                .Build();
            this.Derive();

            workEffortSalesInvoiceItemAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(1, workEffort.TotalOtherRevenue);
        }

        [Fact]
        public void ChangedWorkEffortSalesInvoiceItemAssignmentSalesInvoiceItemQuantityDeriveTotalOtherRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                .WithQuantity(1)
                .WithDescription("desc")
                .WithAssignedUnitPrice(1)
                .Build();

            new WorkEffortSalesInvoiceItemAssignmentBuilder(this.Transaction)
                .WithSalesInvoiceItem(salesInvoiceItem)
                .WithAssignment(workEffort)
                .Build();
            this.Derive();

            salesInvoiceItem.Quantity = 2;
            this.Derive();

            Assert.Equal(2, workEffort.TotalOtherRevenue);
        }

        [Fact]
        public void ChangedWorkEffortSalesInvoiceItemAssignmentSalesInvoiceItemAssignedUnitPriceDeriveTotalOtherRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                .WithQuantity(1)
                .WithDescription("desc")
                .WithAssignedUnitPrice(1)
                .Build();

            new WorkEffortSalesInvoiceItemAssignmentBuilder(this.Transaction)
                .WithSalesInvoiceItem(salesInvoiceItem)
                .WithAssignment(workEffort)
                .Build();
            this.Derive();

            salesInvoiceItem.AssignedUnitPrice = 3;
            this.Derive();

            Assert.Equal(3, workEffort.TotalOtherRevenue);
        }
    }

    public class WorkEffortTotalSubContractedRevenueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalSubContractedRevenueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentAssignmentDeriveTotalSubContractedRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            purchaseOrderItemAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(20, workEffort.TotalSubContractedRevenue);
        }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentQuantityDeriveTotalSubContractedRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            purchaseOrderItemAssignment.Quantity = 1;
            this.Derive();

            Assert.Equal(10, workEffort.TotalSubContractedRevenue);
        }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentUnitPurchasePriceDeriveTotalSubContractedRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            purchaseOrderItemAssignment.AssignedUnitSellingPrice = 5;
            this.Derive();

            Assert.Equal(10, workEffort.TotalSubContractedRevenue);
        }
    }

    public class WorkEffortGrandTotalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortGrandTotalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTimeEntryWorkEffortDeriveGrandTotal()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Derive();

            timeEntry.WorkEffort = workEffort;
            this.Derive();

            Assert.Equal(10, workEffort.GrandTotal);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentAssignmentDeriveGrandTotal()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithPart(part)
                .Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffortInventoryAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(10, workEffort.GrandTotal);
        }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentAssignmentDeriveGrandTotal()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            purchaseOrderItemAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(20, workEffort.GrandTotal);
        }

        [Fact]
        public void ChangedWorkEffortSalesInvoiceItemAssignmentAssignmentDeriveGrandTotal()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                .WithQuantity(1)
                .WithDescription("desc")
                .WithAssignedUnitPrice(1)
                .Build();

            var salesInvoiceItemAssignment = new WorkEffortSalesInvoiceItemAssignmentBuilder(this.Transaction)
                .WithSalesInvoiceItem(salesInvoiceItem)
                .Build();
            this.Derive();

            salesInvoiceItemAssignment.Assignment = workEffort;
            this.Derive();

            Assert.Equal(1, workEffort.GrandTotal);
        }
    }

    public class WorkEffortTotalRevenueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalRevenueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortCustomerDeriveTotalRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithExecutedBy(this.InternalOrganisation)
                .Build();
            this.Derive();

            new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffort.Customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            this.Derive();

            Assert.Equal(10, workEffort.TotalRevenue);
        }

        [Fact]
        public void ChangedWorkEffortExecutedByDeriveTotalRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.InternalOrganisation.ActiveCustomers.FirstOrDefault())
                .Build();
            this.Derive();

            new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .WithAssignedUnitSellingPrice(10)
                .Build();
            this.Derive();

            workEffort.ExecutedBy = this.InternalOrganisation;
            this.Derive();

            Assert.Equal(10, workEffort.TotalRevenue);
        }
    }

    [Trait("Category", "Security")]
    public class WorkTaskDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
        }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void OnChangedCanInvoiceDeriveInvoicePermissionDenied()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var invoicePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Invoice);
            Assert.Contains(invoicePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedCanInvoiceDeriveInvoicePermissionAllowed()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.CanInvoice = true;
            this.Derive();

            var invoicePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Invoice);
            Assert.DoesNotContain(invoicePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveCompletePermission()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.WorkEffortState = new WorkEffortStates(this.Transaction).InProgress;
            this.Derive();

            var completePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Complete);
            Assert.DoesNotContain(completePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedServiceEntryWorkEffortDeriveCompletePermission()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction).Build();
            this.Derive();

            timeEntry.WorkEffort = workTask;
            this.Derive();

            var completePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Complete);
            Assert.Contains(completePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedServiceEntryThroughDateDeriveCompletePermission()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            var timeEntry = new TimeEntryBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithWorkEffort(workTask).Build();
            this.Derive();

            var completePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Complete);
            Assert.Contains(completePermission, workTask.DeniedPermissions);

            timeEntry.ThroughDate = timeEntry.FromDate;
            this.Derive();

            Assert.DoesNotContain(completePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveRevisePermission()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.InternalOrganisation).WithExecutedBy(this.InternalOrganisation).Build();
            this.Derive();

            var revisePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Revise);
            Assert.Contains(revisePermission, workTask.DeniedPermissions);

            workTask.WorkEffortState = new WorkEffortStates(this.Transaction).Finished;
            this.Derive();

            Assert.DoesNotContain(revisePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedCustomerDeriveRevisePermission()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithExecutedBy(this.InternalOrganisation).WithWorkEffortState(new WorkEffortStates(this.Transaction).Finished).Build();
            var result = this.Derive();

            var revisePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Revise);
            Assert.Contains(revisePermission, workTask.DeniedPermissions);

            workTask.Customer = this.InternalOrganisation;
            this.Derive();

            Assert.DoesNotContain(revisePermission, workTask.DeniedPermissions);
        }

        [Fact]
        public void OnChangedExecutedByDeriveRevisePermission()
        {
            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithExecutedBy(this.InternalOrganisation)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Finished)
                .Build();
            this.Derive();

            var revisePermission = new Permissions(this.Transaction).Get(this.M.WorkTask, this.M.WorkTask.Revise);
            Assert.Contains(revisePermission, workTask.DeniedPermissions);

            workTask.ExecutedBy = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();
            this.Derive();

            Assert.DoesNotContain(revisePermission, workTask.DeniedPermissions);
        }
    }
}
