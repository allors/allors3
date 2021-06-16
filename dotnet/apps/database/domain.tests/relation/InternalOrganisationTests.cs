// <copyright file="InternalOrganisationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class InternalOrganisationTests : DomainTest, IClassFixture<Fixture>
    {
        private OwnBankAccount ownBankAccount;
        private WebAddress billingAddress;

        public InternalOrganisationTests(Fixture fixture) : base(fixture)
        {
            this.ownBankAccount = this.Transaction.Extent<OwnBankAccount>().First;

            this.billingAddress = new WebAddressBuilder(this.Transaction).WithElectronicAddressString("billfrom").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenInternalOrganisation_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            this.InstantiateObjects(this.Transaction);

            var builder = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Organisation");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenInternalOrganisation_WhenBuildWithout_ThenDoAccountingIsFalse()
        {
            this.InstantiateObjects(this.Transaction);

            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("Internal")
                .WithDefaultCollectionMethod(this.ownBankAccount)
                .Build();

            this.Transaction.Derive();

            Assert.False(internalOrganisation.DoAccounting);
        }

        [Fact]
        public void GivenInternalOrganisation_WhenBuildWithout_ThenFiscalYearStartMonthIsJanuary()
        {
            this.InstantiateObjects(this.Transaction);

            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("Internal")
                .WithDefaultCollectionMethod(this.ownBankAccount)
                .Build();

            this.Derive();

            Assert.Equal(1, internalOrganisation.SettingsForAccounting.FiscalYearStartMonth);
        }

        [Fact]
        public void GivenInternalOrganisation_WhenBuildWithout_ThenFiscalYearStartDayIsFirstDayOfMonth()
        {
            this.InstantiateObjects(this.Transaction);

            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("Internal")
                .WithDefaultCollectionMethod(this.ownBankAccount)
                .Build();

            this.Derive();

            Assert.Equal(1, internalOrganisation.SettingsForAccounting.FiscalYearStartDay);
        }

        [Fact]
        public void GivenInternalOrganisation_WhenBuildWithout_ThenInvoiceSequenceIsEqualEnforcedSequence()
        {
            this.InstantiateObjects(this.Transaction);

            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("Internal")
                .WithDefaultCollectionMethod(this.ownBankAccount)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new InvoiceSequences(this.Transaction).EnforcedSequence, internalOrganisation.InvoiceSequence);
        }

        [Fact]
        public void GivenInternalOrganisationWithDefaultFiscalYearStartMonthAndNotExistActualAccountingPeriod_WhenStartingNewFiscalYear_ThenAccountingPeriodsAreCreated()
        {
            this.InstantiateObjects(this.Transaction);

            var organisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("Internal")
                .Build();

            this.Derive();

            organisation.StartNewFiscalYear();

            var fromDate = DateTimeFactory.CreateDate(this.Transaction.Now().Year, 01, 01).Date;
            var month = organisation.SettingsForAccounting.ActualAccountingPeriod;

            Assert.Equal(1, month.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Month, month.Frequency);
            Assert.Equal(fromDate, month.FromDate);
            Assert.Equal(fromDate.AddMonths(1).AddSeconds(-1).Date, month.ThroughDate);
            Assert.True(month.ExistParent);

            var trimester = month.Parent;

            Assert.Equal(1, trimester.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Trimester, trimester.Frequency);
            Assert.Equal(fromDate, trimester.FromDate);
            Assert.Equal(fromDate.AddMonths(3).AddSeconds(-1).Date, trimester.ThroughDate);
            Assert.True(trimester.ExistParent);

            var semester = trimester.Parent;

            Assert.Equal(1, semester.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Semester, semester.Frequency);
            Assert.Equal(fromDate, semester.FromDate);
            Assert.Equal(fromDate.AddMonths(6).AddSeconds(-1).Date, semester.ThroughDate);
            Assert.True(semester.ExistParent);

            var year = semester.Parent;

            Assert.Equal(1, year.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Year, year.Frequency);
            Assert.Equal(fromDate, year.FromDate);
            Assert.Equal(fromDate.AddMonths(12).AddSeconds(-1).Date, year.ThroughDate);
            Assert.False(year.ExistParent);
        }

        [Fact]
        public void GivenInternalOrganisationWithCustomFiscalYearStartMonthAndNotExistActualAccountingPeriod_WhenStartingNewFiscalYear_ThenAccountingPeriodsAreCreated()
        {
            this.InstantiateObjects(this.Transaction);

            var organisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("Internal")
                .Build();

            this.Transaction.Derive();

            organisation.SettingsForAccounting.FiscalYearStartMonth = 5;
            organisation.SettingsForAccounting.FiscalYearStartDay = 15;

            organisation.StartNewFiscalYear();

            var fromDate = DateTimeFactory.CreateDate(this.Transaction.Now().Year, 05, 15).Date;
            var month = organisation.SettingsForAccounting.ActualAccountingPeriod;

            Assert.Equal(1, month.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Month, month.Frequency);
            Assert.Equal(fromDate, month.FromDate);
            Assert.Equal(fromDate.AddMonths(1).AddSeconds(-1).Date, month.ThroughDate);
            Assert.True(month.ExistParent);

            var trimester = month.Parent;

            Assert.Equal(1, trimester.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Trimester, trimester.Frequency);
            Assert.Equal(fromDate, trimester.FromDate);
            Assert.Equal(fromDate.AddMonths(3).AddSeconds(-1).Date, trimester.ThroughDate);
            Assert.True(trimester.ExistParent);

            var semester = trimester.Parent;

            Assert.Equal(1, semester.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Semester, semester.Frequency);
            Assert.Equal(fromDate, semester.FromDate);
            Assert.Equal(fromDate.AddMonths(6).AddSeconds(-1).Date, semester.ThroughDate);
            Assert.True(semester.ExistParent);

            var year = semester.Parent;

            Assert.Equal(1, year.PeriodNumber);
            Assert.Equal(new TimeFrequencies(this.Transaction).Year, year.Frequency);
            Assert.Equal(fromDate, year.FromDate);
            Assert.Equal(fromDate.AddMonths(12).AddSeconds(-1).Date, year.ThroughDate);
            Assert.False(year.ExistParent);

            Assert.True(organisation.SettingsForAccounting.ExistActualAccountingPeriod);
        }

        [Fact]
        public void GivenInternalOrganisationWithActiveActualAccountingPeriod_WhenStartingNewFiscalYear_ThenNothingHappens()
        {
            this.InstantiateObjects(this.Transaction);

            var organisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("Internal")
                .Build();

            this.Derive();

            organisation.SettingsForAccounting.FiscalYearStartMonth = 5;
            organisation.SettingsForAccounting.FiscalYearStartDay = 15;

            organisation.StartNewFiscalYear();

            Assert.Equal(4, this.Transaction.Extent<AccountingPeriod>().Count);

            organisation.StartNewFiscalYear();

            Assert.Equal(4, this.Transaction.Extent<AccountingPeriod>().Count);
        }

        [Fact]
        public void GivenInternalOrganisationWithoutDefaultCollectionMethod_WhenExistSingleCollectionMethod_ThenDefaultIsSet()
        {
            this.InternalOrganisation.RemoveDefaultCollectionMethod();

            this.Transaction.Derive();

            Assert.True(this.InternalOrganisation.ExistDefaultCollectionMethod);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.ownBankAccount = (OwnBankAccount)transaction.Instantiate(this.ownBankAccount);
            this.billingAddress = (WebAddress)transaction.Instantiate(this.billingAddress);
        }

        [Fact]
        public void GivenInternalOrganisation_ActiveCustomers_AreDerived()
        {
            var activeCustomersBefore = this.InternalOrganisation.ActiveCustomers.Count;

            var acme = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
            var nike = new OrganisationBuilder(this.Transaction).WithName("Nike").Build();

            var acmeRelation = new CustomerRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation).WithCustomer(acme)
                .WithFromDate(this.Transaction.Now().AddDays(-10))
                .Build();

            var nikeRelation = new CustomerRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithCustomer(nike)
                .Build();

            this.Transaction.Derive();

            Assert.True(this.InternalOrganisation.ExistCustomerRelationshipsWhereInternalOrganisation);
            Assert.True(this.InternalOrganisation.ExistActiveCustomers);
            Assert.Equal(activeCustomersBefore + 2, this.InternalOrganisation.ActiveCustomers.Count);

            // Ending a RelationShip affects the ActiveCustomers
            acmeRelation.ThroughDate = this.Transaction.Now().AddDays(-1).Date;

            this.Transaction.Derive();

            Assert.True(this.InternalOrganisation.ExistCustomerRelationshipsWhereInternalOrganisation);
            Assert.True(this.InternalOrganisation.ExistActiveCustomers);
            Assert.Equal(activeCustomersBefore + 1, this.InternalOrganisation.ActiveCustomers.Count);
        }
    }

    public class InternalOrganisationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InternalOrganisationRuleTests(Fixture fixture) : base(fixture)
        { }

        [Fact]
        public void ChangedDefaultCollectionMethodDeriveDerivedActiveCollectionMethods()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.Contains(internalOrganisation.DefaultCollectionMethod, internalOrganisation.DerivedActiveCollectionMethods);
        }

        [Fact]
        public void ChangedAssignedActiveCollectionMethodsDeriveDerivedActiveCollectionMethods()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            var cash = new CashBuilder(this.Transaction).Build();
            internalOrganisation.AddAssignedActiveCollectionMethod(cash);
            this.Derive();

            Assert.Contains(cash, internalOrganisation.DerivedActiveCollectionMethods);
        }

        [Fact]
        public void ChangedInvoiceSequenceDerivePurchaseInvoiceNumberCounter()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithInvoiceSequence(new InvoiceSequences(this.Transaction).RestartOnFiscalYear)
                .WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.False(internalOrganisation.ExistPurchaseInvoiceNumberCounter);

            internalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.Derive();

            Assert.True(internalOrganisation.ExistPurchaseInvoiceNumberCounter);
        }

        [Fact]
        public void ChangedInvoiceSequenceDerivePurchaseOrderNumberCounter()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithInvoiceSequence(new InvoiceSequences(this.Transaction).RestartOnFiscalYear)
                .WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.False(internalOrganisation.ExistPurchaseOrderNumberCounter);

            internalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.Derive();

            Assert.True(internalOrganisation.ExistPurchaseOrderNumberCounter);
        }

        [Fact]
        public void ChangedRequestSequenceDeriveRequestNumberCounter()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithRequestSequence(new RequestSequences(this.Transaction).RestartOnFiscalYear)
                .WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.False(internalOrganisation.ExistRequestNumberCounter);

            internalOrganisation.RequestSequence = new RequestSequences(this.Transaction).EnforcedSequence;
            this.Derive();

            Assert.True(internalOrganisation.ExistRequestNumberCounter);
        }

        [Fact]
        public void ChangedQuoteSequenceDeriveQuoteNumberCounter()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithQuoteSequence(new QuoteSequences(this.Transaction).RestartOnFiscalYear)
                .WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.False(internalOrganisation.ExistQuoteNumberCounter);

            internalOrganisation.QuoteSequence = new QuoteSequences(this.Transaction).EnforcedSequence;
            this.Derive();

            Assert.True(internalOrganisation.ExistQuoteNumberCounter);
        }

        [Fact]
        public void ChangedWorkEffortSequenceDeriveWorkEffortNumberCounter()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithWorkEffortSequence(new WorkEffortSequences(this.Transaction).RestartOnFiscalYear)
                .WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.False(internalOrganisation.ExistWorkEffortNumberCounter);

            internalOrganisation.WorkEffortSequence = new WorkEffortSequences(this.Transaction).EnforcedSequence;
            this.Derive();

            Assert.True(internalOrganisation.ExistWorkEffortNumberCounter);
        }

        [Fact]
        public void ChangedPurchaseShipmentSequenceDerivePurchaseShipmentNumberCounter()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithPurchaseShipmentSequence(new PurchaseShipmentSequences(this.Transaction).RestartOnFiscalYear)
                .WithIsInternalOrganisation(true).Build();
            this.Derive();

            Assert.False(internalOrganisation.ExistPurchaseShipmentNumberCounter);

            internalOrganisation.PurchaseShipmentSequence = new PurchaseShipmentSequences(this.Transaction).EnforcedSequence;
            this.Derive();

            Assert.True(internalOrganisation.ExistPurchaseShipmentNumberCounter);
        }
    }
}
