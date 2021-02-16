// <copyright file="WorkTaskTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests.Print
{
    using System;
    using System.Linq;
    using Domain.Print.WorkTaskModel;
    using Xunit;

    public class WorkTaskTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkEffort_WhenCreatingModel_ThenValuesAreSet()
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
            var workOrder = this.CreateWorkEffort(organisation, customer, customerContact, salesOrder.SalesOrderItems.First);
            var employee = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(organisation).Build();

            this.Transaction.Derive(true);

            var salesOrderItem = salesOrder.SalesOrderItems.First;
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

            var standardRate = new RateTypes(this.Transaction).StandardRate;
            var overtimeRate = new RateTypes(this.Transaction).OvertimeRate;

            var timeEntryYesterday = this.CreateTimeEntry(yesterday, laterYesterday, frequencies.Day, workOrder, standardRate);
            var timeEntryToday = this.CreateTimeEntry(today, laterToday, frequencies.Hour, workOrder, standardRate);
            var timeEntryTomorrow = this.CreateTimeEntry(tomorrow, laterTomorrow, frequencies.Minute, workOrder, overtimeRate);

            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday);
            employee.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow);

            this.Transaction.Derive(true);

            // Act
            var model = new Model(workOrder);

            // Assert
            Assert.Equal(3, model.TimeEntries.Length);
            Assert.Single(model.TimeEntriesByBillingRate);
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
                .WithInventoryItem(part.InventoryItemsWherePart.First)
                .WithQuantity(quantity)
                .Build();
        }

        private TimeEntry CreateTimeEntry(DateTime fromDate, DateTime throughDate, TimeFrequency frequency, WorkEffort workEffort, RateType rateType) =>
            new TimeEntryBuilder(this.Transaction)
                .WithRateType(rateType)
                .WithFromDate(fromDate)
                .WithThroughDate(throughDate)
                .WithTimeFrequency(frequency)
                .WithWorkEffort(workEffort)
                .Build();

        private PostalAddress CreatePostalAddress(string addressLine1, string addressLine2, string addressLine3, City city, PostalCode postalCode) =>
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

        private WorkTask CreateWorkEffort(Organisation takenBy, Party customer, Person contact, SalesOrderItem salesOrderItem) =>
            new WorkTaskBuilder(this.Transaction)
            .WithName("Task")
            .WithTakenBy(takenBy)
            .WithFacility(new Facilities(this.Transaction).Extent().First)
            .WithCustomer(customer)
            .WithContactPerson(contact)
            .WithWorkEffortPurpose(new WorkEffortPurposes(this.Transaction).Maintenance)
            .WithOrderItemFulfillment(salesOrderItem)
            .WithSpecialTerms("Net 45 Days")
            .Build();
    }
}
