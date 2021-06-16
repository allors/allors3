// <copyright file="RepeatingSalesInvoiceTests.cs" company="Allors bvba">
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
    using Resources;
    using Xunit;
    using DateTime = System.DateTime;

    public class RepeatingSalesInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public RepeatingSalesInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenRepeatingSalesInvoice_WhenDeriving_ThenFrequencyIsEitherMontlyOrWeekly()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithNextExecutionDate(this.Transaction.Now().AddDays(1))
                .Build();

            Assert.False(this.Derive().HasErrors);

            var weekDay = repeatingInvoice.NextExecutionDate.DayOfWeek.ToString();
            var daysOfWeek = new DaysOfWeek(this.Transaction).Extent();
            daysOfWeek.Filter.AddEquals(this.M.Enumeration.Name, weekDay);
            var dayOfWeek = daysOfWeek.First;

            repeatingInvoice.Frequency = new TimeFrequencies(this.Transaction).Week;
            repeatingInvoice.DayOfWeek = dayOfWeek;

            Assert.False(this.Derive().HasErrors);

            repeatingInvoice.Frequency = new TimeFrequencies(this.Transaction).Day;

            var validation = this.Derive();

            Assert.True(validation.HasErrors);
            Assert.Single(validation.Errors);
            Assert.Contains(validation.Errors, v => v.Message.Contains("Selected frequency is not supported"));
        }

        [Fact]
        public void GivenWeeklyRepeatingSalesInvoice_WhenDeriving_ThenDayOfWeekMustExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithNextExecutionDate(this.Transaction.Now().AddDays(1))
                .Build();

            var errors = this.Derive().Errors.OfType<DerivationErrorRequired>();
            Assert.Contains(this.M.RepeatingPurchaseInvoice.DayOfWeek, errors.SelectMany(v => v.RoleTypes).Distinct());

            Assert.False(this.Derive().HasErrors);

            var weekDay = repeatingInvoice.NextExecutionDate.DayOfWeek.ToString();
            var daysOfWeek = new DaysOfWeek(this.Transaction).Extent();
            daysOfWeek.Filter.AddEquals(this.M.Enumeration.Name, weekDay);
            var dayOfWeek = daysOfWeek.First;

            repeatingInvoice.DayOfWeek = dayOfWeek;

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenMontlyRepeatingSalesInvoice_WhenDeriving_ThenDayOfWeekMustNotExist()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var nextExecutionDate = this.Transaction.Now().AddDays(1);
            var weekDay = nextExecutionDate.DayOfWeek.ToString();
            var daysOfWeek = new DaysOfWeek(this.Transaction).Extent();
            daysOfWeek.Filter.AddEquals(this.M.Enumeration.Name, weekDay);
            var dayOfWeek = daysOfWeek.First;

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithDayOfWeek(dayOfWeek)
                .WithNextExecutionDate(nextExecutionDate)
                .Build();

            var errors = this.Derive().Errors.OfType<DerivationErrorNotAllowed>();
            Assert.Equal(new IRoleType[]
            {
                this.M.RepeatingSalesInvoice.DayOfWeek,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());

            repeatingInvoice.RemoveDayOfWeek();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenWeeklyRepeatingSalesInvoice_WhenDeriving_ThenDayOfWeekMustMatchNextExecutionDate()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var nextExecutionDate = this.Transaction.Now().AddDays(1);
            var weekDay = nextExecutionDate.DayOfWeek.ToString();
            var daysOfWeek = new DaysOfWeek(this.Transaction).Extent();
            daysOfWeek.Filter.AddEquals(this.M.Enumeration.Name, weekDay);
            var dayOfWeek = daysOfWeek.First;

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(dayOfWeek)
                .WithNextExecutionDate(nextExecutionDate)
                .Build();

            Assert.False(this.Derive().HasErrors);

            repeatingInvoice.NextExecutionDate = repeatingInvoice.NextExecutionDate.AddDays(1);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.DateDayOfWeek));
        }

        [Fact]
        public void GivenRepeatingSalesInvoice_WhenNotOnNextExecutionDate_ThenNothingChanges()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var mayThirteen2018 = new DateTime(2018, 5, 13, 12, 0, 0, DateTimeKind.Utc);
            var timeShift = mayThirteen2018 - this.Transaction.Now();
            this.TimeShift = timeShift;

            var countBefore = new SalesInvoices(this.Transaction).Extent().Count;

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .WithNextExecutionDate(mayThirteen2018.AddDays(1))
                .Build();

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 5, 14), repeatingInvoice.NextExecutionDate.Date);
            Assert.Empty(repeatingInvoice.SalesInvoices);
        }

        [Fact]
        public void GivenWeeklyRepeatingSalesInvoice_WhenDayArrives_ThenNextInvoiceIsCreated()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var mayFourteen2018 = new DateTime(2018, 5, 14, 12, 0, 0, DateTimeKind.Utc);
            var timeShift = mayFourteen2018 - this.Transaction.Now();
            this.TimeShift = timeShift;

            var countBefore = new SalesInvoices(this.Transaction).Extent().Count;

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .WithNextExecutionDate(mayFourteen2018)
                .Build();

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 1, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 5, 21), repeatingInvoice.NextExecutionDate.Date);
            Assert.Single(repeatingInvoice.SalesInvoices);
        }

        [Fact]
        public void GivenMontlyRepeatingSalesInvoice_WhenDayArrives_ThenNextInvoiceIsCreated()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var mayFourteen2018 = new DateTime(2018, 5, 14, 12, 0, 0, DateTimeKind.Utc);
            var timeShift = mayFourteen2018 - this.Transaction.Now();
            this.TimeShift = timeShift;

            var countBefore = new SalesInvoices(this.Transaction).Extent().Count;

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithNextExecutionDate(mayFourteen2018)
                .Build();

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 1, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 6, 14), repeatingInvoice.NextExecutionDate.Date);
            Assert.Single(repeatingInvoice.SalesInvoices);
        }

        [Fact]
        public void GivenWeeklyRepeatingSalesInvoice_WhenRepeating_ThenRepeatStopReachingLastExecutionDate()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var homeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Sint-Lambertuslaan 78")
                .WithLocality("Muizen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var billingAddress = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(homeAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            customer.AddPartyContactMechanism(billingAddress);

            var mayFourteen2018 = new DateTime(2018, 5, 14, 12, 0, 0, DateTimeKind.Utc);
            var timeShift = mayFourteen2018 - DateTime.UtcNow;
            this.TimeShift = timeShift;

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(mayFourteen2018).WithCustomer(customer).Build();

            this.Transaction.Derive();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            var countBefore = new SalesInvoices(this.Transaction).Extent().Count;

            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithSource(invoice)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .WithNextExecutionDate(mayFourteen2018)
                .WithFinalExecutionDate(mayFourteen2018.AddDays(21))
                .Build();

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 1, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 5, 21), repeatingInvoice.NextExecutionDate.Date);
            Assert.Equal(new DateTime(2018, 5, 14), repeatingInvoice.PreviousExecutionDate.Value.Date);
            Assert.Single(repeatingInvoice.SalesInvoices);

            var mayTwentyOne2018 = new DateTime(2018, 5, 21, 12, 0, 0, DateTimeKind.Utc);
            timeShift = mayTwentyOne2018 - DateTime.UtcNow;
            this.TimeShift = timeShift;

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 2, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 5, 28), repeatingInvoice.NextExecutionDate.Date);
            Assert.Equal(2, repeatingInvoice.SalesInvoices.Count);

            var mayTwentyEight2018 = new DateTime(2018, 5, 28, 12, 0, 0, DateTimeKind.Utc);
            timeShift = mayTwentyEight2018 - DateTime.UtcNow;
            this.TimeShift = timeShift;

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 3, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 6, 4), repeatingInvoice.NextExecutionDate.Date);
            Assert.Equal(new DateTime(2018, 5, 28), repeatingInvoice.PreviousExecutionDate.Value.Date);
            Assert.Equal(3, repeatingInvoice.SalesInvoices.Count);

            var juneFour2018 = new DateTime(2018, 6, 4, 12, 0, 0, DateTimeKind.Utc);
            timeShift = juneFour2018 - DateTime.UtcNow;
            this.TimeShift = timeShift;

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 4, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 6, 4), repeatingInvoice.NextExecutionDate.Date);
            Assert.Equal(new DateTime(2018, 6, 4), repeatingInvoice.PreviousExecutionDate.Value.Date);
            Assert.Equal(4, repeatingInvoice.SalesInvoices.Count);

            var juneEleven2018 = new DateTime(2018, 6, 11, 12, 0, 0, DateTimeKind.Utc);
            timeShift = juneEleven2018 - DateTime.UtcNow;
            this.TimeShift = timeShift;

            Assert.False(this.Derive().HasErrors);

            RepeatingSalesInvoices.Daily(this.Transaction);

            Assert.Equal(countBefore + 4, new SalesInvoices(this.Transaction).Extent().Count);
            Assert.Equal(new DateTime(2018, 6, 4), repeatingInvoice.NextExecutionDate.Date);
            Assert.Equal(new DateTime(2018, 6, 4), repeatingInvoice.PreviousExecutionDate.Value.Date);
            Assert.Equal(4, repeatingInvoice.SalesInvoices.Count);
        }
    }

    public class RepeatingSalesInvoiceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RepeatingSalesInvoiceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedFrequencyThrowValidationError()
        {
            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.FrequencyNotSupported));
        }

        [Fact]
        public void ChangedDayOfWeekThrowValidationErrorAssertExistsDayOfWeek()
        {
            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .Build();
            this.Derive();

            repeatingInvoice.RemoveDayOfWeek();

            var errors = this.Derive().Errors.OfType<DerivationErrorRequired>();
            Assert.Contains(this.M.RepeatingPurchaseInvoice.DayOfWeek, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedDayOfWeekThrowValidationErrorAssertnotExistsDayOfWeek()
        {
            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .Build();
            this.Derive();

            repeatingInvoice.DayOfWeek = new DaysOfWeek(this.Transaction).Monday;

            var errors = this.Derive().Errors.OfType<DerivationErrorNotAllowed>();
            Assert.Equal(new IRoleType[]
            {
                this.M.RepeatingSalesInvoice.DayOfWeek,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedNextExecutionDateThrowValidationError()
        {
            var repeatingInvoice = new RepeatingSalesInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .Build();
            this.Derive();

            repeatingInvoice.NextExecutionDate = new DateTime(2021, 01, 06, 12, 0, 0, DateTimeKind.Utc);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.DateDayOfWeek));
        }
    }
}
