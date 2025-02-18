// <copyright file="EmploymentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class EmploymentTests : DomainTest, IClassFixture<Fixture>
    {
        private Person employee;
        private InternalOrganisation internalOrganisation;
        private Employment employment;

        public EmploymentTests(Fixture fixture) : base(fixture)
        {
            this.employee = new PersonBuilder(this.Transaction).WithLastName("slave").Build();

            this.employment = new EmploymentBuilder(this.Transaction)
                .WithEmployee(this.employee)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenActiveEmployment_WhenDeriving_ThenInternalOrganisationEmployeesContainsEmployee()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var employer = this.InternalOrganisation;

            new EmploymentBuilder(this.Transaction)
                .WithEmployee(employee)
                .WithEmployer(employer)
                .Build();

            this.Transaction.Derive();

            Assert.Contains(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void GivenEmploymentToCome_WhenDeriving_ThenInternalOrganisationEmployeesDosNotContainEmployee()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var employer = this.InternalOrganisation;

            new EmploymentBuilder(this.Transaction)
                .WithEmployee(employee)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            this.Transaction.Derive();

            Assert.DoesNotContain(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void GivenEmploymentThatHasEnded_WhenDeriving_ThenInternalOrganisationEmployeesDosNotContainEmployee()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var employer = this.InternalOrganisation;

            new EmploymentBuilder(this.Transaction)
                .WithEmployee(employee)
                .WithFromDate(this.Transaction.Now().AddDays(-10))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            this.Transaction.Derive();

            Assert.DoesNotContain(employee, employer.ActiveEmployees);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.employee = (Person)transaction.Instantiate(this.employee);
            this.internalOrganisation = (InternalOrganisation)transaction.Instantiate(this.internalOrganisation);
            this.employment = (Employment)transaction.Instantiate(this.employment);
        }
    }

    public class EmploymentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public EmploymentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedEmployeeDeriveParties()
        {
            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Derive();

            var customer = new PersonBuilder(this.Transaction).Build();
            employment.Employee = customer;
            this.Derive();

            Assert.Contains(customer, employment.Parties);
        }

        [Fact]
        public void ChangedEmployerDeriveParties()
        {
            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            employment.Employer = internalOrganisation;
            this.Derive();

            Assert.Contains(internalOrganisation, employment.Parties);
        }
    }

    public class EmploymentFromDateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public EmploymentFromDateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void PeriodActiveThrowValidationError()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("employee").Build();
            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(1)).WithEmployee(employee).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodActiveThrowValidationError_1()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("employee").Build();
            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-1)).WithEmployee(employee).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodActiveThrowValidationError_2()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("employee").Build();
            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithThroughDate(this.Transaction.Now().AddDays(1)).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(1)).WithEmployee(employee).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodNotActive()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("employee").Build();
            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithThroughDate(this.Transaction.Now().AddDays(1)).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(2)).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void PeriodNotActive_1()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("employee").Build();
            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-2)).WithThroughDate(this.Transaction.Now().AddDays(-1)).WithEmployee(employee).Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
